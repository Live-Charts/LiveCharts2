using System.Linq;
using Eto.Forms;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.SkiaSharpView.Eto;
using SkiaSharp;
using ViewModelsSamples.Pies.Basic;

namespace EtoFormsSample.Pies.Basic;

public class View : Panel
{
    public readonly PieChart Chart;

    public View()
    {
        var viewModel = new ViewModel();

        Chart = new PieChart
        {
            Series = viewModel.Data
                .Select(data => new PieSeries<double>
                {
                    Name = data.Name,
                    Values = data.Values
                })
                .ToArray(),
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
