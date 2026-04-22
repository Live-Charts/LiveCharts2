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
using LiveChartsGenerators.Templates;
using Microsoft.CodeAnalysis;

namespace LiveChartsGenerators.Frameworks;

public abstract class FrameworkTemplate(FrameworkTemplate.Context context)
{
    public Context ActiveContext => context;
    public abstract string Key { get; }

    public string GetBindablePropertySyntax(XamlObject target, string key, IPropertySymbol property, BindablePropertyInitializer? initializer = null) =>
        GetBindablePropertySyntax(target, key, property.Name, property.Type.ToDisplayString(), property.Type.IsValueType, initializer);

    public virtual string FullPropertySyntax(XamlProperty property) => string.Empty;

    public string GetBindablePropertySyntax(
        XamlObject target, string key, string propertyName, string propertyType, bool isValueTypeProperty, BindablePropertyInitializer? initializer)
    {
        var originalPropertyType = propertyType;

        var isTypeOverriden = false;
        if (target.OverridenTypes.TryGetValue(propertyName, out var overridenType))
        {
            propertyType = overridenType;
            isTypeOverriden = true;
        }

        var originalPropertyName = propertyName;
        if (target.OverridenNames.TryGetValue(propertyName, out var overridenName))
            propertyName = overridenName;

        var sb = new StringBuilder();

        _ = sb
            .Append(@$"    {DeclareBindableProperty(propertyName, propertyType)}");

        _ = initializer is not null
            ? sb.Append(" = ").Append(CreateBindableProperty(propertyName, propertyType, isValueTypeProperty, initializer.BindableType, initializer.DefaultValue))
            : sb.Append(';').AppendLine();

        // disable type converters in avalonia, they "work" partially and when migrated to v12 properties of a LiveCharts type just throw on bindings
        // https://github.com/AvaloniaUI/Avalonia/issues/12194
        if (Key != "Avalonia" && XamlObjectTempaltes.TypeConverters.TryGetValue(originalPropertyType, out var typeConverter))
            _ = sb.AppendLine(@$"    [System.ComponentModel.TypeConverter(typeof({typeConverter}))]");

        var path = key;
        if (key.Length == 0)
            path = "_baseType";

        var getter = !propertyName.EndsWith("Command")
            ? $"{(isTypeOverriden ? $"({propertyType})" : string.Empty)}{path}.{originalPropertyName}"
            : $"({propertyType})GetValue({propertyName}Property)";

        _ = sb.Append(@$"    public new {propertyType} {propertyName}
    {{
        get => {getter};
        set => SetValue({propertyName}Property, value);
    }}");

        return sb.ToString();
    }

    public string GetBindablePropertyDefinition(XamlObject target, IPropertySymbol property, string path)
    {
        var fallbackInfo = GetFallbackInfo(target.BasedOn);
        var fallBackName = path.Length > 0
            ? $"_default{path}"
            : fallbackInfo.Item2;

        var propertyName = property.Name;
        var propertyType = property.Type.ToDisplayString();

        if (target.OverridenTypes.TryGetValue(propertyName, out var overridenType))
            propertyType = overridenType;

        var originalPropertyName = propertyName;
        if (target.OverridenNames.TryGetValue(propertyName, out var overridenName))
            propertyName = overridenName;

        return CreateBindableProperty(
            propertyName, propertyType, property.Type.IsValueType, target.Name, $"{fallBackName}.{originalPropertyName}");
    }

    public static (string, string) GetFallbackInfo(ITypeSymbol target)
    {
        var baseType = target.OriginalDefinition.ToDisplayString();
        var baseTypeName = target.Name;

        return (baseType, $"_default{baseTypeName}");
    }

    public abstract string DeclareBindableProperty(string propertyName, string propertyType);
    public abstract string CreateBindableProperty(
        string propertyName, string propertyType, bool isValueTypeProperty, string bindableType, string defaultValue, OnChangeInfo? changeInfo = null);
    public abstract string GetPropertyChangedMetod();

    public enum Context
    {
        XamlObject,
        XamlProperty,
    }

    public struct OnChangeInfo(
        string? onChanged, bool isObject = false, bool hasChangeParams = false)
    {
        public string? Expression { get; set; } = onChanged;
        public bool HasChangeObjectParams { get; set; } = isObject;
        public bool HasChangeParams { get; set; } = hasChangeParams;
    }
}
