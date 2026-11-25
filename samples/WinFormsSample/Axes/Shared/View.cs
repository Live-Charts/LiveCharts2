using System;
using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;
using LiveChartsCore.Measure;
using LiveChartsCore.VisualElements;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

#pragma warning disable IDE1006 // Naming Styles

namespace WinFormsSample.Axes.Shared;

public partial class View : UserControl
{
    private readonly Random _random = new();

    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(50, 50);

        // Prepare data
        var values1 = Fetch(100);
        var values2 = Fetch(50);
        static string labeler(double value) => value.ToString("N2");

        var max = Math.Max(values1.Length, values2.Length);

        // Shared axes
        var sharedXAxis = new Axis
        {
            Labeler = labeler,
            MaxLimit = max,
            CrosshairPaint = new SolidColorPaint(new SKColor(0xFF, 0x45, 0x00, 0x40)), // #40FF4500
            CrosshairLabelsBackground = new SKColor(0xFF, 0x45, 0x00).AsLvcColor(), // #FF4500
            CrosshairLabelsPaint = new SolidColorPaint(SKColors.White)
        };
        var sharedXAxis2 = new Axis
        {
            MaxLimit = max,
            CrosshairPaint = new SolidColorPaint(new SKColor(0xFF, 0x45, 0x00, 0x40)) // #40FF4500
        };
        // Optionally, set MinLimit/MaxLimit to align axes if needed

        // force the left margin in both charts to align the axes
        // no matter the size of the labels on each axis
        var auto = LiveChartsCore.Measure.Margin.Auto;
        var drawMargin = new Margin(70, auto, auto, auto);

        // Set axes as shared
        SharedAxes.Set(sharedXAxis, sharedXAxis2);

        var cartesianChart = new CartesianChart
        {
            Series = [
                new LineSeries<int> { Values = values1 }
            ],
            ZoomMode = ZoomAndPanMode.X,
            XAxes = [sharedXAxis],
            DrawMargin = drawMargin,
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(50, 50),
            Dock = DockStyle.Fill
        };

        var cartesianChart2 = new CartesianChart
        {
            Series =
            [
                new ColumnSeries<int> { Values = values2 }
            ],
            ZoomMode = ZoomAndPanMode.X,
            XAxes = [sharedXAxis2],
            DrawMargin = drawMargin,
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

    private int[] Fetch(int length = 50)
    {
        var values = new int[length];
        var t = 0;
        for (var i = 0; i < length; i++)
        {
            t += _random.Next(-90, 100);
            values[i] = t;
        }
        return values;
    }
}
