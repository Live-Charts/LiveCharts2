using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.SkiaSharpView.VisualElements;
using SkiaSharp;

namespace SnapshotTests;

[TestClass]
public sealed class TreemapSeriesTests
{
    [TestMethod]
    public void FlatBasic()
    {
        // Flat treemap (single level): six leaves with descending weights.
        // Exercises the core squarified row-building loop without recursion.
        var nodes = new[]
        {
            new TreemapNode(40, "A"),
            new TreemapNode(25, "B"),
            new TreemapNode(15, "C"),
            new TreemapNode(10, "D"),
            new TreemapNode(7,  "E"),
            new TreemapNode(3,  "F"),
        };

        var chart = new SKTreemapChart
        {
            Series = [
                new TreemapSeries<TreemapNode>
                {
                    Values = nodes,
                    Fill = new SolidColorPaint(new SKColor(96, 138, 218)),
                    Stroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 2 },
                }
            ],
            Width = 600,
            Height = 600,
        };

        chart.AssertSnapshotMatches($"{nameof(TreemapSeriesTests)}_{nameof(FlatBasic)}");
    }

    [TestMethod]
    public void WithDataLabels()
    {
        // Flat treemap with DataLabelsPaint set + LabelMapper auto-wired from
        // TreemapNode.Name. Verifies labels render centered in each leaf tile
        // and that the tiles small enough to not fit the text get culled.
        var nodes = new[]
        {
            new TreemapNode(40, "Asia"),
            new TreemapNode(25, "Europe"),
            new TreemapNode(15, "Americas"),
            new TreemapNode(10, "Africa"),
            new TreemapNode(7,  "Oceania"),
            new TreemapNode(3,  "Antarctica"),
        };

        var chart = new SKTreemapChart
        {
            Series = [
                new TreemapSeries<TreemapNode>
                {
                    Values = nodes,
                    Padding = 4,
                    Fill = new SolidColorPaint(new SKColor(96, 138, 218)),
                    Stroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 2 },
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsSize = 18,
                }
            ],
            Width = 600,
            Height = 600,
        };

        chart.AssertSnapshotMatches($"{nameof(TreemapSeriesTests)}_{nameof(WithDataLabels)}");
    }

    [TestMethod]
    public void HoverTooltip()
    {
        // Probe the synthesized ChartPoint + hover-area pipeline: hover near
        // the China tile (top-left of the squarified layout) and assert the
        // default formatter renders "China: 1400".
        var roots = new[]
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
        };

        var chart = new SKTreemapChart
        {
            Series = [
                new TreemapSeries<TreemapNode>
                {
                    Values = roots,
                    Padding = 4,
                    Fill = new SolidColorPaint(new SKColor(96, 138, 218)),
                    Stroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 2 },
                }
            ],
            Width = 600,
            Height = 600,
        };

        // China is the largest leaf in Asia (which is the larger root) — top-left
        // quadrant in the squarified output.
        chart.PointerAt(150, 150);
        chart.AssertSnapshotMatches($"{nameof(TreemapSeriesTests)}_{nameof(HoverTooltip)}");
    }

    [TestMethod]
    public void TwoSeriesWithTitleAndLegend()
    {
        // Two series with named entries (legend), a title, and the engine's
        // top-level partition splitting the draw margin by series totals. The
        // smaller series (2024, total 92) gets a smaller sub-rectangle than
        // the larger one (2023, total 153). Each series squarifies its own
        // nodes inside its slot.
        var y2023 = new[]
        {
            new TreemapNode(60, "Americas"),
            new TreemapNode(50, "EMEA"),
            new TreemapNode(28, "APAC"),
            new TreemapNode(15, "Other"),
        };
        var y2024 = new[]
        {
            new TreemapNode(40, "Americas"),
            new TreemapNode(28, "EMEA"),
            new TreemapNode(16, "APAC"),
            new TreemapNode(8,  "Other"),
        };

        var chart = new SKTreemapChart
        {
            Title = new DrawnLabelVisual(new LabelGeometry
            {
                Text = "Sales by Region",
                TextSize = 22,
                Padding = new Padding(12),
                Paint = new SolidColorPaint(SKColors.Black),
            }),
            LegendPosition = LegendPosition.Right,
            Series = [
                new TreemapSeries<TreemapNode>
                {
                    Name = "2023",
                    Values = y2023,
                    Padding = 4,
                    Fill = new SolidColorPaint(new SKColor(96, 138, 218)),
                    Stroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 2 },
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsSize = 16,
                },
                new TreemapSeries<TreemapNode>
                {
                    Name = "2024",
                    Values = y2024,
                    Padding = 4,
                    Fill = new SolidColorPaint(new SKColor(232, 105, 105)),
                    Stroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 2 },
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsSize = 16,
                },
            ],
            Width = 900,
            Height = 600,
        };

        chart.AssertSnapshotMatches($"{nameof(TreemapSeriesTests)}_{nameof(TwoSeriesWithTitleAndLegend)}");
    }

    [TestMethod]
    public void HierarchicalTwoLevel()
    {
        // Two-level hierarchy. Internal nodes (Asia, Europe, Americas) have no
        // own Value — the series rolls up children. Verifies the recursive
        // descent and Padding-inset behavior between depth levels.
        var roots = new[]
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

        var chart = new SKTreemapChart
        {
            Series = [
                new TreemapSeries<TreemapNode>
                {
                    Values = roots,
                    Padding = 4,
                    CornerRadius = 4,
                    Fill = new SolidColorPaint(new SKColor(96, 138, 218, 160)),
                    Stroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 2 },
                }
            ],
            Width = 600,
            Height = 600,
        };

        chart.AssertSnapshotMatches($"{nameof(TreemapSeriesTests)}_{nameof(HierarchicalTwoLevel)}");
    }
}
