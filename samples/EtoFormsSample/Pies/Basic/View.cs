using Eto.Forms;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Extensions;
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
            Series = new[] { 2, 4, 1, 4, 3 }.AsPieSeries(),
            Title = new DrawnLabelVisual(
                new LabelGeometry
                {
                    Text = "My chart title",
                    Paint = new SolidColorPaint(SKColors.Black),
                    TextSize = 25,
                    Padding = new LiveChartsCore.Drawing.Padding(15)
                })
        };

        Content = Chart;
    }
}
