using System.Linq;
using Eto.Forms;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using ViewModelsSamples.Pies.Nested;

namespace EtoFormsSample.Pies.Nested;

public class View : Panel
{
    public View()
    {
        var viewModel = new ViewModel();

        var pieChart = new PieChart
        {
            Series = viewModel.PieDataCollection.Select(x => new PieSeries<double?>
            {
                Name = x.Name,
                Values = x.Values,
                Stroke = new SolidColorPaint(SKColor.Parse("#fff"), 2),
                Fill = new SolidColorPaint(SKColor.Parse(x.Color)),
                HoverPushout = 0,
                DataLabelsFormatter = x.Formatter,
                DataLabelsSize = 20,
                ShowDataLabels = x.IsTotal,
                DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Outer,
                IsVisibleAtLegend = x.IsTotal
            }).ToArray(),
            InitialRotation = 120,
            LegendPosition = LiveChartsCore.Measure.LegendPosition.Right
        };
        Content = pieChart;
    }
}
