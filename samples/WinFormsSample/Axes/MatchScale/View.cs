using System.Drawing;
using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WinForms;
using SkiaSharp;

#pragma warning disable IDE1006 // Naming Styles

namespace WinFormsSample.Axes.MatchScale;

public partial class View : UserControl
{
    private readonly CartesianChart cartesianChart;

    public View()
    {
        InitializeComponent();
        Size = new Size(100, 100);

        var values = new ObservablePoint[1001];
        var fx = EasingFunctions.BounceInOut;
        for (var i = 0; i < 1001; i++)
        {
            var x = i / 1000f;
            var y = fx(x);
            values[i] = new ObservablePoint(x - 0.5, y - 0.5);
        }

        var series = new ISeries[]
        {
            new LineSeries<ObservablePoint>
            {
                Values = values,
                Stroke = new SolidColorPaint(new SKColor(33, 150, 243, 255), 4), // DeepSkyBlue
                Fill = null,
                GeometryStroke = null,
                GeometryFill = null
            }
        };

        var separatorColor = new SKColor(119, 148, 180, 100); // #64b4b4b4 with alpha 100

        // we are forcing the step to 0.1 to highlight that
        // both axes take the same amount of pixels per unit of data,
        // but in a real scenario the MinStep is not necessary

        var xAxis = new Axis
        {
            Name = "XAxis",
            SeparatorsPaint = new SolidColorPaint(separatorColor),
            MinStep = 0.1,
            ForceStepToMin = true
        };
        var yAxis = new Axis
        {
            Name = "YAxis",
            SeparatorsPaint = new SolidColorPaint(separatorColor),
            MinStep = 0.1,
            ForceStepToMin = true
        };

        var frame = new DrawMarginFrame
        {
            Stroke = new SolidColorPaint(separatorColor, 2)
        };

        cartesianChart = new CartesianChart
        {
            Series = series,
            XAxes = [xAxis],
            YAxes = [yAxis],
            DrawMarginFrame = frame,
            MatchAxesScreenDataRatio = true,
            ZoomMode = LiveChartsCore.Measure.ZoomAndPanMode.Both,
            TooltipPosition = LiveChartsCore.Measure.TooltipPosition.Hidden,
            Location = new Point(0, 0),
            Size = new Size(100, 100),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(cartesianChart);
    }
}
