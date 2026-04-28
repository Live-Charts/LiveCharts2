using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using LiveChartsCore.SkiaSharpView.SKCharts;
using SkiaSharp;

namespace SnapshotTests;

[TestClass]
public sealed class AxesTests
{
    [TestMethod]
    public void ColorsAndPositions()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new ColumnSeries<double> { Values = [2, 3, 8] }
            ],
            XAxes = [
                new Axis
                {
                    Position = AxisPosition.End,
                    Name = "X Axis",
                    NamePaint = new SolidColorPaint(SKColors.Green),
                    LabelsPaint = new SolidColorPaint(SKColors.Green)
                }
            ],
            YAxes = [
                new Axis
                {
                    Position = AxisPosition.End,
                    Name = "Y Axis",
                    NamePaint = new SolidColorPaint(SKColors.Red),
                    LabelsPaint = new SolidColorPaint(SKColors.Red)
                }
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(AxesTests)}_{nameof(ColorsAndPositions)}");
    }

    [TestMethod]
    public void Crosshairs()
    {
        var crosshairColor = new SKColor(255, 0, 51);
        var crosshairBackground = new LiveChartsCore.Drawing.LvcColor(255, 0, 51);
        static string labelFormatter(double value) => value.ToString("N0");

        var chart = new SKCartesianChart
        {
            Series = [
                new ColumnSeries<double> { Values = [ 200, 558, 458, 249, 457, 339, 587 ] }
            ],
            XAxes = [
                new Axis
                {
                    Name = "X Axis",
                    Labeler = labelFormatter,
                    CrosshairPaint = new SolidColorPaint(crosshairColor, 2),
                    CrosshairLabelsPaint = new SolidColorPaint(SKColors.White),
                    CrosshairLabelsBackground = crosshairBackground,
                }
            ],
            YAxes = [
                new Axis
                {
                    Name = "Y Axis",
                    Labeler = labelFormatter,
                    CrosshairPaint = new SolidColorPaint(crosshairColor, 2),
                    CrosshairLabelsPaint = new SolidColorPaint(SKColors.White),
                    CrosshairLabelsBackground = crosshairBackground,
                    CrosshairSnapEnabled = true
                }
            ],
            Width = 600,
            Height = 600
        };

        // hack to initialize crosshairs, so CrosshairSnapEnabled works.
        // the issue is that livecharts is not able to snap to the data because
        // the drawn shape is not initialized until the chart is rendered for the first time.
        // so lets first build the chart, then move the pointer to initialize the crosshair shapes, and then take the snapshot.
        _ = chart.GetImage();

        chart.PointerAt(320, 300);
        chart.AssertSnapshotMatches($"{nameof(AxesTests)}_{nameof(Crosshairs)}");
    }

    [TestMethod]
    public void CustomSeparatorsInterval()
    {
        double[] customSeparators = [0, 10, 25, 50, 100];

        var chart = new SKCartesianChart
        {
            Series = [
                new LineSeries<double> { Values = [10, 55, 45, 68, 60, 70, 75, 120] }
            ],
            XAxes = [
                new Axis
                {

                }
            ],
            YAxes = [
                new Axis
                {
                    CustomSeparators = customSeparators
                }
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(AxesTests)}_{nameof(CustomSeparatorsInterval)}");
    }

    [TestMethod]
    public void DateTimeScaled()
    {
        var values = new DateTimePoint[]
        {
            new() { DateTime = new DateTime(2021, 1, 1), Value = 3 },
            new() { DateTime = new DateTime(2021, 1, 2), Value = 6 },
            new() { DateTime = new DateTime(2021, 1, 3), Value = 5 },
            new() { DateTime = new DateTime(2021, 1, 4), Value = 3 },
            new() { DateTime = new DateTime(2021, 1, 5), Value = 5 },
            new() { DateTime = new DateTime(2021, 1, 6), Value = 8 },
            new() { DateTime = new DateTime(2021, 1, 7), Value = 6 }
        };

        static string Formatter(DateTime date) => date.ToString("MM dd");

        var chart = new SKCartesianChart
        {
            Series = [
                new ColumnSeries<DateTimePoint> { Values = values }
            ],
            XAxes = [
                new DateTimeAxis(TimeSpan.FromDays(1), Formatter)
            ],
            YAxes = [
                new Axis
                {

                }
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(AxesTests)}_{nameof(DateTimeScaled)}");
    }

    [TestMethod]
    public void LabelsFormat()
    {
        double[] customSeparators = [0, 10, 25, 50, 100];

        var chart = new SKCartesianChart
        {
            Series = [
                new LineSeries<double> { Values = [10, 55, 45, 68, 60, 70, 75, 120] }
            ],
            XAxes = [
                new Axis
                {

                }
            ],
            YAxes = [
                new Axis
                {
                    CustomSeparators = customSeparators
                }
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(AxesTests)}_{nameof(LabelsFormat)}");
    }

    [TestMethod]
    public void NonLatinLabels()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new LineSeries<double> { Values = [1, 2, 3] }
            ],
            XAxes = [
                new Axis
                {
                    Labels = [ "王", "赵", "张" ],
                    TextSize = 50,
                }
            ],
            YAxes = [
                new Axis
                {

                }
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(AxesTests)}_{nameof(NonLatinLabels)}");
    }

    [TestMethod]
    public void LabelsRotation()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new LineSeries<double> { Values = [1, 2, 3] }
            ],
            XAxes = [
                new Axis
                {
                    Labels = [ "HELLO", "THIS", "ROTATE" ],
                    TextSize = 50,
                    LabelsRotation = 45
                }
            ],
            YAxes = [
                new Axis
                {

                }
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(AxesTests)}_{nameof(LabelsRotation)}");
    }

    [TestMethod]
    public void LogarithmicScale()
    {
        var values = new LogarithmicPoint[]
        {
            new(1, 1),
            new(2, 10),
            new(3, 100),
            new(4, 1000),
            new(5, 10000),
            new(6, 100000),
            new(7, 1000000),
            new(8, 10000000)
        };

        var chart = new SKCartesianChart
        {
            Series = [
                new LineSeries<LogarithmicPoint> { Values = values }
            ],
            XAxes = [
                new Axis
                {
                }
            ],
            YAxes = [
                new LogarithmicAxis(10)
                {
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray),
                    SubseparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) { StrokeThickness = 0.5f },
                    SubseparatorsCount = 9
                }
            ],
            Width = 600,
            Height = 600
        };
        chart.AssertSnapshotMatches($"{nameof(AxesTests)}_{nameof(LogarithmicScale)}");
    }

    [TestMethod]
    public void MatchScale()
    {
        // y from 0 to 5. x should calculate the range, so the grid forms a perfect square,
        // so the distance between the separators in x and y are the same.

        var chart = new SKCartesianChart
        {
            Series = [
            ],
            XAxes = [
                new Axis
                {
                    MinStep = 0.25,
                    ForceStepToMin = true,
                    LabelsRotation = 45,
                    SeparatorsPaint = new SolidColorPaint(SKColors.Gray),
                }
            ],
            YAxes = [
                new Axis
                {
                    MinLimit = 0,
                    MaxLimit = 5,
                    MinStep = 0.25,
                    ForceStepToMin = true,
                    LabelsRotation = 45,
                    SeparatorsPaint = new SolidColorPaint(SKColors.Gray),
                }
            ],
            MatchAxesScreenDataRatio = true,
            DrawMarginFrame = new DrawMarginFrame
            {
                Stroke = new SolidColorPaint(SKColors.Gray, 2)
            },
            Width = 1200,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(AxesTests)}_{nameof(MatchScale)}");
    }

    [TestMethod]
    public void MultipleYAxes()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new LineSeries<double> { Values = [1, 2, 3], ScalesYAt = 0 },
                new ColumnSeries<double> { Values = [10, 20, 30], ScalesYAt = 1 },
                new ScatterSeries<double> { Values = [100, 200, 300], ScalesYAt = 2 }
            ],
            XAxes = [
                new Axis
                {
                },
            ],
            YAxes = [
                new Axis
                {
                },
                new Axis
                {
                },
                new Axis
                {
                }
            ],
            Width = 600,
            Height = 600
        };
        chart.AssertSnapshotMatches($"{nameof(AxesTests)}_{nameof(MultipleYAxes)}");
    }

    [TestMethod]
    public void MultipleXAxes()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new RowSeries<double> { Values = [1, 2, 3], ScalesXAt = 0 },
                new RowSeries<double> { Values = [10, 20, 30], ScalesXAt = 1 },
                new RowSeries<double> { Values = [100, 200, 300], ScalesXAt = 2 }
            ],
            XAxes = [
                new Axis
                {
                },
                new Axis
                {
                },
                new Axis
                {
                }
            ],
            YAxes = [
                new Axis
                {
                },
            ],
            Width = 600,
            Height = 600
        };
        chart.AssertSnapshotMatches($"{nameof(AxesTests)}_{nameof(MultipleXAxes)}");
    }

    [TestMethod]
    public void MinSeparatorsFloor()
    {
        // Regression for issue #2071: on short charts with awkward ranges the auto-step would
        // snap so far up that only 1-2 separators remained. The default MinSeparators (3) should
        // force the auto-step to subdivide further so the grid stays readable.

        var chart = new SKCartesianChart
        {
            Series = [
                new LineSeries<double> { Values = [0, 11] }
            ],
            XAxes = [
                new Axis()
            ],
            YAxes = [
                new Axis()
            ],
            Width = 400,
            Height = 120
        };

        chart.AssertSnapshotMatches($"{nameof(AxesTests)}_{nameof(MinSeparatorsFloor)}");
    }

    [TestMethod]
    public void MinSeparatorsDisabled()
    {
        // Same setup as MinSeparatorsFloor but with MinSeparators = 0 so the floor is opted out;
        // the rendered grid should match the original (pre-fix) snap-up behavior.

        var chart = new SKCartesianChart
        {
            Series = [
                new LineSeries<double> { Values = [0, 11] }
            ],
            XAxes = [
                new Axis { MinSeparators = 0 }
            ],
            YAxes = [
                new Axis { MinSeparators = 0 }
            ],
            Width = 400,
            Height = 120
        };

        chart.AssertSnapshotMatches($"{nameof(AxesTests)}_{nameof(MinSeparatorsDisabled)}");
    }

    [TestMethod]
    public void StyledAxes()
    {
        var values = new ObservablePoint[1001];
        var fx = EasingFunctions.BounceInOut;
        for (var i = 0; i < 1001; i++)
        {
            var x = i / 1000f;
            var y = fx(x);
            values[i] = new ObservablePoint(x - 0.5, y - 0.5);
        }

        var gray = new SKColor(195, 195, 195);
        var gray1 = new SKColor(160, 160, 160);
        var gray2 = new SKColor(90, 90, 90);
        var gray3 = new SKColor(60, 60, 60);

        var series = new ISeries[]
        {
            new LineSeries<ObservablePoint>
            {
                Values = values,
                Stroke = new SolidColorPaint(new SKColor(33, 150, 243), 4), // #2196F3
                Fill = null,
                GeometryFill = null,
                GeometryStroke = null
            }
        };

        var dashEffect = new DashEffect([3, 3]);

        var xAxis = new Axis
        {
            Name = "X Axis",
            NamePaint = new SolidColorPaint(gray1),
            TextSize = 18,
            LabelsPaint = new SolidColorPaint(gray),
            SeparatorsPaint = new SolidColorPaint(gray, 1) { PathEffect = dashEffect },
            SubseparatorsPaint = new SolidColorPaint(gray2, 0.5f),
            SubseparatorsCount = 9,
            ZeroPaint = new SolidColorPaint(gray1, 2),
            TicksPaint = new SolidColorPaint(gray, 1.5f),
            SubticksPaint = new SolidColorPaint(gray, 1)
        };
        var yAxis = new Axis
        {
            Name = "Y Axis",
            NamePaint = new SolidColorPaint(gray1),
            TextSize = 18,
            LabelsPaint = new SolidColorPaint(gray),
            SeparatorsPaint = new SolidColorPaint(gray, 1) { PathEffect = dashEffect },
            SubseparatorsPaint = new SolidColorPaint(gray2, 0.5f),
            SubseparatorsCount = 9,
            ZeroPaint = new SolidColorPaint(gray1, 2),
            TicksPaint = new SolidColorPaint(gray, 1.5f),
            SubticksPaint = new SolidColorPaint(gray, 1)
        };

        var frame = new DrawMarginFrame
        {
            Stroke = new SolidColorPaint(gray, 2)
        };

        var chart = new SKCartesianChart
        {
            Background = new SKColor(30, 30, 30),
            Series = series,
            XAxes = [
                xAxis
            ],
            YAxes = [
                yAxis
            ],
            DrawMarginFrame = frame,
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(AxesTests)}_{nameof(StyledAxes)}");
    }

    private class LogarithmicPoint(double x, double y) : IChartEntity
    {
        public double X { get; set; } = x;
        public double Y { get; set; } = y;
        public ChartEntityMetaData? MetaData { get; set; }
        public Coordinate Coordinate => new(X, Math.Log(Y, 10));
    }
}
