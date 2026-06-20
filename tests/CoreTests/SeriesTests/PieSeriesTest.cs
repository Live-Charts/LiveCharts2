using System;
using System.Linq;
using CoreTests.MockedObjects;
using LiveChartsCore.Drawing;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.SeriesTests;

[TestClass]
public class PieSeriesTest
{
    [TestMethod]
    public void ShouldScale()
    {
        var values = new double[] { 100, 50, 25, 25 };
        var total = values.Sum();
        var seriesCollection = values.AsPieSeries((value, series) =>
        {
            series.HoverPushout = 50;
        });

        var chart = new SKPieChart
        {
            Width = 1000,
            Height = 1000,
            Series = seriesCollection,
        };

        _ = chart.GetImage();
        // chart.SaveImage("test.png"); // use this method to see the actual tested image

        var toCompareGuys = seriesCollection.Select(sutSeries =>
        {
            var datafactory = sutSeries.DataFactory;
            var points = datafactory.Fetch(sutSeries, chart.CoreChart).ToArray();

            var unit = points.First();
            return sutSeries.ConvertToTypedChartPoint(unit);
        });

        var startAngle = 0f;
        foreach (var sutPoint in toCompareGuys)
        {
            var series = (PieSeries<double>)sutPoint.Context.Series;

            // test the angle of the slice
            Assert.IsTrue(
                Math.Abs(sutPoint.Visual.SweepAngle / 360f - sutPoint.Model / total) < 0.001);

            // test the start rotation
            Assert.IsTrue(
                Math.Abs(sutPoint.Visual.StartAngle - startAngle) < 0.001);

            // test the width and height
            Assert.IsTrue(Math.Abs(sutPoint.Visual.Width - (chart.Width - series.HoverPushout * 2)) < 0.001);
            Assert.IsTrue(Math.Abs(sutPoint.Visual.Height - (chart.Height - series.HoverPushout * 2)) < 0.001);

            // test the center
            Assert.IsTrue(Math.Abs(sutPoint.Visual.CenterX - chart.Width * 0.5f) < 0.001);
            Assert.IsTrue(Math.Abs(sutPoint.Visual.CenterY - chart.Width * 0.5f) < 0.001);

            startAngle += sutPoint.Visual.SweepAngle;
        }
    }

    [TestMethod]
    public void ShouldPlaceToolTips()
    {
        var tooltip = new SKDefaultTooltip { Easing = null };

        var chart = new SKPieChart
        {
            Width = 300,
            Height = 300,
            Tooltip = tooltip,
            TooltipPosition = TooltipPosition.Auto,
            Series = new double[] { 1, 1, 1, 1 }.AsPieSeries(),
            ExplicitDisposing = true
        };

        chart.CoreChart._isPointerIn = true;
        chart.CoreChart._isToolTipOpen = true;
        chart.CoreChart._pointerPosition = new(150 + 10, 150 + 10);

        _ = chart.GetImage();

        LvcRectangle tp;
        void UpdateTooltipRect()
        {
            var g = tooltip;
            tp = new LvcRectangle(new(g.X, g.Y), tooltip.Measure());
        }

        UpdateTooltipRect();
        Assert.IsTrue(
            tp.X - 150 > 0 &&
            tp.Y + tp.Height - 150 > 0,
            "Tool tip failed");

        chart.CoreChart._pointerPosition = new(150 - 10, 150 + 10);
        _ = chart.GetImage();
        UpdateTooltipRect();
        Assert.IsTrue(
            tp.X - 150 < 0 &&
            tp.Y + tp.Height - 150 > 0,
            "Tool tip failed");

        chart.CoreChart._pointerPosition = new(150 - 10, 150 - 10);
        _ = chart.GetImage();
        UpdateTooltipRect();
        Assert.IsTrue(
            tp.X - 150 < 0 &&
            tp.Y + tp.Height - 150 < 0,
            "Tool tip failed");

        chart.CoreChart._pointerPosition = new(150 + 10, 150 - 10);
        _ = chart.GetImage();
        UpdateTooltipRect();
        Assert.IsTrue(
            tp.X - 150 > 0 &&
            tp.Y + tp.Height - 150 < 0,
            "Tool tip failed");
    }

    [TestMethod]
    public void ShouldPlaceDataLabel()
    {
        LabelGeometry.ShowDebugLines = true;

        var vals = new[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
        var seriesCollection = vals.AsPieSeries<int, DoughnutGeometry, TestLabel>();

        var chart = new SKPieChart
        {
            Width = 500,
            Height = 500,
            DrawMargin = new Margin(50),
            TooltipPosition = TooltipPosition.Top,
            Series = seriesCollection
        };

        // TEST HIDDEN ===========================================================
        _ = chart.GetImage();
        chart.SaveImage("_pie-hidden.png");

        var points = seriesCollection.SelectMany(x =>
            x.DataFactory
                .Fetch(x, chart.CoreChart)
                .Select(x.ConvertToTypedChartPoint));

        Assert.IsTrue(seriesCollection.All(x => x.DataLabelsPosition == PolarLabelsPosition.Middle));
        Assert.IsTrue(points.All(x => x.Label is null));

        foreach (var series in seriesCollection)
            series.DataLabelsPaint = new SolidColorPaint
            {
                Color = SKColors.Black,
                SKTypeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };

        #region normal

        foreach (var series in seriesCollection) series.DataLabelsPosition = PolarLabelsPosition.ChartCenter;
        _ = chart.GetImage();
        chart.SaveImage("_pie-center.png");

        points = seriesCollection.SelectMany(x =>
            x.DataFactory
                .Fetch(x, chart.CoreChart)
                .Select(x.ConvertToTypedChartPoint));

        foreach (var p in points)
        {
            Assert.IsTrue(
                Math.Abs(p.Label.X - 250) < 0.01 &&
                Math.Abs(p.Label.Y - 250) < 0.01);
        }

        foreach (var series in seriesCollection) series.DataLabelsPosition = PolarLabelsPosition.Middle;
        _ = chart.GetImage();
        chart.SaveImage("_pie-middle.png");

        points = seriesCollection.SelectMany(x =>
            x.DataFactory
                .Fetch(x, chart.CoreChart)
                .Select(x.ConvertToTypedChartPoint));

        foreach (var p in points)
        {
            var h = Math.Sqrt(Math.Pow(p.Label.X - 250, 2) + Math.Pow(p.Label.Y - 250, 2));
            Assert.IsTrue(h < 500 * 0.5f * 0.65f);

            var a = Math.Atan2(p.Label.Y - 250, p.Label.X - 250) * 180 / Math.PI;
            if (a < 0) a += 360;
            var r = p.Visual.StartAngle + p.Visual.SweepAngle * 0.5f;

            Assert.IsTrue(Math.Abs(a - r) < 0.01);
        }

        foreach (var series in seriesCollection) series.DataLabelsPosition = PolarLabelsPosition.Start;
        _ = chart.GetImage();
        chart.SaveImage("_pie-start.png");

        points = seriesCollection.SelectMany(x =>
            x.DataFactory
                .Fetch(x, chart.CoreChart)
                .Select(x.ConvertToTypedChartPoint));

        foreach (var p in points)
        {
            var h = Math.Sqrt(Math.Pow(p.Label.X - 250, 2) + Math.Pow(p.Label.Y - 250, 2));
            Assert.IsTrue(h < 500 * 0.5f * 0.65f);

            var a = Math.Atan2(p.Label.Y - 250, p.Label.X - 250) * 180 / Math.PI;
            if (a < 0) a += 360;
            var r = p.Visual.StartAngle;

            Assert.IsTrue(Math.Abs(a - r) < 0.01);
        }

        foreach (var series in seriesCollection) series.DataLabelsPosition = PolarLabelsPosition.End;
        _ = chart.GetImage();
        chart.SaveImage("_pie-end.png");

        foreach (var p in points)
        {
            var h = Math.Sqrt(Math.Pow(p.Label.X - 250, 2) + Math.Pow(p.Label.Y - 250, 2));
            Assert.IsTrue(h < 500 * 0.5f * 0.65f);

            var a = Math.Atan2(p.Label.Y - 250, p.Label.X - 250) * 180 / Math.PI;
            if (a < 0) a += 360;
            var r = p.Visual.StartAngle + p.Visual.SweepAngle;
            if (Math.Abs(r - 360) < 0.001) r = 0;

            Assert.IsTrue(Math.Abs(a - r) < 0.01);
        }

        foreach (var series in seriesCollection) series.DataLabelsPosition = PolarLabelsPosition.Outer;
        _ = chart.GetImage();
        chart.SaveImage("_pie-outer.png");

        points = seriesCollection.SelectMany(x =>
            x.DataFactory
                .Fetch(x, chart.CoreChart)
                .Select(x.ConvertToTypedChartPoint));

        foreach (var p in points)
        {
            var h = Math.Sqrt(Math.Pow(p.Label.X - 250, 2) + Math.Pow(p.Label.Y - 250, 2));
            Assert.IsTrue(h > p.Visual.Width * 0.5f);

            var a = Math.Atan2(p.Label.Y - 250, p.Label.X - 250) * 180 / Math.PI;
            if (a < 0) a += 360;
            var r = p.Visual.StartAngle + p.Visual.SweepAngle * 0.5f;

            Assert.IsTrue(Math.Abs(a - r) < 0.01);
        }

        #endregion

        #region inner

        foreach (var series in seriesCollection) series.InnerRadius = 100;

        foreach (var series in seriesCollection) series.DataLabelsPosition = PolarLabelsPosition.ChartCenter;
        _ = chart.GetImage();
        chart.SaveImage("_pie-center-inner.png");

        points = seriesCollection.SelectMany(x =>
            x.DataFactory
                .Fetch(x, chart.CoreChart)
                .Select(x.ConvertToTypedChartPoint));

        foreach (var p in points)
        {
            Assert.IsTrue(
                Math.Abs(p.Label.X - 250) < 0.01 &&
                Math.Abs(p.Label.Y - 250) < 0.01);
        }

        foreach (var series in seriesCollection) series.DataLabelsPosition = PolarLabelsPosition.Middle;
        _ = chart.GetImage();
        chart.SaveImage("_pie-middle-inner.png");

        points = seriesCollection.SelectMany(x =>
            x.DataFactory
                .Fetch(x, chart.CoreChart)
                .Select(x.ConvertToTypedChartPoint));

        foreach (var p in points)
        {
            var h = Math.Sqrt(Math.Pow(p.Label.X - 250, 2) + Math.Pow(p.Label.Y - 250, 2));
            Assert.IsTrue(h < 500 * 0.5f * 0.65f);

            var a = Math.Atan2(p.Label.Y - 250, p.Label.X - 250) * 180 / Math.PI;
            if (a < 0) a += 360;
            var r = p.Visual.StartAngle + p.Visual.SweepAngle * 0.5f;

            Assert.IsTrue(Math.Abs(a - r) < 0.01);
        }

        foreach (var series in seriesCollection) series.DataLabelsPosition = PolarLabelsPosition.Start;
        _ = chart.GetImage();
        chart.SaveImage("_pie-start-inner.png");

        points = seriesCollection.SelectMany(x =>
            x.DataFactory
                .Fetch(x, chart.CoreChart)
                .Select(x.ConvertToTypedChartPoint));

        foreach (var p in points)
        {
            var h = Math.Sqrt(Math.Pow(p.Label.X - 250, 2) + Math.Pow(p.Label.Y - 250, 2));
            Assert.IsTrue(h < 500 * 0.5f * 0.65f);

            var a = Math.Atan2(p.Label.Y - 250, p.Label.X - 250) * 180 / Math.PI;
            if (a < 0) a += 360;
            var r = p.Visual.StartAngle;

            Assert.IsTrue(Math.Abs(a - r) < 0.01);
        }

        foreach (var series in seriesCollection) series.DataLabelsPosition = PolarLabelsPosition.End;
        _ = chart.GetImage();
        chart.SaveImage("_pie-end-inner.png");

        foreach (var p in points)
        {
            var h = Math.Sqrt(Math.Pow(p.Label.X - 250, 2) + Math.Pow(p.Label.Y - 250, 2));
            Assert.IsTrue(h < 500 * 0.5f * 0.65f);

            var a = Math.Atan2(p.Label.Y - 250, p.Label.X - 250) * 180 / Math.PI;
            if (a < 0) a += 360;
            var r = p.Visual.StartAngle + p.Visual.SweepAngle;
            if (Math.Abs(r - 360) < 0.001) r = 0;

            Assert.IsTrue(Math.Abs(a - r) < 0.01);
        }

        foreach (var series in seriesCollection) series.DataLabelsPosition = PolarLabelsPosition.Outer;
        _ = chart.GetImage();
        chart.SaveImage("_pie-outer-inner.png");

        points = seriesCollection.SelectMany(x =>
            x.DataFactory
                .Fetch(x, chart.CoreChart)
                .Select(x.ConvertToTypedChartPoint));

        foreach (var p in points)
        {
            var h = Math.Sqrt(Math.Pow(p.Label.X - 250, 2) + Math.Pow(p.Label.Y - 250, 2));
            Assert.IsTrue(h > p.Visual.Width * 0.5f);

            var a = Math.Atan2(p.Label.Y - 250, p.Label.X - 250) * 180 / Math.PI;
            if (a < 0) a += 360;
            var r = p.Visual.StartAngle + p.Visual.SweepAngle * 0.5f;

            Assert.IsTrue(Math.Abs(a - r) < 0.01);
        }

        #endregion

        #region outer

        foreach (var series in seriesCollection) series.InnerRadius = 0;
        foreach (var series in seriesCollection) series.OuterRadiusOffset = 10;

        foreach (var series in seriesCollection) series.DataLabelsPosition = PolarLabelsPosition.ChartCenter;
        _ = chart.GetImage();
        chart.SaveImage("_pie-center-outer.png");

        points = seriesCollection.SelectMany(x =>
            x.DataFactory
                .Fetch(x, chart.CoreChart)
                .Select(x.ConvertToTypedChartPoint));

        foreach (var p in points)
        {
            Assert.IsTrue(
                Math.Abs(p.Label.X - 250) < 0.01 &&
                Math.Abs(p.Label.Y - 250) < 0.01);
        }

        foreach (var series in seriesCollection) series.DataLabelsPosition = PolarLabelsPosition.Middle;
        _ = chart.GetImage();
        chart.SaveImage("_pie-middle-outer.png");

        points = seriesCollection.SelectMany(x =>
            x.DataFactory
                .Fetch(x, chart.CoreChart)
                .Select(x.ConvertToTypedChartPoint));

        foreach (var p in points)
        {
            var h = Math.Sqrt(Math.Pow(p.Label.X - 250, 2) + Math.Pow(p.Label.Y - 250, 2));
            Assert.IsTrue(h < 500 * 0.5f * 0.65f);

            var a = Math.Atan2(p.Label.Y - 250, p.Label.X - 250) * 180 / Math.PI;
            if (a < 0) a += 360;
            var r = p.Visual.StartAngle + p.Visual.SweepAngle * 0.5f;

            Assert.IsTrue(Math.Abs(a - r) < 0.01);
        }

        foreach (var series in seriesCollection) series.DataLabelsPosition = PolarLabelsPosition.Start;
        _ = chart.GetImage();
        chart.SaveImage("_pie-start-outer.png");

        points = seriesCollection.SelectMany(x =>
            x.DataFactory
                .Fetch(x, chart.CoreChart)
                .Select(x.ConvertToTypedChartPoint));

        foreach (var p in points)
        {
            var h = Math.Sqrt(Math.Pow(p.Label.X - 250, 2) + Math.Pow(p.Label.Y - 250, 2));
            Assert.IsTrue(h < 500 * 0.5f * 0.65f);

            var a = Math.Atan2(p.Label.Y - 250, p.Label.X - 250) * 180 / Math.PI;
            if (a < 0) a += 360;
            var r = p.Visual.StartAngle;

            Assert.IsTrue(Math.Abs(a - r) < 0.01);
        }

        foreach (var series in seriesCollection) series.DataLabelsPosition = PolarLabelsPosition.End;
        _ = chart.GetImage();
        chart.SaveImage("_pie-end-outer.png");

        foreach (var p in points)
        {
            var h = Math.Sqrt(Math.Pow(p.Label.X - 250, 2) + Math.Pow(p.Label.Y - 250, 2));
            Assert.IsTrue(h < 500 * 0.5f * 0.65f);

            var a = Math.Atan2(p.Label.Y - 250, p.Label.X - 250) * 180 / Math.PI;
            if (a < 0) a += 360;
            var r = p.Visual.StartAngle + p.Visual.SweepAngle;
            if (Math.Abs(r - 360) < 0.001) r = 0;

            Assert.IsTrue(Math.Abs(a - r) < 0.01);
        }

        foreach (var series in seriesCollection) series.DataLabelsPosition = PolarLabelsPosition.Outer;
        _ = chart.GetImage();
        chart.SaveImage("_pie-outer-outer.png");

        points = seriesCollection.SelectMany(x =>
            x.DataFactory
                .Fetch(x, chart.CoreChart)
                .Select(x.ConvertToTypedChartPoint));

        foreach (var p in points)
        {
            var h = Math.Sqrt(Math.Pow(p.Label.X - 250, 2) + Math.Pow(p.Label.Y - 250, 2));
            Assert.IsTrue(h > p.Visual.Width * 0.5f);

            var a = Math.Atan2(p.Label.Y - 250, p.Label.X - 250) * 180 / Math.PI;
            if (a < 0) a += 360;
            var r = p.Visual.StartAngle + p.Visual.SweepAngle * 0.5f;

            Assert.IsTrue(Math.Abs(a - r) < 0.01);
        }

        #endregion

        #region inner and outer

        foreach (var series in seriesCollection) series.InnerRadius = 50;
        foreach (var series in seriesCollection) series.OuterRadiusOffset = 10;

        foreach (var series in seriesCollection) series.DataLabelsPosition = PolarLabelsPosition.ChartCenter;
        _ = chart.GetImage();
        chart.SaveImage("_pie-center-inner-outer.png");

        points = seriesCollection.SelectMany(x =>
            x.DataFactory
                .Fetch(x, chart.CoreChart)
                .Select(x.ConvertToTypedChartPoint));

        foreach (var p in points)
        {
            Assert.IsTrue(
                Math.Abs(p.Label.X - 250) < 0.01 &&
                Math.Abs(p.Label.Y - 250) < 0.01);
        }

        foreach (var series in seriesCollection) series.DataLabelsPosition = PolarLabelsPosition.Middle;
        _ = chart.GetImage();
        chart.SaveImage("_pie-middle-inner-outer.png");

        points = seriesCollection.SelectMany(x =>
            x.DataFactory
                .Fetch(x, chart.CoreChart)
                .Select(x.ConvertToTypedChartPoint));

        foreach (var p in points)
        {
            var h = Math.Sqrt(Math.Pow(p.Label.X - 250, 2) + Math.Pow(p.Label.Y - 250, 2));
            Assert.IsTrue(h < 500 * 0.5f * 0.65f);

            var a = Math.Atan2(p.Label.Y - 250, p.Label.X - 250) * 180 / Math.PI;
            if (a < 0) a += 360;
            var r = p.Visual.StartAngle + p.Visual.SweepAngle * 0.5f;

            Assert.IsTrue(Math.Abs(a - r) < 0.01);
        }

        foreach (var series in seriesCollection) series.DataLabelsPosition = PolarLabelsPosition.Start;
        _ = chart.GetImage();
        chart.SaveImage("_pie-start-inner-outer.png");

        points = seriesCollection.SelectMany(x =>
            x.DataFactory
                .Fetch(x, chart.CoreChart)
                .Select(x.ConvertToTypedChartPoint));

        foreach (var p in points)
        {
            var h = Math.Sqrt(Math.Pow(p.Label.X - 250, 2) + Math.Pow(p.Label.Y - 250, 2));
            Assert.IsTrue(h < 500 * 0.5f * 0.65f);

            var a = Math.Atan2(p.Label.Y - 250, p.Label.X - 250) * 180 / Math.PI;
            if (a < 0) a += 360;
            var r = p.Visual.StartAngle;

            Assert.IsTrue(Math.Abs(a - r) < 0.01);
        }

        foreach (var series in seriesCollection) series.DataLabelsPosition = PolarLabelsPosition.End;
        _ = chart.GetImage();
        chart.SaveImage("_pie-end-inner-outer.png");

        foreach (var p in points)
        {
            var h = Math.Sqrt(Math.Pow(p.Label.X - 250, 2) + Math.Pow(p.Label.Y - 250, 2));
            Assert.IsTrue(h < 500 * 0.5f * 0.65f);

            var a = Math.Atan2(p.Label.Y - 250, p.Label.X - 250) * 180 / Math.PI;
            if (a < 0) a += 360;
            var r = p.Visual.StartAngle + p.Visual.SweepAngle;
            if (Math.Abs(r - 360) < 0.001) r = 0;

            Assert.IsTrue(Math.Abs(a - r) < 0.01);
        }

        foreach (var series in seriesCollection) series.DataLabelsPosition = PolarLabelsPosition.Outer;
        _ = chart.GetImage();
        chart.SaveImage("_pie-outer-inner-outer.png");

        points = seriesCollection.SelectMany(x =>
            x.DataFactory
                .Fetch(x, chart.CoreChart)
                .Select(x.ConvertToTypedChartPoint));

        foreach (var p in points)
        {
            var h = Math.Sqrt(Math.Pow(p.Label.X - 250, 2) + Math.Pow(p.Label.Y - 250, 2));
            Assert.IsTrue(h > p.Visual.Width * 0.5f);

            var a = Math.Atan2(p.Label.Y - 250, p.Label.X - 250) * 180 / Math.PI;
            if (a < 0) a += 360;
            var r = p.Visual.StartAngle + p.Visual.SweepAngle * 0.5f;

            Assert.IsTrue(Math.Abs(a - r) < 0.01);
        }

        #endregion
    }
}
