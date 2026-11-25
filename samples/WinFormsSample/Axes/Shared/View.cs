using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;
using LiveChartsCore.Measure;
using LiveChartsCore.VisualElements;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using ViewModelsSamples.Axes.Shared;

#pragma warning disable IDE1006 // Naming Styles

namespace WinFormsSample.Axes.Shared;

public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(50, 50);

        var viewModel = new ViewModel();

        // Shared axes
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
        // Optionally, set MinLimit/MaxLimit to align axes if needed

        // Set axes as shared
        SharedAxes.Set(sharedXAxis, sharedXAxis2);

        var cartesianChart = new CartesianChart
        {
            Series = [
                new LineSeries<int> { Values = viewModel.Values1 }
            ],
            ZoomMode = ZoomAndPanMode.X,
            XAxes = [sharedXAxis],
            DrawMargin = new LiveChartsCore.Measure.Margin(70, LiveChartsCore.Measure.Margin.Auto, LiveChartsCore.Measure.Margin.Auto, LiveChartsCore.Measure.Margin.Auto),
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(50, 50),
            Dock = DockStyle.Fill
        };

        var cartesianChart2 = new CartesianChart
        {
            Series =
            [
                new ColumnSeries<int> { Values = viewModel.Values2 }
            ],
            ZoomMode = ZoomAndPanMode.X,
            XAxes = [sharedXAxis2],
            DrawMargin = new LiveChartsCore.Measure.Margin(70, LiveChartsCore.Measure.Margin.Auto, LiveChartsCore.Measure.Margin.Auto, LiveChartsCore.Measure.Margin.Auto),
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(50, 50),
            Dock = DockStyle.Fill
        };

        var splitContainer = new SplitContainer
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            Orientation = Orientation.Horizontal
        };

        splitContainer.Panel1.Controls.Add(cartesianChart);
        splitContainer.Panel2.Controls.Add(cartesianChart2);
        Controls.Add(splitContainer);
    }
}
