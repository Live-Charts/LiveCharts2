using System;
using System.Collections.Generic;
using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using LiveChartsCore.Kernel.Sketches;

namespace WinFormsSample.Axes.ColorsAndPosition;

#pragma warning disable IDE1006 // Naming Styles

public partial class View : UserControl
{
    private readonly CartesianChart cartesianChart;
    private readonly ISeries[] series;
    private readonly Axis xAxis;
    private readonly Axis yAxis;
    private readonly Random random = new();

    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(100, 100);

        // Sample data
        series = [
            new ColumnSeries<double> { Values = [2, 3, 8] }
        ];

        // both axes start at the end, painted red
        xAxis = new Axis
        {
            Name = "X axis",
            TextSize = 20,
            Position = AxisPosition.End,
            LabelsPaint = new SolidColorPaint(SKColors.Red),
            SeparatorsPaint = new SolidColorPaint(SKColors.Red)
        };
        yAxis = new Axis
        {
            Name = "Y axis",
            TextSize = 20,
            Position = AxisPosition.End,
            LabelsPaint = new SolidColorPaint(SKColors.Red),
            SeparatorsPaint = new SolidColorPaint(SKColors.Red)
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

        var b1 = new Button { Text = "toggle position", Location = new System.Drawing.Point(0, 0) };
        b1.Click += (sender, e) =>
        {
            foreach (var axis in new[] { xAxis, yAxis })
            {
                axis.Position = axis.Position == AxisPosition.End
                    ? AxisPosition.Start
                    : AxisPosition.End;
            }
        };
        Controls.Add(b1);

        var b2 = new Button { Text = "new color", Location = new System.Drawing.Point(80, 0) };
        b2.Click += (sender, e) =>
        {
            var r = (byte)random.Next(0, 255);
            var g = (byte)random.Next(0, 255);
            var b = (byte)random.Next(0, 255);
            foreach (var axis in new[] { xAxis, yAxis })
            {
                axis.LabelsPaint = new SolidColorPaint(new SKColor(r, g, b));
                axis.SeparatorsPaint = new SolidColorPaint(new SKColor(r, g, b));
            }
        };
        Controls.Add(b2);
    }
}
