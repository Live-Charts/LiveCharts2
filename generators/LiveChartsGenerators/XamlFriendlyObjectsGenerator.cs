// The MIT License(MIT)
//
// Copyright(c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Text;
using LiveChartsGenerators.Definitions;
using LiveChartsGenerators.Frameworks;
using LiveChartsGenerators.Templates;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace LiveChartsGenerators;

[Generator]
public class XamlFriendlyObjectsGenerator : IIncrementalGenerator
{
    private const string XamlAttribute = "LiveChartsCore.Generators.XamlClassAttribute";
    private const string MotionPropertyAttribute = "LiveChartsCore.Generators.MotionPropertyAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // To debug the source generator, go to directory.build.props and set the property <DebugCodeGeneration> to true

#if DEBUG_CODE_GENERATION
        if (!System.Diagnostics.Debugger.IsAttached)
        {
            System.Diagnostics.Debugger.Launch();
            System.Diagnostics.Debugger.Break();
        }
#endif

        var assemblyAttributes = context.CompilationProvider
            .Select(GetConsumerAssemblyType);

        var xamlObjects = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                XamlAttribute,
                predicate: static (syntaxNode, _) => syntaxNode is ClassDeclarationSyntax,
                transform: static (ctx, _) => GetXamlObjectsToGenerate(ctx.SemanticModel, ctx.TargetNode))
            .Where(static m => m is not null);

        var motionProperties = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                MotionPropertyAttribute,
                predicate: static (syntaxNode, _) => syntaxNode is PropertyDeclarationSyntax,
                transform: static (ctx, _) => GetMotionPropertiesToGenerate(ctx.SemanticModel, ctx.TargetNode))
            .Where(static m => m is not null)
            .Collect()
            .Select((props, _) => props
                .GroupBy(prop => prop?.Property.ContainingType, SymbolEqualityComparer.Default)
                .ToList());

        var xamlProperties = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) =>
                    node is FieldDeclarationSyntax fieldDecl &&
                    fieldDecl.Modifiers.Any(SyntaxKind.StaticKeyword),
                static (ctx, _) => GetUIPropertiesToGenerate(ctx.SemanticModel, ctx.Node)
            )
            .Where(symbol => symbol is not null)
            .Collect()
            .Select((props, _) => props
                .GroupBy(prop => prop?.DeclaringType, SymbolEqualityComparer.Default)
                .ToList());

        context.RegisterSourceOutput(xamlObjects.Combine(assemblyAttributes), static (spc, pair) =>
        {
            var (xamlObject, consumerType) = pair;
            if (xamlObject is not { } value) return;

            spc.AddSource(
                $"{value.Name}.g.cs".Replace('<', '_').Replace('>', '_'),
                SourceText.From(XamlObjectTempaltes.GetTemplate(value, GetFrameworkTemplate(consumerType, FrameworkTemplate.Context.XamlObject)), Encoding.UTF8));
        });

        context.RegisterSourceOutput(motionProperties, (spc, groups) =>
        {
            foreach (var group in groups)
            {
                if (group is null || group.Key is null) continue;

                foreach (var item in group)
                {
                    if (item is not { } value) return;

                    spc.AddSource(
                        $"{value.Property.ContainingType.Name}.{value.Property.Name}.g.cs".Replace('<', '_').Replace('>', '_'),
                        SourceText.From(MotionPropertyTemplates.GetTemplate(value), Encoding.UTF8));
                }

                spc.AddSource(
                    $"{group.Key.Name}.g.cs".Replace('<', '_').Replace('>', '_'),
                    MotionPropertyTemplates.GetClassTemplate(group));
            }
        });

        context.RegisterSourceOutput(xamlProperties.Combine(assemblyAttributes), (spc, pair) =>
        {
            var (groups, consumerType) = pair;

            foreach (var group in groups)
            {
                if (group is null || group.Key is null) continue;

                if (consumerType == "Avalonia")
                {
                    var name = ((INamedTypeSymbol)group.Key).ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

                    // exception for avalonia, we need to generate additional code for the base type
                    spc.AddSource(
                        $"{name}._avaloina_onChange.g.cs".Replace('<', '_').Replace('>', '_'),
                        SourceText.From(
                            UIPropertyTempaltes.GetAvaloniaBaseTypeTemplate(group, GetFrameworkTemplate(consumerType, FrameworkTemplate.Context.XamlProperty)),
                            Encoding.UTF8));
                }

                foreach (var property in group)
                {
                    if (property is not { } value) return;

                    spc.AddSource(
                        $"{value.DeclaringType.Name}.{value.Name}.g.cs".Replace('<', '_').Replace('>', '_'),
                        SourceText.From(UIPropertyTempaltes.GetTemplate(value, GetFrameworkTemplate(consumerType, FrameworkTemplate.Context.XamlProperty)), Encoding.UTF8));
                }
            }
        });
    }

    private static XamlObject? GetXamlObjectsToGenerate(SemanticModel semanticModel, SyntaxNode declarationSyntax)
    {
        if (semanticModel.GetDeclaredSymbol(declarationSyntax) is not INamedTypeSymbol symbol)
            return null; // something went wrong

        var bindablePropertiesDic = new Dictionary<string, List<IPropertySymbol>>();
        var notBindableProperties = new Dictionary<string, IPropertySymbol>();
        var events = new List<IEventSymbol>();
        var methods = new Dictionary<string, IMethodSymbol>();
        var explicitMethods = new Dictionary<string, IMethodSymbol>();

        var targetAttribute = symbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == XamlAttribute);
        if (targetAttribute is null) return null;

        if (targetAttribute.ConstructorArguments.FirstOrDefault().Value is not ITypeSymbol baseType)
            return null;

        string? fileHeader = null;
        string? propertyChangeMap = null;
        string? overridenTypes = null;
        string? overridenNames = null;
        var manualOnPropertyChanged = false;
        ITypeSymbol? alsoMap = null;
        ITypeSymbol? tModel = null;
        ITypeSymbol? tVisual = null;
        ITypeSymbol? tLabel = null;
        string? alsoMapPath = null;
        var generateBaseTypeDeclaration = true;

        foreach (var arg in targetAttribute.NamedArguments)
        {
            switch (arg.Key)
            {
                case "FileHeader":
                    fileHeader = arg.Value.Value as string;
                    break;
                case "GenerateOnChange":
                    manualOnPropertyChanged = !(((bool?)arg.Value.Value) ?? true);
                    break;
                case "PropertyChangeMap":
                    propertyChangeMap = arg.Value.Value as string;
                    break;
                case "PropertyTypeOverride":
                    overridenTypes = arg.Value.Value as string;
                    break;
                case "PropertyNameOverride":
                    overridenNames = arg.Value.Value as string;
                    break;
                case "Map":
                    alsoMap = arg.Value.Value as ITypeSymbol;
                    break;
                case "MapPath":
                    alsoMapPath = arg.Value.Value as string;
                    break;
                case "GenerateBaseTypeDeclaration":
                    generateBaseTypeDeclaration = ((bool?)arg.Value.Value) ?? true;
                    break;
                case "TModel":
                    tModel = arg.Value.Value as ITypeSymbol;
                    break;
                case "TVisual":
                    tVisual = arg.Value.Value as ITypeSymbol;
                    break;
                case "TLabel":
                    tLabel = arg.Value.Value as ITypeSymbol;
                    break;
                default:
                    break;
            }
        }

        var ns = symbol.ContainingNamespace.ToString();

        // use this method to get also the generic argument name.
        var nameParts = symbol.ToDisplayString().Split('.');
        var name = nameParts[nameParts.Length - 1];

        var members = new Dictionary<string, List<ISymbol>>
        {
            [string.Empty] = GetLiveChartsMembers(baseType)
        };

        if (alsoMap is not null && alsoMapPath is not null)
            members[alsoMapPath] = GetLiveChartsMembers(alsoMap);

        foreach (var pair in members)
        {
            var bindableProperties = new List<IPropertySymbol>();
            bindablePropertiesDic[pair.Key] = bindableProperties;

            foreach (var member in pair.Value)
            {
                if (member is IPropertySymbol property)
                {
                    if (property.DeclaredAccessibility == Accessibility.Protected || property.IsStatic)
                        continue;

                    var notExplicit = property.ExplicitInterfaceImplementations.Length == 0;
                    var hasSetter = property.SetMethod is not null;
                    var isPublic = property.DeclaredAccessibility == Accessibility.Public;

                    if (notExplicit && hasSetter && isPublic)
                    {
                        bindableProperties.Add(property);
                    }
                    else if (pair.Key == string.Empty)
                    {
                        // when key is empty, we are on the base type, we only map regular properties from the base type
                        var propertyKey = property.Name;
                        notBindableProperties[propertyKey] = property;
                    }
                }

                // ignore methods and events when not in the base type
                if (pair.Key != string.Empty) continue;

                if (member is IMethodSymbol method)
                {
                    // the key is the method name, this is to avoid duplicated methods
                    var methodDisplayString = method.Name;
                    var methodParts = method.ToDisplayString().Split('(');
                    var methodKey = $"{method.Name}({methodParts[methodParts.Length - 1]}";

                    if (method.MethodKind == MethodKind.Ordinary && method.DeclaredAccessibility == Accessibility.Public)
                        methods[methodKey] = method;

                    if (method.MethodKind == MethodKind.ExplicitInterfaceImplementation)
                        explicitMethods[methodKey] = method;
                }

                if (member is IEventSymbol @event)
                {
                    // normally, the property changed event is already implemented in XAML objects
                    // for now we just ignore it...
                    //if (@event.Name == "PropertyChanged")
                    //    continue;
                    events.Add(@event);
                }
            }
        }

        return new(
            generateBaseTypeDeclaration, ns, name, symbol, baseType, bindablePropertiesDic, [.. notBindableProperties.Values], events,
            [.. methods.Values], [.. explicitMethods.Values], fileHeader, manualOnPropertyChanged, propertyChangeMap, overridenTypes, overridenNames,
            tModel, tVisual, tLabel);
    }

    private static MotionProperty? GetMotionPropertiesToGenerate(SemanticModel semanticModel, SyntaxNode declarationSyntax)
    {
        if (semanticModel.GetDeclaredSymbol(declarationSyntax) is not IPropertySymbol symbol)
            return null; // something went wrong

        var targetAttribute = symbol.GetAttributes()
           .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == MotionPropertyAttribute);
        if (targetAttribute is null) return null;

        var hasExplicitAcessors = false;

        foreach (var arg in targetAttribute.NamedArguments)
        {
            switch (arg.Key)
            {
                case "HasExplicitAcessors":
                    hasExplicitAcessors = ((bool?)arg.Value.Value) ?? true;
                    break;
                default:
                    break;
            }
        }

        return new(symbol, hasExplicitAcessors);
    }

    private static XamlProperty? GetUIPropertiesToGenerate(SemanticModel semanticModel, SyntaxNode node)
    {
        var fieldDecl = (FieldDeclarationSyntax)node;
        // Assume we only care about one variable per declaration
        var variable = fieldDecl.Declaration.Variables.FirstOrDefault();
        if (variable is null)
            return null;

        if (semanticModel.GetDeclaredSymbol(variable) is not IFieldSymbol fieldSymbol)
            return null;

        var displayFormat = new SymbolDisplayFormat(
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.None);

        string? xmlDocs = null;

        var trivia = fieldDecl.GetLeadingTrivia().FirstOrDefault(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia));
        xmlDocs = trivia.GetStructure()?.ToFullString();

        // Check if field's type is the type you're interested in
        if (fieldSymbol.Type.ToDisplayString(displayFormat) == "LiveChartsCore.Generators.UIProperty")
        {
            var camelCasedName = variable.Identifier.Text;
            var propertyName = $"{camelCasedName.Substring(0, 1).ToUpperInvariant()}{camelCasedName.Substring(1, camelCasedName.Length - 1)}";
            var propertyType = ((INamedTypeSymbol)fieldSymbol.Type).TypeArguments[0];
            string? defaultValue = null;
            FrameworkTemplate.OnChangeInfo? onChangeInfo = null;

            var syntaxReference = fieldSymbol.DeclaringSyntaxReferences.First();
            var syntaxTree = syntaxReference.SyntaxTree;
            var root = syntaxTree.GetRoot() as CompilationUnitSyntax;
            var usingDirectives = root?.Usings.Select(u => u.ToString());
            var headers = string.Join(@"
", usingDirectives);

            foreach (var syntaxRef in fieldSymbol.DeclaringSyntaxReferences)
            {
                var syntaxNode = syntaxRef.GetSyntax();

                if (syntaxNode is VariableDeclaratorSyntax variableDeclarator)
                {
                    if (variableDeclarator.Initializer != null)
                    {
                        var initializerExpression = variableDeclarator.Initializer.Value;

                        if (initializerExpression is ImplicitObjectCreationExpressionSyntax ioce)
                        {
                            for (var i = 0; i < ioce.ArgumentList.Arguments.Count; i++)
                            {
                                var argument = ioce.ArgumentList.Arguments[i];
                                var nameColon = argument.NameColon?.ToString() ?? XamlProperty.ByPosition[i];

                                switch (nameColon)
                                {
                                    case "defaultValue:":
                                        defaultValue = argument.Expression.ToString();
                                        break;
                                    case "onChanged:":
                                        var hasParams = false;
                                        var hasObjectParams = false;

                                        var info = semanticModel.GetSymbolInfo(argument.Expression);
                                        // maybe we need to improve the next line?
                                        if ((info.Symbol ?? info.CandidateSymbols.FirstOrDefault()) is IMethodSymbol methodSymbol)
                                        {
                                            hasParams = methodSymbol.Parameters.Length > 2;
                                            hasObjectParams =
                                                hasParams &&
                                                SymbolEqualityComparer.Default.Equals(
                                                    methodSymbol.Parameters[1].Type, semanticModel.Compilation.ObjectType);
                                        }
                                        var onChanged = argument.Expression.ToString();
                                        onChangeInfo = new FrameworkTemplate.OnChangeInfo(
                                            onChanged, hasObjectParams, hasParams);
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
            }

            return new(propertyName, propertyType, fieldSymbol.ContainingType, headers, defaultValue, onChangeInfo, xmlDocs);
        }

        return null;
    }

    private static string GetConsumerAssemblyType(Compilation compilation, CancellationToken cancellationToken)
    {
        var currentConsumerType = ResolveConsumerType(compilation.Assembly.Name);
        if (currentConsumerType is not null) return currentConsumerType;

        foreach (var reference in compilation.References)
        {
            if (reference is PortableExecutableReference peReference)
            {
                var symbol = compilation.GetAssemblyOrModuleSymbol(peReference);
                if (symbol is IAssemblySymbol assemblySymbol)
                {
                    var type = ResolveConsumerType(assemblySymbol.Name);
                    if (type is null) continue;
                    return type;
                }
            }
        }

        return "Unknown";
    }

    private static string? ResolveConsumerType(string assemblyName)
    {
        return assemblyName switch
        {
            "LiveChartsCore.SkiaSharpView.Maui" => "Maui",
            "LiveChartsCore.SkiaSharpView.WinUI" => "WinUI",
            "LiveChartsCore.SkiaSharpView.Uno.WinUI" => "WinUI",
            "LiveChartsCore.SkiaSharpView.Avalonia" => "Avalonia",
            "LiveChartsCore.SkiaSharpView.WPF" => "WPF",
            "LiveChartsCore.SkiaSharpView.Blazor" => "Blazor",
            "LiveChartsCore.SkiaSharpView.WinForms" => "WinForms",
            "LiveChartsCore.SkiaSharpView.Eto" => "WinForms",
            _ => "WinForms" // as a default, use winforms which is just classic style properties
        };
    }

    private static FrameworkTemplate GetFrameworkTemplate(string consumerType, FrameworkTemplate.Context context)
    {
        return consumerType switch
        {
            "Maui" => new MauiTemplate(context),
            "WinUI" => new WinUITemplate(context),
            "Avalonia" => new AvaloniaTemplate(context),
            "WPF" => new WPFTemplate(context),
            "WinForms" => new WinformsTemplate(context),
            "Blazor" => new BlazorTemplate(context),
            _ => throw new NotSupportedException($"The consumer type '{consumerType}' is not supported.")
        };
    }

    private static List<ISymbol> GetLiveChartsMembers(ITypeSymbol typeSymbol)
    {
        var allMembers = new List<ISymbol>();

        while (typeSymbol is not null)
        {
            allMembers.AddRange(typeSymbol.GetMembers());

            if (typeSymbol.OriginalDefinition.BaseType is null) break;
            typeSymbol = typeSymbol.OriginalDefinition.BaseType;

            if (!typeSymbol.ContainingNamespace.ToDisplayString().StartsWith("LiveChartsCore"))
                break;
        }

        return allMembers;
    }
}
