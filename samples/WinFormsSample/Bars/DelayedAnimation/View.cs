using System;
using System.Collections.Generic;
using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;

namespace WinFormsSample.Bars.DelayedAnimation;

public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(50, 50);

        var values1 = FetchValues(0);
        var values2 = FetchValues(-0.15f);

        var series1 = new ColumnSeries<float>
        {
            Values = values1
        };

        series1.PointMeasured += OnPointMeasured;

        var series2 = new ColumnSeries<float>
        {
            Values = values2,
            Padding = 0
        };

        series2.PointMeasured += OnPointMeasured;

        var cartesianChart = new CartesianChart
        {
            Series = [series1, series2],
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(50, 50),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(cartesianChart);
    }

    private void OnPointMeasured(ChartPoint point)
    {
        var baseAnimationsSpeed = 800f; // in milliseconds
        var perPointDelay = 100f; // in milliseconds
        var delay = point.Context.Entity.MetaData!.EntityIndex * perPointDelay;
        var speed = baseAnimationsSpeed + delay;
        var baseEasingFunction = EasingFunctions.BuildCustomElasticOut(1.5f, 0.60f);

        point.Context.Visual?.SetTransition(
            new Animation(progress =>
            {
                var d = delay / speed;
                return progress <= d
                    ? 0
                    : baseEasingFunction((progress - d) / (1 - d));
            },
            TimeSpan.FromMilliseconds(speed)));
    }

    private static float[] FetchValues(float offset)
    {
        var values = new List<float>();
        var fx = EasingFunctions.BounceInOut;
        var x = 0f;
        while (x <= 1)
        {
            values.Add(fx(x + offset));
            x += 0.025f;
        }
        return [.. values];
    }
}
