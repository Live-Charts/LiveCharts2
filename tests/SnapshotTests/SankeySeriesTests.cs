using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using SkiaSharp;

namespace SnapshotTests;

[TestClass]
public sealed class SankeySeriesTests
{
    [TestMethod]
    public void TwoLevelFlow()
    {
        // Three sources flow into two sinks with mixed weights. Pins the
        // d3-sankey core: depth assignment, column placement, sweep relaxation,
        // and per-link Y stacking at both ends.
        var sources = new[] { new SankeyNode("A"), new SankeyNode("B"), new SankeyNode("C") };
        var sinks = new[] { new SankeyNode("X"), new SankeyNode("Y") };

        var nodes = sources.Concat(sinks).ToArray();
        var links = new[]
        {
            new SankeyLink<SankeyNode>(sources[0], sinks[0], 8),
            new SankeyLink<SankeyNode>(sources[0], sinks[1], 4),
            new SankeyLink<SankeyNode>(sources[1], sinks[0], 6),
            new SankeyLink<SankeyNode>(sources[1], sinks[1], 2),
            new SankeyLink<SankeyNode>(sources[2], sinks[1], 10),
        };

        var chart = new SKSankeyChart
        {
            Series = [
                new SankeySeries<SankeyNode>
                {
                    Values = nodes,
                    Links = links,
                    NodeWidth = 16,
                    Fill = new SolidColorPaint(new SKColor(96, 138, 218)),
                    LinkFill = new SolidColorPaint(new SKColor(96, 138, 218, 90)),
                }
            ],
            Width = 600,
            Height = 400,
        };

        chart.AssertSnapshotMatches($"{nameof(SankeySeriesTests)}_{nameof(TwoLevelFlow)}");
    }

    [TestMethod]
    public void WithLabels()
    {
        // DataLabelsPaint opt-in + auto-wired SankeyNode.Name. The label-side
        // heuristic places labels right-of-node for the left column (A/B/C)
        // and left-of-node for the right column (X/Y), keeping text out of
        // the ribbon area on both sides.
        var sources = new[] { new SankeyNode("Alice"), new SankeyNode("Bob"), new SankeyNode("Carol") };
        var sinks = new[] { new SankeyNode("Trips"), new SankeyNode("Other") };

        var nodes = sources.Concat(sinks).ToArray();
        var links = new[]
        {
            new SankeyLink<SankeyNode>(sources[0], sinks[0], 8),
            new SankeyLink<SankeyNode>(sources[0], sinks[1], 4),
            new SankeyLink<SankeyNode>(sources[1], sinks[0], 6),
            new SankeyLink<SankeyNode>(sources[1], sinks[1], 2),
            new SankeyLink<SankeyNode>(sources[2], sinks[1], 10),
        };

        var chart = new SKSankeyChart
        {
            Series = [
                new SankeySeries<SankeyNode>
                {
                    Values = nodes,
                    Links = links,
                    NodeWidth = 16,
                    Fill = new SolidColorPaint(new SKColor(96, 138, 218)),
                    LinkFill = new SolidColorPaint(new SKColor(96, 138, 218, 90)),
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                    DataLabelsSize = 16,
                }
            ],
            Width = 700,
            Height = 400,
        };

        chart.AssertSnapshotMatches($"{nameof(SankeySeriesTests)}_{nameof(WithLabels)}");
    }

    [TestMethod]
    public void JobApplicationFunnel()
    {
        // Best-effort approximation of a real-world sankey: job-application
        // funnel with 4 columns. The interesting stresses are (a) a node
        // that is BOTH a sink and a source (1st Interviews, 2nd Interviews,
        // Offers), (b) labels showing both the flow value and the node name,
        // and (c) per-node coloring via NodeColorMapper so each outcome
        // reads as its own swimlane.
        var applications = new SankeyNode("Applications");
        var firstInterviews = new SankeyNode("1st Interviews");
        var secondInterviews = new SankeyNode("2nd Interviews");
        var offers = new SankeyNode("Offers");
        var accepted = new SankeyNode("Accepted");
        var declined = new SankeyNode("Declined");
        var dropped = new SankeyNode("Dropped by Myself");
        var noOffer = new SankeyNode("No Offer Received");
        var rejected = new SankeyNode("Rejected");
        var noReply = new SankeyNode("No Reply");

        var nodes = new[]
        {
            applications,
            firstInterviews, rejected, noReply,
            secondInterviews, dropped, noOffer,
            offers,
            accepted, declined,
        };

        var links = new[]
        {
            new SankeyLink<SankeyNode>(applications, firstInterviews, 4),
            new SankeyLink<SankeyNode>(applications, rejected, 3),
            new SankeyLink<SankeyNode>(applications, noReply, 2),
            new SankeyLink<SankeyNode>(firstInterviews, secondInterviews, 2),
            new SankeyLink<SankeyNode>(firstInterviews, dropped, 1),
            new SankeyLink<SankeyNode>(firstInterviews, noOffer, 1),
            new SankeyLink<SankeyNode>(secondInterviews, offers, 2),
            new SankeyLink<SankeyNode>(offers, accepted, 1),
            new SankeyLink<SankeyNode>(offers, declined, 1),
        };

        // Sum incoming + outgoing per node so the label can show the value
        // that appears in the source diagram ("9 Applications", "4 1st
        // Interviews", etc). max(in, out) matches what SankeyLayout uses
        // internally as the node "value".
        double NodeValue(SankeyNode n)
        {
            var inSum = links.Where(l => ReferenceEquals(l.Target, n)).Sum(l => l.Weight);
            var outSum = links.Where(l => ReferenceEquals(l.Source, n)).Sum(l => l.Weight);
            return inSum > outSum ? inSum : outSum;
        }

        var palette = new Dictionary<SankeyNode, LvcColor>(ReferenceEqualityComparer.Instance)
        {
            [applications] = new(96, 138, 218),
            [firstInterviews] = new(96, 138, 218),
            [secondInterviews] = new(96, 138, 218),
            [offers] = new(96, 138, 218),
            [accepted] = new(64, 175, 110),
            [declined] = new(220, 130, 50),
            [dropped] = new(220, 130, 50),
            [noOffer] = new(220, 130, 50),
            [rejected] = new(210, 80, 80),
            [noReply] = new(180, 180, 180),
        };

        var chart = new SKSankeyChart
        {
            Series = [
                new SankeySeries<SankeyNode>
                {
                    Values = nodes,
                    Links = links,
                    NodeWidth = 18,
                    NodePadding = 14,
                    NodeCornerRadius = 3,
                    Fill = new SolidColorPaint(new SKColor(96, 138, 218)),
                    LinkFill = new SolidColorPaint(SKColors.White),
                    NodeColorMapper = n => palette[n],
                    LabelMapper = n => $"{NodeValue(n):0} {n.Name}",
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                    DataLabelsSize = 13,
                }
            ],
            Width = 900,
            Height = 600,
        };

        chart.AssertSnapshotMatches($"{nameof(SankeySeriesTests)}_{nameof(JobApplicationFunnel)}");
    }

    [TestMethod]
    public void BipartiteArc()
    {
        // BipartiteArc layout: 3 sources → 5 targets with mixed weights.
        // Pins (a) the d3-sankey topology + value sums, (b) the angular sweep
        // distribution proportional to node value, (c) per-column visual-Y
        // band sorting that prevents source-side crossings, and (d) the
        // chord-ribbon math (cubic with control points at center).
        var furniture = new SankeyNode("Furniture");
        var technology = new SankeyNode("Technology");
        var office = new SankeyNode("Office Supplies");

        var chairs = new SankeyNode("Chairs");
        var phones = new SankeyNode("Phones");
        var storage = new SankeyNode("Storage");
        var tables = new SankeyNode("Tables");
        var paper = new SankeyNode("Paper");

        var nodes = new[] { furniture, technology, office, chairs, phones, storage, tables, paper };
        var links = new[]
        {
            new SankeyLink<SankeyNode>(furniture, chairs, 12),
            new SankeyLink<SankeyNode>(furniture, tables, 6),
            new SankeyLink<SankeyNode>(technology, phones, 14),
            new SankeyLink<SankeyNode>(technology, storage, 8),
            new SankeyLink<SankeyNode>(office, storage, 4),
            new SankeyLink<SankeyNode>(office, paper, 9),
            new SankeyLink<SankeyNode>(office, chairs, 2),
        };

        var palette = new Dictionary<SankeyNode, LvcColor>(ReferenceEqualityComparer.Instance)
        {
            [furniture] = new(96, 138, 218),
            [technology] = new(52, 78, 144),
            [office] = new(150, 188, 232),
            [chairs] = new(96, 138, 218),
            [phones] = new(52, 78, 144),
            [storage] = new(120, 158, 220),
            [tables] = new(170, 60, 80),
            [paper] = new(232, 168, 130),
        };

        var chart = new SKSankeyChart
        {
            Series = [
                new SankeySeries<SankeyNode>
                {
                    Values = nodes,
                    Links = links,
                    Layout = SankeyLayoutKind.BipartiteArc,
                    ArcSpanDegrees = 150,
                    NodeWidth = 20,
                    NodePadding = 8,
                    Fill = new SolidColorPaint(new SKColor(96, 138, 218)),
                    LinkFill = new SolidColorPaint(new SKColor(96, 138, 218, 90)),
                    NodeColorMapper = n => palette[n],
                }
            ],
            Width = 700,
            Height = 600,
        };

        chart.AssertSnapshotMatches($"{nameof(SankeySeriesTests)}_{nameof(BipartiteArc)}");
    }

    [TestMethod]
    public void BipartiteArcWithLabels()
    {
        // Same shape as BipartiteArc + radial labels enabled. Pins the
        // label-margin reserve (outer radius shrinks to fit max label width)
        // and the rotation-flip on left-half labels.
        var a = new SankeyNode("Alpha");
        var b = new SankeyNode("Beta");
        var c = new SankeyNode("Gamma");
        var x = new SankeyNode("Xenon");
        var y = new SankeyNode("Yttrium");
        var z = new SankeyNode("Zinc");

        var nodes = new[] { a, b, c, x, y, z };
        var links = new[]
        {
            new SankeyLink<SankeyNode>(a, x, 8),
            new SankeyLink<SankeyNode>(a, y, 4),
            new SankeyLink<SankeyNode>(b, x, 6),
            new SankeyLink<SankeyNode>(b, z, 5),
            new SankeyLink<SankeyNode>(c, y, 10),
            new SankeyLink<SankeyNode>(c, z, 3),
        };

        var chart = new SKSankeyChart
        {
            Series = [
                new SankeySeries<SankeyNode>
                {
                    Values = nodes,
                    Links = links,
                    Layout = SankeyLayoutKind.BipartiteArc,
                    NodeWidth = 14,
                    Fill = new SolidColorPaint(new SKColor(96, 138, 218)),
                    LinkFill = new SolidColorPaint(new SKColor(96, 138, 218, 90)),
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                    DataLabelsSize = 14,
                }
            ],
            Width = 700,
            Height = 600,
        };

        chart.AssertSnapshotMatches($"{nameof(SankeySeriesTests)}_{nameof(BipartiteArcWithLabels)}");
    }

    [TestMethod]
    public void HoverTooltip_Vertical()
    {
        // Pointer over the first source node ("A") on the left column should
        // render the default tooltip text ("A: 12" — A is the source of 8+4
        // weight). Pins the rect hover area + default formatter.
        var sources = new[] { new SankeyNode("A"), new SankeyNode("B"), new SankeyNode("C") };
        var sinks = new[] { new SankeyNode("X"), new SankeyNode("Y") };
        var nodes = sources.Concat(sinks).ToArray();
        var links = new[]
        {
            new SankeyLink<SankeyNode>(sources[0], sinks[0], 8),
            new SankeyLink<SankeyNode>(sources[0], sinks[1], 4),
            new SankeyLink<SankeyNode>(sources[1], sinks[0], 6),
            new SankeyLink<SankeyNode>(sources[2], sinks[1], 10),
        };

        var chart = new SKSankeyChart
        {
            Series = [
                new SankeySeries<SankeyNode>
                {
                    Values = nodes,
                    Links = links,
                    NodeWidth = 16,
                    Fill = new SolidColorPaint(new SKColor(96, 138, 218)),
                    LinkFill = new SolidColorPaint(new SKColor(96, 138, 218, 90)),
                }
            ],
            Width = 600,
            Height = 400,
        };

        // Left column at x ≈ 0..16; aim at the top "A" node.
        chart.PointerAt(8, 80);

        chart.AssertSnapshotMatches($"{nameof(SankeySeriesTests)}_{nameof(HoverTooltip_Vertical)}");
    }

    [TestMethod]
    public void HoverTooltip_BipartiteArc()
    {
        // Pointer over one of the right-arc nodes; exercises
        // AnnularSectorHoverArea polar hit-test + default tooltip formatter.
        var furniture = new SankeyNode("Furniture");
        var technology = new SankeyNode("Technology");
        var chairs = new SankeyNode("Chairs");
        var phones = new SankeyNode("Phones");
        var storage = new SankeyNode("Storage");

        var nodes = new[] { furniture, technology, chairs, phones, storage };
        var links = new[]
        {
            new SankeyLink<SankeyNode>(furniture, chairs, 12),
            new SankeyLink<SankeyNode>(technology, phones, 14),
            new SankeyLink<SankeyNode>(technology, storage, 8),
        };

        var chart = new SKSankeyChart
        {
            Series = [
                new SankeySeries<SankeyNode>
                {
                    Values = nodes,
                    Links = links,
                    Layout = SankeyLayoutKind.BipartiteArc,
                    NodeWidth = 20,
                    Fill = new SolidColorPaint(new SKColor(96, 138, 218)),
                    LinkFill = new SolidColorPaint(new SKColor(96, 138, 218, 90)),
                }
            ],
            Width = 600,
            Height = 600,
        };

        // 600x600 chart; arc center ~ (300, 300); outerR ~ rectMin/2 = 300.
        // Node ring sits between innerR=280 (=300-NodeWidth) and outerR=300
        // at the perimeter. Pointer at (590, 300) is angle 0 (east-most
        // point of the right arc) → lands inside whichever target node
        // happens to sit there after relaxation.
        chart.PointerAt(590, 300);

        chart.AssertSnapshotMatches($"{nameof(SankeySeriesTests)}_{nameof(HoverTooltip_BipartiteArc)}");
    }

    [TestMethod]
    public void DefaultTheme_AppliesPaletteColor()
    {
        // No Fill / LinkFill set on the series; the default theme rule
        // should pull a palette color and seed both. Pins the
        // SeriesProperties.Sankey ↔ SankeySeriesBuilder dispatch.
        var a = new SankeyNode("A");
        var b = new SankeyNode("B");
        var x = new SankeyNode("X");
        var y = new SankeyNode("Y");
        var nodes = new[] { a, b, x, y };
        var links = new[]
        {
            new SankeyLink<SankeyNode>(a, x, 5),
            new SankeyLink<SankeyNode>(a, y, 3),
            new SankeyLink<SankeyNode>(b, y, 7),
        };

        var chart = new SKSankeyChart
        {
            Series = [
                new SankeySeries<SankeyNode>
                {
                    Values = nodes,
                    Links = links,
                    NodeWidth = 16,
                    // Intentionally no Fill / LinkFill — let the theme decide.
                }
            ],
            Width = 600,
            Height = 400,
        };

        chart.AssertSnapshotMatches($"{nameof(SankeySeriesTests)}_{nameof(DefaultTheme_AppliesPaletteColor)}");
    }

    [TestMethod]
    public void BipartiteArc_ThrowsOnMultiColumn()
    {
        // BipartiteArc rejects graphs where any node is BOTH a target and a
        // source — i.e. pass-through nodes that push maxDepth > 1. The same
        // ThreeColumnsWithBranching data should throw a clear message
        // pointing users at SankeyLayoutKind.Vertical.
        var aa = new SankeyNode("A");
        var bb = new SankeyNode("B");
        var mm = new SankeyNode("M");
        var pp = new SankeyNode("P");

        var nodes = new[] { aa, bb, mm, pp };
        var links = new[]
        {
            new SankeyLink<SankeyNode>(aa, mm, 5),
            new SankeyLink<SankeyNode>(bb, mm, 3),
            new SankeyLink<SankeyNode>(mm, pp, 8), // mm is a pass-through -> 3 columns
        };

        var chart = new SKSankeyChart
        {
            Series = [
                new SankeySeries<SankeyNode>
                {
                    Values = nodes,
                    Links = links,
                    Layout = SankeyLayoutKind.BipartiteArc,
                    Fill = new SolidColorPaint(new SKColor(96, 138, 218)),
                }
            ],
            Width = 400,
            Height = 400,
        };

        var ex = Assert.ThrowsExactly<InvalidOperationException>(() => chart.GetImage());
        StringAssert.Contains(ex.Message, "BipartiteArc");
        StringAssert.Contains(ex.Message, "Vertical");
    }

    [TestMethod]
    public void ThreeColumnsWithBranching()
    {
        // Three columns (A,B,C -> M,N -> P,Q) — exercises depth > 2 and the
        // relaxation loop on the middle column.
        var a = new SankeyNode("A");
        var b = new SankeyNode("B");
        var c = new SankeyNode("C");
        var m = new SankeyNode("M");
        var n = new SankeyNode("N");
        var p = new SankeyNode("P");
        var q = new SankeyNode("Q");

        var nodes = new[] { a, b, c, m, n, p, q };
        var links = new[]
        {
            new SankeyLink<SankeyNode>(a, m, 10),
            new SankeyLink<SankeyNode>(b, m, 5),
            new SankeyLink<SankeyNode>(b, n, 3),
            new SankeyLink<SankeyNode>(c, n, 12),
            new SankeyLink<SankeyNode>(m, p, 8),
            new SankeyLink<SankeyNode>(m, q, 7),
            new SankeyLink<SankeyNode>(n, p, 5),
            new SankeyLink<SankeyNode>(n, q, 10),
        };

        var chart = new SKSankeyChart
        {
            Series = [
                new SankeySeries<SankeyNode>
                {
                    Values = nodes,
                    Links = links,
                    NodeWidth = 14,
                    NodePadding = 12,
                    Fill = new SolidColorPaint(new SKColor(232, 105, 105)),
                    LinkFill = new SolidColorPaint(new SKColor(232, 105, 105, 80)),
                }
            ],
            Width = 700,
            Height = 400,
        };

        chart.AssertSnapshotMatches($"{nameof(SankeySeriesTests)}_{nameof(ThreeColumnsWithBranching)}");
    }
}
