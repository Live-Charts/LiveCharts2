using LiveChartsCore.Geo;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.OtherTests;

// Regression for https://github.com/Live-Charts/LiveCharts2/issues/1417
// Reported: GeoMap inside a TabControl crashes on the second tab switch
// because Unload was not idempotent and there was no paired Load to restore
// the chart on re-attach.
[TestClass]
public class GeoMapReattachTests
{
    [TestMethod]
    public void Unload_IsIdempotent()
    {
        var chart = NewChart();
        using var image = chart.GetImage();

        chart.CoreChart.Unload();
        chart.CoreChart.Unload();
    }

    [TestMethod]
    public void Reattach_DetachLoadDetach_DoesNotThrowAndRendersAfterReload()
    {
        var chart = NewChart();
        using (chart.GetImage()) { }

        // simulates: TabItem switch away → swap back → switch away again
        chart.CoreChart.Unload();
        chart.CoreChart.Load();

        // image generation after Load proves the chart is functional again
        using var image = chart.GetImage();
        Assert.IsNotNull(image);

        chart.CoreChart.Unload();
    }

    [TestMethod]
    public void Load_BeforeAnyUnload_IsNoOpAndDoesNotThrow()
    {
        // SourceGenSKMapChart.DrawOnCanvas calls Unload() at the end of every
        // render, so we deliberately skip GetImage() here — Load() must be
        // exercised on a chart that has never been unloaded.
        var chart = NewChart();

        chart.CoreChart.Load();

        using var image = chart.GetImage();
        Assert.IsNotNull(image);
    }

    // NOTE: The original PR #2189 also asserted Unload's tooltip-cleanup branch
    // (Unload_HidesTooltipAndClearsHoveredLand). That assertion required
    // IGeoMapTooltip / GeoTooltipPoint / GeoMapChart._hoveredLand /
    // _isToolTipOpen — all introduced by PR #2127 (improve-geomap), which is
    // not part of this 2.0.3 backport. The tooltip-reset block in
    // GeoMapChart.Unload was likewise dropped; see backport commit
    // "fix(geomap): drop tooltip-reset on Unload (depends on unbackported infra)".

    private static SKGeoMap NewChart() => new()
    {
        Width = 400,
        Height = 400,
        Series = [
            new HeatLandSeries { Lands = [ new() { Name = "bra", Value = 13 } ] }
        ]
    };
}
