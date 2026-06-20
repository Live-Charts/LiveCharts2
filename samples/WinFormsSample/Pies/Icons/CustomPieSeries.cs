using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using ViewModelsSamples.Pies.Icons;

namespace WinFormsSample.Pies.Icons;

public class CustomPieSeries : PieSeries<double, DoughnutGeometry, SvgLabel>
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

    public string Icon { get; set; }

    public string IconName { get; set; }
}
