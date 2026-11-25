using System.Linq;
using Eto.Forms;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;
using ViewModelsSamples.Pies.Pushout;

namespace EtoFormsSample.Pies.Pushout;

public class View : Panel
{
    private readonly PieChart pieChart;

    public View()
    {
        var viewModel = new ViewModel();

        pieChart = new PieChart
        {
            Series = viewModel.Data
                .Select(data => new PieSeries<double>
                {
                    Name = data.Name,
                    Values = data.Values,
                    Pushout = data.Pushout
                })
                .ToArray()
        };

        Content = pieChart;
    }
}
