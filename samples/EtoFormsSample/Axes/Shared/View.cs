using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using ViewModelsSamples.Axes.Shared;

namespace EtoFormsSample.Axes.Shared;

public class View : Panel
{
    public View()
    {
        var viewModel = new ViewModel();

        var sharedXAxis = new Axis
        {
            Labeler = viewModel.Labeler,
            MaxLimit = viewModel.Max,
            CrosshairPaint = new SolidColorPaint(new SKColor(0xFF, 0x45, 0x00, 0x40)), // #40FF4500
            CrosshairLabelsBackground = new SKColor(0xFF, 0x45, 0x00).AsLvcColor(), // #FF4500
            CrosshairLabelsPaint = new SolidColorPaint(SKColors.White)
        };
        var sharedXAxis2 = new Axis
        {
            MaxLimit = viewModel.Max,
            CrosshairPaint = new SolidColorPaint(new SKColor(0xFF, 0x45, 0x00, 0x40)) // #40FF4500
        };
        LiveChartsCore.SharedAxes.Set(sharedXAxis, sharedXAxis2);

        var cartesianChart = new CartesianChart
        {
            Series = new ISeries[]
            {
                new LineSeries<int> { Values = viewModel.Values1 }
            },
            ZoomMode = LiveChartsCore.Measure.ZoomAndPanMode.X,
            XAxes = new[] { sharedXAxis },
            DrawMargin = new LiveChartsCore.Measure.Margin(70, LiveChartsCore.Measure.Margin.Auto, LiveChartsCore.Measure.Margin.Auto, LiveChartsCore.Measure.Margin.Auto)
        };

        var cartesianChart2 = new CartesianChart
        {
            Series = new ISeries[]
            {
                new ColumnSeries<int> { Values = viewModel.Values2 }
            },
            ZoomMode = LiveChartsCore.Measure.ZoomAndPanMode.X,
            XAxes = new[] { sharedXAxis2 },
            DrawMargin = new LiveChartsCore.Measure.Margin(70, LiveChartsCore.Measure.Margin.Auto, LiveChartsCore.Measure.Margin.Auto, LiveChartsCore.Measure.Margin.Auto)
        };

        Content = new DynamicLayout(
            new DynamicRow(new DynamicControl { Control = cartesianChart, YScale = true }),
            new DynamicRow(new DynamicControl { Control = cartesianChart2, YScale = true })
        );
    }
}
