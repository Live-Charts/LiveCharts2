using System;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.Themes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.OtherTests;

[TestClass]
public class LiveChartsSettingsTests
{
    // Each test instantiates a local LiveChartsSettings to avoid mutating the global
    // LiveCharts.DefaultSettings used by the rest of the suite.

    [TestMethod]
    public void FluentScalarSettersAssignAndReturnSelf()
    {
        var s = new LiveChartsSettings();

        var same = s
            .WithAnimationsSpeed(TimeSpan.FromMilliseconds(123))
            .WithEasingFunction(EasingFunctions.Lineal)
            .WithZoomSpeed(0.5)
            .WithZoomMode(ZoomAndPanMode.Both)
            .WithUpdateThrottlingTimeout(TimeSpan.FromMilliseconds(77));

        Assert.AreSame(s, same);
        Assert.AreEqual(TimeSpan.FromMilliseconds(123), s.AnimationsSpeed);
        Assert.AreSame((Func<float, float>)EasingFunctions.Lineal, s.EasingFunction);
        Assert.AreEqual(0.5, s.ZoomSpeed);
        Assert.AreEqual(ZoomAndPanMode.Both, s.ZoomMode);
        Assert.AreEqual(TimeSpan.FromMilliseconds(77), s.UpdateThrottlingTimeout);
    }

    [TestMethod]
    public void FluentLegendAndTooltipSettersAssignAndReturnSelf()
    {
        var s = new LiveChartsSettings();
        var bg = new SolidColorPaint(SKColors.Red);
        var text = new SolidColorPaint(SKColors.Black);

        var same = s
            .WithLegendBackgroundPaint(bg)
            .WithLegendTextPaint(text)
            .WithLegendTextSize(15)
            .WithTooltipBackgroundPaint(bg)
            .WithTooltipTextPaint(text)
            .WithTooltipTextSize(11);

        Assert.AreSame(s, same);
        Assert.AreSame(bg, s.LegendBackgroundPaint);
        Assert.AreSame(text, s.LegendTextPaint);
        Assert.AreEqual(15, s.LegendTextSize);
        Assert.AreSame(bg, s.TooltipBackgroundPaint);
        Assert.AreSame(text, s.TooltipTextPaint);
        Assert.AreEqual(11, s.TooltipTextSize);
    }

    [TestMethod]
    public void HasMapStoresAndGetMapRetrievesMapper()
    {
        var s = new LiveChartsSettings();
        Func<string, int, Coordinate> mapper = (model, idx) => new(idx, model.Length);

        var same = s.HasMap(mapper);
        Assert.AreSame(s, same);

        var found = s.GetMap<string>();
        Assert.AreSame(mapper, found);
    }

    [TestMethod]
    public void RemoveMapDropsTheMapping()
    {
        var s = new LiveChartsSettings()
            .HasMap<string>((model, idx) => new(idx, model.Length));

        _ = s.RemoveMap<string>();

        // After removal, querying the same type throws because no mapper is registered.
        Assert.ThrowsExactly<NotImplementedException>(() => _ = s.GetMap<string>());
    }

    [TestMethod]
    public void GetMapForUnregisteredTypeThrowsNotImplementedException()
    {
        var s = new LiveChartsSettings();
        Assert.ThrowsExactly<NotImplementedException>(() => _ = s.GetMap<DateTime>());
    }

    [TestMethod]
    public void GetMapForObjectThrowsXamlSpecificException()
    {
        var s = new LiveChartsSettings();
        // The object special-case has its own message that hints at XAML usage.
        var ex = Assert.ThrowsExactly<Exception>(() => _ = s.GetMap<object>());
        StringAssert.Contains(ex.Message, "x:TypeArguments");
    }

    [TestMethod]
    public void AddDefaultMappersRegistersBuiltInNumericTypes()
    {
        var s = new LiveChartsSettings();
        _ = s.AddDefaultMappers();

        // Pick a representative subset across nullable/non-nullable and integer/float.
        Assert.IsNotNull(s.GetMap<int>());
        Assert.IsNotNull(s.GetMap<long>());
        Assert.IsNotNull(s.GetMap<float>());
        Assert.IsNotNull(s.GetMap<double>());
        Assert.IsNotNull(s.GetMap<decimal>());
        Assert.IsNotNull(s.GetMap<int?>());
        Assert.IsNotNull(s.GetMap<double?>());
    }

    [TestMethod]
    public void HasThemeInvokesBuilderAndGetThemeReturnsConfiguredInstance()
    {
        var s = new LiveChartsSettings();
        Theme? captured = null;

        var same = s.HasTheme(theme => captured = theme);
        Assert.AreSame(s, same);

        Assert.IsNotNull(captured);
        Assert.AreSame(captured, s.GetTheme());
    }

    [TestMethod]
    public void UseRightToLeftSettingsSetsTheInternalFlag()
    {
        var s = new LiveChartsSettings();

        var same = s.UseRightToLeftSettings();

        Assert.AreSame(s, same);
        Assert.IsTrue(s.IsRightToLeft);
    }
}
