using CoreTests.MockedObjects;
using LiveChartsCore.Drawing;
using LiveChartsCore.Drawing.Segments;
using LiveChartsCore.Kernel;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using SkiaSharp;

namespace CoreTests.SeriesTests;

[TestClass]
public class LineSeriesTest
{
    [TestMethod]
    public void ShouldScale()
    {
        var sutSeries = new LineSeries<double>
        {
            Values = [1, 2, 4, 8, 16, 32, 64, 128, 256, 512],
            GeometrySize = 10
        };

        var chart = new SKCartesianChart
        {
            Width = 1000,
            Height = 1000,
            Series = [sutSeries],
            XAxes = [new Axis { MinLimit = -1, MaxLimit = 10 }],
            YAxes = [new Axis { MinLimit = 0, MaxLimit = 512 }]
        };

        _ = chart.GetImage();
        // chart.SaveImage("test.png"); // use this method to see the actual tested image

        var datafactory = sutSeries.DataFactory;
        var points = datafactory.Fetch(sutSeries, chart.CoreChart).ToArray();

        var unit = points.First(x => x.Coordinate.PrimaryValue == 1);
        var typedUnit = sutSeries.ConvertToTypedChartPoint(unit);

        var toCompareGuys = points.Where(x => x != unit).Select(sutSeries.ConvertToTypedChartPoint);

        // ensure the unit has valid dimensions
        Assert.IsTrue(typedUnit.Visual.Width == 10 && typedUnit.Visual.Height == 10);

        var previous = typedUnit;
        float? previousX = null;
        float? previousXArea = null;

        foreach (var sutPoint in toCompareGuys)
        {
            // test x
            var currentDeltaX = previous.Visual.X - sutPoint.Visual.X;

            var previousBezier = ((CubicSegmentVisualPoint)previous.Context.AdditionalVisuals)?.Segment;
            var sutBezier = ((CubicSegmentVisualPoint)sutPoint.Context.AdditionalVisuals).Segment;

            var currentDeltaAreaX = previousBezier.Xj - sutBezier.Xj;

            Assert.IsTrue(
                previousX is null
                ||
                Math.Abs(previousX.Value - currentDeltaX) < 0.001);
            Assert.IsTrue(
                previousXArea is null
                ||
                Math.Abs(previousXArea.Value - currentDeltaX) < 0.001);

            // test y
            var p = 1f - sutPoint.Coordinate.PrimaryValue / 512f;
            Assert.IsTrue(
                Math.Abs(p * chart.CoreChart.DrawMarginSize.Height - sutPoint.Visual.Y + chart.CoreChart.DrawMarginLocation.Y) < 0.001);
            Assert.IsTrue(
                Math.Abs(p * chart.CoreChart.DrawMarginSize.Height - sutBezier.Yj + chart.CoreChart.DrawMarginLocation.Y) < 0.001);

            previousX = previous.Visual.X - sutPoint.Visual.X;
            previousXArea = previousBezier.Xj - sutBezier.Xj;
            previous = sutPoint;
        }
    }

    [TestMethod]
    public void ShouldPlaceToolTips()
    {
        var sutSeries = new LineSeries<double>
        {
            GeometrySize = 0,
            Values = [1, 2, 3, 4, 5],
            DataPadding = new LvcPoint(0, 0)
        };

        var tooltip = new SKDefaultTooltip { Easing = null };

        var chart = new SKCartesianChart
        {
            Width = 300,
            Height = 300,
            Tooltip = tooltip,
            TooltipPosition = TooltipPosition.Top,
            Series = [sutSeries],
            XAxes = [new Axis { IsVisible = false }],
            YAxes = [new Axis { IsVisible = false }],
            ExplicitDisposing = true
        };

        chart.CoreChart._isPointerIn = true;
        chart.CoreChart._isToolTipOpen = true;
        chart.CoreChart._pointerPosition = new(150, 150);

        chart.TooltipPosition = TooltipPosition.Top;
        _ = chart.GetImage();

        LvcRectangle tp;
        void UpdateTooltipRect()
        {
            var g = tooltip;
            tp = new LvcRectangle(new(g.X, g.Y), tooltip.Measure());
        }

        UpdateTooltipRect();
        Assert.IsTrue(
            Math.Abs(tp.X + tp.Width * 0.5f - 150) < 0.1 &&
            Math.Abs(tp.Y - (150 - tp.Height)) < 0.1,
            "Tool tip on top failed");

        chart.TooltipPosition = TooltipPosition.Bottom;
        _ = chart.GetImage();
        UpdateTooltipRect();
        Assert.IsTrue(
            Math.Abs(tp.X + tp.Width * 0.5f - 150) < 0.1 &&
            Math.Abs(tp.Y - 150) < 0.1,
            "Tool tip on bottom failed");

        chart.TooltipPosition = TooltipPosition.Left;
        _ = chart.GetImage();
        UpdateTooltipRect();
        Assert.IsTrue(
            Math.Abs(tp.X - (150 - tp.Width)) < 0.1 &&
            Math.Abs(tp.Y + tp.Height * 0.5f - 150) < 0.1,
            "Tool tip on left failed");

        chart.TooltipPosition = TooltipPosition.Right;
        _ = chart.GetImage();
        UpdateTooltipRect();
        Assert.IsTrue(
            Math.Abs(tp.X - 150) < 0.1 &&
            Math.Abs(tp.Y + tp.Height * 0.5f - 150) < 0.1,
            "Tool tip on right failed");

        chart.TooltipPosition = TooltipPosition.Center;
        _ = chart.GetImage();
        UpdateTooltipRect();
        Assert.IsTrue(
            Math.Abs(tp.X + tp.Width * 0.5f - 150) < 0.1 &&
            Math.Abs(tp.Y + tp.Height * 0.5f - 150) < 0.1,
            "Tool tip on center failed");

        chart.TooltipPosition = TooltipPosition.Auto;
        _ = chart.GetImage();
        UpdateTooltipRect();
        Assert.IsTrue(
            Math.Abs(tp.X + tp.Width * 0.5f - 150) < 0.1 &&
            Math.Abs(tp.Y - (150 - tp.Height)) < 0.1 &&
            chart.CoreChart.AutoToolTipsInfo.ToolTipPlacement == PopUpPlacement.Top,
            "Tool tip on top failed [AUTO]");

        sutSeries.Values = [-1, -2, -3, -4, -5];
        _ = chart.GetImage();
        UpdateTooltipRect();
        Assert.IsTrue(
            Math.Abs(tp.X + tp.Width * 0.5f - 150) < 0.1 &&
            Math.Abs(tp.Y - 150) < 0.1 &&
            chart.CoreChart.AutoToolTipsInfo.ToolTipPlacement == PopUpPlacement.Bottom,
            "Tool tip on bottom failed [AUTO]");

        sutSeries.Values = [1, 2, 3, 4, 5];
        chart.CoreChart._pointerPosition = new(299, 150);
        _ = chart.GetImage();
        UpdateTooltipRect();
        Assert.IsTrue(
            // that 2... it seems that the lineseries.DataPadding takes more space than expected
            Math.Abs(tp.X - (300 - tp.Width)) < 2 &&
            //Math.Abs(tp.Y - -tp.Height * 0.5f) < 2 &&
            chart.CoreChart.AutoToolTipsInfo.ToolTipPlacement == PopUpPlacement.Left,
            "Tool tip on left failed [AUTO]");

        chart.CoreChart._pointerPosition = new(1, 150);
        _ = chart.GetImage();
        UpdateTooltipRect();
        Assert.IsTrue(
            Math.Abs(tp.X) < 2 &&
            //Math.Abs(tp.Y - (300 - tp.Height * 0.5f)) < 2 &&
            chart.CoreChart.AutoToolTipsInfo.ToolTipPlacement == PopUpPlacement.Right,
            "Tool tip on left failed [AUTO]");
    }

    [TestMethod]
    public void ShouldPlaceDataLabel()
    {
        var gs = 5f;
        var sutSeries = new LineSeries<double, RectangleGeometry, TestLabel>
        {
            Values = [-10, -5, -1, 0, 1, 5, 10],
            DataPadding = new LvcPoint(0, 0),
            GeometrySize = gs * 2,
        };

        var chart = new SKCartesianChart
        {
            Width = 500,
            Height = 500,
            DrawMargin = new Margin(100),
            DrawMarginFrame = new DrawMarginFrame { Stroke = new SolidColorPaint(SKColors.Yellow, 2) },
            TooltipPosition = TooltipPosition.Top,
            Series = [sutSeries],
            XAxes = [new Axis { IsVisible = false }],
            YAxes = [new Axis { IsVisible = false }]
        };

        var datafactory = sutSeries.DataFactory;

        // TEST HIDDEN ===========================================================
        _ = chart.GetImage();

        var points = datafactory
            .Fetch(sutSeries, chart.CoreChart)
            .Select(sutSeries.ConvertToTypedChartPoint);

        Assert.IsTrue(sutSeries.DataLabelsPosition == DataLabelsPosition.End);
        Assert.IsTrue(points.All(x => x.Label is null));

        sutSeries.DataLabelsPaint = new SolidColorPaint
        {
            Color = SKColors.Black,
            SKTypeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
        };

        // TEST TOP ===============================================================
        sutSeries.DataLabelsPosition = DataLabelsPosition.Top;
        _ = chart.GetImage();

        points = datafactory
            .Fetch(sutSeries, chart.CoreChart)
            .Select(sutSeries.ConvertToTypedChartPoint);

        foreach (var p in points)
        {
            var v = p.Visual;
            var l = p.Label;

            l.Paint = sutSeries.DataLabelsPaint;
            var ls = l.Measure();

            Assert.IsTrue(
                Math.Abs(v.X + v.Width * 0.5f - l.X - gs) < 0.01 &&    // x is centered
                Math.Abs(v.Y - (l.Y + ls.Height * 0.5 + gs)) < 0.01);  // y is top
        }

        // TEST BOTTOM ===========================================================
        sutSeries.DataLabelsPosition = DataLabelsPosition.Bottom;

        _ = chart.GetImage();

        points = datafactory
            .Fetch(sutSeries, chart.CoreChart)
            .Select(sutSeries.ConvertToTypedChartPoint);

        foreach (var p in points)
        {
            var v = p.Visual;
            var l = p.Label;

            l.Paint = sutSeries.DataLabelsPaint;
            var ls = l.Measure();

            Assert.IsTrue(
                Math.Abs(v.X + v.Width * 0.5f - l.X - gs) < 0.01 &&              // x is centered
                Math.Abs(v.Y + v.Height - (l.Y - ls.Height * 0.5 + gs)) < 0.01); // y is bottom
        }

        // TEST RIGHT ============================================================
        sutSeries.DataLabelsPosition = DataLabelsPosition.Right;

        _ = chart.GetImage();

        points = datafactory
            .Fetch(sutSeries, chart.CoreChart)
            .Select(sutSeries.ConvertToTypedChartPoint);

        foreach (var p in points)
        {
            var v = p.Visual;
            var l = p.Label;

            l.Paint = sutSeries.DataLabelsPaint;
            var ls = l.Measure();

            Assert.IsTrue(
                Math.Abs(v.X + v.Width - (l.X - ls.Width * 0.5 + gs)) < 0.01 &&  // x is right
                Math.Abs(v.Y + v.Height * 0.5 - l.Y - gs) < 0.01);               // y is centered
        }

        // TEST LEFT =============================================================
        sutSeries.DataLabelsPosition = DataLabelsPosition.Left;

        _ = chart.GetImage();

        points = datafactory
            .Fetch(sutSeries, chart.CoreChart)
            .Select(sutSeries.ConvertToTypedChartPoint);

        foreach (var p in points)
        {
            var v = p.Visual;
            var l = p.Label;

            l.Paint = sutSeries.DataLabelsPaint;
            var ls = l.Measure();

            Assert.IsTrue(
                Math.Abs(v.X - (l.X + ls.Width * 0.5f + gs)) < 0.01 &&   // x is left
                Math.Abs(v.Y + v.Height * 0.5f - l.Y - gs) < 0.01);      // y is centered
        }

        // TEST MIDDLE ===========================================================
        sutSeries.DataLabelsPosition = DataLabelsPosition.Middle;

        _ = chart.GetImage();

        points = datafactory
            .Fetch(sutSeries, chart.CoreChart)
            .Select(sutSeries.ConvertToTypedChartPoint);

        foreach (var p in points)
        {
            var v = p.Visual;
            var l = p.Label;

            l.Paint = sutSeries.DataLabelsPaint;
            var ls = l.Measure();

            Assert.IsTrue(
                Math.Abs(v.X + v.Width * 0.5f - l.X - gs) < 0.01 &&      // x is centered
                Math.Abs(v.Y + v.Height * 0.5f - l.Y - gs) < 0.01);      // y is centered
        }

        // TEST START ===========================================================
        sutSeries.DataLabelsPosition = DataLabelsPosition.Start;

        _ = chart.GetImage();

        points = datafactory
            .Fetch(sutSeries, chart.CoreChart)
            .Select(sutSeries.ConvertToTypedChartPoint);

        foreach (var p in points)
        {
            var v = p.Visual;
            var l = p.Label;

            l.Paint = sutSeries.DataLabelsPaint;
            var ls = l.Measure();

            if (p.Model <= 0)
            {
                // it should be placed using the top position
                Assert.IsTrue(
                    Math.Abs(v.X + v.Width * 0.5f - l.X - gs) < 0.01 &&    // x is centered
                    Math.Abs(v.Y - (l.Y + ls.Height * 0.5 + gs)) < 0.01);  // y is top
            }
            else
            {
                // it should be placed using the bottom position
                Assert.IsTrue(
                    Math.Abs(v.X + v.Width * 0.5f - l.X - gs) < 0.01 &&              // x is centered
                    Math.Abs(v.Y + v.Height - (l.Y - ls.Height * 0.5 + gs)) < 0.01); // y is bottom
            }
        }

        // TEST END ===========================================================
        sutSeries.DataLabelsPosition = DataLabelsPosition.End;

        _ = chart.GetImage();

        points = datafactory
            .Fetch(sutSeries, chart.CoreChart)
            .Select(sutSeries.ConvertToTypedChartPoint);

        foreach (var p in points)
        {
            var v = p.Visual;
            var l = p.Label;

            l.Paint = sutSeries.DataLabelsPaint;
            var ls = l.Measure();

            if (p.Model <= 0)
            {
                // it should be placed using the bottom position
                Assert.IsTrue(
                    Math.Abs(v.X + v.Width * 0.5f - l.X - gs) < 0.01 &&              // x is centered
                    Math.Abs(v.Y + v.Height - (l.Y - ls.Height * 0.5 + gs)) < 0.01); // y is bottom
            }
            else
            {
                // it should be placed using the top position
                Assert.IsTrue(
                    Math.Abs(v.X + v.Width * 0.5f - l.X - gs) < 0.01 &&    // x is centered
                    Math.Abs(v.Y - (l.Y + ls.Height * 0.5 + gs)) < 0.01);  // y is top
            }
        }

        // FINALLY IF LABELS ARE NULL, IT SHOULD REMOVE THE CURRENT LABELS.
        var previousPaint = sutSeries.DataLabelsPaint;
        sutSeries.DataLabelsPaint = null;
        _ = chart.GetImage();

        Assert.IsTrue(!chart.CoreCanvas.ContainsPaintTask(previousPaint));
    }

    [TestMethod]
    public void ShouldScaleBeziersOnAddRemoveOrInsert()
    {
        var values = new List<double> { 1, 2, 3, 4, 5 };

        var series = new LineSeries<double>
        {
            Values = values,
            GeometrySize = 10
        };

        var chart = new SKCartesianChart
        {
            Width = 1000,
            Height = 1000,
            Series = [series]
        };

        void AssertIsStraightLine(ChartPoint[] points)
        {
            Assert.IsTrue(points.Length > 0);

            CubicBezierSegment? previous = null;
            var slope = 0f;

            foreach (var p in points)
            {
                var tp = series.ConvertToTypedChartPoint(p);
                var segment = (CubicSegmentVisualPoint?)tp.Context.AdditionalVisuals;

                Assert.IsNotNull(segment);

                if (previous is null)
                {
                    previous = segment?.Segment;
                    continue;
                }

                if (slope == 0f)
                {
                    var dx = segment!.Segment.Xj - previous.Xj;
                    var dy = segment.Segment.Yj - previous.Yj;
                    slope = dx / dy;
                }

                var currentSlope = (segment!.Segment.Xj - previous.Xj) / (segment.Segment.Yj - previous.Yj);

                Assert.IsTrue(Math.Abs(slope - currentSlope) < 0.1);
                previous = segment.Segment;
            }
        }

        ChartPoint[] points;
        int segments;
        _ = chart.GetImage();

        points = [.. series.DataFactory.Fetch(series, chart.CoreChart)];
        segments = series._fillPathHelperDictionary.Sum(x => x.Value.Sum(y => y.Commands.Count));
        AssertIsStraightLine(points);
        Assert.IsTrue(segments == 5);

        // if a the first point is removed, it should still be a straight line
        values.RemoveAt(0);
        _ = chart.GetImage();

        points = [.. series.DataFactory.Fetch(series, chart.CoreChart)];
        segments = series._fillPathHelperDictionary.Sum(x => x.Value.Sum(y => y.Commands.Count));
        AssertIsStraightLine(points);
        Assert.IsTrue(segments == 4);

        // if we insert a point at the start that keeps the straight line, it should be a straight line
        values.Insert(0, 1);
        _ = chart.GetImage();

        points = [.. series.DataFactory.Fetch(series, chart.CoreChart)];
        segments = series._fillPathHelperDictionary.Sum(x => x.Value.Sum(y => y.Commands.Count));
        AssertIsStraightLine(points);
        Assert.IsTrue(segments == 5);

        // if the last point is removed, it should still be a straight line
        values.RemoveAt(values.Count - 1);
        _ = chart.GetImage();

        points = [.. series.DataFactory.Fetch(series, chart.CoreChart)];
        segments = series._fillPathHelperDictionary.Sum(x => x.Value.Sum(y => y.Commands.Count));
        AssertIsStraightLine(points);
        Assert.IsTrue(segments == 4);

        // if we add a point that keeps the straight line, it should be a straight line
        values.Add(5);
        _ = chart.GetImage();

        points = [.. series.DataFactory.Fetch(series, chart.CoreChart)];
        segments = series._fillPathHelperDictionary.Sum(x => x.Value.Sum(y => y.Commands.Count));
        AssertIsStraightLine(points);
        Assert.IsTrue(segments == 5);

        // if we insert a point in the middle that keeps the straight line, it should be a straight line
        values.RemoveAt(1);
        _ = chart.GetImage();
        // at this point the values are [1,3,4,5] it is not a straight line
        //AssertIsStraightLine([.. series.DataFactory.Fetch(series, chart.CoreChart)]);

        values.Insert(1, 2);
        _ = chart.GetImage();

        points = [.. series.DataFactory.Fetch(series, chart.CoreChart)];
        segments = series._fillPathHelperDictionary.Sum(x => x.Value.Sum(y => y.Commands.Count));
        AssertIsStraightLine(points);
        Assert.IsTrue(segments == 5);
    }

    [TestMethod]
    public void NullGapsShouldNotLeaveOrphanSegments()
    {
        // Regression test for https://github.com/Live-Charts/LiveCharts2/issues/2132
        // When a gap (null value) is introduced, the path for the segment BEFORE the gap
        // must not retain orphan segments that now belong to the segment AFTER the gap.
        var values = new List<double?> { 1, 2, 3, 4, 5 };

        var series = new LineSeries<double?> { Values = values };
        var chart = new SKCartesianChart
        {
            Width = 1000,
            Height = 1000,
            Series = [series]
        };

        _ = chart.GetImage();

        // Initially 5 non-null values: expect 1 path segment with 5 bezier commands
        var pathContainer = series._fillPathHelperDictionary.Values.First();
        Assert.AreEqual(1, pathContainer.Count, "should have 1 path");
        Assert.AreEqual(5, pathContainer[0].Commands.Count, "path should have 5 commands");

        // Introduce a gap at index 2: [1, 2, null, 4, 5]
        // Expect 2 path segments: path[0]=[p0,p1] (2 commands), path[1]=[p3,p4] (2 commands)
        values[2] = null;
        _ = chart.GetImage();

        pathContainer = series._fillPathHelperDictionary.Values.First();
        Assert.AreEqual(2, pathContainer.Count, "should have 2 paths after gap");
        Assert.AreEqual(2, pathContainer[0].Commands.Count,
            "path before the gap should only have 2 commands (not orphan segments from after the gap)");
        Assert.AreEqual(2, pathContainer[1].Commands.Count,
            "path after the gap should have 2 commands");

        // Remove the gap: [1, 2, 3, 4, 5] → back to 1 segment with 5 commands
        values[2] = 3;
        _ = chart.GetImage();

        pathContainer = series._fillPathHelperDictionary.Values.First();
        Assert.AreEqual(1, pathContainer.Count, "should have 1 path after gap is removed");
        Assert.AreEqual(5, pathContainer[0].Commands.Count,
            "path should have all 5 commands after gap is removed");
    }
}
