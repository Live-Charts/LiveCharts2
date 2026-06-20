using System.Globalization;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using SkiaSharp;

namespace SnapshotTests;

// End-to-end pixel pins for the date-axis grouping (DateTimeAxis.GroupTimeUnits — adaptive
// two-line labels: fine unit on top, coarse context at each coarse boundary) and the
// alternating (zebra) axis bands (ICartesianAxis.AlternatingBandsPaint — every other
// separator gap filled behind the series, composing with the grouping). The CoreTests
// suites cover the algorithms; these cover the drawn result.
[TestClass]
public sealed class AxisBandsAndGroupingTests
{
    private CultureInfo _originalCulture = null!;

    // The grouping formats month/time labels with the current culture; pin one so the
    // baselines match on any machine.
    [TestInitialize]
    public void SetInvariantCulture()
    {
        _originalCulture = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
    }

    [TestCleanup]
    public void RestoreCulture() => Thread.CurrentThread.CurrentCulture = _originalCulture;

    private static SKCartesianChart MakeDateChart(DateTime from, DateTime to, bool groupTimeUnits, bool bands = false)
    {
        var days = (to - from).TotalDays;
        var values = Enumerable.Range(0, 200)
            .Select(i => new DateTimePoint(
                from.AddDays(days * i / 199d),
                Math.Sin(i * 0.15) * 40 + 60))
            .ToArray();

        return new SKCartesianChart
        {
            Width = 600,
            Height = 400,
            Background = SKColors.White,
            Series =
            [
                new LineSeries<DateTimePoint>
                {
                    Values = values,
                    Stroke = new SolidColorPaint(new SKColor(33, 150, 243), 2),
                    Fill = null,
                    GeometrySize = 0,
                }
            ],
            XAxes =
            [
                new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("MM-dd", CultureInfo.InvariantCulture))
                {
                    GroupTimeUnits = groupTimeUnits,
                    AlternatingBandsPaint = bands ? new SolidColorPaint(new SKColor(0, 0, 0, 16)) : null,
                }
            ],
            YAxes = [new Axis { MinLimit = 0, MaxLimit = 110, IsVisible = false }],
        };
    }

    [TestMethod]
    public void TwoYears_MonthsWithYearContext() =>
        // A two-year span lands on a month tier: month names on the fine line, the year on
        // the context line at each January and at the first visible tick.
        MakeDateChart(new DateTime(2020, 1, 1), new DateTime(2021, 12, 31), groupTimeUnits: true)
            .AssertSnapshotMatches($"{nameof(AxisBandsAndGroupingTests)}_TwoYears");

    [TestMethod]
    public void TwoDays_HoursWithDateContext() =>
        // A two-day span lands on an hour tier: clock times on the fine line, the date on
        // the context line at each midnight and at the first visible tick.
        MakeDateChart(new DateTime(2020, 6, 1), new DateTime(2020, 6, 3), groupTimeUnits: true)
            .AssertSnapshotMatches($"{nameof(AxisBandsAndGroupingTests)}_TwoDays");

    [TestMethod]
    public void GroupingOff_RendersDifferently()
    {
        // Same chart, flag off → the axis' own labeler draws; the pixels must differ from
        // the grouped render (pins the built-in opt-in end-to-end).
        var grouped = MakeDateChart(new DateTime(2020, 1, 1), new DateTime(2021, 12, 31), groupTimeUnits: true);
        var plain = MakeDateChart(new DateTime(2020, 1, 1), new DateTime(2021, 12, 31), groupTimeUnits: false);

        using var groupedImage = grouped.GetImage();
        using var plainImage = plain.GetImage();
        using var groupedCpu = SKImage.FromBitmap(SKBitmap.FromImage(groupedImage));
        using var plainCpu = SKImage.FromBitmap(SKBitmap.FromImage(plainImage));

        var comparison = Extensions.Compare(
            groupedCpu, plainCpu, perChannelTolerance: 2, maxDifferentPixelsRatio: 0.001);

        Assert.IsFalse(
            comparison.IsSuccessful,
            "grouped and ungrouped renders must differ — identical pixels mean the opt-in never engaged");
    }

    [TestMethod]
    public void NumericYAxis_Zebra()
    {
        // Zebra bands on a plain numeric Y axis: a draw-margin-wide stripe per step.
        var yAxis = new Axis
        {
            MinLimit = 0,
            MaxLimit = 100,
            AlternatingBandsPaint = new SolidColorPaint(new SKColor(0, 0, 0, 16)),
        };

        new SKCartesianChart
        {
            Width = 600,
            Height = 400,
            Background = SKColors.White,
            Series =
            [
                new LineSeries<double>
                {
                    Values = [.. Enumerable.Range(0, 50).Select(i => Math.Sin(i * 0.25) * 40 + 50)],
                    Stroke = new SolidColorPaint(new SKColor(33, 150, 243), 2),
                    Fill = null,
                    GeometrySize = 0,
                }
            ],
            XAxes = [new Axis { MinLimit = 0, MaxLimit = 49, IsVisible = false }],
            YAxes = [yAxis],
        }.AssertSnapshotMatches($"{nameof(AxisBandsAndGroupingTests)}_NumericYZebra");
    }

    [TestMethod]
    public void GroupedDateTimeXAxis_BandPerTimeUnit() =>
        // Bands composing with the grouping: a stripe per grouped time unit, re-tiering
        // with the zoom.
        MakeDateChart(new DateTime(2020, 1, 1), new DateTime(2021, 12, 31), groupTimeUnits: true, bands: true)
            .AssertSnapshotMatches($"{nameof(AxisBandsAndGroupingTests)}_GroupedXZebra");
}
