using System.Linq;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.VisualElements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.OtherTests;

[TestClass]
public class EventsTests
{
    [TestMethod]
    public void CartesianBasicCase()
    {
        var chart = new SKCartesianChart
        {
            Width = 500,
            Height = 500,
            Series = new[]
            {
                new ScatterSeries<double>
                {
                    Values = new double[] { 0, 1, 2 },
                    DataPadding = new LvcPoint(0, 0)
                }
            },
            XAxes = new[] { new Axis { IsVisible = false, } },
            YAxes = new[] { new Axis { IsVisible = false, } },
            VisualElements = new VisualElement[]
            {
                new LabelVisual
                {
                    X = 1,
                    Y = 1,
                    Text = "Hello world",
                    TextSize = 20,
                    Paint = new SolidColorPaint(SKColors.Red),
                    LocationUnit = MeasureUnit.ChartValues
                },
                new GeometryVisual<RectangleGeometry>
                {
                    X = 1,
                    Y = 1,
                    Width = 0.5,
                    Height = 0.5,
                    Fill = new SolidColorPaint(SKColors.Blue),
                    LocationUnit = MeasureUnit.ChartValues,
                    SizeUnit = MeasureUnit.ChartValues
                }
            }
        };

        _ = chart.GetImage();

        // Test points.
        // Charts use the Series.FindHitPoints method to check if the mouse is over a point.
        var strategy = chart.Series.GetFindingStrategy();
        var s = chart.Series
            .SelectMany(x => x.FindHitPoints(chart.CoreChart, new LvcPoint(251, 251), strategy, FindPointFor.HoverEvent))
            .ToArray();
        Assert.IsTrue(s.Length == 1);

        // Test visual elements.
        // Charts use the VisualElement.IsHitBy method to check if the mouse is over a visual element.
        var v = chart.VisualElements
            .Cast<VisualElement>()
            .SelectMany(x => x.IsHitBy(chart.CoreChart, new LvcPoint(251, 251)))
            .ToArray();
        Assert.IsTrue(v.Length == 2);
    }
}
