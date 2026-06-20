using System;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.ChartTests;

[TestClass]
public class PolarDrawMarginTests
{
    // PolarChartEngine.Measure computed `actualMargin` from the view's DrawMargin (honoring explicit
    // sides, falling back to the auto margin for Margin.Auto sides) but then called
    // SetDrawMargin(ControlSize, m) with the raw AUTO margin `m`, silently ignoring the user's
    // DrawMargin entirely (every other engine — cartesian, pie, treemap, sankey — uses actualMargin).
    // This pins that an explicit DrawMargin is honored.
    [TestMethod]
    public void ExplicitDrawMargin_IsHonored()
    {
        // distinct sides so a left/right or top/bottom swap would be caught.
        var margin = new Margin(40, 50, 60, 70); // left, top, right, bottom

        var chart = new SKPolarChart
        {
            Width = 300,
            Height = 300,
            AnimationsSpeed = TimeSpan.Zero,
            ExplicitDisposing = true,
            DrawMargin = margin,
            Series = [new PolarLineSeries<int> { Values = [1, 2, 3] }],
        };

        _ = chart.GetImage();

        var core = ((IChartView)chart).CoreChart;

        // location is the (left, top) of the requested margin...
        Assert.AreEqual(40f, core.DrawMarginLocation.X, 1e-4, "left margin not honored");
        Assert.AreEqual(50f, core.DrawMarginLocation.Y, 1e-4, "top margin not honored");

        // ...and the size is the control minus the requested left+right / top+bottom.
        Assert.AreEqual(300f - 40f - 60f, core.DrawMarginSize.Width, 1e-4, "left/right margin not honored");
        Assert.AreEqual(300f - 50f - 70f, core.DrawMarginSize.Height, 1e-4, "top/bottom margin not honored");
    }
}
