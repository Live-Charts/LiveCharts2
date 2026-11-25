using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using ViewModelsSamples.Axes.Crosshairs;

#pragma warning disable IDE1006 // Naming Styles

namespace WinFormsSample.Axes.Crosshairs;

public partial class View : UserControl
{
    private readonly CartesianChart cartesianChart;

    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(100, 100);

        var viewModel = new ViewModel();

        var series = new ISeries[]
        {
            new LineSeries<double> { Values = viewModel.Values }
        };

        var crosshairColor = new SKColor(0xFF, 0x8C, 0x00); // #FF8C00
        var crosshairLabelsColor = new SKColor(0x8B, 0x00, 0x00); // #8B0000
        var crosshairBackground = new LiveChartsCore.Drawing.LvcColor(0xFF, 0x8C, 0x00);

        var xAxis = new Axis
        {
            Labeler = viewModel.LabelFormatter,
            CrosshairPaint = new SolidColorPaint(crosshairColor),
            CrosshairLabelsPaint = new SolidColorPaint(crosshairLabelsColor),
            CrosshairLabelsBackground = crosshairBackground
        };

        var yAxis = new Axis
        {
            Labeler = viewModel.LabelFormatter,
            CrosshairPaint = new SolidColorPaint(crosshairColor),
            CrosshairLabelsPaint = new SolidColorPaint(crosshairLabelsColor),
            CrosshairLabelsBackground = crosshairBackground,
            CrosshairSnapEnabled = true
        };

        cartesianChart = new CartesianChart
        {
            Series = series,
            XAxes = [xAxis],
            YAxes = [yAxis],
            Location = new System.Drawing.Point(0, 50),
            Size = new System.Drawing.Size(100, 50),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(cartesianChart);
    }
}
