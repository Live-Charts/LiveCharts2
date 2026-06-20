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
using LiveChartsCore.Drawing.Segments;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Drawing;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.Motion;
using LiveChartsCore.Painting;

namespace LiveChartsCore;

/// <summary>
/// Defines a scatter series.
/// </summary>
/// <typeparam name="TModel">The type of the model.</typeparam>
/// <typeparam name="TVisual">The type of the visual.</typeparam>
/// <typeparam name="TLabel">The type of the label.</typeparam>
/// <typeparam name="TErrorGeometry">The type of the error geometry.</typeparam>
/// <seealso cref="CartesianSeries{TModel, TVisual, TLabel}" />
/// <seealso cref="IScatterSeries" />
public abstract class CoreScatterSeries<TModel, TVisual, TLabel, TErrorGeometry>
    : StrokeAndFillCartesianSeries<TModel, TVisual, TLabel>, IScatterSeries
        where TVisual : BoundedDrawnGeometry, new()
        where TLabel : BaseLabelGeometry, new()
        where TErrorGeometry : BaseLineGeometry, new()
{
    private bool _showError;

    /// <summary>
    /// Initializes a new instance of the <see cref="CoreScatterSeries{TModel, TVisual, TLabel, TErrorGeometry}"/> class.
    /// </summary>
    /// <param name="values">The values.</param>
    public CoreScatterSeries(IReadOnlyCollection<TModel>? values)
        : base(GetProperties(), values)
    {
        DataPadding = new LvcPoint(1, 1);

        DataLabelsFormatter = (point) => $"{point.Coordinate.PrimaryValue}";
        YToolTipLabelFormatter = point =>
        {
            var series = (CoreScatterSeries<TModel, TVisual, TLabel, TErrorGeometry>)point.Context.Series;
            var c = point.Coordinate;
            return series.IsWeighted
                ? $"X = {c.SecondaryValue}{Environment.NewLine}" +
                  $"Y = {c.PrimaryValue}{Environment.NewLine}" +
                  $"W = {c.TertiaryValue}"
                : $"X = {c.SecondaryValue}{Environment.NewLine}" +
                  $"Y = {c.PrimaryValue}";
        };
    }

    /// <summary>
    /// Gets or sets the minimum size of the geometry.
    /// </summary>
    /// <value>
    /// The minimum size of the geometry.
    /// </value>
    public double MinGeometrySize { get; set => SetProperty(ref field, value); } = 6d;
    /// <summary>
    /// Gets or sets the size of the geometry.
    /// </summary>
    /// <value>
    /// The size of the geometry.
    /// </value>
    public double GeometrySize { get; set => SetProperty(ref field, value); } = 24d;

    /// <summary>
    /// Gets a value indicating whether the points in this series use weight.
    /// </summary>
    public bool IsWeighted { get; private set; }

    /// <inheritdoc cref="IErrorSeries.ShowError"/>
    public bool ShowError
    {
        get => _showError;
        set
        {
            SetProperty(ref _showError, value);
            ErrorPaint?.IsPaused = !value;
        }
    }

    /// <inheritdoc cref="IErrorSeries.ErrorPaint"/>
    public Paint? ErrorPaint
    {
        get;
        set
        {
            SetPaintProperty(ref field, value, PaintStyle.Stroke);
            _showError = value is not null && value != Paint.Default;
        }
    } = Paint.Default;

    /// <inheritdoc cref="IScatterSeries.StackGroup"/>
    public int? StackGroup { get; set => SetProperty(ref field, value); }

    // ---- template method ----------------------------------------------------

    /// <summary>
    /// Builds a per-frame measure context from the chart. Subclasses may
    /// override to refine context construction (e.g. additional pre-computed
    /// per-frame values).
    /// </summary>
    protected virtual ScatterMeasureContext BeginMeasure(CartesianChartEngine chart)
    {
        var primaryAxis = chart.GetYAxis(this);
        var secondaryAxis = chart.GetXAxis(this);

        var drawLocation = chart.DrawMarginLocation;
        var drawMarginSize = chart.DrawMarginSize;
        var xScale = new Scaler(drawLocation, drawMarginSize, secondaryAxis);
        var yScale = new Scaler(drawLocation, drawMarginSize, primaryAxis);

        var weightStackIndex = StackGroup ?? ((ISeries)this).SeriesId;
        var weightBounds = chart.SeriesContext.GetWeightBounds(weightStackIndex);

        IsWeighted = weightBounds.Max - weightBounds.Min > 0;
        var wm = -(GeometrySize - MinGeometrySize) / (weightBounds.Max - weightBounds.Min);

        var hasSvg = this.HasVariableSvgGeometry();
        var isFirstDraw = !((Chart)chart).IsDrawn(((ISeries)this).SeriesId);

        return new ScatterMeasureContext(
            chart, primaryAxis, secondaryAxis,
            xScale, yScale,
            baseGeometrySize: (float)GeometrySize,
            isWeighted: IsWeighted,
            weightBounds: weightBounds,
            weightMultiplier: wm,
            isFirstDraw: isFirstDraw,
            hasSvg: hasSvg,
            drawLocation: drawLocation,
            drawMarginSize: drawMarginSize,
            dataLabelsSize: (float)DataLabelsSize);
    }

    /// <summary>
    /// Computes the final-frame marker geometry for a single point. Default
    /// implementation produces a square marker of <see cref="GeometrySize"/>
    /// (or weight-adjusted size when the stack group carries a weight range)
    /// centered on the data point.
    /// </summary>
    protected virtual ScatterLayout MeasureScatterLayout(ChartPoint point, in ScatterMeasureContext ctx)
    {
        var coordinate = point.Coordinate;
        var dataX = ctx.XScale.ToPixels(coordinate.SecondaryValue);
        var dataY = ctx.YScale.ToPixels(coordinate.PrimaryValue);

        var gs = ctx.BaseGeometrySize;
        if (ctx.IsWeighted)
            gs = (float)(ctx.WeightMultiplier * (ctx.WeightBounds.Max - coordinate.TertiaryValue) + GeometrySize);
        var hgs = gs * 0.5f;

        return new ScatterLayout(
            x: dataX - hgs,
            y: dataY - hgs,
            width: gs,
            height: gs,
            dataX: dataX,
            dataY: dataY);
    }

    /// <summary>
    /// Ensures the visual + any additional visuals (error bars) exist for the
    /// point. On first creation initializes the visual at the data point with
    /// zero size so the marker animates from a point.
    /// </summary>
    protected virtual TVisual EnsureVisualForPoint(ChartPoint point, in ScatterMeasureContext ctx)
    {
        var visual = (TVisual?)point.Context.Visual;

        if (visual is not null)
        {
            AttachErrorVisualsToPaint(point.Context.AdditionalVisuals as ErrorVisual<TErrorGeometry>, ctx.Chart.Canvas);
            return visual;
        }

        var coordinate = point.Coordinate;
        var x = ctx.XScale.ToPixels(coordinate.SecondaryValue);
        var y = ctx.YScale.ToPixels(coordinate.PrimaryValue);

        var r = new TVisual
        {
            X = x,
            Y = y,
            Width = 0,
            Height = 0,
        };

        ErrorVisual<TErrorGeometry>? e = null;
        if (ShowError && ErrorPaint is not null && ErrorPaint != Paint.Default)
        {
            e = new ErrorVisual<TErrorGeometry>();

            e.YError.X = x;
            e.YError.X1 = x;
            e.YError.Y = y;
            e.YError.Y1 = y;

            e.XError.X = x;
            e.XError.X1 = x;
            e.XError.Y = y;
            e.XError.Y1 = y;

            point.Context.AdditionalVisuals = e;
        }

        point.Context.Visual = r;
        OnPointCreated(point);

        _ = everFetched.Add(point);

        AttachErrorVisualsToPaint(e, ctx.Chart.Canvas);

        return r;
    }

    /// <summary>
    /// Per-frame error-bar geometry update. No-op when the series carries no
    /// error visuals or no point error data.
    /// </summary>
    protected virtual void MeasureErrorBars(ChartPoint point, in ScatterLayout layout, in ScatterMeasureContext ctx)
    {
        var coordinate = point.Coordinate;
        if (coordinate.PointError.IsEmpty) return;
        if (!ShowError || ErrorPaint is null || ErrorPaint == Paint.Default) return;
        if (point.Context.AdditionalVisuals is not ErrorVisual<TErrorGeometry> e) return;

        var pe = coordinate.PointError;

        e.YError!.X = layout.DataX;
        e.YError.X1 = layout.DataX;
        e.YError.Y = layout.DataY + ctx.YScale.MeasureInPixels(pe.Yi);
        e.YError.Y1 = layout.DataY - ctx.YScale.MeasureInPixels(pe.Yj);
        e.YError.RemoveOnCompleted = false;

        e.XError!.X = layout.DataX - ctx.XScale.MeasureInPixels(pe.Xi);
        e.XError.X1 = layout.DataX + ctx.XScale.MeasureInPixels(pe.Xj);
        e.XError.Y = layout.DataY;
        e.XError.Y1 = layout.DataY;
        e.XError.RemoveOnCompleted = false;
    }

    /// <summary>
    /// Collapses the point's visual to zero size at the data point for
    /// empty/invisible points.
    /// </summary>
    protected virtual void CollapseEmptyVisual(ChartPoint point, in ScatterMeasureContext ctx)
    {
        var coordinate = point.Coordinate;
        var x = ctx.XScale.ToPixels(coordinate.SecondaryValue);
        var y = ctx.YScale.ToPixels(coordinate.PrimaryValue);
        var hgs = ctx.BaseGeometrySize * 0.5f;

        if (point.Context.Visual is TVisual visual)
        {
            visual.X = x - hgs;
            visual.Y = y - hgs;
            visual.Width = 0;
            visual.Height = 0;
            visual.RemoveOnCompleted = true;
            point.Context.Visual = null;
        }

        if (point.Context.Label is TLabel label)
        {
            // Preserves the original CoreScatterSeries.Invalidate collapse pattern,
            // where the empty-label X / Y both used (x - hgs). Snapshot baselines
            // pin this; if it ever looks wrong the fix is to set label.Y = y - hgs.
            label.X = x - hgs;
            label.Y = x - hgs;
            label.Opacity = 0;
            label.RemoveOnCompleted = true;
            point.Context.Label = null;
        }
    }

    /// <summary>
    /// Sets the per-Z-index ordering on each non-default paint and registers it as a
    /// drawable task on the chart's canvas (within the DrawMargin zone). Run once at
    /// the top of <see cref="Invalidate"/>.
    /// </summary>
    private void InitializePaints(CartesianChartEngine chart)
    {
        var actualZIndex = ZIndex == 0 ? ((ISeries)this).SeriesId : ZIndex;
        if (Fill is not null && Fill != Paint.Default)
        {
            Fill.ZIndex = actualZIndex + PaintConstants.SeriesFillZIndexOffset;
            chart.Canvas.AddDrawableTask(Fill, zone: CanvasZone.DrawMargin);
        }
        if (Stroke is not null && Stroke != Paint.Default)
        {
            Stroke.ZIndex = actualZIndex + PaintConstants.SeriesStrokeZIndexOffset;
            chart.Canvas.AddDrawableTask(Stroke, zone: CanvasZone.DrawMargin);
        }
        if (ShowError && ErrorPaint is not null && ErrorPaint != Paint.Default)
        {
            ErrorPaint.ZIndex = actualZIndex + PaintConstants.SeriesGeometryFillZIndexOffset;
            chart.Canvas.AddDrawableTask(ErrorPaint, zone: CanvasZone.DrawMargin);
        }
        if (ShowDataLabels && DataLabelsPaint is not null && DataLabelsPaint != Paint.Default)
        {
            DataLabelsPaint.ZIndex = actualZIndex + PaintConstants.SeriesGeometryStrokeZIndexOffset;
            chart.Canvas.AddDrawableTask(DataLabelsPaint, zone: CanvasZone.DrawMargin);
        }
    }

    /// <summary>
    /// Creates the data label visual if it doesn't exist yet (animation-sourced from
    /// the marker's top-left), updates its text + style, and positions it via
    /// <c>GetLabelPosition</c> with the marker rect and the point's "above pivot" hint.
    /// No-op when the series has no data-label paint configured.
    /// </summary>
    private void MeasureDataLabel(ChartPoint point, in ScatterLayout layout, in ScatterMeasureContext ctx)
    {
        if (!ShowDataLabels || DataLabelsPaint is null || DataLabelsPaint == Paint.Default) return;

        var chart = ctx.Chart;
        var label = (TLabel?)point.Context.Label;

        if (label is null)
        {
            var l = new TLabel
            {
                X = layout.X,
                Y = layout.Y,
                RotateTransform = (float)DataLabelsRotation,
                MaxWidth = (float)DataLabelsMaxWidth
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

        var m = label.Measure();
        var labelPosition = GetLabelPosition(
            layout.X, layout.Y, layout.Width, layout.Height, m, DataLabelsPosition,
            SeriesProperties, point.Coordinate.PrimaryValue > 0, ctx.DrawLocation, ctx.DrawMarginSize);
        if (DataLabelsTranslate is not null)
            label.TranslateTransform = new LvcPoint(
                m.Width * DataLabelsTranslate.Value.X, m.Height * DataLabelsTranslate.Value.Y);

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

            if (Fill is not null && Fill != Paint.Default)
                Fill.AddGeometryToPaintTask(cartesianChart.Canvas, visual);
            if (Stroke is not null && Stroke != Paint.Default)
                Stroke.AddGeometryToPaintTask(cartesianChart.Canvas, visual);

            var layout = MeasureScatterLayout(point, in ctx);

            visual.X = layout.X;
            visual.Y = layout.Y;
            visual.Width = layout.Width;
            visual.Height = layout.Height;

            MeasureErrorBars(point, in layout, in ctx);

            visual.RemoveOnCompleted = false;

            if (point.Context.HoverArea is not RectangleHoverArea ha)
                point.Context.HoverArea = ha = new RectangleHoverArea();
            _ = ha.SetDimensions(layout.X, layout.Y, layout.Width, layout.Height).CenterXToolTip().CenterYToolTip();

            pointsCleanup.Clean(point);

            MeasureDataLabel(point, in layout, in ctx);

            OnPointMeasured(point);
        }

        pointsCleanup.CollectPoints(everFetched, cartesianChart.View, ctx.YScale, ctx.XScale, SoftDeleteOrDisposePoint);
        _geometrySvgChanged = false;
    }

    private void AttachErrorVisualsToPaint(ErrorVisual<TErrorGeometry>? e, CoreMotionCanvas canvas)
    {
        if (e is null) return;
        if (ErrorPaint is null || ErrorPaint == Paint.Default) return;
        ErrorPaint.AddGeometryToPaintTask(canvas, e.YError);
        ErrorPaint.AddGeometryToPaintTask(canvas, e.XError);
    }

    /// <inheritdoc cref="CartesianSeries{TModel, TVisual, TLabel}.GetBounds(Chart, ICartesianAxis, ICartesianAxis)"/>
    public override SeriesBounds GetBounds(Chart chart, ICartesianAxis secondaryAxis, ICartesianAxis primaryAxis)
    {
        var seriesBounds = base.GetBounds(chart, secondaryAxis, primaryAxis);

        chart.SeriesContext.AppendWeightBounds(
            StackGroup ?? ((ISeries)this).SeriesId, seriesBounds.Bounds.TertiaryBounds);

        return seriesBounds;
    }

    /// <inheritdoc cref="Series{TModel, TVisual, TLabel}.GetMiniatureGeometry(ChartPoint)"/>
    public override IDrawnElement GetMiniatureGeometry(ChartPoint? point)
    {
        var v = point?.Context.Visual;

        var m = new TVisual
        {
            Fill = v?.Fill ?? Fill,
            Stroke = v?.Stroke ?? Stroke,
            StrokeThickness = (float)MiniatureStrokeThickness,
            ClippingBounds = LvcRectangle.Empty,
            Width = (float)MiniatureShapeSize,
            Height = (float)MiniatureShapeSize,
            RotateTransform = v?.RotateTransform ?? 0,
        };

        if (m is IVariableSvgPath svg) svg.SVGPath = GeometrySvg;

        return m;
    }

    /// <inheritdoc cref="ChartElement.GetPaintTasks"/>
    protected internal override Paint?[] GetPaintTasks() =>
        [Stroke, Fill, DataLabelsPaint, ErrorPaint];

    /// <inheritdoc cref="SetDefaultPointTransitions(ChartPoint)"/>
    protected override void SetDefaultPointTransitions(ChartPoint chartPoint)
    {
        var visual = (TVisual?)chartPoint.Context.Visual;
        var chart = chartPoint.Context.Chart;

        if (visual is null) throw new Exception("Unable to initialize the point instance.");

        var animation = GetAnimation(chart.CoreChart);

        visual.Animate(animation);

        if (chartPoint.Context.AdditionalVisuals is not null)
        {
            var e = (ErrorVisual<TErrorGeometry>)chartPoint.Context.AdditionalVisuals;
            e.YError.Animate(animation);
            e.XError.Animate(animation);
        }
    }

    /// <inheritdoc cref="SoftDeleteOrDisposePoint(ChartPoint, Scaler, Scaler)"/>
    protected internal override void SoftDeleteOrDisposePoint(ChartPoint point, Scaler primaryScale, Scaler secondaryScale)
    {
        var visual = (TVisual?)point.Context.Visual;
        if (visual is null) return;
        if (DataFactory is null) throw new Exception("Data provider not found");

        var c = point.Coordinate;

        var x = secondaryScale.ToPixels(c.SecondaryValue);
        var y = primaryScale.ToPixels(c.PrimaryValue);

        visual.X = x;
        visual.Y = y;
        visual.Height = 0;
        visual.Width = 0;
        visual.RemoveOnCompleted = true;

        if (point.Context.AdditionalVisuals is not null)
        {
            var e = (ErrorVisual<TErrorGeometry>)point.Context.AdditionalVisuals;

            e.YError.Y = y;
            e.YError.Y1 = y;
            e.YError.RemoveOnCompleted = true;

            e.XError.X = x;
            e.XError.X1 = x;
            e.XError.RemoveOnCompleted = true;
        }

        DataFactory.DisposePoint(point);

        var label = (TLabel?)point.Context.Label;
        if (label is null) return;

        label.TextSize = 1;
        label.RemoveOnCompleted = true;
    }

    private static SeriesProperties GetProperties() =>
        SeriesProperties.Scatter | SeriesProperties.Solid | SeriesProperties.PrefersXYStrategyTooltips;
}
