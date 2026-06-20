// The MIT License(MIT)
//
// Copyright(c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;

namespace LiveChartsCore.SkiaSharpView;

// -----------------------------------------------------------------------------
// Adaptive, two-tier date axis. When a DateTimeAxis opts in via GroupTimeUnits,
// the axis consults this while measuring and uses the separators + labeler it
// returns for the visible range. It rides the existing label pipeline: the
// labeler emits a two-line string (fine unit on the top line, coarser context on
// the bottom) and the multi-line text renderer draws it — no custom drawing. As
// you zoom, the visible span changes, the axis re-consults, and the tier walks
// years -> months -> days -> hours -> minutes -> seconds.
//
// Axis units are DateTime.Ticks (the convention DateTimeAxis establishes:
// UnitWidth = unit.Ticks, Labeler = v => fmt(v.AsDate())). So min/max here are
// tick counts; double.AsDate() turns them back into DateTime.
// -----------------------------------------------------------------------------

/// <summary>
/// Supplies adaptive, multi-level date labels for axes that set
/// <see cref="DateTimeAxis.GroupTimeUnits"/>.
/// </summary>
internal static class DateTimeGrouping
{
    // Aim for at most this many fine separators across the visible range; the tier that
    // first fits under this budget is the finest granularity that won't crowd the axis.
    private const int TargetMaxTicks = 10;

    // Hard backstop so a degenerate range can never spin the generation loop.
    private const int MaxSeparators = 4000;

    private enum Unit { Second, Minute, Hour, Day, Month, Year }

    private readonly struct Tier(Unit fine, int step, Unit? coarse)
    {
        public Unit Fine { get; } = fine;
        public int Step { get; } = step;
        public Unit? Coarse { get; } = coarse;
    }

    // Candidate tiers from finest to coarsest. Every step divides its parent evenly
    // (minutes/seconds 60, hours 24, months 12) so the coarser boundary always lands on
    // a fine tick — that is what lets the bottom-line context appear at, e.g., every Jan
    // or every midnight. Day is the exception (months have unequal lengths) and is
    // generated per-month so day 1 always lands; see Generate.
    private static readonly Tier[] s_tiers =
    [
        new(Unit.Second, 1, Unit.Minute), new(Unit.Second, 5, Unit.Minute),
        new(Unit.Second, 15, Unit.Minute), new(Unit.Second, 30, Unit.Minute),
        new(Unit.Minute, 1, Unit.Hour), new(Unit.Minute, 5, Unit.Hour),
        new(Unit.Minute, 15, Unit.Hour), new(Unit.Minute, 30, Unit.Hour),
        new(Unit.Hour, 1, Unit.Day), new(Unit.Hour, 3, Unit.Day),
        new(Unit.Hour, 6, Unit.Day), new(Unit.Hour, 12, Unit.Day),
        new(Unit.Day, 1, Unit.Month), new(Unit.Day, 2, Unit.Month),
        new(Unit.Day, 5, Unit.Month), new(Unit.Day, 10, Unit.Month),
        new(Unit.Month, 1, Unit.Year), new(Unit.Month, 3, Unit.Year),
        new(Unit.Year, 1, null), new(Unit.Year, 2, null), new(Unit.Year, 5, null),
        new(Unit.Year, 10, null), new(Unit.Year, 20, null), new(Unit.Year, 50, null),
        new(Unit.Year, 100, null), new(Unit.Year, 1000, null),
    ];

    /// <summary>
    /// Groups the visible range into adaptive multi-level date separators + labeler.
    /// Returns false for invalid ranges so the axis lays itself out as usual.
    /// </summary>
    public static bool TryGroup(
        double min, double max,
        out IEnumerable<double>? separators, out Func<double, string>? labeler)
    {
        separators = null;
        labeler = null;

        // NaN-safe decline: a NaN range must fall through to "lay the axis out as usual"
        // (note `max <= min` alone would let NaN ranges continue).
        if (double.IsNaN(min) || double.IsNaN(max) || max <= min) return false;
        if (min < DateTime.MinValue.Ticks || max > DateTime.MaxValue.Ticks) return false;

        var tier = PickTier(max - min);
        var ticks = Generate(min, max, tier);
        if (ticks.Count == 0) return false;

        separators = ticks;
        labeler = value => Format(value, tier);
        return true;
    }

    // Ordinal of the tier cell that starts at the given tick value, for the tier the
    // [min, max] range lands on. Used by the alternating-bands feature to anchor zebra
    // parity to something stable across pan (a list index would swap colors every time a
    // separator scrolls in or out). Exact for the uniform tiers (seconds/minutes/hours are
    // even in ticks, months divide 12, years divide by the step); the Day tier is generated
    // per-month with a trailing-remainder drop, so its day-number quotient is approximate —
    // stable per value (no flicker), but two adjacent cells can share a color at some month
    // seams.
    public static long GetCellOrdinal(double min, double max, double value)
    {
        var tier = PickTier(max - min);
        var dt = FromTicksClamped(value);

        return tier.Fine switch
        {
            Unit.Second => (long)value / (TimeSpan.TicksPerSecond * tier.Step),
            Unit.Minute => (long)value / (TimeSpan.TicksPerMinute * tier.Step),
            Unit.Hour => (long)value / (TimeSpan.TicksPerHour * tier.Step),
            Unit.Day => (long)value / (TimeSpan.TicksPerDay * tier.Step),
            Unit.Month => (dt.Year * 12L + dt.Month - 1) / tier.Step,
            _ => dt.Year / (long)tier.Step,
        };
    }

    // Smallest (finest) tier whose approximate fine-tick count fits the budget. Durations
    // are approximate (month ~30.44d, year ~365.25d) — only used to pick granularity; the
    // actual ticks are exact calendar boundaries.
    private static Tier PickTier(double spanTicks)
    {
        foreach (var tier in s_tiers)
            if (spanTicks / (ApproxTicks(tier.Fine) * tier.Step) <= TargetMaxTicks)
                return tier;
        return s_tiers[s_tiers.Length - 1];
    }

    private static double ApproxTicks(Unit unit) => unit switch
    {
        Unit.Second => TimeSpan.TicksPerSecond,
        Unit.Minute => TimeSpan.TicksPerMinute,
        Unit.Hour => TimeSpan.TicksPerHour,
        Unit.Day => TimeSpan.TicksPerDay,
        Unit.Month => TimeSpan.TicksPerDay * 30.436875,
        _ => TimeSpan.TicksPerDay * 365.25,
    };

    // Axis values are DateTime ticks carried as doubles, and tick magnitudes (~3.15e18 at
    // DateTime.MaxValue) exceed double's exact-integer range (2^53) — a value at the upper
    // bound can round ABOVE MaxValue.Ticks and overflow the DateTime constructor. Clamp.
    private static DateTime FromTicksClamped(double value) =>
        value >= DateTime.MaxValue.Ticks
            ? DateTime.MaxValue
            : value <= DateTime.MinValue.Ticks
                ? DateTime.MinValue
                : new DateTime((long)value);

    private static List<double> Generate(double min, double max, Tier tier)
    {
        var minDate = FromTicksClamped(min);
        var maxDate = FromTicksClamped(max);
        var result = new List<double>();

        if (tier.Fine == Unit.Day)
        {
            // Walk month by month so day 1 (the month boundary that carries the bottom-line
            // context) always lands, regardless of step or month length.
            var month = new DateTime(minDate.Year, minDate.Month, 1);
            while (month <= maxDate && result.Count < MaxSeparators)
            {
                var days = DateTime.DaysInMonth(month.Year, month.Month);
                for (var d = 1; d <= days; d += tier.Step)
                {
                    // Drop the trailing remainder day (e.g. the 31st at step 10): it sits one
                    // step short of the month end and would collide with next month's day 1.
                    // Day 1 is always kept — it carries the month/year context.
                    if (d != 1 && d + tier.Step > days + 1) continue;

                    var dt = new DateTime(month.Year, month.Month, d);
                    if (dt >= minDate && dt <= maxDate) result.Add(dt.Ticks);
                }

                // The last month of year 9999 has no successor — a valid range reaching
                // DateTime.MaxValue must terminate cleanly, not throw out of AddMonths.
                try { month = month.AddMonths(1); }
                catch (ArgumentOutOfRangeException) { break; }
            }
            return result;
        }

        var cur = AlignFloor(minDate, tier);
        while (cur <= maxDate && result.Count < MaxSeparators)
        {
            if (cur >= minDate) result.Add(cur.Ticks);
            if (!TryAdvance(cur, tier, out cur)) break;
        }
        return result;
    }

    // Advancing past the last tick can overflow DateTime.MaxValue (e.g. a 1000-year tier
    // stepping from year 9500, or the final hours of year 9999) — the range itself is
    // valid, so generation must terminate cleanly instead of throwing.
    private static bool TryAdvance(DateTime dt, Tier tier, out DateTime next)
    {
        try
        {
            next = Advance(dt, tier);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            next = dt;
            return false;
        }
    }

    private static DateTime AlignFloor(DateTime dt, Tier tier) => tier.Fine switch
    {
        Unit.Second => new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second - dt.Second % tier.Step),
        Unit.Minute => new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute - dt.Minute % tier.Step, 0),
        Unit.Hour => new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour - dt.Hour % tier.Step, 0, 0),
        Unit.Month => new DateTime(dt.Year, 1 + (dt.Month - 1) / tier.Step * tier.Step, 1),
        Unit.Year => new DateTime(dt.Year - dt.Year % tier.Step, 1, 1),
        _ => dt.Date,
    };

    private static DateTime Advance(DateTime dt, Tier tier) => tier.Fine switch
    {
        Unit.Second => dt.AddSeconds(tier.Step),
        Unit.Minute => dt.AddMinutes(tier.Step),
        Unit.Hour => dt.AddHours(tier.Step),
        Unit.Month => dt.AddMonths(tier.Step),
        Unit.Year => dt.AddYears(tier.Step),
        _ => dt.AddDays(tier.Step),
    };

    // Two lines: fine unit on top, coarser context on the bottom. The bottom line is drawn
    // only where the fine tick opens a new coarse bucket (e.g. month == January). Tiers
    // without a coarse unit (years) stay single-line.
    private static string Format(double value, Tier tier)
    {
        var dt = FromTicksClamped(value);
        var top = FineLabel(dt, tier.Fine);

        if (tier.Coarse is null) return top;

        return IsCoarseStart(dt, tier.Fine)
            ? $"{top}\n{CoarseLabel(dt, tier.Fine)}"
            : $"{top}\n";
    }

    private static string FineLabel(DateTime dt, Unit fine) => fine switch
    {
        Unit.Second => dt.ToString("ss"),
        Unit.Minute => dt.ToString("HH:mm"),
        Unit.Hour => dt.ToString("HH:mm"),
        Unit.Day => dt.Day.ToString(),
        Unit.Month => dt.ToString("MMM"),
        _ => dt.ToString("yyyy"),
    };

    private static string CoarseLabel(DateTime dt, Unit fine) => fine switch
    {
        Unit.Second => dt.ToString("MMM d, HH:mm"),
        Unit.Minute => dt.ToString("MMM d, HH:00"),
        Unit.Hour => dt.ToString("MMM d, yyyy"),
        Unit.Day => dt.ToString("MMM yyyy"),
        _ => dt.ToString("yyyy"),
    };

    private static bool IsCoarseStart(DateTime dt, Unit fine) => fine switch
    {
        Unit.Second => dt.Second == 0,
        Unit.Minute => dt.Minute == 0,
        Unit.Hour => dt.Hour == 0,
        Unit.Day => dt.Day == 1,
        _ => dt.Month == 1,
    };
}
