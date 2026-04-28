using LiveChartsCore.Defaults;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView.SKCharts;

namespace SnapshotTests;

[TestClass]
public sealed class PieChartTests
{
    [TestMethod]
    public void Basic()
    {
        var chart = new SKPieChart
        {
            Series = new[] { 2, 4, 1, 4, 3 }.AsPieSeries(),
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(PieChartTests)}_{nameof(Basic)}");
    }

    [TestMethod]
    public void OuterRadius()
    {
        var outer = 0;
        var data = new[] { 6, 5, 4, 3 };

        var seriesCollection = data.AsPieSeries((value, series) =>
        {
            series.OuterRadiusOffset = outer;
            outer += 50;
        });

        var chart = new SKPieChart
        {
            Series = seriesCollection,
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(PieChartTests)}_{nameof(OuterRadius)}");
    }

    [TestMethod]
    public void InnerRadius()
    {
        var seriesCollection = new[] { 2, 4, 1, 4, 3 }
            .AsPieSeries((value, series) =>
            {
                series.MaxRadialColumnWidth = 60;
            });

        var chart = new SKPieChart
        {
            Series = seriesCollection,
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(PieChartTests)}_{nameof(InnerRadius)}");
    }

    [TestMethod]
    public void Pushout()
    {
        var seriesCollection = new[] { 6, 5, 4, 3, 2 }.AsPieSeries((value, series) =>
            {
                if (value != 6) return;
                series.Pushout = 30;
            });

        var chart = new SKPieChart
        {
            Series = seriesCollection,
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(PieChartTests)}_{nameof(Pushout)}");
    }

    [TestMethod]
    public void Gauge()
    {
        var chart = new SKPieChart
        {
            Series = GaugeGenerator.BuildSolidGauge(
                new GaugeItem(
                    30,          // the gauge value
                    series =>    // the series style
                    {
                        series.MaxRadialColumnWidth = 50;
                        series.DataLabelsSize = 50;
                    })),
            InitialRotation = -90,
            MinValue = 0,
            MaxValue = 100,
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(PieChartTests)}_{nameof(Gauge)}");
    }

    [TestMethod]
    public void GaugeValueExceedsMaxValue()
    {
        // issue #2131: a gauge value greater than the chart's MaxValue used to
        // produce a sweep larger than 360deg, which rendered as a broken arc.
        // The series and background should now render as if the value matched MaxValue.
        var chart = new SKPieChart
        {
            Series = GaugeGenerator.BuildSolidGauge(
                new GaugeItem(
                    150,
                    series =>
                    {
                        series.MaxRadialColumnWidth = 50;
                        series.DataLabelsSize = 50;
                    })),
            InitialRotation = -90,
            MinValue = 0,
            MaxValue = 100,
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(PieChartTests)}_{nameof(GaugeValueExceedsMaxValue)}");
    }

    [TestMethod]
    public void GaugeValueExceedsRangeWithMinValue()
    {
        // issue #2131 (follow-up): when MinValue is non-zero the angle math uses
        // (MaxValue - MinValue) as the range, so the clamp must use the same range
        // or a value above the effective range still produces a sweep > 360deg.
        var chart = new SKPieChart
        {
            Series = GaugeGenerator.BuildSolidGauge(
                new GaugeItem(
                    150,
                    series =>
                    {
                        series.MaxRadialColumnWidth = 50;
                        series.DataLabelsSize = 50;
                    })),
            InitialRotation = -90,
            MinValue = 30,
            MaxValue = 100,
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(PieChartTests)}_{nameof(GaugeValueExceedsRangeWithMinValue)}");
    }

    [TestMethod]
    public void GaugeMultiple()
    {
        var chart = new SKPieChart
        {
            Series = GaugeGenerator.BuildSolidGauge(
                new GaugeItem(30, series => SetStyle("Vanessa", series)),
                new GaugeItem(50, series => SetStyle("Charles", series)),
                new GaugeItem(70, series => SetStyle("Ana", series)),
                new GaugeItem(GaugeItem.Background, series =>
                {
                    series.InnerRadius = 20;
                })),
            InitialRotation = 45,
            MaxAngle = 270,
            MinValue = 0,
            MaxValue = 100,
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(PieChartTests)}_{nameof(GaugeMultiple)}");
    }

    public static void SetStyle(string name, PieSeries<ObservableValue> series)
    {
        series.Name = name;
        series.DataLabelsPosition = PolarLabelsPosition.Start;
        series.DataLabelsFormatter =
                point => $"{point.Coordinate.PrimaryValue} {point.Context.Series.Name}";
        series.InnerRadius = 20;
        series.RelativeOuterRadius = 8;
        series.RelativeInnerRadius = 8;
    }
}
