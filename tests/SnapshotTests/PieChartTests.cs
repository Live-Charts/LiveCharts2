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

    [TestMethod]
    public void GaugeInTightBounds()
    {
        // issue #2131 (re-open): with a small chart size, the default HoverPushout=20
        // on every PieSeries had each gauge subtract 40px from the available diameter
        // via PushoutBounds. That pushed the user's requested InnerRadius outside the
        // chart's available radius, so the MaxRadialColumnWidth clamp never fired and
        // the value ring broke. The geometry here is the reporter's actual layout — a
        // wide, short "card row" (240x100) — which renders the value gauge as an
        // oversized blob escaping the top of the chart; a square tight chart only
        // collapses it to a small concentric ring, a milder symptom. Gauges never pop
        // out on hover, so they no longer contribute to PushoutBounds — the ring now
        // honors InnerRadius + MaxRadialColumnWidth the same as in a roomy chart.
        var chart = new SKPieChart
        {
            Series = GaugeGenerator.BuildSolidGauge(
                new GaugeItem(
                    100,
                    series =>
                    {
                        series.InnerRadius = 38;
                        series.RelativeInnerRadius = -4;
                        series.MaxRadialColumnWidth = 5;
                    }),
                new GaugeItem(GaugeItem.Background, series =>
                {
                    series.InnerRadius = 40;
                    series.RelativeInnerRadius = 4;
                    series.OuterRadiusOffset = -20;
                    series.MaxRadialColumnWidth = 5;
                })),
            InitialRotation = 270,
            MaxAngle = 360,
            MinValue = 0,
            MaxValue = 100,
            Width = 240,
            Height = 100
        };

        chart.AssertSnapshotMatches($"{nameof(PieChartTests)}_{nameof(GaugeInTightBounds)}");
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
