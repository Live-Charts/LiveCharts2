using System;
using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;
using ViewModelsSamples.Bars.DelayedAnimation;

namespace EtoFormsSample.Bars.DelayedAnimation;

public class View : Panel
{
    private readonly CartesianChart cartesianChart;

    public View()
    {
        var viewModel = new ViewModel();

        var series1 = new ColumnSeries<float>
        {
            Values = viewModel.Values1
        };
        series1.PointMeasured += OnPointMeasured;

        var series2 = new ColumnSeries<float>
        {
            Values = viewModel.Values2,
            Padding = 0
        };
        series2.PointMeasured += OnPointMeasured;

        cartesianChart = new CartesianChart
        {
            Series = new ISeries[] { series1, series2 },
        };

        Content = cartesianChart;
    }

    private void OnPointMeasured(ChartPoint point)
    {
        var baseAnimationsSpeed = 800f; // in milliseconds
        var perPointDelay = 100f; // in milliseconds
        var delay = point.Context.Entity.MetaData!.EntityIndex * perPointDelay;
        var speed = baseAnimationsSpeed + delay;
        var baseEasingFunction = LiveChartsCore.EasingFunctions.BuildCustomElasticOut(1.5f, 0.60f);

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
}
