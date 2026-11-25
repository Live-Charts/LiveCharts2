using System.Linq;
using Eto.Forms;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;
using ViewModelsSamples.Pies.OutLabels;

namespace EtoFormsSample.Pies.OutLabels;

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
                ShowDataLabels = true,
                DataLabelsPosition = PolarLabelsPosition.Outer,
                DataLabelsSize = 15,
                DataLabelsFormatter = data.LabelFormatter,
                ToolTipLabelFormatter = data.TooltipFormatter
            })
            .ToArray();

        pieChart = new PieChart
        {
            Series = seriesCollection,
            IsClockwise = false,
            InitialRotation = -90
        };

        Content = pieChart;
    }
}
