using System.Collections.ObjectModel;
using System;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;
using LiveChartsCore.Defaults;
using System.Diagnostics;
using LiveChartsCore.Measure;
using CoreTests.CoreObjectsTests;

namespace CoreTests.OtherTests;

[TestClass]
public class AxisTesting
{
    [TestMethod]
    public void BasicCase()
    {
        var x = new Axis();
        var y = new Axis();

        var chart = new SKCartesianChart
        {
            Width = 1000,
            Height = 1000,
            Series =
            [
                new LineSeries<double>
                {
                    Values = [1, 2, 3]
                }
            ],
            XAxes = [x],
            YAxes = [y]
        };

        _ = chart.GetImage();

        // ensure the separators are created
        Assert.IsTrue(x.activeSeparators[chart.CoreChart].Values.Count > 0);
        Assert.IsTrue(y.activeSeparators[chart.CoreChart].Values.Count > 0);
    }

    [TestMethod]
    public void RejectUnnecessaryCases()
    {
        // The axis class throws when ~10,000 separators are drawn in the same axis (only in DEBUG mode).

        var chart = new SKCartesianChart
        {
            Width = 1000,
            Height = 1000,
            Series =
            [
                new LineSeries<double>
                {
                    Values = [1, 2, 3]
                },
            ],
            XAxes = [new Axis { MinLimit = 0, MaxLimit = 100000, ForceStepToMin = true, MinStep = 1, LabelsPaint = new SolidColorPaint(SKColors.Red) }]
        };

        bool succeed;
        try
        {
            _ = chart.GetImage();
            succeed = true;
        }
        catch
        {
            succeed = false;
        }

        Assert.IsTrue(!succeed);
    }

    [TestMethod]
    public void AutoLimits()
    {
        // based on https://github.com/beto-rodriguez/LiveCharts2/issues/755
        // when the range in the axis is zero, there is a change that the for instruction that
        // draws the separators never ends, this is because the step is calculated as 0
        // this test ensures the step is never 0.

        var x = new Axis { MinLimit = 10, MaxLimit = 10, LabelsPaint = new SolidColorPaint(SKColors.Red) };

        var chart = new SKCartesianChart
        {
            Width = 1000,
            Height = 1000,
            Series =
            [
                new LineSeries<double>
                {
                    Values = [1, 2, 3]
                }
            ],
            XAxes = [x]
        };

        var sw = new Stopwatch();
        sw.Start();

        _ = chart.GetImage();
        Assert.IsTrue(x.activeSeparators[chart.CoreChart].Values.Count > 0 && sw.ElapsedMilliseconds < 1000);
    }

    [TestMethod]
    public void DateTimeWithInvalidUnitWidth()
    {
        // based on https://github.com/beto-rodriguez/LiveCharts2/issues/763

        var chart = new SKCartesianChart
        {
            Width = 1000,
            Height = 1000,
            Series =
            [
                new ColumnSeries<DateTimePoint>
                {
                    Values = new ObservableCollection<DateTimePoint>
                    {
                        new(new DateTime(2021, 1, 1), 3)
                    }
                }
            ]
        };

        var sw = new Stopwatch();
        sw.Start();
        _ = chart.GetImage();
        Assert.IsTrue(sw.ElapsedMilliseconds < 1000);
    }

    [TestMethod]
    public void DoublePrecisionPossibleError()
    {
        // I have seen cases (not able to reproduce yet) where the calculated step in the axis is smaller than the double type precision.
        // this results in an infinite loop when drawing the separators.
        // I am not completely sure if that issue is already fixed, also not able to reproduce it.

        var chart = new SKCartesianChart
        {
            Width = 1000,
            Height = 1000,
            Series =
            [
                new LineSeries<double>
                {
                    Values = [1, 2, 3]
                }
            ],
            XAxes = [new Axis { MinLimit = 0, MaxLimit = 1e100, LabelsPaint = new SolidColorPaint(SKColors.Red) }]
        };

        var sw = new Stopwatch();
        sw.Start();
        _ = chart.GetImage();
        Assert.IsTrue(sw.ElapsedMilliseconds < 1000);
    }

    [TestMethod]
    public void InvertedAxis()
    {
        var x1 = new Axis { MaxLimit = 10, MinLimit = 0 };
        var y1 = new Axis { MaxLimit = 10, MinLimit = 0 };
        var chart1 = new SKCartesianChart
        {
            Width = 1000,
            Height = 1000,
            Series = [new LineSeries<double>([1, 2, 3])],
            XAxes = [x1],
            YAxes = [y1]
        };

        var x2 = new Axis { MinLimit = 0, MaxLimit = 10, IsInverted = true };
        var y2 = new Axis { MinLimit = 0, MaxLimit = 10, IsInverted = true };
        var chart2 = new SKCartesianChart
        {
            Width = 1000,
            Height = 1000,
            Series = [new LineSeries<double>([1, 2, 3])],
            XAxes = [x2],
            YAxes = [y2]
        };

        _ = ChangingPaintTasks.DrawChart(chart1);
        _ = ChangingPaintTasks.DrawChart(chart2);

        Assert.IsTrue(x1._size.Width > 0 && x2._size.Height > 0);
        Assert.IsTrue(x1._size == x2._size);
        Assert.IsTrue(x1.activeSeparators.Count == x2.activeSeparators.Count);

        Assert.IsTrue(y1._size.Width > 0 && y2._size.Height > 0);
        Assert.IsTrue(y1._size == y2._size);
        Assert.IsTrue(y1.activeSeparators.Count == y2.activeSeparators.Count);
    }

    [TestMethod]
    public void LimitsValidation()
    {
        // When an axis has not enough data, it must decide the limits by itself.
        // this test ensures that this decision is made correctly.

        // when there is no data in the axis, the limits are initialized to
        // double.MaxValue or double.MinValue depending on the axis direction.
        const double unset = double.MaxValue;
        const double minDefault = 0;
        const double maxDefault = 10;

        // ===============================================================
        // CASE 1
        // when both not set, limits are 0 and 10, just a default value.
        // ===============================================================
        var max = unset;
        var min = unset;
        var step = 0d; // the step is 0 by default, meaning that the library will decide it.

        AxisLimit.ValidateLimits(ref min, ref max, step);
        Assert.IsTrue(min == minDefault && max == maxDefault);

        // ===============================================================
        // CASE 2
        // when the step is defined, we need to consider it.
        // ===============================================================
        max = unset;
        min = unset;
        step = 0.1;

        AxisLimit.ValidateLimits(ref min, ref max, step);
        Assert.IsTrue(min == minDefault * step && max == maxDefault * step);

        // ==============================================================
        // CASE 3
        // when the min is set, but the max is not, we need to calculate the max
        // ===============================================================
        min = 5;
        max = unset;
        step = 0d;

        AxisLimit.ValidateLimits(ref min, ref max, step);
        Assert.IsTrue(min == 5 && max == 5 + (maxDefault - minDefault));

        // ==============================================================
        // CASE 4
        // when the max is set, but the min is not, we need to calculate the min
        // ===============================================================
        min = unset;
        max = 5;
        step = 0d;

        AxisLimit.ValidateLimits(ref min, ref max, step);
        Assert.IsTrue(min == 5 - (maxDefault - minDefault) && max == 5);

        // ==============================================================
        // CASE 5
        // when both are set, we do nothing.
        // ===============================================================
        min = 5;
        max = 10;
        step = 0d;

        AxisLimit.ValidateLimits(ref min, ref max, step);
        Assert.IsTrue(min == 5 && max == 10);

        // ==============================================================
        // CASE 6
        // when both are set, but the step is defined, we do nothing.
        // ===============================================================
        min = 5;
        max = 10;
        step = 0.1;

        AxisLimit.ValidateLimits(ref min, ref max, step);
        Assert.IsTrue(min == 5 && max == 10);

        // ==============================================================
        // CASE 7
        // when min is set, max is not, step defined, we calculate the max
        // ===============================================================
        min = 5;
        max = unset;
        step = 0.1;

        AxisLimit.ValidateLimits(ref min, ref max, step);
        Assert.IsTrue(min == 5 && max == 5 + (maxDefault - minDefault) * step);

        // ==============================================================
        // CASE 8
        // when max is set, min is not, step defined, we calculate the min
        // ===============================================================
        min = unset;
        max = 5;
        step = 0.1;

        AxisLimit.ValidateLimits(ref min, ref max, step);
        Assert.IsTrue(min == 5 - (maxDefault - minDefault) * step && max == 5);

        // ==============================================================
        // CASE 9
        // ensure double.MinValue is also treated as unset.
        // ===============================================================
        min = double.MinValue;
        max = double.MinValue;
        step = 0d;

        AxisLimit.ValidateLimits(ref min, ref max, step);
        Assert.IsTrue(min == minDefault && max == maxDefault);
    }
}
