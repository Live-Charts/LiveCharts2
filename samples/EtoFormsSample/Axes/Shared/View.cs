using System;
using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace EtoFormsSample.Axes.Shared;

public class View : Panel
{
    private readonly Random _random = new();

    public View()
    {
        var values1 = Fetch(100);
        var values2 = Fetch(50);
        static string labeler(double value) => value.ToString("N2");
        var max = Math.Max(values1.Length, values2.Length);

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
        LiveChartsCore.SharedAxes.Set(sharedXAxis, sharedXAxis2);

        var cartesianChart = new CartesianChart
        {
            Series = new ISeries[]
            {
                new LineSeries<int> { Values = values1 }
            },
            ZoomMode = ZoomAndPanMode.X,
            XAxes = new[] { sharedXAxis },
            DrawMargin = new Margin(70, Margin.Auto, Margin.Auto, Margin.Auto)
        };

        var cartesianChart2 = new CartesianChart
        {
            Series = new ISeries[]
            {
                new ColumnSeries<int> { Values = values2 }
            },
            ZoomMode = ZoomAndPanMode.X,
            XAxes = new[] { sharedXAxis2 },
            DrawMargin = new Margin(70, Margin.Auto, Margin.Auto, Margin.Auto)
        };

        Content = new DynamicLayout(
            new DynamicRow(new DynamicControl { Control = cartesianChart, YScale = true }),
            new DynamicRow(new DynamicControl { Control = cartesianChart2, YScale = true })
        );
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
