using Noesis;
using LiveChartsCore.Noesis;
using LiveChartsCore.Noesis.Drawing.Geometries;
using ViewModelsSamples.Pies.Icons;

namespace NoesisSample.Pies.Icons;

public class CustomPieSeries : XamlPieSeries<double, DoughnutGeometry, SvgLabel>
{
    public CustomPieSeries()
    {
        PointMeasured +=
            point =>
            {
                var label = point.Label!;

                label.SvgString = Icon;
                label.Name = IconName;
                label.Size = 50;
                label.TranslateTransform = new(-25, -25);
            };
    }

    public static DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(string), typeof(CustomPieSeries));
    public static DependencyProperty IconNameProperty =
        DependencyProperty.Register(nameof(IconName), typeof(string), typeof(CustomPieSeries));

    public string Icon
    {
        get => (string)GetValue(IconProperty)!;
        set => SetValue(IconProperty, value);
    }

    public string IconName
    {
        get => (string)GetValue(IconNameProperty)!;
        set => SetValue(IconNameProperty, value);
    }
}
