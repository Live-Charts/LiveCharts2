using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using LiveChartsCore.SkiaSharpView.SKCharts;
using SkiaSharp;

namespace SnapshotTests;

[TestClass]
public sealed class ShadersAndEffectsTests
{
    [TestMethod]
    public void LinearGradient()
    {
        var values1 = new int[] { 3, 7, 2, 9, 4 };
        var values2 = new int[] { 4, 2, 8, 5, 3 };
        var columnGradient = new LinearGradientPaint(
            [new SKColor(0xFF, 0x8C, 0x94), new SKColor(0xDC, 0xED, 0xC2)],
            new SKPoint(0.5f, 0f),
            new SKPoint(0.5f, 1f)
        );
        var lineGradient = new LinearGradientPaint(
            [new SKColor(0x2D, 0x40, 0x59), new SKColor(0xFF, 0xD3, 0x60)],
            new SKPoint(0f, 0f),
            new SKPoint(1f, 1f)
        )
        {
            StrokeThickness = 10
        };
        var series = new ISeries[]
        {
            new ColumnSeries<int>
            {
                Values = values1,
                Fill = columnGradient
            },
            new LineSeries<int>
            {
                Values = values2,
                GeometrySize = 22,
                Fill = null,
                Stroke = lineGradient,
                GeometryStroke = lineGradient
            }
        };
        var chart = new SKCartesianChart
        {
            Series = series,
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(ShadersAndEffectsTests)}_{nameof(LinearGradient)}");
    }

    [TestMethod]
    public void RadialGradient()
    {
        var values1 = new int[] { 3, 7, 2, 9, 4 };
        var values2 = new int[] { 4, 2, 8, 5, 3 };

        var mariaValues = new double[] { 7 };
        var charlesValues = new double[] { 3 };

        var mariaGradient = new RadialGradientPaint(
            [new SKColor(0xB3, 0xE5, 0xFC), new SKColor(0x01, 0x57, 0x9B)],
            new SKPoint(0.5f, 0.5f)
        );
        var charlesGradient = new RadialGradientPaint(
            [new SKColor(0xFF, 0xCD, 0xD2), new SKColor(0xB7, 0x1C, 0x1C)],
            new SKPoint(0.5f, 0.5f)
        );

        var series = new ISeries[]
        {
            new PieSeries<double>
            {
                Values = mariaValues,
                Pushout = 10,
                OuterRadiusOffset = 20,
                Fill = mariaGradient
            },
            new PieSeries<double>
            {
                Values = charlesValues,
                Fill = charlesGradient
            }
        };

        var chart = new SKPieChart
        {
            Series = series,
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(ShadersAndEffectsTests)}_{nameof(RadialGradient)}");
    }

    [TestMethod]
    public void StrokeDashEffect()
    {
        var values = new int[] { 4, 2, 8, 5, 3 };

        var dashedPaint = new SolidColorPaint(new SKColor(0x64, 0x95, 0xED), 10)
        {
            StrokeCap = SKStrokeCap.Round,
            PathEffect = new DashEffect([30, 20])
        };

        var series = new ISeries[]
        {
            new LineSeries<int>
            {
                Values = values,
                LineSmoothness = 1,
                GeometrySize = 22,
                Stroke = dashedPaint,
                Fill = null
            }
        };

        var chart = new SKCartesianChart
        {
            Series = series,
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(ShadersAndEffectsTests)}_{nameof(StrokeDashEffect)}");
    }
}
