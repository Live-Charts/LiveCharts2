using System;
using LiveChartsCore.SkiaSharpView;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.OtherTests;

[TestClass]
public class TimeSpanAxisTests
{
    [TestMethod]
    public void ConstructorSetsUnitWidthAndMinStepToUnitTicks()
    {
        var unit = TimeSpan.FromMinutes(5);
        var axis = new TimeSpanAxis(unit, ts => ts.ToString());

        Assert.AreEqual((double)unit.Ticks, axis.UnitWidth);
        Assert.AreEqual((double)unit.Ticks, axis.MinStep);
    }

    [TestMethod]
    public void LabelerFormatsTicksAsTimeSpan()
    {
        var unit = TimeSpan.FromSeconds(1);
        var axis = new TimeSpanAxis(unit, ts => $"{ts.TotalMinutes:0.#}m");

        var oneMinuteInTicks = (double)TimeSpan.FromMinutes(1).Ticks;
        Assert.AreEqual("1m", axis.Labeler(oneMinuteInTicks));
    }

    [TestMethod]
    public void LabelerClampsNegativeTicksToZero()
    {
        var unit = TimeSpan.FromMilliseconds(1);
        var axis = new TimeSpanAxis(unit, ts => ts.Ticks.ToString());

        // AsTimeSpan clamps negatives to zero before building a TimeSpan.
        Assert.AreEqual("0", axis.Labeler(-1000));
    }
}
