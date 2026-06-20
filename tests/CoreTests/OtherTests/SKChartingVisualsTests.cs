using System.Linq;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Geo;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.OtherTests;

[TestClass]
public class SKChartingVisualsTests
{
    [TestMethod]
    public void SKHeatLegend_IsDrawnWhenAttachedToCartesianChartWithHeatSeries()
    {
        // Exercises SKHeatLegend.Draw / Measure / GetLayout / Initialize through a
        // real chart render. The legend was 0%-covered before these tests.
        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            Series = [
                new HeatSeries<WeightedPoint>
                {
                    Values = [
                        new(0, 0, 10), new(0, 1, 20),
                        new(1, 0, 30), new(1, 1, 40)
                    ],
                    HeatMap = [
                        SKColor.Parse("#FFF176").AsLvcColor(),
                        SKColor.Parse("#0000FF").AsLvcColor()
                    ]
                }
            ],
            LegendPosition = LegendPosition.Right,
            Legend = new SKHeatLegend()
        };

        using var image = chart.GetImage();
        Assert.IsNotNull(image);
    }

    [TestMethod]
    public void SKHeatLegend_HorizontalPositionExercisesBottomBranch()
    {
        // When LegendPosition is Top/Bottom, GetLayout takes the horizontal branch.
        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            Series = [
                new HeatSeries<WeightedPoint>
                {
                    Values = [new(0, 0, 5), new(1, 1, 15)],
                    HeatMap = [
                        SKColor.Parse("#000000").AsLvcColor(),
                        SKColor.Parse("#FFFFFF").AsLvcColor()
                    ]
                }
            ],
            LegendPosition = LegendPosition.Bottom,
            Legend = new SKHeatLegend { BadgeWidth = 25 }
        };

        using var image = chart.GetImage();
        Assert.IsNotNull(image);
    }

    [TestMethod]
    public void SKDefaultGeoTooltip_ShowThenHideRunsTheFullLifecycle()
    {
        // Render a geo map first so the underlying GeoMapChart and its theme/view
        // are fully wired up. Then invoke the tooltip directly.
        var chart = new SKGeoMap
        {
            Width = 400,
            Height = 400,
            Series = [
                new HeatLandSeries { Lands = [ new() { Name = "bra", Value = 13 } ] }
            ]
        };
        using var image = chart.GetImage();

        var tooltip = new SKDefaultGeoTooltip();

        var series = (IGeoSeries)chart.Series!.First();
        var point = new GeoTooltipPoint
        {
            Land = new LandDefinition(shortName: "bra", name: "brazil", setOf: "world"),
            Values = [new GeoTooltipValue { Series = series, Value = 13 }],
            LandCenter = new LvcPoint(200, 200)
        };

        // Show then hide — exercises Initialize, GetLayout (Top branch), layout
        // measurement, placement, and Hide's early-return guard on the second call.
        tooltip.Show(point, chart.CoreChart);
        tooltip.Hide(chart.CoreChart);
        tooltip.Hide(chart.CoreChart); // second call takes the early-return branch
    }

    [TestMethod]
    public void SKDefaultGeoTooltip_BottomPlacementIsExercisedByFixedTooltipPosition()
    {
        // Forcing TooltipPosition.Bottom takes the other preferredPlacement path and
        // the matching padding/placement in GetLayout.
        var chart = new SKGeoMap
        {
            Width = 400,
            Height = 400,
            Series = [
                new HeatLandSeries { Lands = [ new() { Name = "usa", Value = 25 } ] }
            ],
            TooltipPosition = TooltipPosition.Bottom
        };
        using var image = chart.GetImage();

        var tooltip = new SKDefaultGeoTooltip();
        var point = new GeoTooltipPoint
        {
            Land = new LandDefinition(shortName: "usa", name: "united states", setOf: "world"),
            // No Values — exercise the no-value path in GetLayout.
            LandCenter = new LvcPoint(100, 50)
        };

        tooltip.Show(point, chart.CoreChart);
    }
}
