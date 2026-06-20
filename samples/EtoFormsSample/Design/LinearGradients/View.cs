using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace EtoFormsSample.Design.LinearGradients;

public class View : Panel
{
    public View()
    {
        var values1 = new int[] { 3, 7, 2, 9, 4 };
        var values2 = new int[] { 4, 2, 8, 5, 3 };

        var columnGradient = new LinearGradientPaint(
            new[] { new SKColor(0xFF, 0x8C, 0x94), new SKColor(0xDC, 0xED, 0xC2) },
            new SKPoint(0.5f, 0f),
            new SKPoint(0.5f, 1f)
        );

        // Use separate paint instances for Stroke and GeometryStroke. Sharing a
        // single instance would tie both to the same z-index, which forces the
        // line to render above the marker fill and slice through it.
        LinearGradientPaint MakeLineGradient() => new(
            new[] { new SKColor(0x2D, 0x40, 0x59), new SKColor(0xFF, 0xD3, 0x60) },
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
                Name = "John",
                Values = values1,
                Fill = columnGradient
            },
            new LineSeries<int>
            {
                Name = "Charles",
                Values = values2,
                GeometrySize = 22,
                Fill = null,
                Stroke = MakeLineGradient(),
                GeometryStroke = MakeLineGradient()
            }
        };

        var cartesianChart = new CartesianChart
        {
            Series = series,
            LegendPosition = LiveChartsCore.Measure.LegendPosition.Right,
        };

        Content = cartesianChart;
    }
}
