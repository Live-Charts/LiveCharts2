using System.ComponentModel;
using System.Globalization;
using LiveChartsCore.Drawing;
using LiveChartsCore.Noesis.Painting;

namespace LiveChartsCore.Noesis.TypeConverters;

/// <summary>Converts hexadecimal colors to chart paints.</summary>
public sealed class HexToPaintTypeConverter : TypeConverter
{
    /// <inheritdoc />
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
        sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    /// <inheritdoc />
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
        value is string text && LvcColor.TryParse(text, out var color)
            ? new SolidColorPaint(color)
            : base.ConvertFrom(context, culture, value);
}
