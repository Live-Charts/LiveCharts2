using System;
using System.Globalization;
using Noesis;
using LiveChartsCore.Noesis.Painting;
using LiveChartsCore.Drawing;

namespace NoesisSample.Pies.Nested;

public class StringToPaintConverter : IValueConverter
{

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string colorString)
            return new SolidColorPaint(LvcColor.Parse(colorString));
        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is SolidColorPaint paint)
        {
            var c = paint.Color;
            return $"#{c.R:X2}{c.G:X2}{c.B:X2}";
        }
        return DependencyProperty.UnsetValue;
    }
}

public sealed class PaintConverterExtension : MarkupExtension
{
    public override object ProvideValue(IServiceProvider serviceProvider) => new StringToPaintConverter();
}
