namespace LiveChartsGenerators.Frameworks;

/// <summary>Generates Noesis dependency properties.</summary>
/// <param name="context">The generated member category.</param>
/// <param name="native">Whether the renderer uses native Noesis geometry.</param>
public class NoesisTemplate(FrameworkTemplate.Context context, bool native = false) : FrameworkTemplate(context)
{
    /// <inheritdoc />
    public override string Key => "Noesis";

    /// <inheritdoc />
    public override string TypeConvertersNamespace => native
        ? "LiveChartsCore.Noesis.TypeConverters"
        : base.TypeConvertersNamespace;

    /// <inheritdoc />
    public override string DeclareBindableProperty(string propertyName, string propertyType) =>
        $"public static readonly new global::Noesis.DependencyProperty {propertyName}Property";

    /// <inheritdoc />
    public override string CreateBindableProperty(
        string propertyName,
        string propertyType,
        bool isValueTypeProperty,
        string bindableType,
        string defaultValue,
        OnChangeInfo? onChangeInfo = null)
    {
        var sanitizedPropertyType = !isValueTypeProperty && propertyType.EndsWith("?")
            ? propertyType.Substring(0, propertyType.Length - 1)
            : propertyType;

        if (isValueTypeProperty && propertyType.EndsWith("?") && propertyType.StartsWith("LiveChartsCore."))
        {
            sanitizedPropertyType = "object";
        }

        var callback = onChangeInfo is null
            ? ActiveContext == Context.XamlObject
                ? $", (o, args) => (({bindableType})o).MapChangeToBaseType(\"{propertyName}\")"
                : string.Empty
            : $", (o, args) => {GetOnChangedExpression(onChangeInfo.Value, bindableType, propertyType)}";

        return $"global::Noesis.DependencyProperty.Register(\"{propertyName}\", typeof({sanitizedPropertyType}), typeof({bindableType}), new global::Noesis.PropertyMetadata({defaultValue}{callback}));";
    }

    /// <inheritdoc />
    public override string GetPropertyChangedMetod() => string.Empty;

    private static string GetOnChangedExpression(OnChangeInfo onChangeInfo, string bindableType, string propertyType)
    {
        if (!onChangeInfo.HasChangeParams)
        {
            return $"{onChangeInfo.Expression}(({bindableType})o)";
        }

        var cast = onChangeInfo.HasChangeObjectParams ? string.Empty : $"({propertyType})";

        return $"{onChangeInfo.Expression}(({bindableType})o, {cast}args.OldValue, {cast}args.NewValue)";
    }
}
