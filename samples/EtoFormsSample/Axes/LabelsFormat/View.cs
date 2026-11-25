using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace EtoFormsSample.Axes.LabelsFormat;

public class View : Panel
{
    private readonly CartesianChart cartesianChart;

    public View()
    {
        var values1 = new double[] { 426, 583, 104 };
        var values2 = new double[] { 200, 558, 458 };
        var labels = new string[] { "Sergio", "Lando", "Lewis" };
        static string Labeler(double value) => value.ToString("C2");

        var series = new ISeries[]
        {
            new ColumnSeries<double> { Values = values1 },
            new ColumnSeries<double> { Values = values2, Fill = null }
        };

        var xAxis = new Axis
        {
            Name = "Salesman/woman",
            Labels = labels
        };

        var yAxis = new Axis
        {
            Name = "Sales",
            NamePadding = new LiveChartsCore.Drawing.Padding(0, 15),
            Labeler = Labeler,
            LabelsPaint = new SolidColorPaint(SKColors.Blue)
            {
                SKTypeface = SKTypeface.FromFamilyName(
                    "Times New Roman",
                    SKFontStyleWeight.ExtraBold,
                    SKFontStyleWidth.Normal,
                    SKFontStyleSlant.Italic)
            }
        };

        cartesianChart = new CartesianChart
        {
            Series = series,
            XAxes = [xAxis],
            YAxes = [yAxis]
        };

        Content = cartesianChart;
    }
}
