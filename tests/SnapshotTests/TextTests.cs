using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using SkiaSharp;

namespace SnapshotTests;

[TestClass]
public sealed class TextTests
{
    [TestMethod]
    public void MultilineText()
    {
        var label = $"Hi this is a label with {Environment.NewLine}a long text that generates {Environment.NewLine}multi lines";

        var chart = new SKCartesianChart
        {
            Series = [
                new LineSeries<double> { Values = [1, 2, 3] }
            ],
            XAxes = [
                new Axis
                {
                    LabelsRotation = 45,
                    Labels = [
                        label,
                        label,
                        label
                    ],
                }
            ],
            YAxes = [
                new Axis
                {
                }
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(TextTests)}_{nameof(MultilineText)}");
    }

    [TestMethod]
    public void MultilineTextInTooltips()
    {
        var label = $"Hi this is a label with {Environment.NewLine}a long text that generates {Environment.NewLine}multi lines";

        var chart = new SKCartesianChart
        {
            Series = [
                new LineSeries<double> { Values = [1, 2, 3] }
            ],
            XAxes = [
                new Axis
                {
                    LabelsRotation = 45,
                    Labels = [
                        label,
                        label,
                        label
                    ],
                }
            ],
            YAxes = [
                new Axis
                {
                }
            ],
            Width = 600,
            Height = 600
        };

        chart.PointerAt(300, 300);
        chart.AssertSnapshotMatches($"{nameof(TextTests)}_{nameof(MultilineTextInTooltips)}");
    }

    [TestMethod]
    public void RenderUnshapedGlyphs()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new LineSeries<double> { Values = [1, 2, 3] }
            ],
            XAxes = [
                new Axis
                {
                    Labels = [ "王", "赵", "张" ],
                    TextSize = 50,
                }
            ],
            YAxes = [
                new Axis
                {

                }
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(TextTests)}_{nameof(RenderUnshapedGlyphs)}");
    }

    [TestMethod]
    public void RenderShapedGlyphs()
    {
        var label = "مرحبا بالعالم";
        var values = new double[] { 1, 2, 3 };

        var chart = new SKCartesianChart
        {
            Series = [
                new ColumnSeries<double> { Values = values },
            ],
            XAxes = [
                new Axis
                {
                    TextSize = 30,
                    LabelsRotation = 45,
                    Labels = [
                        label,
                        label,
                        label
                    ]
                }
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(TextTests)}_{nameof(RenderShapedGlyphs)}");
    }

    [TestMethod]
    public void RenderShapedGlyphsMultiLine()
    {
        var label =
            "هذا نص طويل" + Environment.NewLine +
            "يحتوي على عدة أسطر" + Environment.NewLine +
            "مكتوب باللغة العربية";
        var values = new double[] { 1, 2, 3 };

        var chart = new SKCartesianChart
        {
            Series = [
                new ColumnSeries<double> { Values = values },
            ],
            XAxes = [
                new Axis
                {
                    TextSize = 30,
                    LabelsRotation = 45,
                    Labels = [
                        label,
                        label,
                        label
                    ]
                }
            ],
            Width = 800,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(TextTests)}_{nameof(RenderShapedGlyphsMultiLine)}");
    }

    [TestMethod]
    public void RenderShapedGlyphsMultiLineInTooltips()
    {
        var label =
            "هذا نص طويل" + Environment.NewLine +
            "يحتوي على عدة أسطر" + Environment.NewLine +
            "مكتوب باللغة العربية";
        var values = new double[] { 1, 2, 3 };

        var chart = new SKCartesianChart
        {
            Series = [
                new ColumnSeries<double> { Values = values },
            ],
            XAxes = [
                new Axis
                {
                    TextSize = 30,
                    LabelsRotation = 45,
                    Labels = [
                        label,
                        label,
                        label
                    ]
                }
            ],
            Width = 800,
            Height = 600
        };

        chart.PointerAt(200, 100);
        chart.AssertSnapshotMatches($"{nameof(TextTests)}_{nameof(RenderShapedGlyphsMultiLineInTooltips)}");
    }

    [TestMethod]
    public void RenderRtlTextMixedWithLtrNumbers()
    {
        // issue #1229: when a label mixed LTR digits with RTL text (e.g. "60.52 درصد"
        // or "853,432,672 تومان"), the shaper used to reverse characters within the
        // numeric run, so "60.52" rendered as "25.06" and digit groups got jumbled.
        // The fix tokenizes by whitespace, shapes each token independently (preserving
        // per-run directionality), and reverses only token order for RTL display.
        var values = new double[] { 1, 2, 3 };

        var chart = new SKCartesianChart
        {
            Series = [
                new ColumnSeries<double> { Values = values },
            ],
            XAxes = [
                new Axis
                {
                    TextSize = 24,
                    Labels = [
                        "60.52 درصد",
                        "853,432,672 تومان",
                        "12.5 خرید"
                    ]
                }
            ],
            Width = 800,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(TextTests)}_{nameof(RenderRtlTextMixedWithLtrNumbers)}");
    }
}
