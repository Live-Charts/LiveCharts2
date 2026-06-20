using System.Reflection;
using LiveChartsCore;
using LiveChartsCore.Defaults;
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

    [TestMethod]
    public void Unload_HidesTooltipAndClearsHoveredLand()
    {
        // If a land is hovered when the chart is detached, Unload must hide the
        // tooltip and clear hover state — otherwise Measure on the next Load
        // re-shows the tooltip from a stale _hoveredLand.
        var chart = NewChart();
        var tooltip = new RecordingTooltip();
        ((IGeoMapView)chart).Tooltip = tooltip;

        var hoveredField = typeof(GeoMapChart).GetField(
            "_hoveredLand", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var isOpenField = typeof(GeoMapChart).GetField(
            "_isToolTipOpen", BindingFlags.Instance | BindingFlags.NonPublic)!;

        hoveredField.SetValue(chart.CoreChart, new LandDefinition("bra", "Brazil", "default"));
        isOpenField.SetValue(chart.CoreChart, true);

        chart.CoreChart.Unload();

        Assert.AreEqual(1, tooltip.HideCalls, "Tooltip.Hide should be called once during Unload.");
        Assert.IsNull(hoveredField.GetValue(chart.CoreChart), "_hoveredLand must be cleared.");
        Assert.AreEqual(false, isOpenField.GetValue(chart.CoreChart), "_isToolTipOpen must be reset.");
    }

    private static SKGeoMap NewChart() => new()
    {
        Width = 400,
        Height = 400,
        Series = [
            new HeatLandSeries { Lands = [ new() { Name = "bra", Value = 13 } ] }
        ]
    };

    private sealed class RecordingTooltip : IGeoMapTooltip
    {
        public int HideCalls { get; private set; }
        public int ShowCalls { get; private set; }
        public void Show(GeoTooltipPoint point, GeoMapChart chart) => ShowCalls++;
        public void Hide(GeoMapChart chart) => HideCalls++;
    }
}
