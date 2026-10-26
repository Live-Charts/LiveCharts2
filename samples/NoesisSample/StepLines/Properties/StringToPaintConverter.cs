using System;
using System.Globalization;
using Noesis;
using LiveChartsCore.Noesis.Painting;
using LiveChartsCore.Drawing;

namespace NoesisSample.StepLines.Properties;

public class StringToPaintConverter : IValueConverter
{

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string s && !string.IsNullOrWhiteSpace(s))
        {
            // Try to parse as a color
            if (s.StartsWith("#"))
            {
                try
                {
                    var color = LvcColor.Parse(s);
                    return new SolidColorPaint(color);
                }
                catch { }
            }
        }
        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public sealed class PaintConverterExtension : MarkupExtension
{
    public override object ProvideValue(IServiceProvider serviceProvider) => new StringToPaintConverter();
}
