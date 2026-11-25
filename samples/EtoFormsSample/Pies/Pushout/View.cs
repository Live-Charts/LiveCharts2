using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;

namespace EtoFormsSample.Pies.Pushout;

public class View : Panel
{
    private readonly PieChart pieChart;

    public View()
    {
        pieChart = new PieChart
        {
            Series = new ISeries[]
            {
                // pushes out Maria's slice 30 pixels.
                new PieSeries<double> { Name = "Maria", Values = new double[] { 8 }, Pushout = 30 },
                new PieSeries<double> { Name = "Susan", Values = new double[] { 6 } },
                new PieSeries<double> { Name = "Charles", Values = new double[] { 5 } },
                new PieSeries<double> { Name = "Fiona", Values = new double[] { 3 } },
                new PieSeries<double> { Name = "George", Values = new double[] { 3 } }
            }
        };

        Content = pieChart;
    }
}
