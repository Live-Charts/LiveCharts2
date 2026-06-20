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
/// Defines a heat series.
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

    /// <inheritdoc cref="IHeatLegendSource.WeightBounds"/>
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

    // ---- template method ----------------------------------------------------

    /// <summary>
    /// Builds a per-frame measure context from the chart. Subclasses may
    /// override to refine context construction.
    /// </summary>
    protected virtual HeatMeasureContext BeginMeasure(CartesianChartEngine chart)
    {
        var primaryAxis = chart.GetYAxis(this);
        var secondaryAxis = chart.GetXAxis(this);

        var drawLocation = chart.DrawMarginLocation;
        var drawMarginSize = chart.DrawMarginSize;
        var secondaryScale = secondaryAxis.GetNextScaler(chart);
        var primaryScale = primaryAxis.GetNextScaler(chart);
        var previousPrimaryScale = primaryAxis.GetActualScaler(chart);
        var previousSecondaryScale = secondaryAxis.GetActualScaler(chart);

        // Cell size is driven by the actual data spacing (computed once per measure
        // cycle in GetBounds) rather than Axis.UnitWidth, which defaults to 1 and is
        // correct only for unit-stepped axes. See issue #1511.
        var xStep = double.IsNaN(_xStep) ? secondaryAxis.UnitWidth : _xStep;
        var yStep = double.IsNaN(_yStep) ? primaryAxis.UnitWidth : _yStep;
        var uws = secondaryScale.MeasureInPixels(xStep);
        var uwp = primaryScale.MeasureInPixels(yStep);

        if (_heatKnownLength != HeatMap.Length)
        {
            _heatStops = HeatFunctions.BuildColorStops(HeatMap, ColorStops);
            _heatKnownLength = HeatMap.Length;
        }

        var hasSvg = this.HasVariableSvgGeometry();
        var isFirstDraw = !((Chart)chart).IsDrawn(((ISeries)this).SeriesId);

        return new HeatMeasureContext(
            chart, primaryAxis, secondaryAxis,
            primaryScale, secondaryScale,
            previousPrimaryScale, previousSecondaryScale,
            cellWidth: uws,
            cellHeight: uwp,
            pointPadding: PointPadding,
            weightBounds: WeightBounds,
            heatMap: HeatMap,
            heatStops: _heatStops,
            isFirstDraw: isFirstDraw,
            hasSvg: hasSvg,
            drawLocation: drawLocation,
            drawMarginSize: drawMarginSize,
            dataLabelsSize: (float)DataLabelsSize);
    }

    /// <summary>
    /// Computes the final-frame cell rect + interpolated color for a single
    /// heat cell.
    /// </summary>
    protected virtual HeatLayout MeasureHeatLayout(ChartPoint point, in HeatMeasureContext ctx)
    {
        var coordinate = point.Coordinate;
        var primary = ctx.PrimaryScale.ToPixels(coordinate.PrimaryValue);
        var secondary = ctx.SecondaryScale.ToPixels(coordinate.SecondaryValue);
        var tertiary = (float)coordinate.TertiaryValue;

        var color = HeatFunctions.InterpolateColor(tertiary, ctx.WeightBounds, ctx.HeatMap, ctx.HeatStops);

        var uws = ctx.CellWidth;
        var uwp = ctx.CellHeight;
        var p = ctx.PointPadding;

        return new HeatLayout(
            x: secondary - uws * 0.5f + p.Left,
            y: primary - uwp * 0.5f + p.Top,
            width: uws - p.Left - p.Right,
            height: uwp - p.Top - p.Bottom,
            hoverX: secondary - uws * 0.5f,
            hoverY: primary - uwp * 0.5f,
            hoverWidth: uws,
            hoverHeight: uwp,
            color: color);
    }

    /// <summary>
    /// Ensures the cell visual exists. On first creation initializes it with
    /// transparent color so the heat color animates in via alpha. Mid-life
    /// entries (the previous-scale-available branch) source the initial X/Y
    /// from where the cell would have been in the previous frame's scaling.
    /// </summary>
    protected virtual TVisual EnsureVisualForPoint(ChartPoint point, in HeatMeasureContext ctx)
    {
        var visual = point.Context.Visual as TVisual;
        if (visual is not null) return visual;

        var coordinate = point.Coordinate;
        var uws = ctx.CellWidth;
        var uwp = ctx.CellHeight;
        var p = ctx.PointPadding;

        var secondary = ctx.SecondaryScale.ToPixels(coordinate.SecondaryValue);
        var primary = ctx.PrimaryScale.ToPixels(coordinate.PrimaryValue);

        var xi = secondary - uws * 0.5f;
        var yi = primary - uwp * 0.5f;

        if (ctx.PreviousSecondaryScale is not null && ctx.PreviousPrimaryScale is not null)
        {
            xi = ctx.PreviousSecondaryScale.ToPixels(coordinate.SecondaryValue) - uws * 0.5f;
            yi = ctx.PreviousPrimaryScale.ToPixels(coordinate.PrimaryValue) - uwp * 0.5f;
        }

        var baseColor = HeatFunctions.InterpolateColor(
            (float)coordinate.TertiaryValue, ctx.WeightBounds, ctx.HeatMap, ctx.HeatStops);

        var r = new TVisual
        {
            X = xi + p.Left,
            Y = yi + p.Top,
            Width = uws - p.Left - p.Right,
            Height = uwp - p.Top - p.Bottom,
            Color = LvcColor.FromArgb(0, baseColor.R, baseColor.G, baseColor.B),
        };

        point.Context.Visual = r;
        OnPointCreated(point);

        _ = everFetched.Add(point);

        return r;
    }

    /// <summary>
    /// Collapses the cell to its current grid position with alpha=0 + remove-on-completed.
    /// </summary>
    protected virtual void CollapseEmptyVisual(ChartPoint point, in HeatMeasureContext ctx)
    {
        var coordinate = point.Coordinate;
        var uws = ctx.CellWidth;
        var uwp = ctx.CellHeight;
        var secondary = ctx.SecondaryScale.ToPixels(coordinate.SecondaryValue);
        var primary = ctx.PrimaryScale.ToPixels(coordinate.PrimaryValue);

        if (point.Context.Visual is TVisual visual)
        {
            visual.X = secondary - uws * 0.5f;
            visual.Y = primary - uwp * 0.5f;
            visual.Width = uws;
            visual.Height = uwp;
            visual.RemoveOnCompleted = true;
            visual.Color = LvcColor.FromArgb(0, visual.Color);
            point.Context.Visual = null;
        }

        if (point.Context.Label is TLabel label)
        {
            label.X = secondary - uws * 0.5f;
            label.Y = primary - uwp * 0.5f;
            label.Opacity = 0;
            label.RemoveOnCompleted = true;
            point.Context.Label = null;
        }
    }

    /// <summary>
    /// Sets per-Z-index ordering on the heat-fill paint + data-labels paint and
    /// registers them as drawable tasks. Heat has no Fill / Stroke / ErrorPaint;
    /// the shared <c>_paintTaks</c> SolidColorPaint carries every cell's color
    /// via the per-visual <c>Color</c> property.
    /// </summary>
    private void InitializePaints(CartesianChartEngine chart)
    {
        if (_paintTaks is null)
        {
            _paintTaks = LiveCharts.DefaultSettings.GetProvider().GetSolidColorPaint();
            _paintTaks.PaintStyle = PaintStyle.Fill;
        }

        var actualZIndex = ZIndex == 0 ? ((ISeries)this).SeriesId : ZIndex;

        _paintTaks.ZIndex = actualZIndex + PaintConstants.SeriesStrokeZIndexOffset;
        chart.Canvas.AddDrawableTask(_paintTaks, zone: CanvasZone.DrawMargin);

        if (ShowDataLabels && DataLabelsPaint is not null && DataLabelsPaint != Paint.Default)
        {
            DataLabelsPaint.ZIndex = actualZIndex + PaintConstants.SeriesGeometryFillZIndexOffset;
            chart.Canvas.AddDrawableTask(DataLabelsPaint, zone: CanvasZone.DrawMargin);
        }
    }

    /// <summary>
    /// Creates the data label visual if it doesn't exist yet, updates its text
    /// + style, and positions it via <c>GetLabelPosition</c> with the cell rect.
    /// </summary>
    private void MeasureDataLabel(ChartPoint point, in HeatLayout layout, in HeatMeasureContext ctx)
    {
        if (!ShowDataLabels || DataLabelsPaint is null || DataLabelsPaint == Paint.Default) return;

        var chart = ctx.Chart;
        var label = (TLabel?)point.Context.Label;

        if (label is null)
        {
            // Preserves the original CoreHeatSeries quirk: initial label Y used
            // `uws` (the X step) instead of `uwp` (the Y step). The label is
            // animation-sourced so the position only matters for the first frame;
            // snapshot baselines pin this behavior.
            var primary = ctx.PrimaryScale.ToPixels(point.Coordinate.PrimaryValue);
            var l = new TLabel
            {
                X = layout.HoverX,
                Y = primary - ctx.CellWidth * 0.5f,
                RotateTransform = (float)DataLabelsRotation,
                MaxWidth = (float)DataLabelsMaxWidth,
            };
            l.Animate(
                GetAnimation(chart),
                BaseLabelGeometry.XProperty,
                BaseLabelGeometry.YProperty);
            label = l;
            point.Context.Label = l;
        }

        DataLabelsPaint.AddGeometryToPaintTask(chart.Canvas, label);

        label.Text = DataLabelsFormatter(new ChartPoint<TModel, TVisual, TLabel>(point));
        label.TextSize = ctx.DataLabelsSize;
        label.Padding = DataLabelsPadding;
        label.Paint = DataLabelsPaint;

        if (ctx.IsFirstDraw)
            label.CompleteTransition(
                BaseLabelGeometry.TextSizeProperty,
                BaseLabelGeometry.XProperty,
                BaseLabelGeometry.YProperty,
                BaseLabelGeometry.RotateTransformProperty);

        var labelPosition = GetLabelPosition(
            layout.X, layout.Y, layout.Width, layout.Height,
            label.Measure(), DataLabelsPosition, SeriesProperties,
            point.Coordinate.PrimaryValue > Pivot, ctx.DrawLocation, ctx.DrawMarginSize);
        label.X = labelPosition.X;
        label.Y = labelPosition.Y;
    }

    /// <inheritdoc cref="ChartElement.Invalidate(Chart)"/>
    public sealed override void Invalidate(Chart chart)
    {
        var cartesianChart = (CartesianChartEngine)chart;
        _ = GetAnimation(cartesianChart);

        var ctx = BeginMeasure(cartesianChart);
        var pointsCleanup = ChartPointCleanupContext.For(everFetched);

        InitializePaints(cartesianChart);

        foreach (var point in Fetch(cartesianChart))
        {
            if (point.IsEmpty || !IsVisible)
            {
                CollapseEmptyVisual(point, in ctx);
                pointsCleanup.Clean(point);
                continue;
            }

            var visual = EnsureVisualForPoint(point, in ctx);

            if (ctx.HasSvg)
            {
                var svgVisual = (IVariableSvgPath)visual;
                if (_geometrySvgChanged || svgVisual.SVGPath is null)
                    svgVisual.SVGPath = GeometrySvg ?? throw new Exception("svg path is not defined");
            }

            _paintTaks?.AddGeometryToPaintTask(cartesianChart.Canvas, visual);

            var layout = MeasureHeatLayout(point, in ctx);

            visual.X = layout.X;
            visual.Y = layout.Y;
            visual.Width = layout.Width;
            visual.Height = layout.Height;
            visual.Color = layout.Color;
            visual.RemoveOnCompleted = false;

            if (point.Context.HoverArea is not RectangleHoverArea ha)
                point.Context.HoverArea = ha = new RectangleHoverArea();
            _ = ha
                .SetDimensions(layout.HoverX, layout.HoverY, layout.HoverWidth, layout.HoverHeight)
                .CenterXToolTip()
                .CenterYToolTip();

            pointsCleanup.Clean(point);

            MeasureDataLabel(point, in layout, in ctx);

            OnPointMeasured(point);
        }

        pointsCleanup.CollectPoints(
            everFetched, cartesianChart.View, ctx.PrimaryScale, ctx.SecondaryScale, SoftDeleteOrDisposePoint);
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
