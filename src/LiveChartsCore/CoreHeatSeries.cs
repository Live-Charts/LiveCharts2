// The MIT License(MIT)
//
// Copyright(c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Drawing;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.Motion;
using LiveChartsCore.Painting;

namespace LiveChartsCore;

/// <summary>
/// Defines a column series.
/// </summary>
/// <typeparam name="TModel">The type of the model.</typeparam>
/// <typeparam name="TVisual">The type of the visual.</typeparam>
/// <typeparam name="TLabel">The type of the label.</typeparam>
public abstract class CoreHeatSeries<TModel, TVisual, TLabel>
    : CartesianSeries<TModel, TVisual, TLabel>, IHeatSeries
        where TVisual : BoundedDrawnGeometry, IColoredGeometry, new()
        where TLabel : BaseLabelGeometry, new()
{
    private Paint? _paintTaks;
    private int _heatKnownLength = 0;
    private List<Tuple<double, LvcColor>> _heatStops = [];
    private double _xStep = double.NaN;
    private double _yStep = double.NaN;

    /// <summary>
    /// Initializes a new instance of the <see cref="CoreHeatSeries{TModel, TVisual, TLabel}"/> class.
    /// </summary>
    /// <param name="values">The values.</param>
    protected CoreHeatSeries(IReadOnlyCollection<TModel>? values)
        : base(GetProperties(), values)
    {
        DataPadding = new LvcPoint(0, 0);
        YToolTipLabelFormatter = (point) =>
        {
            var cc = (CartesianChartEngine)point.Context.Chart.CoreChart;
            var cs = (ICartesianSeries)point.Context.Series;

            var ax = cc.YAxes[cs.ScalesYAt];

            var labeler = ax.Labeler;
            if (ax.Labels is not null) labeler = Labelers.BuildNamedLabeler(ax.Labels);

            var c = point.Coordinate;

            return $"{labeler(c.PrimaryValue)} {c.TertiaryValue}";
        };
        DataLabelsPosition = DataLabelsPosition.Middle;
    }

    /// <inheritdoc cref="IHeatSeries.WeightBounds"/>
    public Bounds WeightBounds { get; private set; } = new();

    /// <inheritdoc cref="IHeatSeries.HeatMap"/>
    public LvcColor[] HeatMap
    {
        get;
        set => SetProperty(ref field, value);
    } = [
        LvcColor.FromArgb(255, 87, 103, 222), // cold (min value)
        LvcColor.FromArgb(255, 95, 207, 249) // hot (max value)
    ];

    /// <inheritdoc cref="IHeatSeries.ColorStops"/>
    public double[]? ColorStops { get; set => SetProperty(ref field, value); }

    /// <inheritdoc cref="IHeatSeries.PointPadding"/>
    public Padding PointPadding { get; set => SetProperty(ref field, value); } = new(4);

    /// <inheritdoc cref="IHeatSeries.MinValue"/>
    public double? MinValue { get; set => SetProperty(ref field, value); }

    /// <inheritdoc cref="IHeatSeries.MaxValue"/>
    public double? MaxValue { get; set => SetProperty(ref field, value); }

    /// <inheritdoc cref="ChartElement.Invalidate(Chart)"/>
    public override void Invalidate(Chart chart)
    {
        if (_paintTaks is null)
        {
            _paintTaks = LiveCharts.DefaultSettings.GetProvider().GetSolidColorPaint();
            _paintTaks.PaintStyle = PaintStyle.Fill;
        }

        var cartesianChart = (CartesianChartEngine)chart;
        _ = GetAnimation(cartesianChart);

        var primaryAxis = cartesianChart.GetYAxis(this);
        var secondaryAxis = cartesianChart.GetXAxis(this);

        var drawLocation = cartesianChart.DrawMarginLocation;
        var drawMarginSize = cartesianChart.DrawMarginSize;
        var secondaryScale = secondaryAxis.GetNextScaler(cartesianChart);
        var primaryScale = primaryAxis.GetNextScaler(cartesianChart);
        var previousPrimaryScale = primaryAxis.GetActualScaler(cartesianChart);
        var previousSecondaryScale = secondaryAxis.GetActualScaler(cartesianChart);

        // Cell size is driven by the actual data spacing (computed once per measure
        // cycle in GetBounds) rather than Axis.UnitWidth, which defaults to 1 and is
        // correct only for unit-stepped axes. See issue #1511.
        var xStep = double.IsNaN(_xStep) ? secondaryAxis.UnitWidth : _xStep;
        var yStep = double.IsNaN(_yStep) ? primaryAxis.UnitWidth : _yStep;
        var uws = secondaryScale.MeasureInPixels(xStep);
        var uwp = primaryScale.MeasureInPixels(yStep);

        var actualZIndex = ZIndex == 0 ? ((ISeries)this).SeriesId : ZIndex;

        if (_paintTaks is not null)
        {
            _paintTaks.ZIndex = actualZIndex + PaintConstants.SeriesStrokeZIndexOffset;
            cartesianChart.Canvas.AddDrawableTask(_paintTaks, zone: CanvasZone.DrawMargin);
        }
        if (ShowDataLabels && DataLabelsPaint is not null && DataLabelsPaint != Paint.Default)
        {
            DataLabelsPaint.ZIndex = actualZIndex + PaintConstants.SeriesGeometryFillZIndexOffset;
            cartesianChart.Canvas.AddDrawableTask(DataLabelsPaint, zone: CanvasZone.DrawMargin);
        }

        var dls = (float)DataLabelsSize;
        var pointsCleanup = ChartPointCleanupContext.For(everFetched);

        var p = PointPadding;

        if (_heatKnownLength != HeatMap.Length)
        {
            _heatStops = HeatFunctions.BuildColorStops(HeatMap, ColorStops);
            _heatKnownLength = HeatMap.Length;
        }

        var hasSvg = this.HasVariableSvgGeometry();

        var isFirstDraw = !chart.IsDrawn(((ISeries)this).SeriesId);

        var provider = LiveCharts.DefaultSettings.GetProvider();

        foreach (var point in Fetch(cartesianChart))
        {
            var coordinate = point.Coordinate;
            var visual = point.Context.Visual as TVisual;
            var primary = primaryScale.ToPixels(coordinate.PrimaryValue);
            var secondary = secondaryScale.ToPixels(coordinate.SecondaryValue);
            var tertiary = (float)coordinate.TertiaryValue;

            var baseColor = HeatFunctions.InterpolateColor(tertiary, WeightBounds, HeatMap, _heatStops);

            if (point.IsEmpty || !IsVisible)
            {
                if (visual is not null)
                {
                    visual.X = secondary - uws * 0.5f;
                    visual.Y = primary - uwp * 0.5f;
                    visual.Width = uws;
                    visual.Height = uwp;
                    visual.RemoveOnCompleted = true;
                    visual.Color = LvcColor.FromArgb(0, visual.Color);
                    point.Context.Visual = null;
                }

                if (point.Context.Label is not null)
                {
                    var label = (TLabel)point.Context.Label;

                    label.X = secondary - uws * 0.5f;
                    label.Y = primary - uwp * 0.5f;
                    label.Opacity = 0;
                    label.RemoveOnCompleted = true;

                    point.Context.Label = null;
                }

                pointsCleanup.Clean(point);

                continue;
            }

            if (visual is null)
            {
                var xi = secondary - uws * 0.5f;
                var yi = primary - uwp * 0.5f;

                if (previousSecondaryScale is not null && previousPrimaryScale is not null)
                {
                    var previousP = previousPrimaryScale.ToPixels(pivot);
                    var previousPrimary = previousPrimaryScale.ToPixels(coordinate.PrimaryValue);
                    var bp = Math.Abs(previousPrimary - previousP);
                    var cyp = coordinate.PrimaryValue > pivot ? previousPrimary : previousPrimary - bp;

                    xi = previousSecondaryScale.ToPixels(coordinate.SecondaryValue) - uws * 0.5f;
                    yi = previousPrimaryScale.ToPixels(coordinate.PrimaryValue) - uwp * 0.5f;
                }

                var r = new TVisual
                {
                    X = xi + p.Left,
                    Y = yi + p.Top,
                    Width = uws - p.Left - p.Right,
                    Height = uwp - p.Top - p.Bottom,
                    Color = LvcColor.FromArgb(0, baseColor.R, baseColor.G, baseColor.B)
                };

                visual = r;
                point.Context.Visual = visual;
                OnPointCreated(point);

                _ = everFetched.Add(point);
            }

            if (hasSvg)
            {
                var svgVisual = (IVariableSvgPath)visual;
                if (_geometrySvgChanged || svgVisual.SVGPath is null)
                    svgVisual.SVGPath = GeometrySvg ?? throw new Exception("svg path is not defined");
            }

            _paintTaks?.AddGeometryToPaintTask(cartesianChart.Canvas, visual);

            visual.X = secondary - uws * 0.5f + p.Left;
            visual.Y = primary - uwp * 0.5f + p.Top;
            visual.Width = uws - p.Left - p.Right;
            visual.Height = uwp - p.Top - p.Bottom;
            visual.Color = LvcColor.FromArgb(baseColor.A, baseColor.R, baseColor.G, baseColor.B);
            visual.RemoveOnCompleted = false;

            if (point.Context.HoverArea is not RectangleHoverArea ha)
                point.Context.HoverArea = ha = new RectangleHoverArea();
            _ = ha
                .SetDimensions(secondary - uws * 0.5f, primary - uwp * 0.5f, uws, uwp)
                .CenterXToolTip()
                .CenterYToolTip();

            pointsCleanup.Clean(point);

            if (ShowDataLabels && DataLabelsPaint is not null && DataLabelsPaint != Paint.Default)
            {
                var label = (TLabel?)point.Context.Label;

                if (label is null)
                {
                    var l = new TLabel { X = secondary - uws * 0.5f, Y = primary - uws * 0.5f, RotateTransform = (float)DataLabelsRotation, MaxWidth = (float)DataLabelsMaxWidth };
                    l.Animate(
                        GetAnimation(cartesianChart),
                        BaseLabelGeometry.XProperty,
                        BaseLabelGeometry.YProperty);
                    label = l;
                    point.Context.Label = l;
                }

                DataLabelsPaint.AddGeometryToPaintTask(cartesianChart.Canvas, label);

                label.Text = DataLabelsFormatter(new ChartPoint<TModel, TVisual, TLabel>(point));
                label.TextSize = dls;
                label.Padding = DataLabelsPadding;
                label.Paint = DataLabelsPaint;

                if (isFirstDraw)
                    label.CompleteTransition(
                        BaseLabelGeometry.TextSizeProperty,
                        BaseLabelGeometry.XProperty,
                        BaseLabelGeometry.YProperty,
                        BaseLabelGeometry.RotateTransformProperty);

                var labelPosition = GetLabelPosition(
                     secondary - uws * 0.5f + p.Left, primary - uwp * 0.5f + p.Top, uws - p.Left - p.Right, uwp - p.Top - p.Bottom,
                     label.Measure(), DataLabelsPosition, SeriesProperties, coordinate.PrimaryValue > Pivot, drawLocation, drawMarginSize);
                label.X = labelPosition.X;
                label.Y = labelPosition.Y;
            }

            OnPointMeasured(point);
        }

        pointsCleanup.CollectPoints(
            everFetched, cartesianChart.View, primaryScale, secondaryScale, SoftDeleteOrDisposePoint);
        _geometrySvgChanged = false;
    }

    /// <inheritdoc cref="CartesianSeries{TModel, TVisual, TLabel}.GetBounds(Chart, ICartesianAxis, ICartesianAxis)"/>
    public override SeriesBounds GetBounds(Chart chart, ICartesianAxis secondaryAxis, ICartesianAxis primaryAxis)
    {
        // Derive cell steps from the data once per measure cycle and cache them so
        // Invalidate (the per-frame hot path) can read them without an extra scan.
        ComputeCellSteps(chart, secondaryAxis.UnitWidth, primaryAxis.UnitWidth);

        var seriesBounds = base.GetBounds(chart, secondaryAxis, primaryAxis);
        var b = seriesBounds.Bounds;
        WeightBounds = new(MinValue ?? b.TertiaryBounds.Min, MaxValue ?? b.TertiaryBounds.Max);

        // SeriesBounds.HasData is true when there's no data to render; base.GetBounds
        // returns the un-padded raw bounds in that case, so nothing to compensate.
        if (seriesBounds.HasData) return seriesBounds;

        // base.GetBounds padded SecondaryBounds/PrimaryBounds by offset * Axis.UnitWidth,
        // which over-expands the auto axis when the data step is finer than UnitWidth
        // (e.g. UnitWidth=1 on a Y axis stepped by 0.1 adds 0.5 of empty space each
        // side). Add the (cellStep - UnitWidth) * offset delta so padding matches
        // cell sizing.
        var rso = GetRequestedSecondaryOffset();
        var rpo = GetRequestedPrimaryOffset();
        var dx = (_xStep - secondaryAxis.UnitWidth) * rso;
        var dy = (_yStep - primaryAxis.UnitWidth) * rpo;

        Expand(b.SecondaryBounds, dx);
        Expand(b.VisibleSecondaryBounds, dx);
        Expand(b.PrimaryBounds, dy);
        Expand(b.VisiblePrimaryBounds, dy);

        return seriesBounds;

        static void Expand(Bounds bounds, double delta)
        {
            bounds.Max += delta;
            bounds.Min -= delta;
        }
    }

    /// <inheritdoc cref="CartesianSeries{TModel, TVisual, TLabel}.GetRequestedSecondaryOffset"/>
    protected override double GetRequestedSecondaryOffset() => 0.5f;

    /// <inheritdoc cref="CartesianSeries{TModel, TVisual, TLabel}.GetRequestedPrimaryOffset"/>
    protected override double GetRequestedPrimaryOffset() => 0.5f;

    /// <inheritdoc cref="Series{TModel, TVisual, TLabel}.SetDefaultPointTransitions(ChartPoint)"/>
    protected override void SetDefaultPointTransitions(ChartPoint chartPoint)
    {
        var chart = chartPoint.Context.Chart;
        if (chartPoint.Context.Visual is not TVisual visual) throw new Exception("Unable to initialize the point instance.");
        visual.Animate(GetAnimation(chart.CoreChart));
    }

    /// <inheritdoc cref="CartesianSeries{TModel, TVisual, TLabel}.SoftDeleteOrDisposePoint(ChartPoint, Scaler, Scaler)"/>
    protected internal override void SoftDeleteOrDisposePoint(ChartPoint point, Scaler primaryScale, Scaler secondaryScale)
    {
        var visual = (TVisual?)point.Context.Visual;
        if (visual is null) return;
        if (DataFactory is null) throw new Exception("Data provider not found");

        visual.Color = LvcColor.FromArgb(255, visual.Color);
        visual.RemoveOnCompleted = true;

        var label = (TLabel?)point.Context.Label;
        if (label is null) return;

        label.TextSize = 1;
        label.RemoveOnCompleted = true;
    }

    /// <inheritdoc cref="Series{TModel, TVisual, TLabel}.GetMiniatureGeometry"/>
    public override IDrawnElement GetMiniatureGeometry(ChartPoint? point)
    {
        // ToDo <- draw the gradient?
        // what to show in the legend?

        return new TVisual
        {
            Width = 0,
            Height = 0,
        };
    }

    /// <inheritdoc cref="ChartElement.GetPaintTasks"/>
    protected internal override Paint?[] GetPaintTasks() =>
        [_paintTaks];

    private static SeriesProperties GetProperties()
    {
        return SeriesProperties.Heat | SeriesProperties.PrimaryAxisVerticalOrientation |
            SeriesProperties.Solid | SeriesProperties.PrefersXYStrategyTooltips;
    }

    private void ComputeCellSteps(Chart chart, double xFallback, double yFallback)
    {
        var xs = new HashSet<double>();
        var ys = new HashSet<double>();
        foreach (var point in Fetch(chart))
        {
            // Empty points carry Coordinate(0, 0); including them would inject a
            // spurious 0 into the distinct-values set and shrink the computed step.
            if (point.IsEmpty) continue;
            var c = point.Coordinate;
            _ = xs.Add(c.SecondaryValue);
            _ = ys.Add(c.PrimaryValue);
        }

        _xStep = MinStep(xs, xFallback);
        _yStep = MinStep(ys, yFallback);

        static double MinStep(HashSet<double> values, double fallback)
        {
            if (values.Count < 2) return fallback;

            var sorted = new double[values.Count];
            values.CopyTo(sorted);
            Array.Sort(sorted);

            var min = double.PositiveInfinity;
            for (var i = 1; i < sorted.Length; i++)
            {
                var delta = sorted[i] - sorted[i - 1];
                if (delta > 0 && delta < min) min = delta;
            }

            return double.IsPositiveInfinity(min) ? fallback : min;
        }
    }
}
