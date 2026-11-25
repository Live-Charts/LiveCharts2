using System.Linq;
using Eto.Forms;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;
using ViewModelsSamples.Pies.NightingaleRose;

namespace EtoFormsSample.Pies.NightingaleRose;

public class View : Panel
{
    private readonly PieChart pieChart;

    public View()
    {
        var viewModel = new ViewModel();

        var seriesCollection = viewModel.Data
            .Select(data => new PieSeries<double>
            {
                Name = data.Name,
                Values = data.Values,
                OuterRadiusOffset = data.Offset,
                InnerRadius = 50
            })
            .ToArray();

        pieChart = new PieChart
        {
            Series = seriesCollection
        };

        Content = pieChart;
    }
}
