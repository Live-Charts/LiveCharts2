using System.Windows.Forms;
using LiveChartsCore.SkiaSharpView.WinForms;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using SkiaSharp;

namespace WinFormsSample.Lines.CustomPoints;

public class DataPoint
{
    public int Value { get; set; }
    public float Rotation { get; set; }
}

public class ArrowGeometry : BaseSVGPathGeometry
{
    private static SKPath _path = SKPath.ParseSvgPathData(
        "M1 16 A15 15 0 1 0 31 16 A15 15 0 1 0 1 16 Z " + // the circle
        "M16 25 L16 7 M9.5 13.5 L16 7 L22.5 13.5");        // the arrow, pointing up

    public ArrowGeometry() : base(_path) { }
}

public class CustomArrowLineSeries : LineSeries<DataPoint, ArrowGeometry>
{
    public CustomArrowLineSeries()
    {
        LiveCharts.Configure(config =>
            config.HasMap<DataPoint>((dataPoint, index) => new(index, dataPoint.Value)));

        Fill = null;
        GeometrySize = 34;
        PointMeasured += (point) =>
        {
            var dataPoint = (DataPoint)point.Context.DataSource;
            point.Context.Visual.TransformOrigin = new(0f, 0f);
            point.Context.Visual.RotateTransform = dataPoint.Rotation;
        };
    }
}

public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(50, 50);

        var values = new DataPoint[]
        {
            new() { Value = 4, Rotation = 0 },
            new() { Value = 6, Rotation = 20 },
            new() { Value = 8, Rotation = 90 },
            new() { Value = 2, Rotation = 176 },
            new() { Value = 7, Rotation = 55 },
            new() { Value = 9, Rotation = 226 },
            new() { Value = 3, Rotation = 320 }
        };

        var series = new ISeries[]
        {
            new CustomArrowLineSeries
            {
                Values = values
            }
        };

        var cartesianChart = new CartesianChart
        {
            Series = series,
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(50, 50),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(cartesianChart);
    }
}
