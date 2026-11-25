using System;
using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WinForms;

#pragma warning disable IDE1006 // Naming Styles

namespace WinFormsSample.Axes.LabelsRotation;

public partial class View : UserControl
{
    private readonly CartesianChart cartesianChart;
    private readonly Axis yAxis;

    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(100, 100);

        var values = new double[] { 200, 558, 458, 249, 457, 339, 587 };
        static string tooltipFormat(ChartPoint point) =>
            $"This is {Environment.NewLine}" +
            $"A multi-line label {Environment.NewLine}" +
            $"With a value of {Environment.NewLine}" + point.Coordinate.PrimaryValue;
        static string labeler(double value) =>
            $"This is {Environment.NewLine}" +
            $"A multi-line label {Environment.NewLine}" +
            $"With a value of {Environment.NewLine}" + value * 100;

        var series = new ISeries[]
        {
            new LineSeries<double>
            {
                Values = values,
                XToolTipLabelFormatter = tooltipFormat
            }
        };

        var xAxis = new Axis
        {
            Labeler = labeler,
            MinStep = 1,
            LabelsRotation = 45,
            SeparatorsPaint = new SolidColorPaint(new SkiaSharp.SKColor(238, 238, 238), 2)
        };

        yAxis = new Axis
        {
            LabelsRotation = 15, // initial value
            SeparatorsPaint = new SolidColorPaint(new SkiaSharp.SKColor(238, 238, 238), 2)
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

        var trackBar = new TrackBar { Location = new System.Drawing.Point(0, 0), Width = 300, Minimum = -360, Maximum = 720, Value = 15 };
        trackBar.ValueChanged += (sender, e) =>
        {
            yAxis.LabelsRotation = trackBar.Value;
        };
        Controls.Add(trackBar);
    }
}
