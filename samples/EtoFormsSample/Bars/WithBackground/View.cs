using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using ViewModelsSamples.Bars.WithBackground;

namespace EtoFormsSample.Bars.WithBackground;

public class View : Panel
{
    private readonly CartesianChart cartesianChart;

    public View()
    {
        var viewModel = new ViewModel();

        var series = new ISeries[]
        {
            new ColumnSeries<double>
            {
                Values = viewModel.Values1,
                Fill = new SolidColorPaint(new SKColor(180, 180, 180, 50)),
                IgnoresBarPosition = true,
                IsHoverable = false
            },
            new ColumnSeries<double>
            {
                Values = viewModel.Values2,
                IgnoresBarPosition = true
            }
        };

        var yAxis = new Axis
        {
            MinLimit = 0,
            MaxLimit = 10
        };

        cartesianChart = new CartesianChart
        {
            Series = series,
            YAxes = new[] { yAxis },
        };

        Content = cartesianChart;
    }
}
