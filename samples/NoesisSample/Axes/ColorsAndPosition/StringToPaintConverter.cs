using System;
using System.Globalization;
using Noesis;
using LiveChartsCore.Noesis.Painting;
using LiveChartsCore.Drawing;

namespace NoesisSample.Axes.ColorsAndPosition;

public class StringToPaintConverter : IValueConverter
{

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string colorString)
        {
            return new SolidColorPaint(LvcColor.Parse(colorString));
        }
        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is SolidColorPaint paint)
        {
            return paint.Color.R.ToString("X2") +
                   paint.Color.G.ToString("X2") +
                   paint.Color.B.ToString("X2");
        }
        return DependencyProperty.UnsetValue;
    }
}

public sealed class PaintConverterExtension : MarkupExtension
{
    public override object ProvideValue(IServiceProvider serviceProvider) => new StringToPaintConverter();
}
