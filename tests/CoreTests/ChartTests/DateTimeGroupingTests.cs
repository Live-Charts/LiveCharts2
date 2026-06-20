using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.ChartTests;

// Verifies the adaptive two-tier date grouping behind DateTimeAxis.GroupTimeUnits: tier
// selection (years -> months -> ... -> hours) as the visible span shrinks, exact calendar
// boundaries for separators, the two-line labeler (fine unit on top, coarse context on
// the bottom — shown at each coarse boundary), and the built-in axis plumbing (no engine
// override required).
[TestClass]
public class DateTimeGroupingTests
{
    private CultureInfo _originalCulture = null!;

    // The grouping formats month/time labels with the current culture (a German user sees
    // German months) — pin a known culture so these assertions read deterministic English.
    [TestInitialize]
    public void SetInvariantCulture()
    {
        _originalCulture = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
    }

    [TestCleanup]
    public void RestoreCulture() => Thread.CurrentThread.CurrentCulture = _originalCulture;

    private static (List<double> ticks, Func<double, string> labeler) Group(DateTime min, DateTime max)
    {
        var ok = DateTimeGrouping.TryGroup(min.Ticks, max.Ticks, out var separators, out var labeler);
        Assert.IsTrue(ok, "TryGroup should take over for a valid date range");
        return (separators!.ToList(), labeler!);
    }

    [TestMethod]
    public void InvalidRange_IsDeclined()
    {
        var t = new DateTime(2020, 1, 1).Ticks;
        Assert.IsFalse(
            DateTimeGrouping.TryGroup(t, t, out _, out _),
            "an empty (max == min) range must be declined so the axis lays itself out");
    }

    [TestMethod]
    public void Separators_AreWithinRange_AndAscending()
    {
        var (ticks, _) = Group(new DateTime(2020, 2, 1), new DateTime(2020, 8, 1));

        Assert.IsTrue(ticks.Count > 0);
        var min = new DateTime(2020, 2, 1).Ticks;
        var max = new DateTime(2020, 8, 1).Ticks;
        for (var i = 0; i < ticks.Count; i++)
        {
            Assert.IsTrue(ticks[i] >= min && ticks[i] <= max, "every separator must sit inside the visible range");
            if (i > 0) Assert.IsTrue(ticks[i] > ticks[i - 1], "separators must be strictly ascending");
        }
    }

    [TestMethod]
    public void SixMonthSpan_GroupsByMonth()
    {
        // Feb..Aug 2020 -> a month tier: one separator per month start.
        var (ticks, labeler) = Group(new DateTime(2020, 2, 1), new DateTime(2020, 8, 1));

        CollectionAssert.AreEqual(
            new[] { 2, 3, 4, 5, 6, 7, 8 },
            ticks.Select(t => new DateTime((long)t).Month).ToArray(),
            "month tier should place a separator at the first of each visible month");

        // Top line is the month abbreviation; a mid-range month carries no second line.
        var march = labeler(new DateTime(2020, 3, 1).Ticks);
        Assert.AreEqual("Mar\n", march, "a non-boundary month shows only the fine (month) line");
    }

    [TestMethod]
    public void FirstVisibleTick_DoesNotForceCoarseContext()
    {
        // The range opens in February (not a year boundary); the first label shows only the
        // month — the coarse context appears at real boundaries, not forced onto the first tick.
        var (ticks, labeler) = Group(new DateTime(2020, 2, 1), new DateTime(2020, 8, 1));

        Assert.AreEqual("Feb\n", labeler(ticks[0]), "the first tick must not be forced to carry the year");
    }

    [TestMethod]
    public void YearBoundary_ShowsYearOnSecondLine()
    {
        // Nov 2019 .. Apr 2020 crosses a new year; the January tick must surface 2020.
        var (_, labeler) = Group(new DateTime(2019, 11, 1), new DateTime(2020, 4, 1));

        var jan = labeler(new DateTime(2020, 1, 1).Ticks);
        Assert.AreEqual("Jan\n2020", jan, "January opens a new year, so its label is Jan over 2020");

        var dec = labeler(new DateTime(2019, 12, 1).Ticks);
        Assert.AreEqual("Dec\n", dec, "December is not a year boundary, so no second line");
    }

    [TestMethod]
    public void MultiYearSpan_GroupsByYear_SingleLine()
    {
        // A decade has no coarser tier than the year, so labels stay single-line.
        var (ticks, labeler) = Group(new DateTime(2010, 1, 1), new DateTime(2020, 1, 1));

        foreach (var label in ticks.Select(labeler))
        {
            Assert.IsFalse(label.Contains('\n'), "year tier labels are single-line");
            Assert.IsTrue(int.TryParse(label, out _), $"a year label should be a plain year, got '{label}'");
        }
    }

    [TestMethod]
    public void DayTier_DoesNotCrowdMonthBoundary()
    {
        // A ~quarter spans into the day tier (step 10). Without dropping the trailing
        // remainder day, the 31st would land one day before the next month's 1st and the
        // two labels would overlap. No two separators may sit a single day apart.
        var (ticks, _) = Group(new DateTime(2020, 4, 1), new DateTime(2020, 7, 1));

        for (var i = 1; i < ticks.Count; i++)
        {
            var gapDays = new DateTime((long)ticks[i]).Subtract(new DateTime((long)ticks[i - 1])).TotalDays;
            Assert.IsTrue(gapDays >= 2, $"day-tier separators must not crowd a month boundary (gap was {gapDays:0.#} days)");
        }

        // The first of every visible month is still present (it carries the context).
        var months = ticks.Select(t => new DateTime((long)t)).Where(d => d.Day == 1).Select(d => d.Month).ToArray();
        CollectionAssert.AreEqual(new[] { 4, 5, 6, 7 }, months, "each month's day 1 must be kept");
    }

    [TestMethod]
    public void SingleDaySpan_GroupsByTime_WithDateContext()
    {
        // One day -> an hour tier: the fine line is a time, midnight carries the date.
        var (ticks, labeler) = Group(new DateTime(2020, 6, 1), new DateTime(2020, 6, 2));

        Assert.IsTrue(ticks.Count > 1, "a one-day span should produce several intraday separators");

        var midnight = labeler(new DateTime(2020, 6, 1).Ticks);
        StringAssert.Contains(midnight, "00:00", "the fine line is a time");
        StringAssert.Contains(midnight, "2020", "midnight opens a new day, so the date context shows");
    }

    [TestMethod]
    public void TierAdapts_AsRangeShrinks()
    {
        // The same anchor seen at decade / half-year / single-day scales must yield
        // progressively finer fine-units (year number -> month name -> clock time).
        var decade = Group(new DateTime(2010, 1, 1), new DateTime(2020, 1, 1)).labeler;
        var halfYear = Group(new DateTime(2020, 1, 1), new DateTime(2020, 7, 1)).labeler;
        var oneDay = Group(new DateTime(2020, 6, 1), new DateTime(2020, 6, 2)).labeler;

        Assert.IsTrue(int.TryParse(decade(new DateTime(2015, 1, 1).Ticks), out _), "decade -> year fine unit");
        StringAssert.Contains(halfYear(new DateTime(2020, 3, 1).Ticks), "Mar", "half-year -> month fine unit");
        StringAssert.Contains(oneDay(new DateTime(2020, 6, 1, 6, 0, 0).Ticks), ":", "one day -> time fine unit");
    }

    // Ranges that reach DateTime.MaxValue are valid inputs, but ADVANCING past the last
    // tick overflows DateTime arithmetic — generation must terminate cleanly per tier
    // instead of throwing out of AddMonths/AddYears/Add*.
    [TestMethod]
    public void RangesReachingDateTimeMaxValue_DoNotThrow()
    {
        var max = DateTime.MaxValue;

        // Year tier: AlignFloor lands on a step multiple; the next AddYears overflows.
        var (yearTicks, _) = Group(max.AddYears(-10), max);
        Assert.IsTrue(yearTicks.Count > 0, "the year tier terminates at the bound");

        // Month tier: the last AddMonths from 9999-12 overflows.
        var (monthTicks, _) = Group(new DateTime(9999, 6, 1), max);
        Assert.IsTrue(monthTicks.Count > 0, "the month tier terminates at the bound");

        // Day tier (the per-month walk): advancing past month 9999-12 overflows.
        var (dayTicks, _) = Group(new DateTime(9999, 10, 1), new DateTime(9999, 12, 28));
        Assert.IsTrue(dayTicks.Count > 0, "the day tier's month walk terminates at the bound");

        // Hour tier: the fine-unit Advance overflows in the final hours of year 9999.
        var (hourTicks, _) = Group(max.AddDays(-1), max);
        Assert.IsTrue(hourTicks.Count > 0, "the time tiers terminate at the bound");
    }

    [TestMethod]
    public void GroupTimeUnits_IsBuiltIntoTheAxis_NoEngineRequired()
    {
        // The whole point of the move: a plain DateTimeAxis with GroupTimeUnits renders the
        // adaptive two-line labels with the DEFAULT engine — observable through the drawn
        // label texts (a grouped month tier emits "MMM\n[context]" labels).
        var labelsPaint = new SolidColorPaint(SKColors.Black);
        var from = new DateTime(2020, 1, 1);
        var to = new DateTime(2021, 12, 31);

        var xAxis = new DateTimeAxis(TimeSpan.FromDays(1), d => d.ToString("d"))
        {
            GroupTimeUnits = true,
            LabelsPaint = labelsPaint,
            MinLimit = from.Ticks,
            MaxLimit = to.Ticks,
        };

        var chart = new SKCartesianChart
        {
            Width = 600,
            Height = 400,
            Series = [new LineSeries<double> { Values = [0, 100], GeometrySize = 0 }],
            XAxes = [xAxis],
            YAxes = [new Axis()],
        };
        _ = CoreObjectsTests.ChangingPaintTasks.DrawChart(chart);

        var labels = labelsPaint.GetGeometries(chart.CoreCanvas)
            .OfType<LabelGeometry>()
            .Select(l => l.Text)
            .Where(t => !string.IsNullOrEmpty(t))
            .ToArray();

        Assert.IsTrue(labels.Length > 0, "the grouped axis draws labels");
        Assert.IsTrue(labels.Any(t => t!.Contains('\n')), "grouped labels are two-line (fine unit over context)");
        Assert.IsTrue(
            labels.Any(t => t!.StartsWith("Jan", StringComparison.Ordinal)),
            "a two-year span lands on the month tier");
    }
}
