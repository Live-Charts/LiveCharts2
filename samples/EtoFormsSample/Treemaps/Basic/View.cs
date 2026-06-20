using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace EtoFormsSample.Treemaps.Basic;

public class View : Panel
{
    public readonly TreemapChart Chart;

    public View()
    {
        var regions = new[]
        {
            new TreemapNode("Asia", new[]
            {
                new TreemapNode(1400, "China"),
                new TreemapNode(1300, "India"),
                new TreemapNode(125,  "Japan"),
                new TreemapNode(270,  "Indonesia"),
            }),
            new TreemapNode("Europe", new[]
            {
                new TreemapNode(83, "Germany"),
                new TreemapNode(67, "France"),
                new TreemapNode(60, "Italy"),
                new TreemapNode(46, "Spain"),
            }),
            new TreemapNode("Americas", new[]
            {
                new TreemapNode(330, "USA"),
                new TreemapNode(220, "Brazil"),
                new TreemapNode(130, "Mexico"),
            }),
        };

        var series = new ISeries[]
        {
            new TreemapSeries<TreemapNode>
            {
                Values = regions,
                Padding = 4,
                Fill = new SolidColorPaint(new SKColor(96, 138, 218)),
                Stroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 2 },
                DataLabelsPaint = new SolidColorPaint(SKColors.White),
                DataLabelsSize = 16,
            }
        };

        Chart = new TreemapChart
        {
            Series = series,
        };

        Content = Chart;
    }
}
