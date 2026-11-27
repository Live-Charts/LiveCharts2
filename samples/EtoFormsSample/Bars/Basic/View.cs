using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace EtoFormsSample.Bars.Basic;

public class View : Panel
{
    public View()
    {
        var maryValues = new double[] { 2, 5, 4 };
        var anaValues = new double[] { 3, 1, 6 };
        var labels = new string[] { "Category 1", "Category 2", "Category 3" };

        var series = new ISeries[]
        {
            new ColumnSeries<double> { Name = "Mary", Values = maryValues },
            new ColumnSeries<double> { Name = "Ana", Values = anaValues }
        };

        var xAxis = new Axis
        {
            Labels = labels,
            LabelsRotation = 0,
            SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200)),
            SeparatorsAtCenter = false,
            TicksPaint = new SolidColorPaint(new SKColor(35, 35, 35)),
            TicksAtCenter = true,
            MinStep = 1,
            ForceStepToMin = true
        };

        var cartesianChart = new CartesianChart
        {
            Series = series,
            XAxes = [xAxis]
        };

        Content = cartesianChart;
    }
}
