using System.Linq;
using System.Threading.Tasks;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Drawing;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.SeriesTests;

// GeometryVisibility.Auto hides the geometries (markers) of a line while its points are closer than a geometry, and
// shows them again above 1.3 geometries. These lines have 10px geometries without stroke, so they hide below 10px
// between points and show again above 13px. The axes are hidden and the plot fills the chart, so the spacing is
// width / (points - 1).
[TestClass]
public class GeometryVisibilityTests
{
    private const int Points = 11;

    [TestMethod]
    public void AutoHidesTheGeometriesOfADenseLine()
    {
        var series = Line();
        var chart = Chart(series, width: 80); // 8px between points

        _ = chart.GetImage();

        Assert.IsTrue(Opacities(series, chart).All(o => o == 0), "a dense line draws no geometries");
    }

    [TestMethod]
    public void AutoShowsTheGeometriesOfASparseLine()
    {
        var series = Line();
        var chart = Chart(series, width: 500); // 50px between points

        _ = chart.GetImage();

        Assert.IsTrue(Opacities(series, chart).All(o => o == 1), "a sparse line draws its geometries");
    }

    [TestMethod]
    public void AlwaysDrawsTheGeometriesOfADenseLine()
    {
        var series = Line(GeometryVisibility.Always);
        var chart = Chart(series, width: 80);

        _ = chart.GetImage();

        Assert.IsTrue(Opacities(series, chart).All(o => o == 1));
    }

    [TestMethod]
    public void NeverDrawsTheGeometriesOfASparseLine()
    {
        var series = Line(GeometryVisibility.Never);
        var chart = Chart(series, width: 500);

        _ = chart.GetImage();

        Assert.IsTrue(Opacities(series, chart).All(o => o == 0));
    }

    [TestMethod]
    public void AutoDoesNotFlickerBetweenTheTwoLimits()
    {
        var series = Line();
        var chart = Chart(series, width: 80); // 8px: hidden
        _ = chart.GetImage();
        Assert.IsTrue(Opacities(series, chart).All(o => o == 0));

        chart.Width = 115; // 11.5px: between the limits, stays hidden
        _ = chart.GetImage();
        Assert.IsTrue(Opacities(series, chart).All(o => o == 0), "11.5px is not enough to show them again");

        chart.Width = 150; // 15px: shown
        _ = chart.GetImage();
        Assert.IsTrue(Opacities(series, chart).All(o => o == 1));

        chart.Width = 115; // 11.5px: between the limits, stays shown
        _ = chart.GetImage();
        Assert.IsTrue(Opacities(series, chart).All(o => o == 1), "11.5px is not close enough to hide them");
    }

    [TestMethod]
    public async Task TheHoveredPointShowsItsGeometryWhenTheyAreHidden()
    {
        var series = Line();
        var chart = Chart(series, width: 80);
        chart.Tooltip = new SKDefaultTooltip();
        _ = chart.GetImage();

        var first = ((ISeries)series).Fetch(chart.CoreChart).First();
        var area = (RectangleHoverArea)first.Context.HoverArea!;
        chart.CoreChart.InvokePointerMove(new LvcPoint(area.X + area.Width / 2, area.Y + area.Height / 2));
        await chart.CoreChart.TooltipThrottlerUnlocked();

        Assert.IsTrue(((IDrawnElement)first.Context.Visual!).Opacity > 0, "the hovered point shows its geometry");
        Assert.IsTrue(
            ((ISeries)series).Fetch(chart.CoreChart).Skip(1).All(p => ((IDrawnElement)p.Context.Visual!).Opacity == 0),
            "the other points stay hidden");
    }

    [TestMethod]
    public void AutoHidesTheGeometriesOfADenseStepLine()
    {
        var series = new StepLineSeries<double>
        {
            Values = Values(),
            GeometrySize = 10,
            GeometryStroke = null,
            GeometryFill = new SolidColorPaint(SKColors.Red)
        };
        var chart = Chart(series, width: 80);

        _ = chart.GetImage();

        Assert.IsTrue(((ISeries)series).Fetch(chart.CoreChart).All(p => ((IDrawnElement)p.Context.Visual!).Opacity == 0));
    }

    [TestMethod]
    public void APointAloneBetweenTwoGapsKeepsItsGeometry()
    {
        // the point at index 6 has a gap on each side: it has no line, its geometry is the only thing that shows it
        var series = new LineSeries<double?>
        {
            Values = [1, 2, 3, 4, 5, null, 3, null, 2, 1, 2],
            GeometrySize = 10,
            GeometryStroke = null,
            GeometryFill = new SolidColorPaint(SKColors.Red)
        };
        // 9 points with values over 10 steps of 6px: 7.5px apart on average, a dense line
        var chart = Chart(series, width: 60);

        _ = chart.GetImage();

        var opacities = ((ISeries)series).Fetch(chart.CoreChart)
            .Where(p => !p.IsEmpty)
            .ToDictionary(p => p.Index, p => ((IDrawnElement)p.Context.Visual!).Opacity);
        Assert.AreEqual(1f, opacities[6], "the lone point keeps its geometry");
        Assert.IsTrue(opacities.Where(o => o.Key != 6).All(o => o.Value == 0), "the points on a line are hidden");
    }

    [TestMethod]
    public void AutoHidesTheGeometriesOfADensePolarLine()
    {
        var dense = new PolarLineSeries<double>
        {
            Values = Enumerable.Range(0, 200).Select(i => 10d).ToArray(),
            GeometrySize = 10,
            GeometryStroke = null,
            GeometryFill = new SolidColorPaint(SKColors.Red)
        };
        var sparse = new PolarLineSeries<double>
        {
            Values = Enumerable.Range(0, 6).Select(i => 10d).ToArray(),
            GeometrySize = 10,
            GeometryStroke = null,
            GeometryFill = new SolidColorPaint(SKColors.Red)
        };

        foreach (var (series, hidden) in new[] { (dense, true), (sparse, false) })
        {
            var chart = new SKPolarChart { Width = 400, Height = 400, Series = [series] };
            _ = chart.GetImage();
            var expected = hidden ? 0 : 1;
            Assert.IsTrue(
                ((ISeries)series).Fetch(chart.CoreChart).All(p => ((IDrawnElement)p.Context.Visual!).Opacity == expected),
                hidden ? "200 points around the chart are too close" : "6 points around the chart are far apart");
        }
    }

    private static double[] Values() => Enumerable.Range(0, Points).Select(i => (double)(i % 5)).ToArray();

    private static LineSeries<double> Line(GeometryVisibility visibility = GeometryVisibility.Auto) => new()
    {
        Values = Values(),
        GeometrySize = 10,
        GeometryStroke = null,
        GeometryFill = new SolidColorPaint(SKColors.Red),
        GeometryVisibility = visibility
    };

    private static SKCartesianChart Chart(ISeries series, int width) => new()
    {
        Width = width,
        Height = 300,
        Series = [series],
        DrawMargin = new Margin(0),
        XAxes = [new Axis { IsVisible = false, MinLimit = 0, MaxLimit = Points - 1 }],
        YAxes = [new Axis { IsVisible = false }]
    };

    private static float[] Opacities(ISeries series, SKCartesianChart chart) =>
        series.Fetch(chart.CoreChart).Select(p => ((IDrawnElement)p.Context.Visual!).Opacity).ToArray();
}
