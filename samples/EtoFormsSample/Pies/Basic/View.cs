using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.SkiaSharpView.Eto;
using SkiaSharp;

namespace EtoFormsSample.Pies.Basic;

public class View : Panel
{
    public readonly PieChart Chart;

    public View()
    {
        Chart = new PieChart
        {
            Series = new ISeries[]
            {
                new PieSeries<double> { Name = "Mary", Values = new double[] { 10 } },
                new PieSeries<double> { Name = "John", Values = new double[] { 20 } },
                new PieSeries<double> { Name = "Alice", Values = new double[] { 30 } },
                new PieSeries<double> { Name = "Bob", Values = new double[] { 40 } },
                new PieSeries<double> { Name = "Charlie", Values = new double[] { 50 } }
            },
            Title = new DrawnLabelVisual(
                new LabelGeometry
                {
                    Text = "My chart title",
                    TextSize = 25,
                    Padding = new LiveChartsCore.Drawing.Padding(15)
                })
        };

        Content = Chart;
    }
}
