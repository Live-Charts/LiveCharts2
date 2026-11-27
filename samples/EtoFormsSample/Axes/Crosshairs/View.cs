using System;
using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace EtoFormsSample.Axes.Crosshairs;

public class View : Panel
{
    private readonly CartesianChart cartesianChart;

    public View()
    {
        // Sample data and label formatter
        var values = new double[] { 200, 558, 458, 249, 457, 339, 587 };
        static string labelFormatter(double value) => value.ToString("N2");

        var series = new ISeries[]
        {
            new LineSeries<double> { Values = values }
        };

        var crosshairColor = new SKColor(0xFF, 0x8C, 0x00); // #FF8C00
        var crosshairLabelsColor = new SKColor(0x8B, 0x00, 0x00); // #8B0000
        var crosshairBackground = new LiveChartsCore.Drawing.LvcColor(0xFF, 0x8C, 0x00);

        var xAxis = new Axis
        {
            Labeler = labelFormatter,
            CrosshairPaint = new SolidColorPaint(crosshairColor),
            CrosshairLabelsPaint = new SolidColorPaint(crosshairLabelsColor),
            CrosshairLabelsBackground = crosshairBackground
        };

        var yAxis = new Axis
        {
            Labeler = labelFormatter,
            CrosshairPaint = new SolidColorPaint(crosshairColor),
            CrosshairLabelsPaint = new SolidColorPaint(crosshairLabelsColor),
            CrosshairLabelsBackground = crosshairBackground,
            CrosshairSnapEnabled = true
        };

        cartesianChart = new CartesianChart
        {
            Series = series,
            XAxes = [xAxis],
            YAxes = [yAxis],
        };

        Content = new DynamicLayout(cartesianChart);
    }
}
