using System;
using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;
using ViewModelsSamples.Bars.DelayedAnimation;

namespace WinFormsSample.Bars.DelayedAnimation;

public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(50, 50);

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
}
