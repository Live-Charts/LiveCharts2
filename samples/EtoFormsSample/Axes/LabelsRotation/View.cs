using System;
using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace EtoFormsSample.Axes.LabelsRotation;

public class View : Panel
{
    private readonly CartesianChart cartesianChart;
    private readonly Axis yAxis;

    public View()
    {
        var values = new double[] { 200, 558, 458, 249, 457, 339, 587 };
        static string tooltipFormat(ChartPoint point) =>
            $"This is {Environment.NewLine}" +
            $"A multi-line label {Environment.NewLine}" +
            $"With a value of {Environment.NewLine}" + point.Coordinate.PrimaryValue;
        static string labeler(double value) =>
            $"This is {Environment.NewLine}" +
            $"A multi-line label {Environment.NewLine}" +
            $"With a value of {Environment.NewLine}" + value * 100;

        var series = new ISeries[]
        {
            new LineSeries<double>
            {
                Values = values,
                XToolTipLabelFormatter = tooltipFormat
            }
        };

        var xAxis = new Axis
        {
            Labeler = labeler,
            MinStep = 1,
            LabelsRotation = 45,
            SeparatorsPaint = new SolidColorPaint(new SKColor(238, 238, 238), 2)
        };

        yAxis = new Axis
        {
            LabelsRotation = 15, // initial value
            SeparatorsPaint = new SolidColorPaint(new SKColor(238, 238, 238), 2)
        };

        cartesianChart = new CartesianChart
        {
            Series = series,
            XAxes = [xAxis],
            YAxes = [yAxis],
        };

        var slider = new Slider { Width = 300, MinValue = -360, MaxValue = 720, Value = 15 };
        slider.ValueChanged += (sender, e) =>
        {
            yAxis.LabelsRotation = slider.Value;
        };

        Content = new DynamicLayout(new StackLayout(slider), cartesianChart);
    }
}
