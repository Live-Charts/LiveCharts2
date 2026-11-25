using System.Linq;
using Eto.Forms;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;
using ViewModelsSamples.Pies.Custom;

namespace EtoFormsSample.Pies.Custom;

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
                ShowDataLabels = true,
                ToolTipLabelFormatter = data.Formatter,
                DataLabelsFormatter = data.Formatter
            })
            .ToArray();

        pieChart = new PieChart
        {
            Series = seriesCollection,
            InitialRotation = -90
        };

        Content = pieChart;
    }
}
