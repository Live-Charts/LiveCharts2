using System.Collections.Generic;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.VisualElements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;
using LiveChartsCore.Kernel;

namespace CoreTests.OtherTests;

[TestClass]
public class VisualElementsTests
{
    [TestMethod]
    public void Dispose()
    {
        var suts = new List<IChartElement>
        {
            new StackPanel<RectangleGeometry>(),
            new RelativePanel<RectangleGeometry>(),
            new TableLayout<RectangleGeometry>(),
            new GeometryVisual<RectangleGeometry>(),
            new VariableGeometryVisual(new RectangleGeometry()),
            new NeedleVisual
            {
                Fill = new SolidColorPaint(SKColors.Red)
            },
            new AngularTicksVisual
            {
                Stroke = new SolidColorPaint(SKColors.Black),
                LabelsPaint = new SolidColorPaint(SKColors.Blue)
            }
        };

        var chart = new SKPieChart
        {
            Width = 1000,
            Height = 1000,
            MaxValue = 100
        };

        void Draw()
        {
            var coreChart = chart.CoreChart;

            chart.CoreCanvas.DisableAnimations = true;
            coreChart.IsLoaded = true;
            coreChart._isFirstDraw = true;
            coreChart.Measure();

            using var surface = SKSurface.Create(new SKImageInfo(chart.Width, chart.Height));
            using var canvas = surface.Canvas;

            chart.CoreCanvas.DrawFrame(
                new SkiaSharpDrawingContext(
                    chart.CoreCanvas,
                    canvas,
                    SKColors.White));
        }

        Draw();

        // get the count of geometries and paint tasks without the visuals
        var g = chart.CoreCanvas.CountGeometries();
        var p = chart.CoreCanvas.CountPaintTasks();

        // add trhe visuals and ensure that the visuals were drawn
        chart.VisualElements = suts;
        Draw();
        Assert.IsTrue(
            chart.CoreCanvas.CountGeometries() > g &&
            chart.CoreCanvas.CountPaintTasks() > p);

        // clear the visuals and ensure that all the geometries and paints were removed
        chart.VisualElements = new List<IChartElement>();
        Draw();
        Assert.IsTrue(
            chart.CoreCanvas.CountGeometries() == g &&
            chart.CoreCanvas.CountPaintTasks() == p);
    }

    [TestMethod]
    public void LineVisual_InChartExercisesMeasureAndInvalidate()
    {
        // LineVisual<TGeometry> has no non-generic SkiaSharp counterpart and is missing
        // from the Dispose() test above; add it here so OnInvalidated + Measure run.
        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            Series = [
                new LineSeries<double> { Values = [1d, 2d, 3d] }
            ],
            VisualElements = new List<IChartElement>
            {
                new LineVisual<LineGeometry>
                {
                    X = 10,
                    Y = 10,
                    Width = 100,
                    Height = 60,
                    Stroke = new SolidColorPaint(SKColors.Red, 2),
                    Fill = new SolidColorPaint(SKColors.Blue),
                    LocationUnit = MeasureUnit.Pixels,
                    SizeUnit = MeasureUnit.Pixels
                }
            }
        };

        using var image = chart.GetImage();
        Assert.IsNotNull(image);
    }

    [TestMethod]
    public void VisualElement_ZIndexPropagatesToPaints_RendersAboveSeries()
    {
        // Regression for #1856: VisualElement paints used to default to ZIndex=0
        // while a default-ZIndex ColumnSeries puts its Fill at SeriesId+0.1, so
        // visuals were drawn behind the series. Setting VisualElement.ZIndex must
        // push every paint produced by the visual to that z-index.
        var visual = new GeometryVisual<RectangleGeometry>
        {
            X = 10,
            Y = 10,
            Width = 50,
            Height = 50,
            Fill = new SolidColorPaint(SKColors.Blue),
            Stroke = new SolidColorPaint(SKColors.Red),
            Label = "x",
            LabelPaint = new SolidColorPaint(SKColors.Black),
            LabelSize = 12,
            ZIndex = 1000,
            LocationUnit = MeasureUnit.Pixels,
            SizeUnit = MeasureUnit.Pixels
        };

        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            Series = [
                new ColumnSeries<double> { Values = [1d, 2d, 3d] }
            ],
            VisualElements = [visual]
        };

        using var image = chart.GetImage();

        Assert.AreEqual(1000d, visual.Fill!.ZIndex);
        Assert.AreEqual(1000d, visual.Stroke!.ZIndex);
        Assert.AreEqual(1000d, visual.LabelPaint!.ZIndex);
    }

#if !NET462 // covariant returns on TestRectangleVisual.DrawnElement need .NET 5+
    [TestMethod]
    public void Visual_ZIndexPropagatesToDrawnTask_RendersAboveSeries()
    {
        // Same regression as above (#1856) but for the Visual base class, used
        // by the canonical docs samples (RectangleVisual et al.).
        var visual = new TestRectangleVisual { ZIndex = 1000 };

        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            Series = [
                new ColumnSeries<double> { Values = [1d, 2d, 3d] }
            ],
            VisualElements = [visual]
        };

        using var image = chart.GetImage();

        var drawnTask = visual.GetPaintTasksForTest()[0];
        Assert.IsNotNull(drawnTask);
        Assert.AreEqual(1000d, drawnTask!.ZIndex);
    }

    private sealed class TestRectangleVisual : Visual
    {
        protected internal override RectangleGeometry DrawnElement { get; } =
            new RectangleGeometry { Fill = new SolidColorPaint(SKColors.Red) };

        protected override void Measure(LiveChartsCore.Chart chart)
        {
            DrawnElement.X = 10;
            DrawnElement.Y = 10;
            DrawnElement.Width = 40;
            DrawnElement.Height = 40;
        }

        public LiveChartsCore.Painting.Paint?[] GetPaintTasksForTest() => GetPaintTasks();
    }
#endif

    [TestMethod]
    public void GeometryVisual_WithLabelInChartExercisesLabelPaintBranch()
    {
        // Existing VisualElementsTests.Dispose uses GeometryVisual without a Label/LabelPaint,
        // leaving the label branch in OnInvalidated uncovered. Exercise it here.
        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            Series = [
                new LineSeries<double> { Values = [1d, 2d, 3d] }
            ],
            VisualElements = new List<IChartElement>
            {
                new GeometryVisual<RectangleGeometry>
                {
                    X = 10, Y = 10, Width = 50, Height = 50,
                    Fill = new SolidColorPaint(SKColors.Blue),
                    Label = "hello",
                    LabelPaint = new SolidColorPaint(SKColors.Red),
                    LabelSize = 12,
                    LocationUnit = MeasureUnit.Pixels,
                    SizeUnit = MeasureUnit.Pixels
                }
            }
        };

        using var image = chart.GetImage();
        Assert.IsNotNull(image);
    }
}
