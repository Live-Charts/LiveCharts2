using System;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Measure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.CoreObjectsTests;

[TestClass]
public class HeatFunctionsTesting
{
    [TestMethod]
    public void BuildColorStopsWithNullStopsGeneratesUniformDistribution()
    {
        var colors = new[] { LvcColor.FromArgb(255, 255, 0, 0), LvcColor.FromArgb(255, 0, 0, 255) };
        var stops = HeatFunctions.BuildColorStops(colors, null);

        Assert.IsTrue(stops.Count == 2);
        Assert.IsTrue(Math.Abs(stops[0].Item1 - 0) < 0.001);
        Assert.IsTrue(Math.Abs(stops[1].Item1 - 1) < 0.001);
    }

    [TestMethod]
    public void BuildColorStopsWithThreeColorsAndNullStops()
    {
        var colors = new[]
        {
            LvcColor.FromArgb(255, 255, 0, 0),
            LvcColor.FromArgb(255, 0, 255, 0),
            LvcColor.FromArgb(255, 0, 0, 255)
        };
        var stops = HeatFunctions.BuildColorStops(colors, null);

        Assert.IsTrue(stops.Count == 3);
        Assert.IsTrue(Math.Abs(stops[0].Item1 - 0) < 0.001);
        Assert.IsTrue(Math.Abs(stops[1].Item1 - 0.5) < 0.001);
        Assert.IsTrue(Math.Abs(stops[2].Item1 - 1.0) < 0.001);
    }

    [TestMethod]
    public void BuildColorStopsWithExplicitStops()
    {
        var colors = new[]
        {
            LvcColor.FromArgb(255, 255, 0, 0),
            LvcColor.FromArgb(255, 0, 0, 255)
        };
        var customStops = new double[] { 0.2, 0.8 };
        var stops = HeatFunctions.BuildColorStops(colors, customStops);

        Assert.IsTrue(stops.Count == 2);
        Assert.IsTrue(Math.Abs(stops[0].Item1 - 0.2) < 0.001);
        Assert.IsTrue(Math.Abs(stops[1].Item1 - 0.8) < 0.001);
    }

    [TestMethod]
    public void BuildColorStopsThrowsWithFewerThanTwoColors()
    {
        var colors = new[] { LvcColor.FromArgb(255, 255, 0, 0) };
        Assert.ThrowsExactly<Exception>(() => HeatFunctions.BuildColorStops(colors, null));
    }

    [TestMethod]
    public void BuildColorStopsThrowsWhenStopsLengthMismatch()
    {
        var colors = new[]
        {
            LvcColor.FromArgb(255, 255, 0, 0),
            LvcColor.FromArgb(255, 0, 0, 255)
        };
        var customStops = new double[] { 0.0, 0.5, 1.0 };
        Assert.ThrowsExactly<Exception>(() => HeatFunctions.BuildColorStops(colors, customStops));
    }

    [TestMethod]
    public void InterpolateColorAtStart()
    {
        var red = LvcColor.FromArgb(255, 255, 0, 0);
        var blue = LvcColor.FromArgb(255, 0, 0, 255);
        var heatMap = new[] { red, blue };
        var stops = HeatFunctions.BuildColorStops(heatMap, null);
        var bounds = new Bounds();
        bounds.AppendValue(0);
        bounds.AppendValue(100);

        var result = HeatFunctions.InterpolateColor(0, bounds, heatMap, stops);

        Assert.IsTrue(result.R == 255);
        Assert.IsTrue(result.G == 0);
        Assert.IsTrue(result.B == 0);
    }

    [TestMethod]
    public void InterpolateColorAtEnd()
    {
        var red = LvcColor.FromArgb(255, 255, 0, 0);
        var blue = LvcColor.FromArgb(255, 0, 0, 255);
        var heatMap = new[] { red, blue };
        var stops = HeatFunctions.BuildColorStops(heatMap, null);
        var bounds = new Bounds();
        bounds.AppendValue(0);
        bounds.AppendValue(100);

        var result = HeatFunctions.InterpolateColor(100, bounds, heatMap, stops);

        Assert.IsTrue(result.R == 0);
        Assert.IsTrue(result.G == 0);
        Assert.IsTrue(result.B == 255);
    }

    [TestMethod]
    public void InterpolateColorAtMidpoint()
    {
        var red = LvcColor.FromArgb(255, 255, 0, 0);
        var blue = LvcColor.FromArgb(255, 0, 0, 255);
        var heatMap = new[] { red, blue };
        var stops = HeatFunctions.BuildColorStops(heatMap, null);
        var bounds = new Bounds();
        bounds.AppendValue(0);
        bounds.AppendValue(100);

        var result = HeatFunctions.InterpolateColor(50, bounds, heatMap, stops);

        // At midpoint, expect ~127 for R and B
        Assert.IsTrue(result.R > 100 && result.R < 160);
        Assert.IsTrue(result.B > 100 && result.B < 160);
    }

    [TestMethod]
    public void InterpolateColorClampsBelow()
    {
        var red = LvcColor.FromArgb(255, 255, 0, 0);
        var blue = LvcColor.FromArgb(255, 0, 0, 255);
        var heatMap = new[] { red, blue };
        var stops = HeatFunctions.BuildColorStops(heatMap, null);
        var bounds = new Bounds();
        bounds.AppendValue(0);
        bounds.AppendValue(100);

        // Value below minimum should clamp to 0
        var result = HeatFunctions.InterpolateColor(-50, bounds, heatMap, stops);
        Assert.IsTrue(result.R == 255);
    }

    [TestMethod]
    public void InterpolateColorClampsAbove()
    {
        var red = LvcColor.FromArgb(255, 255, 0, 0);
        var blue = LvcColor.FromArgb(255, 0, 0, 255);
        var heatMap = new[] { red, blue };
        var stops = HeatFunctions.BuildColorStops(heatMap, null);
        var bounds = new Bounds();
        bounds.AppendValue(0);
        bounds.AppendValue(100);

        // Value above maximum should clamp to 1
        var result = HeatFunctions.InterpolateColor(200, bounds, heatMap, stops);
        Assert.IsTrue(result.B == 255);
    }

    [TestMethod]
    public void InterpolateColorHandlesZeroRange()
    {
        var red = LvcColor.FromArgb(255, 255, 0, 0);
        var blue = LvcColor.FromArgb(255, 0, 0, 255);
        var heatMap = new[] { red, blue };
        var stops = HeatFunctions.BuildColorStops(heatMap, null);
        var bounds = new Bounds();
        bounds.AppendValue(50);

        // When min == max, range is 0 but should not crash
        var result = HeatFunctions.InterpolateColor(50, bounds, heatMap, stops);
        Assert.IsTrue(result.A == 255);
    }
}
