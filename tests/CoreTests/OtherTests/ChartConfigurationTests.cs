using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.OtherTests;

[TestClass]
public class ChartConfigurationTests
{
    private static ISeries[] LineSeries() =>
    [
        new LineSeries<double> { Values = [1, 2, 3, 4, 5], Name = "A" },
        new LineSeries<double> { Values = [5, 4, 3, 2, 1], Name = "B" }
    ];

    [DataTestMethod]
    [DataRow(LegendPosition.Top)]
    [DataRow(LegendPosition.Bottom)]
    [DataRow(LegendPosition.Left)]
    [DataRow(LegendPosition.Right)]
    [DataRow(LegendPosition.Hidden)]
    public void CartesianChart_RendersWithEachLegendPosition(LegendPosition position)
    {
        // Each LegendPosition drives a different branch of Chart.GetLegendPosition.
        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            Series = LineSeries(),
            LegendPosition = position
        };

        using var image = chart.GetImage();
        Assert.IsNotNull(image);
    }

    [DataTestMethod]
    [DataRow(TooltipPosition.Top)]
    [DataRow(TooltipPosition.Bottom)]
    [DataRow(TooltipPosition.Left)]
    [DataRow(TooltipPosition.Right)]
    [DataRow(TooltipPosition.Center)]
    [DataRow(TooltipPosition.Auto)]
    [DataRow(TooltipPosition.Hidden)]
    public void CartesianChart_RendersWithEachTooltipPosition(TooltipPosition position)
    {
        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            Series = LineSeries(),
            TooltipPosition = position
        };

        using var image = chart.GetImage();
        Assert.IsNotNull(image);
    }

    [TestMethod]
    public void CartesianChart_WithExplicitDrawMarginRenders()
    {
        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            Series = LineSeries(),
            DrawMargin = new Margin(30, 40, 30, 40)
        };

        using var image = chart.GetImage();
        Assert.IsNotNull(image);
    }

    [TestMethod]
    public void CartesianChart_WithDrawMarginFrameRenders()
    {
        // DrawMarginFrame exercises the optional frame path in Chart.Measure.
        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            Series = LineSeries(),
            DrawMarginFrame = new DrawMarginFrame
            {
                Fill = new SolidColorPaint(SKColors.LightGray),
                Stroke = new SolidColorPaint(SKColors.Black, 2)
            }
        };

        using var image = chart.GetImage();
        Assert.IsNotNull(image);
    }

    [DataTestMethod]
    [DataRow(0d, 0d, 360d)]
    [DataRow(-90d, 0d, 360d)]
    [DataRow(0d, 0.3d, 270d)]
    [DataRow(45d, 0.5d, 180d)]
    public void PolarChart_RendersAcrossRotationInnerRadiusAndTotalAngle(
        double initialRotation, double innerRadius, double totalAngle)
    {
        // Exercises CorePolarAxis / PolarChartEngine with non-default transforms.
        var chart = new SKPolarChart
        {
            Width = 400,
            Height = 400,
            InitialRotation = initialRotation,
            InnerRadius = innerRadius,
            TotalAngle = totalAngle,
            Series = [
                new PolarLineSeries<double> { Values = [1, 2, 3, 4, 5] }
            ]
        };

        using var image = chart.GetImage();
        Assert.IsNotNull(image);
    }

    [TestMethod]
    public void PolarChart_FitToBoundsRenders()
    {
        var chart = new SKPolarChart
        {
            Width = 400,
            Height = 400,
            FitToBounds = true,
            Series = [
                new PolarLineSeries<double> { Values = [1, 2, 3, 4, 5], IsClosed = true }
            ]
        };

        using var image = chart.GetImage();
        Assert.IsNotNull(image);
    }

    [TestMethod]
    public void CartesianChart_WithLogAxisRenders()
    {
        // Exercises CoreAxis paths specific to logarithmic scale.
        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            Series = [
                new LineSeries<double> { Values = [1, 10, 100, 1000, 10000] }
            ],
            YAxes = [
                new LogarithmicAxis(10) { MinLimit = 1, MaxLimit = 10000 }
            ]
        };

        using var image = chart.GetImage();
        Assert.IsNotNull(image);
    }

    [TestMethod]
    public void CartesianChart_WithLabelsAndRotatedTextRenders()
    {
        // Non-default axis labels + rotation hit additional CoreAxis layout paths.
        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            Series = LineSeries(),
            XAxes = [
                new Axis
                {
                    Labels = ["one", "two", "three", "four", "five"],
                    LabelsRotation = 45,
                    TextSize = 10
                }
            ]
        };

        using var image = chart.GetImage();
        Assert.IsNotNull(image);
    }

    [TestMethod]
    public void CartesianChart_EmptySeriesAndEmptyAxesDoNotThrow()
    {
        var chart = new SKCartesianChart
        {
            Width = 300,
            Height = 300,
            Series = [],
            XAxes = [new Axis()],
            YAxes = [new Axis()]
        };

        using var image = chart.GetImage();
        Assert.IsNotNull(image);
    }
}
