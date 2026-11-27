using System.Windows.Forms;
using LiveChartsCore.SkiaSharpView.WinForms;
using LiveChartsCore.SkiaSharpView;
using ViewModelsSamples.General.TemplatedTooltips;
using LiveChartsCore;
using LiveChartsCore.Drawing;

namespace WinFormsSample.General.TemplatedTooltips;

public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(50, 50);

        // Register a map to convert the GeometryPoint to a coordinate in the chart
        LiveCharts.Configure(config =>
            config.HasMap<GeometryPoint>((dataPoint, index) => new(index, dataPoint.Value)));

        var values = new GeometryPoint[]
        {
            new() { Value = 4, Geometry = SVGPoints.Star },
            new() { Value = 2, Geometry = SVGPoints.Square },
            new() { Value = 6, Geometry = SVGPoints.Circle },
            new() { Value = 3, Geometry = SVGPoints.Cross },
            new() { Value = 5, Geometry = SVGPoints.Diamond }
        };

        var series = new ISeries[]
        {
            new ColumnSeries<GeometryPoint> { Values = values }
        };

        var cartesianChart = new CartesianChart
        {
            Series = series,
            Tooltip = new CustomTooltip(),
            // out of livecharts properties...
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(50, 50),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(cartesianChart);
    }
}
