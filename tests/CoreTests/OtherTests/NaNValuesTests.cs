using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.OtherTests;

// https://github.com/Live-Charts/LiveCharts2/issues/2340
// A null value is "no value": the point is empty and a line breaks there. A NaN value was drawn as is,
// and a single NaN made the whole line path invalid, so nothing of the line was drawn. A NaN value now
// means the same as null, for every point type that accepts null and for plain values.
[TestClass]
public class NaNValuesTests
{
    private static readonly double[] s_values = [2, 5, 4, 3, 6, 5, 4, 3, 6, 5, 4, 3, 6, 5, 4, 3, 6];

    [TestMethod]
    public void Coordinate_WithANaNXOrY_IsEmpty()
    {
        Assert.IsTrue(new Coordinate(0, double.NaN).IsEmpty, "NaN Y");
        Assert.IsTrue(new Coordinate(double.NaN, 1).IsEmpty, "NaN X");
        Assert.IsFalse(new Coordinate(0, 1).IsEmpty, "a regular coordinate is not empty");
    }

    // For every point type that accepts null in a field, a NaN in that field gives the same coordinate.
    [TestMethod]
    public void PointTypes_TreatANaNValueLikeNull()
    {
        var cases = new List<(string Field, Func<double?, IChartEntity> Create)>
        {
            ("ObservableValue.Value", v => new ObservableValue(v)),
            ("DateTimePoint.Value", v => new DateTimePoint(new DateTime(2026, 1, 1), v)),
            ("TimeSpanPoint.Value", v => new TimeSpanPoint(TimeSpan.FromSeconds(1), v)),
            ("ObservablePoint.X", v => new ObservablePoint(v, 1)),
            ("ObservablePoint.Y", v => new ObservablePoint(1, v)),
            ("ErrorPoint.X", v => new ErrorPoint(1, 1, 0, 0) { X = v }),
            ("ErrorPoint.Y", v => new ErrorPoint(1, 1, 0, 0) { Y = v }),
            ("ErrorValue.Y", v => new ErrorValue(1, 0) { Y = v }),
            ("ErrorDateTimePoint.Y", v => new ErrorDateTimePoint(new DateTime(2026, 1, 1), 1, 0, 0) { Y = v }),
            ("ObservablePolarPoint.Angle", v => new ObservablePolarPoint(v, 1)),
            ("ObservablePolarPoint.Radius", v => new ObservablePolarPoint(1, v)),
            ("WeightedPoint.X", v => new WeightedPoint(v, 1, 1)),
            ("WeightedPoint.Y", v => new WeightedPoint(1, v, 1)),
            ("WeightedPoint.Weight", v => new WeightedPoint(1, 1, v)),
        };

        var failures = new List<string>();
        foreach (var (field, create) in cases)
        {
            var withNull = create(null).Coordinate;
            var withNaN = create(double.NaN).Coordinate;

            if (withNaN.IsEmpty != withNull.IsEmpty ||
                (!withNull.IsEmpty && !SameValues(withNull, withNaN)))
                failures.Add($"{field}: with null {Describe(withNull)}, with NaN {Describe(withNaN)}");
        }

        Assert.AreEqual(0, failures.Count, string.Join(Environment.NewLine, failures));
    }

    // The reported case: a value of an existing point changes to NaN.
    [TestMethod]
    public void Line_ValueChangedToNaN_BreaksLikeNull()
    {
        var withNaN = LinePixelsAfterChangingTheMiddleValue(double.NaN);
        var withNull = LinePixelsAfterChangingTheMiddleValue(null);

        Assert.IsTrue(withNull > 0, "the line should be drawn when a value is null");
        Assert.AreEqual(withNull, withNaN, "a NaN value should break the line like a null value");
    }

    [TestMethod]
    public void Line_StartingWithANaNValue_IsDrawnWithAGap()
    {
        var values = new ObservableCollection<ObservableValue>(s_values.Select(v => new ObservableValue(v)));
        values[values.Count / 2].Value = double.NaN;
        var withNaN = RedPixels(LineChart(values));

        values[values.Count / 2].Value = null;
        var withNull = RedPixels(LineChart(values));

        Assert.IsTrue(withNull > 0);
        Assert.AreEqual(withNull, withNaN);
    }

    [TestMethod]
    public void Line_OfPlainDoublesWithANaN_IsDrawnWithAGap()
    {
        var values = s_values.ToArray();
        values[values.Length / 2] = double.NaN;

        var chart = new SKCartesianChart
        {
            Width = 600,
            Height = 300,
            Background = SKColors.White,
            Series = [new LineSeries<double>
            {
                Values = values,
                Fill = null,
                GeometryFill = null,
                GeometryStroke = null,
                Stroke = new SolidColorPaint(SKColors.Red, 4)
            }],
        };

        Assert.IsTrue(RedPixels(chart) > 1000, "the line should be drawn, with a gap at the NaN value");
    }

    private static int LinePixelsAfterChangingTheMiddleValue(double? value)
    {
        var values = new ObservableCollection<ObservableValue>(s_values.Select(v => new ObservableValue(v)));
        var chart = LineChart(values);
        _ = RedPixels(chart);

        values[values.Count / 2].Value = value;
        return RedPixels(chart);
    }

    // ExplicitDisposing keeps the chart loaded between images, like a chart in a window, so a value
    // changed between images updates the existing line.
    private static SKCartesianChart LineChart(ObservableCollection<ObservableValue> values) => new()
    {
        ExplicitDisposing = true,
        Width = 600,
        Height = 300,
        Background = SKColors.White,
        Series = [new LineSeries<ObservableValue>
        {
            Values = values,
            Fill = null,
            GeometryFill = null,
            GeometryStroke = null,
            Stroke = new SolidColorPaint(SKColors.Red, 4)
        }],
    };

    private static int RedPixels(SKCartesianChart chart)
    {
        using var image = chart.GetImage();
        using var bitmap = SKBitmap.FromImage(image);
        var count = 0;
        for (var y = 0; y < bitmap.Height; y++)
            for (var x = 0; x < bitmap.Width; x++)
            {
                var c = bitmap.GetPixel(x, y);
                if (c.Red > 180 && c.Green < 90 && c.Blue < 90) count++;
            }
        return count;
    }

    private static bool SameValues(Coordinate a, Coordinate b) =>
        a.PrimaryValue.Equals(b.PrimaryValue) && a.SecondaryValue.Equals(b.SecondaryValue) &&
        a.TertiaryValue.Equals(b.TertiaryValue) && a.QuaternaryValue.Equals(b.QuaternaryValue) &&
        a.QuinaryValue.Equals(b.QuinaryValue) && a.SenaryValue.Equals(b.SenaryValue);

    private static string Describe(Coordinate c) => c.IsEmpty
        ? "empty"
        : $"({c.PrimaryValue}, {c.SecondaryValue}, {c.TertiaryValue})";
}
