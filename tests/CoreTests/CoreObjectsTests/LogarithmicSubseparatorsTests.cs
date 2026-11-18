using System;
using System.Collections.Generic;
using System.Linq;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.CoreObjectsTests;

// https://github.com/Live-Charts/LiveCharts2/pull/2388
// On a logarithmic axis the minor lines of a decade mark 2, 3, ... 9, so they sit at log10(k) of the
// way from the lower power of the decade to the upper one and tighten toward the upper power, like
// log paper. The subseparators and subticks of a normal Y axis and of an inverted X axis came out
// mirrored (dense next to the lower power). This measures where every minor line of every decade is.
[TestClass]
public class LogarithmicSubseparatorsTests
{
    // log10(k) for k = 2..9: where the minor lines of a decade belong, from its lower power (0) to the upper (1)
    private static readonly double[] s_expected = [.. Enumerable.Range(2, 8).Select(k => Math.Log10(k))];

    [TestMethod]
    [DataRow(AxisOrientation.Y, false, AxisPosition.Start, DisplayName = "Y")]
    [DataRow(AxisOrientation.Y, false, AxisPosition.End, DisplayName = "Y at the end")]
    [DataRow(AxisOrientation.Y, true, AxisPosition.Start, DisplayName = "Y inverted")]
    [DataRow(AxisOrientation.Y, true, AxisPosition.End, DisplayName = "Y inverted at the end")]
    [DataRow(AxisOrientation.X, false, AxisPosition.Start, DisplayName = "X")]
    [DataRow(AxisOrientation.X, false, AxisPosition.End, DisplayName = "X at the end")]
    [DataRow(AxisOrientation.X, true, AxisPosition.Start, DisplayName = "X inverted")]
    [DataRow(AxisOrientation.X, true, AxisPosition.End, DisplayName = "X inverted at the end")]
    public void MinorLinesFollowTheLogDistribution(AxisOrientation orientation, bool inverted, AxisPosition position)
    {
        var separators = new SolidColorPaint(SKColors.Black);
        var subseparators = new SolidColorPaint(SKColors.Red);
        var subticks = new SolidColorPaint(SKColors.Blue);

        var logAxis = new LogarithmicAxis(10)
        {
            IsInverted = inverted,
            Position = position,
            MinStep = 1,
            SeparatorsPaint = separators,
            SubseparatorsPaint = subseparators,
            SubticksPaint = subticks,
            SubseparatorsCount = 9,
        };

        // values 1, 10, 100, 1000 plotted as their log10: three full decades on the log axis
        var logValues = new[] { 0d, 1, 2, 3 };
        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            Series = [new LineSeries<ObservablePoint>
            {
                Values = orientation == AxisOrientation.Y
                    ? [.. logValues.Select((v, i) => new ObservablePoint(i, v))]
                    : [.. logValues.Select((v, i) => new ObservablePoint(v, i))]
            }],
            XAxes = [orientation == AxisOrientation.X ? logAxis : new Axis()],
            YAxes = [orientation == AxisOrientation.Y ? logAxis : new Axis()],
        };

        _ = ChangingPaintTasks.DrawChart(chart);

        // the lower value of a decade is on the left (X) / at the bottom (Y), unless the axis is inverted
        var lowerAtBiggerPixel = (orientation == AxisOrientation.Y) != inverted;

        AssertDecades("subseparators", Coordinates(separators), Coordinates(subseparators), lowerAtBiggerPixel);
        AssertDecades("subticks", Coordinates(separators), Coordinates(subticks), lowerAtBiggerPixel);

        // the coordinate that varies along the axis: X for an X axis, Y for a Y axis
        List<double> Coordinates(SolidColorPaint paint) =>
            [.. paint.GetGeometries(chart.CoreCanvas)
                .OfType<BaseLineGeometry>()
                .Select(g => Math.Round(orientation == AxisOrientation.X ? g.X : g.Y, 3))
                .Distinct()
                .OrderBy(c => c)];
    }

    private static void AssertDecades(string what, List<double> majors, List<double> minors, bool lowerAtBiggerPixel)
    {
        var decades = 0;

        for (var i = 0; i + 1 < majors.Count; i++)
        {
            var a = majors[i];
            var b = majors[i + 1];
            var inside = minors.Where(m => m > a + 0.5 && m < b - 0.5).ToList();
            if (inside.Count == 0) continue;

            var lower = lowerAtBiggerPixel ? b : a;
            var fractions = inside.Select(m => Math.Abs(m - lower) / (b - a)).OrderBy(f => f).ToArray();

            Assert.AreEqual(s_expected.Length, fractions.Length, $"{what}: decade {i} has {fractions.Length} minor lines");
            for (var k = 0; k < s_expected.Length; k++)
                Assert.AreEqual(s_expected[k], fractions[k], 0.005,
                    $"{what}: decade {i}, minor line {k + 2} is at {fractions[k]:0.000} of the decade, expected log10({k + 2}) = {s_expected[k]:0.000}. All: {string.Join(" ", fractions.Select(f => f.ToString("0.00")))}");

            decades++;
        }

        Assert.IsTrue(decades >= 2, $"{what}: expected at least two full decades between major separators, found {decades}");
    }
}
