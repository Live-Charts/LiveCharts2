using LiveChartsCore.Painting;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.OtherTests;

// CoreLineSeries was assigning the wrong PaintConstants to its geometry
// paints — SeriesGeometryStrokeZIndexOffset for GeometryFill, and
// SeriesDataLabelsZIndexOffset for GeometryStroke. CoreStepLineSeries and
// CorePolarLineSeries already used the correctly-named constants.
//
// In addition to being misleading, this collided GeometryStroke with
// DataLabelsPaint (both at offset 0.5), which is order-dependent.
[TestClass]
public class LineSeriesGeometryZIndexTests
{
    [TestMethod]
    public void LineSeries_GeometryPaintZIndices_Match_NamedConstants()
    {
        var geometryFill = new SolidColorPaint(SKColors.White);
        var geometryStroke = new SolidColorPaint(SKColors.Black, 4);

        var chart = new SKCartesianChart
        {
            Width = 200,
            Height = 200,
            Series = [
                new LineSeries<double>
                {
                    Values = [1, 2, 3, 4],
                    GeometryFill = geometryFill,
                    GeometryStroke = geometryStroke
                }
            ]
        };

        using var image = chart.GetImage();

        // Default series have ZIndex == null, so actualZIndex == 0 in
        // CoreLineSeries and the paint ZIndex equals the offset itself.
        Assert.AreEqual(
            PaintConstants.SeriesGeometryFillZIndexOffset,
            geometryFill.ZIndex,
            "GeometryFill.ZIndex must use SeriesGeometryFillZIndexOffset.");
        Assert.AreEqual(
            PaintConstants.SeriesGeometryStrokeZIndexOffset,
            geometryStroke.ZIndex,
            "GeometryStroke.ZIndex must use SeriesGeometryStrokeZIndexOffset.");

        // Stroke must render above Fill so the marker outline isn't hidden.
        Assert.IsTrue(geometryStroke.ZIndex > geometryFill.ZIndex);
    }
}
