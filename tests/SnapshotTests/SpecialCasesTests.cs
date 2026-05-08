using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.VisualStates;
using SkiaSharp;
using ViewModelsSamples.General.VisualElements;

namespace SnapshotTests;

[TestClass]
public sealed class SpecialCasesTests
{
    [TestMethod]
    public void NullPoints()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new LineSeries<int?> { Values = [1, 6, 5, null, 3, 2, 5] }
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(SpecialCasesTests)}_{nameof(NullPoints)}");
    }

    // https://github.com/Live-Charts/LiveCharts2/issues/1847
    // The library does not silently rewrite NaN / Infinity to a gap; users opt
    // in via a custom mapper or IChartEntity (see docs/overview/1.5.mappers.md
    // "NaN and Infinity"). This test locks the sentinel-coordinate recipe for
    // the mapper variant: Mapping returns Coordinate(index, 0) so the bar
    // collapses but the column's hover area survives, paired with a
    // YToolTipLabelFormatter that prints the original non-finite sign so the
    // user can still see what was at that index. Five frames cover the three
    // non-finite kinds plus a recovery to a finite value.
    [TestMethod]
    public void Issue1847_NonFiniteValuesViaCustomMapper()
    {
        Coordinate Map(double value, int index) =>
            double.IsNaN(value) || double.IsInfinity(value)
                ? new Coordinate(index, 0)
                : new Coordinate(index, value);

        static string Format(ChartPoint<double, RoundedRectangleGeometry, LabelGeometry> p) =>
            double.IsPositiveInfinity(p.Model) ? "+∞"
            : double.IsNegativeInfinity(p.Model) ? "-∞"
            : double.IsNaN(p.Model) ? "NaN"
            : p.Model.ToString();

        var values = new ObservableCollection<double> { 1, 2, 3, 4, 5 };

        var chart = new SKCartesianChart
        {
            Series = [
                new ColumnSeries<double>
                {
                    Values = values,
                    Mapping = Map,
                    YToolTipLabelFormatter = Format
                }
            ],
            Width = 600,
            Height = 600
        };

        // Hover at index 1 so every snapshot includes the tooltip overlay.
        chart.PointerAt(180, 300);

        // Frame 0: all finite. Tooltip on bar 1 reads "2".
        chart.AssertSnapshotMatches($"{nameof(SpecialCasesTests)}_{nameof(Issue1847_NonFiniteValuesViaCustomMapper)}_0");

        // Frame 1: index 1 is +Infinity. Bar collapses, tooltip reads "+∞".
        values[1] = double.PositiveInfinity;
        chart.AssertSnapshotMatches($"{nameof(SpecialCasesTests)}_{nameof(Issue1847_NonFiniteValuesViaCustomMapper)}_1");

        // Frame 2: NaN at index 1. Tooltip reads "NaN".
        values[1] = double.NaN;
        chart.AssertSnapshotMatches($"{nameof(SpecialCasesTests)}_{nameof(Issue1847_NonFiniteValuesViaCustomMapper)}_2");

        // Frame 3: -Infinity at index 1. Tooltip reads "-∞".
        values[1] = double.NegativeInfinity;
        chart.AssertSnapshotMatches($"{nameof(SpecialCasesTests)}_{nameof(Issue1847_NonFiniteValuesViaCustomMapper)}_3");

        // Frame 4: restore a finite value. Bar comes back, tooltip reads "2".
        values[1] = 2;
        chart.AssertSnapshotMatches($"{nameof(SpecialCasesTests)}_{nameof(Issue1847_NonFiniteValuesViaCustomMapper)}_4");
    }

    // Companion to Issue1847_NonFiniteValuesViaCustomMapper for the
    // IChartEntity recipe. The same sentinel-coordinate-plus-formatter policy
    // is applied inside an entity's OnCoordinateChanged instead of through
    // Mapping. Mutating Value in place exercises the entity-reuse path - the
    // same SafeValue instance persists across frames, which is the scenario
    // from the original bug report (#1847).
    [TestMethod]
    public void Issue1847_NonFiniteValuesViaCustomEntity()
    {
        static string Format(ChartPoint<SafeValue, RoundedRectangleGeometry, LabelGeometry> p) =>
            double.IsPositiveInfinity(p.Model!.Value) ? "+∞"
            : double.IsNegativeInfinity(p.Model!.Value) ? "-∞"
            : double.IsNaN(p.Model!.Value) ? "NaN"
            : p.Model!.Value.ToString();

        var values = new ObservableCollection<SafeValue>
        {
            new(1), new(2), new(3), new(4), new(5)
        };

        var chart = new SKCartesianChart
        {
            Series = [
                new ColumnSeries<SafeValue> { Values = values, YToolTipLabelFormatter = Format }
            ],
            Width = 600,
            Height = 600
        };

        chart.PointerAt(180, 300);

        chart.AssertSnapshotMatches($"{nameof(SpecialCasesTests)}_{nameof(Issue1847_NonFiniteValuesViaCustomEntity)}_0");

        values[1].Value = double.PositiveInfinity;
        chart.AssertSnapshotMatches($"{nameof(SpecialCasesTests)}_{nameof(Issue1847_NonFiniteValuesViaCustomEntity)}_1");

        values[1].Value = double.NaN;
        chart.AssertSnapshotMatches($"{nameof(SpecialCasesTests)}_{nameof(Issue1847_NonFiniteValuesViaCustomEntity)}_2");

        values[1].Value = double.NegativeInfinity;
        chart.AssertSnapshotMatches($"{nameof(SpecialCasesTests)}_{nameof(Issue1847_NonFiniteValuesViaCustomEntity)}_3");

        values[1].Value = 2;
        chart.AssertSnapshotMatches($"{nameof(SpecialCasesTests)}_{nameof(Issue1847_NonFiniteValuesViaCustomEntity)}_4");
    }

    // Test-only IChartEntity that mirrors the documented "Policy B" recipe:
    // non-finite values render as a zero-height stub so the column's hover
    // area survives, finite values render normally. The series-level
    // YToolTipLabelFormatter inspects Value to print the original sign.
    private class SafeValue : IChartEntity, INotifyPropertyChanged
    {
        public SafeValue() => MetaData = new ChartEntityMetaData(OnCoordinateChanged);
        public SafeValue(double value) : this() => Value = value;

        public double Value { get; set { field = value; OnPropertyChanged(); } }

        public ChartEntityMetaData? MetaData { get; set; }
        public Coordinate Coordinate { get; set; } = Coordinate.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            if (MetaData is not null) OnCoordinateChanged(MetaData.EntityIndex);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnCoordinateChanged(int index) =>
            Coordinate = double.IsNaN(Value) || double.IsInfinity(Value)
                ? new Coordinate(index, 0)
                : new Coordinate(index, Value);
    }

    [TestMethod]
    public void CustomPoints()
    {
        var values1 = new double[] { 2, 1, 4 };
        var values2 = new double[] { 4, 3, 6 };
        var values3 = new double[] { -2, 2, 1 };
        var values4 = new double[] { 1, 2, 3 };
        var starPath = SVGPoints.Star;

        var chart = new SKCartesianChart
        {
            Series = [
                new ColumnSeries<double> { Values = values1 },
                new ColumnSeries<double, DiamondGeometry> { Values = values2 },
                new ColumnSeries<double, VariableSVGPathGeometry> { Values = values3, GeometrySvg = starPath },
                new ColumnSeries<double, MyGeometry> { Values = values4 }
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(SpecialCasesTests)}_{nameof(CustomPoints)}");
    }

    [TestMethod]
    public void DrawMarginFrameFillBehindSeries()
    {
        // Regression for https://github.com/Live-Charts/LiveCharts2/issues/1978
        // Opaque DrawMarginFrame.Fill must render behind the series, not on top of it.
        var chart = new SKCartesianChart
        {
            Series = [
                new LineSeries<double>
                {
                    Values = [1, 6, 5, 3, 8, 2, 9],
                    Fill = null,
                    Stroke = new SolidColorPaint(SKColors.Black, 4),
                    GeometryFill = new SolidColorPaint(SKColors.Black),
                    GeometryStroke = new SolidColorPaint(SKColors.Black, 4)
                }
            ],
            DrawMarginFrame = new DrawMarginFrame
            {
                Fill = new SolidColorPaint(SKColors.Red),
                Stroke = new SolidColorPaint(SKColors.DarkRed, 2)
            },
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(SpecialCasesTests)}_{nameof(DrawMarginFrameFillBehindSeries)}");
    }

    [TestMethod]
    public void Sections()
    {
        var values = new ObservablePoint[]
        {
            new(2.2, 5.4),
            new(4.5, 2.5),
            new(4.2, 7.4),
            new(6.4, 9.9),
            new(8.9, 3.9),
            new(9.9, 5.2)
        };

        var sections = new RectangularSection[]
        {
            // Section from 3 to 4 in X axis
            new() {
                Xi = 3,
                Xj = 4,
                Fill = new SolidColorPaint(SKColor.Parse("#FFCDD2"))
            },
            // Section from 5 to 6 in X axis and 2 to 8 in Y axis
            new() {
                Xi = 5,
                Xj = 6,
                Yi = 2,
                Yj = 8,
                Fill = new SolidColorPaint(SKColor.Parse("#BBDEFB"))
            },
            // Section from 8 to end in X axis
            new() {
                Xi = 8,
                Label = "A section here!",
                LabelSize = 14,
                LabelPaint = new SolidColorPaint(SKColor.Parse("#FF6F00")),
                Fill = new SolidColorPaint(SKColor.Parse("#F9FBE7"))
            }
        };

        var chart = new SKCartesianChart
        {
            Series = [
                new ScatterSeries<ObservablePoint> { Values = values }
            ],
            Sections = sections,
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(SpecialCasesTests)}_{nameof(Sections)}");
    }

    [TestMethod]
    public void Visibility()
    {
        var values0 = new double[] { 2, 5, 4 };
        var values1 = new double[] { 1, 2, 3 };
        var values2 = new double[] { 4, 3, 2 };

        ISeries[] series = [
            new ColumnSeries<double> { Values = values0 },
            new ColumnSeries<double> { Values = values1 },
            new ColumnSeries<double> { Values = values2 }
        ];

        var chart = new SKCartesianChart
        {
            Series = series,
            Width = 600,
            Height = 600
        };


        chart.AssertSnapshotMatches($"{nameof(SpecialCasesTests)}_{nameof(Visibility)}_0");

        series[1].IsVisible = false;
        chart.AssertSnapshotMatches($"{nameof(SpecialCasesTests)}_{nameof(Visibility)}_1");

        series[1].IsVisible = true;
        chart.AssertSnapshotMatches($"{nameof(SpecialCasesTests)}_{nameof(Visibility)}_2");
    }

    [TestMethod]
    public void VisualElements()
    {
        var chart = new SKCartesianChart
        {
            VisualElements = [
                new RectangleVisual(),
                new ScaledRectangleVisual(),
                new PointerDownAwareVisual(),
                new SvgVisual(),
                new ThemedVisual(),
                new CustomVisual(),
                new AbsoluteVisual(),
                new StackedVisual(),
                new TableVisual(),
                new ContainerVisual()
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(SpecialCasesTests)}_{nameof(VisualElements)}");
    }

    [TestMethod]
    public void States()
    {
        var values = new ObservableCollection<ObservableValue>
        {
            new(2), new(3), new(4)
        };

        var columnSeries = new ColumnSeries<ObservableValue>
        {
            ShowDataLabels = true,
            DataLabelsSize = 15,
            Values = values
        };

        // define the danger state, a red fill.
        _ = columnSeries.HasState("Danger", [
            (nameof(IDrawnElement.Fill), new SolidColorPaint(SKColors.Yellow))
        ]);

        _ = columnSeries.HasState("LabelDanger", [
            (nameof(IDrawnElement.Paint), new SolidColorPaint(SKColors.Yellow)),
            (nameof(BaseLabelGeometry.TextSize), 30f),
        ]);

        columnSeries.PointMeasured += point =>
        {
            var ctx = point.Context;
            if (ctx.DataSource is not ObservableValue observable) return;

            var states = ctx.Series.VisualStates;

            if (observable.Value > 5)
            {
                states.SetState("Danger", ctx.Visual);
                states.SetState("LabelDanger", ctx.Label);
            }
            else
            {
                states.ClearState("Danger", ctx.Visual);
                states.ClearState("LabelDanger", ctx.Label);
            }
        };

        var chart = new SKCartesianChart
        {
            Series = [
                columnSeries
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(SpecialCasesTests)}_{nameof(States)}_0");

        values[1].Value = 10;
        chart.AssertSnapshotMatches($"{nameof(SpecialCasesTests)}_{nameof(States)}_1");

        values[1].Value = 1;
        chart.AssertSnapshotMatches($"{nameof(SpecialCasesTests)}_{nameof(States)}_2");

        values[1].Value = 10;
        chart.AssertSnapshotMatches($"{nameof(SpecialCasesTests)}_{nameof(States)}_3");
    }

    private class MyGeometry : BoundedDrawnGeometry, IDrawnElement<SkiaSharpDrawingContext>
    {
        public void Draw(SkiaSharpDrawingContext context)
        {
            var paint = context.ActiveSkiaPaint;
            var canvas = context.Canvas;
            var y = Y;

            while (y < Y + Height)
            {
                canvas.DrawLine(X, y, X + Width, y, paint);
                y += 5;
            }
        }
    }
}
