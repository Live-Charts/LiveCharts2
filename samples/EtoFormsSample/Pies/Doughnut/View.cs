using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;

namespace EtoFormsSample.Pies.Doughnut;

public class View : Panel
{
    private readonly PieChart pieChart;

    public View()
    {
        var seriesCollection = new ISeries[]
        {
            new PieSeries<double> { Name = "Mary", Values = new double[] { 10 }, MaxRadialColumnWidth = 60 },
            new PieSeries<double> { Name = "John", Values = new double[] { 20 }, MaxRadialColumnWidth = 60 },
            new PieSeries<double> { Name = "Alice", Values = new double[] { 30 }, MaxRadialColumnWidth = 60 },
            new PieSeries<double> { Name = "Bob", Values = new double[] { 40 }, MaxRadialColumnWidth = 60 },
            new PieSeries<double> { Name = "Charlie", Values = new double[] { 50 }, MaxRadialColumnWidth = 60 }
        };

        pieChart = new PieChart
        {
            Series = seriesCollection
        };

        Content = pieChart;
    }
}
