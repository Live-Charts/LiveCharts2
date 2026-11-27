using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;

namespace EtoFormsSample.General.Legends;

public class View : Panel
{
    private readonly CartesianChart cartesianChart;

    public View()
    {
        var series = new ISeries[]
        {
            new ColumnSeries<double> { Name = "Peru", Values = [3, 7, 3] },
            new ColumnSeries<double> { Name = "Colombia", Values = [4, 6, 2] },
            new ColumnSeries<double> { Name = "Mexico", Values = [5, 5, 5] },
            new ColumnSeries<double> { Name = "Argentina", Values = [6, 4, 6] },
            new ColumnSeries<double> { Name = "Chile", Values = [7, 3, 7] },
            new ColumnSeries<double> { Name = "Ecuador", Values = [8, 2, 8] },
            new ColumnSeries<double> { Name = "Bolivia", Values = [9, 1, 9] },
            new ColumnSeries<double> { Name = "Venezuela", Values = [10, 0, 10] },
            new ColumnSeries<double> { Name = "Uruguay", Values = [11, 1, 11] },
            new ColumnSeries<double> { Name = "Paraguay", Values = [12, 2, 12] },
            new ColumnSeries<double> { Name = "Brazil", Values = [13, 3, 13] },
            new ColumnSeries<double> { Name = "Costa Rica", Values = [14, 4, 14] },
            new ColumnSeries<double> { Name = "Panama", Values = [15, 5, 15] },
            new ColumnSeries<double> { Name = "Honduras", Values = [16, 6, 16] }
        };

        cartesianChart = new CartesianChart
        {
            Series = series,
            LegendPosition = LegendPosition.Left,
        };

        var b1 = new DropDown();
        b1.Items.AddRange(new ListItem[] { "hidden", "top", "left", "right", "bottom" });
        b1.SelectedValueChanged += (object sender, System.EventArgs e) =>
        {
            if (b1.SelectedKey == "hidden") cartesianChart.LegendPosition = LegendPosition.Hidden;
            if (b1.SelectedKey == "top") cartesianChart.LegendPosition = LegendPosition.Top;
            if (b1.SelectedKey == "bottom") cartesianChart.LegendPosition = LegendPosition.Bottom;
            if (b1.SelectedKey == "left") cartesianChart.LegendPosition = LegendPosition.Left;
            if (b1.SelectedKey == "right") cartesianChart.LegendPosition = LegendPosition.Right;
        };
        b1.SelectedKey = "left";

        Content = new DynamicLayout(
            new DynamicRow(new DynamicControl { Control = b1, XScale = true }),
            cartesianChart);
    }
}
