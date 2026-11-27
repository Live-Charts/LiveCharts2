using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;

namespace EtoFormsSample.Pies.NightingaleRose;

public class View : Panel
{
    private readonly PieChart pieChart;

    public View()
    {
        var seriesCollection = new ISeries[]
        {
            new PieSeries<double> { Name = "Mary", Values = new double[] { 10 }, OuterRadiusOffset = 0, InnerRadius = 50 },
            new PieSeries<double> { Name = "John", Values = new double[] { 20 }, OuterRadiusOffset = 50, InnerRadius = 50 },
            new PieSeries<double> { Name = "Alice", Values = new double[] { 30 }, OuterRadiusOffset = 100, InnerRadius = 50 },
            new PieSeries<double> { Name = "Bob", Values = new double[] { 40 }, OuterRadiusOffset = 150, InnerRadius = 50 }
        };

        pieChart = new PieChart
        {
            Series = seriesCollection
        };

        Content = pieChart;
    }
}
