using System.Linq;
using LiveChartsCore.Drawing;
using LiveChartsCore.Geo;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Drawing;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.CoreObjectsTests;

[TestClass]
public class GeoMapEventsTests
{
    [TestMethod]
    public void DataPointerDownFiresWhenClickingInsideALand()
    {
        var chart = NewGeoMap();
        chart.CoreChart.Measure();

        ChartPoint[]? captured = null;
        chart.DataPointerDown += (_, points) => captured = points.ToArray();

        var moscow = MoscowPixel(chart);
        chart.CoreChart.InvokePointerDown(moscow, isSecondaryAction: false);
        chart.CoreChart.InvokePointerUp(moscow, isSecondaryAction: false);

        Assert.IsNotNull(captured);
        Assert.AreEqual(1, captured.Length);
        var land = Assert.IsInstanceOfType<LandDefinition>(captured[0].Context.DataSource);
        Assert.AreEqual("rus", land.ShortName);
    }

    [TestMethod]
    public void DataPointerDownDoesNotFireWhenClickingTheOcean()
    {
        var chart = NewGeoMap();
        chart.CoreChart.Measure();

        var fired = false;
        chart.DataPointerDown += (_, _) => fired = true;

        // Mid-Atlantic: lon=-30, lat=0 sits in open ocean on Mercator.
        var projector = Maps.BuildProjector(MapProjection.Mercator, [600f, 600f]);
        projector.ToMap(-30, 0, out var oceanX, out var oceanY);
        var ocean = new LvcPoint(oceanX, oceanY);
        chart.CoreChart.InvokePointerDown(ocean, isSecondaryAction: false);
        chart.CoreChart.InvokePointerUp(ocean, isSecondaryAction: false);

        Assert.IsFalse(fired);
    }

    [TestMethod]
    public void DataPointerDownPayloadCarriesLandAndPositionInsideHoverArea()
    {
        var chart = NewGeoMap();
        chart.CoreChart.Measure();

        ChartPoint[]? captured = null;
        chart.DataPointerDown += (_, points) => captured = points.ToArray();

        var moscow = MoscowPixel(chart);
        chart.CoreChart.InvokePointerDown(moscow, isSecondaryAction: false);
        chart.CoreChart.InvokePointerUp(moscow, isSecondaryAction: false);

        Assert.IsNotNull(captured);
        var land = Assert.IsInstanceOfType<LandDefinition>(captured[0].Context.DataSource);
        Assert.AreEqual("rus", land.ShortName);

        // Heat value is read off the source series (see overview.md recipe).
        var series = (HeatLandSeries)chart.Series.Single();
        Assert.IsTrue(series.TryGetValue(land.ShortName, out var value));
        Assert.AreEqual(42d, value);

        // The hover area is the projected screen bbox of the land; the click
        // pixel must fall inside it.
        var bbox = Assert.IsInstanceOfType<RectangleHoverArea>(captured[0].Context.HoverArea);
        Assert.IsTrue(moscow.X >= bbox.X && moscow.X <= bbox.X + bbox.Width);
        Assert.IsTrue(moscow.Y >= bbox.Y && moscow.Y <= bbox.Y + bbox.Height);
    }

    [TestMethod]
    public void DataPointerDownDoesNotFireWhenPointerMovesPastClickThreshold()
    {
        // GeoMapChart tracks a 5px click-vs-drag threshold in InvokePointerMove;
        // a drag that exceeds it flips _pointerDownIsClick=false and the
        // subsequent pointer-up must NOT fire DataPointerDown.
        var chart = NewGeoMap();
        chart.CoreChart.Measure();

        var fired = false;
        chart.DataPointerDown += (_, _) => fired = true;

        var moscow = MoscowPixel(chart);
        chart.CoreChart.InvokePointerDown(moscow, isSecondaryAction: false);
        chart.CoreChart.InvokePointerMove(new LvcPoint(moscow.X + 20, moscow.Y + 20));
        chart.CoreChart.InvokePointerUp(new LvcPoint(moscow.X + 20, moscow.Y + 20), isSecondaryAction: false);

        Assert.IsFalse(fired);
    }

    private static SKGeoMap NewGeoMap() => new()
    {
        Width = 600,
        Height = 600,
        MapProjection = MapProjection.Mercator,
        Series = [new HeatLandSeries { Lands = [new() { Name = "rus", Value = 42 }] }],
    };

    private static LvcPoint MoscowPixel(SKGeoMap chart)
    {
        var projector = Maps.BuildProjector(
            MapProjection.Mercator,
            [chart.Width, chart.Height]);
        projector.ToMap(37.62, 55.75, out var x, out var y);
        return new LvcPoint(x, y);
    }
}
