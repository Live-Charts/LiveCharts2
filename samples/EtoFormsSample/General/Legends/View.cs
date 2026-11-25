using System.Linq;
using Eto.Forms;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;
using ViewModelsSamples.General.Legends;

namespace EtoFormsSample.General.Legends;

public class View : Panel
{
    private readonly CartesianChart cartesianChart;

    public View()
    {
        var viewModel = new ViewModel();

        var series = viewModel.Data
            .Select(data => new ColumnSeries<double> { Name = data.Name, Values = data.Values })
            .ToArray();

        cartesianChart = new CartesianChart
        {
            Series = series,
            LegendPosition = viewModel.SelectedPosition,
        };

        var b1 = new DropDown { DataStore = viewModel.Positions.Cast<object>() };
        b1.SelectedValue = viewModel.SelectedPosition;
        b1.SelectedValueChanged += (object sender, System.EventArgs e) =>
            cartesianChart.LegendPosition = (LegendPosition)b1.SelectedValue;

        Content = new DynamicLayout(
            new DynamicRow(new DynamicControl { Control = b1, XScale = true }),
            cartesianChart);
    }
}
