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
/// Defines a line-shaped series whose stroke traces two curves — a "high" curve
/// over <see cref="Kernel.Coordinate.PrimaryValue"/> and a "low" curve over
/// <see cref="Kernel.Coordinate.TertiaryValue"/> — with the fill drawn as the
/// band between them. The simplest point model is <see cref="Defaults.RangeValue"/>;
/// custom models can map their endpoints into <c>PrimaryValue = high</c> and
/// <c>TertiaryValue = low</c>. Stacking and error bars don't apply — the band
/// is the uncertainty band.
/// </summary>
public abstract class CoreRangeLineSeries<TModel, TVisual, TLabel, TStrokePathGeometry, TBandPathGeometry>
    : StrokeAndFillCartesianSeries<TModel, TVisual, TLabel>, ILineSeries
        where TStrokePathGeometry : BaseVectorGeometry, new()
        where TBandPathGeometry : BaseVectorGeometry, IBandVectorGeometry, new()
        where TVisual : BoundedDrawnGeometry, new()
        where TLabel : BaseLabelGeometry, new()
{
    private readonly Dictionary<object, List<TStrokePathGeometry>> _highStrokePathDictionary = [];
    private readonly Dictionary<object, List<TStrokePathGeometry>> _lowStrokePathDictionary = [];
    private readonly Dictionary<object, List<TBandPathGeometry>> _bandFillPathDictionary = [];
    private float _lineSmoothness = 0.65f;
    private float _geometrySize = 14f;

    /// <summary>
    /// Initializes a new instance of <see cref="CoreRangeLineSeries{TModel, TVisual, TLabel, TStrokePathGeometry, TBandPathGeometry}"/>.
    /// </summary>
    /// <param name="values">The values.</param>
    protected CoreRangeLineSeries(IReadOnlyCollection<TModel>? values)
        : base(GetProperties(), values)
    {
        DataPadding = new LvcPoint(0.5f, 1f);
    }

    /// <inheritdoc cref="ILineSeries.GeometrySize"/>
    public double GeometrySize { get => _geometrySize; set => SetProperty(ref _geometrySize, (float)value); }

    /// <inheritdoc cref="ILineSeries.LineSmoothness"/>
    public double LineSmoothness
    {
        get => _lineSmoothness;
        set
        {
            var v = value;
            if (value > 1) v = 1;
            if (value < 0) v = 0;
            SetProperty(ref _lineSmoothness, (float)v);
        }
    }

    /// <inheritdoc cref="ILineSeries.EnableNullSplitting"/>
    public bool EnableNullSplitting { get; set => SetProperty(ref field, value); } = true;

    /// <inheritdoc cref="ILineSeries.GeometryFill"/>
    public Paint? GeometryFill
    {
        get;
        set => SetPaintProperty(ref field, value);
    } = Paint.Default;

    /// <inheritdoc cref="ILineSeries.GeometryStroke"/>
    public Paint? GeometryStroke
    {
        get;
        set => SetPaintProperty(ref field, value, PaintStyle.Stroke);
    } = Paint.Default;

    bool IErrorSeries.ShowError { get => false; set { } }
    Paint? IErrorSeries.ErrorPaint { get => null; set { } }

    // ---- template method ----------------------------------------------------

    /// <summary>
    /// Builds a per-frame measure context from the chart. Subclasses may override
    /// to refine context construction (e.g. additional pre-computed per-frame values).
    /// </summary>
    protected virtual RangeLineMeasureContext BeginMeasure(CartesianChartEngine chart)
    {
        var primaryAxis = chart.GetYAxis(this);
        var secondaryAxis = chart.GetXAxis(this);

        var drawLocation = chart.DrawMarginLocation;
        var drawMarginSize = chart.DrawMarginSize;
        var secondaryScale = secondaryAxis.GetNextScaler(chart);
        var primaryScale = primaryAxis.GetNextScaler(chart);

        // Side-effect call (matches CoreLineSeries.BeginMeasure): drops the registration
        // into the axis scaler cache for this frame. Locals are unused.
        _ = secondaryAxis.GetActualScaler(chart);
        _ = primaryAxis.GetActualScaler(chart);

        var actualZIndex = ZIndex == 0 ? ((ISeries)this).SeriesId : ZIndex;

        var gs = _geometrySize;
        var hgs = gs / 2f;

        var uwx = secondaryScale.MeasureInPixels(secondaryAxis.UnitWidth);
        if (uwx < gs) uwx = gs;

        var isFirstDraw = !chart.IsDrawn(((ISeries)this).SeriesId);

        return new RangeLineMeasureContext(
            chart, primaryAxis, secondaryAxis,
            primaryScale, secondaryScale,
            drawLocation, drawMarginSize,
            actualZIndex: actualZIndex,
            unitWidthX: uwx,
            geometrySize: gs,
            halfGeometrySize: hgs,
            dataLabelsSize: (float)DataLabelsSize,
            isFirstDraw: isFirstDraw);
    }

    /// <summary>
    /// Ensures the per-point visual (high marker, low marker, high segment, low
    /// segment) exists, seeded at the band midpoint with zero size so first-draw
    /// animates outward to the real endpoints.
    /// </summary>
    protected virtual RangeCubicSegmentVisualPoint EnsureVisualForPoint(
        ChartPoint point, BezierData highData, BezierData lowData, in RangeLineMeasureContext ctx)
    {
        var coordinate = point.Coordinate;
        var x = ctx.SecondaryScale.ToPixels(coordinate.SecondaryValue);
        var highY = ctx.PrimaryScale.ToPixels(coordinate.PrimaryValue);
        var lowY = ctx.PrimaryScale.ToPixels(coordinate.TertiaryValue);
        var midY = (highY + lowY) * 0.5f;

        var v = new RangeCubicSegmentVisualPoint(new TVisual(), new TVisual());

        // Seed both markers at the band midpoint with zero size — symmetric to the
        // range bar entry pattern (see CoreRangeColumnSeries.EnsureVisualForPoint).
        // High marker animates outward to highY; low marker animates outward to lowY.
        v.HighGeometry.X = x;
        v.HighGeometry.Y = midY;
        v.HighGeometry.Width = 0;
        v.HighGeometry.Height = 0;

        v.LowGeometry.X = x;
        v.LowGeometry.Y = midY;
        v.LowGeometry.Width = 0;
        v.LowGeometry.Height = 0;

        // Seed segments at the midpoint too: the spline starts as a degenerate line
        // along the band midpoints, then morphs into the actual cubic curves as the
        // setter below drives Xi/Ym/etc. toward their real target values.
        v.HighSegment.Xi = ctx.SecondaryScale.ToPixels(highData.X0);
        v.HighSegment.Xm = ctx.SecondaryScale.ToPixels(highData.X1);
        v.HighSegment.Xj = ctx.SecondaryScale.ToPixels(highData.X2);
        v.HighSegment.Yi = midY;
        v.HighSegment.Ym = midY;
        v.HighSegment.Yj = midY;

        v.LowSegment.Xi = ctx.SecondaryScale.ToPixels(lowData.X0);
        v.LowSegment.Xm = ctx.SecondaryScale.ToPixels(lowData.X1);
        v.LowSegment.Xj = ctx.SecondaryScale.ToPixels(lowData.X2);
        v.LowSegment.Yi = midY;
        v.LowSegment.Ym = midY;
        v.LowSegment.Yj = midY;

        point.Context.Visual = v.HighGeometry;
        point.Context.AdditionalVisuals = v;
        OnPointCreated(point);

        return v;
    }

    /// <summary>
    /// Collapses both markers and segments to the band midpoint when the series
    /// becomes invisible, and removes the visual / label from the point so future
    /// paints don't redraw them.
    /// </summary>
    protected virtual void CollapseInvisibleLinePoint(
        ChartPoint point, BezierData highData, BezierData lowData, in RangeLineMeasureContext ctx)
    {
        if (point.Context.AdditionalVisuals is RangeCubicSegmentVisualPoint visual)
        {
            var c = point.Coordinate;
            var highY = ctx.PrimaryScale.ToPixels(c.PrimaryValue);
            var lowY = ctx.PrimaryScale.ToPixels(c.TertiaryValue);
            var midY = (highY + lowY) * 0.5f;
            var x = ctx.SecondaryScale.ToPixels(c.SecondaryValue);

            visual.HighGeometry.X = x;
            visual.HighGeometry.Y = midY;
            visual.HighGeometry.Opacity = 0;
            visual.HighGeometry.RemoveOnCompleted = true;

            visual.LowGeometry.X = x;
            visual.LowGeometry.Y = midY;
            visual.LowGeometry.Opacity = 0;
            visual.LowGeometry.RemoveOnCompleted = true;

            visual.HighSegment.Xi = ctx.SecondaryScale.ToPixels(highData.X0);
            visual.HighSegment.Xm = ctx.SecondaryScale.ToPixels(highData.X1);
            visual.HighSegment.Xj = ctx.SecondaryScale.ToPixels(highData.X2);
            visual.HighSegment.Yi = midY;
            visual.HighSegment.Ym = midY;
            visual.HighSegment.Yj = midY;

            visual.LowSegment.Xi = ctx.SecondaryScale.ToPixels(lowData.X0);
            visual.LowSegment.Xm = ctx.SecondaryScale.ToPixels(lowData.X1);
            visual.LowSegment.Xj = ctx.SecondaryScale.ToPixels(lowData.X2);
            visual.LowSegment.Yi = midY;
            visual.LowSegment.Ym = midY;
            visual.LowSegment.Yj = midY;

            point.Context.Visual = null;
            point.Context.AdditionalVisuals = null;
        }

        if (point.Context.Label is BaseLabelGeometry label)
        {
            var c = point.Coordinate;
            var highY = ctx.PrimaryScale.ToPixels(c.PrimaryValue);
            var lowY = ctx.PrimaryScale.ToPixels(c.TertiaryValue);
            label.X = ctx.SecondaryScale.ToPixels(c.SecondaryValue);
            label.Y = (highY + lowY) * 0.5f;
            label.Opacity = 0;
            label.RemoveOnCompleted = true;
            point.Context.Label = null;
        }
    }

    /// <inheritdoc cref="ChartElement.Invalidate(Chart)"/>
    public sealed override void Invalidate(Chart chart)
    {
        var cartesianChart = (CartesianChartEngine)chart;
        _ = GetAnimation(cartesianChart);

        var ctx = BeginMeasure(cartesianChart);
        var pointsCleanup = ChartPointCleanupContext.For(everFetched);

        // Lifted out of the ref-struct context so the lambda below can close over
        // the scales — ref locals can't be captured by an anonymous method.
        var secondaryScale = ctx.SecondaryScale;
        var primaryScale = ctx.PrimaryScale;

        var segments = EnableNullSplitting
            ? Fetch(cartesianChart).SplitByNullGaps(p => DeleteNullPoint(p, secondaryScale, primaryScale))
            : [Fetch(cartesianChart)];

        if (!_highStrokePathDictionary.TryGetValue(cartesianChart.Canvas.Sync, out var highStrokeContainer))
        {
            highStrokeContainer = [];
            _highStrokePathDictionary[cartesianChart.Canvas.Sync] = highStrokeContainer;
        }

        if (!_lowStrokePathDictionary.TryGetValue(cartesianChart.Canvas.Sync, out var lowStrokeContainer))
        {
            lowStrokeContainer = [];
            _lowStrokePathDictionary[cartesianChart.Canvas.Sync] = lowStrokeContainer;
        }

        if (!_bandFillPathDictionary.TryGetValue(cartesianChart.Canvas.Sync, out var bandFillContainer))
        {
            bandFillContainer = [];
            _bandFillPathDictionary[cartesianChart.Canvas.Sync] = bandFillContainer;
        }

        var segmentI = 0;

        foreach (var segment in segments)
        {
            var hasPaths = false;
            var isSegmentEmpty = true;
            VectorManager? highStrokeVm = null;
            VectorManager? lowStrokeVm = null;
            VectorManager? bandHighVm = null;
            VectorManager? bandLowVm = null;

            foreach (var (highData, lowData) in GetPairedSplines(segment))
            {
                if (!hasPaths)
                {
                    hasPaths = true;
                    AttachSegmentPaths(
                        segmentI, highStrokeContainer, lowStrokeContainer, bandFillContainer, in ctx,
                        out highStrokeVm, out lowStrokeVm, out bandHighVm, out bandLowVm);
                }

                isSegmentEmpty = false;
                var point = highData.TargetPoint;
                var coordinate = point.Coordinate;
                var visual = (RangeCubicSegmentVisualPoint?)point.Context.AdditionalVisuals;
                var isVisualNew = visual is null;

                if (!IsVisible)
                {
                    CollapseInvisibleLinePoint(point, highData, lowData, in ctx);
                    pointsCleanup.Clean(point);
                    continue;
                }

                visual ??= EnsureVisualForPoint(point, highData, lowData, in ctx);
                visual.HighGeometry.Opacity = 1;
                visual.LowGeometry.Opacity = 1;

                _ = everFetched.Add(point);

                if (GeometryFill is not null && GeometryFill != Paint.Default)
                {
                    GeometryFill.AddGeometryToPaintTask(cartesianChart.Canvas, visual.HighGeometry);
                    GeometryFill.AddGeometryToPaintTask(cartesianChart.Canvas, visual.LowGeometry);
                }
                if (GeometryStroke is not null && GeometryStroke != Paint.Default)
                {
                    GeometryStroke.AddGeometryToPaintTask(cartesianChart.Canvas, visual.HighGeometry);
                    GeometryStroke.AddGeometryToPaintTask(cartesianChart.Canvas, visual.LowGeometry);
                }

                var segmentId = point.Context.Entity.MetaData!.EntityIndex;
                visual.HighSegment.Id = segmentId;
                visual.LowSegment.Id = segmentId;

                var animatedAsNew = isVisualNew && !ctx.IsFirstDraw;

                if (Stroke is not null)
                {
                    highStrokeVm!.AddConsecutiveSegment(visual.HighSegment, animatedAsNew);
                    lowStrokeVm!.AddConsecutiveSegment(visual.LowSegment, animatedAsNew);
                }
                if (Fill is not null)
                {
                    bandHighVm!.AddConsecutiveSegment(visual.HighSegment, animatedAsNew);
                    bandLowVm!.AddConsecutiveSegment(visual.LowSegment, animatedAsNew);
                }

                visual.HighSegment.Xi = ctx.SecondaryScale.ToPixels(highData.X0);
                visual.HighSegment.Xm = ctx.SecondaryScale.ToPixels(highData.X1);
                visual.HighSegment.Xj = ctx.SecondaryScale.ToPixels(highData.X2);
                visual.HighSegment.Yi = ctx.PrimaryScale.ToPixels(highData.Y0);
                visual.HighSegment.Ym = ctx.PrimaryScale.ToPixels(highData.Y1);
                visual.HighSegment.Yj = ctx.PrimaryScale.ToPixels(highData.Y2);

                visual.LowSegment.Xi = ctx.SecondaryScale.ToPixels(lowData.X0);
                visual.LowSegment.Xm = ctx.SecondaryScale.ToPixels(lowData.X1);
                visual.LowSegment.Xj = ctx.SecondaryScale.ToPixels(lowData.X2);
                visual.LowSegment.Yi = ctx.PrimaryScale.ToPixels(lowData.Y0);
                visual.LowSegment.Ym = ctx.PrimaryScale.ToPixels(lowData.Y1);
                visual.LowSegment.Yj = ctx.PrimaryScale.ToPixels(lowData.Y2);

                // Markers track their respective segment endpoints so they slide along
                // the curve as the underlying spline animates.
                DrawnGeometry.XProperty.GetMotion(visual.HighGeometry)!
                    .CopyFrom(Segment.XjProperty.GetMotion(visual.HighSegment)!);
                DrawnGeometry.YProperty.GetMotion(visual.HighGeometry)!
                    .CopyFrom(Segment.YjProperty.GetMotion(visual.HighSegment)!);
                DrawnGeometry.XProperty.GetMotion(visual.LowGeometry)!
                    .CopyFrom(Segment.XjProperty.GetMotion(visual.LowSegment)!);
                DrawnGeometry.YProperty.GetMotion(visual.LowGeometry)!
                    .CopyFrom(Segment.YjProperty.GetMotion(visual.LowSegment)!);

                visual.HighGeometry.TranslateTransform = new LvcPoint(-ctx.HalfGeometrySize, -ctx.HalfGeometrySize);
                visual.HighGeometry.Width = ctx.GeometrySize;
                visual.HighGeometry.Height = ctx.GeometrySize;
                visual.HighGeometry.RemoveOnCompleted = false;

                visual.LowGeometry.TranslateTransform = new LvcPoint(-ctx.HalfGeometrySize, -ctx.HalfGeometrySize);
                visual.LowGeometry.Width = ctx.GeometrySize;
                visual.LowGeometry.Height = ctx.GeometrySize;
                visual.LowGeometry.RemoveOnCompleted = false;

                var x = ctx.SecondaryScale.ToPixels(coordinate.SecondaryValue);
                var highY = ctx.PrimaryScale.ToPixels(coordinate.PrimaryValue);
                var lowY = ctx.PrimaryScale.ToPixels(coordinate.TertiaryValue);
                var minY = Math.Min(highY, lowY);
                var bandH = Math.Abs(highY - lowY);

                if (point.Context.HoverArea is not RectangleHoverArea ha)
                    point.Context.HoverArea = ha = new RectangleHoverArea();
                _ = ha
                    .SetDimensions(x - ctx.UnitWidthX * 0.5f, minY, ctx.UnitWidthX, bandH)
                    .CenterXToolTip()
                    .StartYToolTip();

                pointsCleanup.Clean(point);
                MeasureDataLabel(point, x, (highY + lowY) * 0.5f, in ctx);

                OnPointMeasured(point);
            }

            if (GeometryFill is not null && GeometryFill != Paint.Default)
            {
                cartesianChart.Canvas.AddDrawableTask(GeometryFill, zone: CanvasZone.DrawMargin);
                GeometryFill.ZIndex = ctx.ActualZIndex + PaintConstants.SeriesGeometryFillZIndexOffset;
            }
            if (GeometryStroke is not null && GeometryStroke != Paint.Default)
            {
                cartesianChart.Canvas.AddDrawableTask(GeometryStroke, zone: CanvasZone.DrawMargin);
                GeometryStroke.ZIndex = ctx.ActualZIndex + PaintConstants.SeriesGeometryStrokeZIndexOffset;
            }

            if (!isSegmentEmpty) segmentI++;

            highStrokeVm?.TrimTail();
            lowStrokeVm?.TrimTail();
            bandHighVm?.TrimTail();
            bandLowVm?.TrimTail();
        }

        CleanupOrphanSegmentPaths(segmentI, highStrokeContainer, lowStrokeContainer, bandFillContainer, cartesianChart);

        if (ShowDataLabels && DataLabelsPaint is not null && DataLabelsPaint != Paint.Default)
        {
            cartesianChart.Canvas.AddDrawableTask(DataLabelsPaint, zone: CanvasZone.DrawMargin);
            DataLabelsPaint.ZIndex = ctx.ActualZIndex + PaintConstants.SeriesDataLabelsZIndexOffset;
        }

        pointsCleanup.CollectPoints(
            everFetched, cartesianChart.View, ctx.PrimaryScale, ctx.SecondaryScale, SoftDeleteOrDisposePoint);
    }

    /// <inheritdoc cref="Series{TModel, TVisual, TLabel}.GetMiniatureGeometry(ChartPoint?)"/>
    public override IDrawnElement GetMiniatureGeometry(ChartPoint? point)
    {
        var v = point?.Context.AdditionalVisuals as RangeCubicSegmentVisualPoint;
        var sampled = v?.HighGeometry;

        var m = new TVisual
        {
            Fill = sampled?.Fill ?? GeometryFill ?? Fill,
            Stroke = sampled?.Stroke ?? GeometryStroke ?? Stroke,
            StrokeThickness = (float)MiniatureStrokeThickness,
            ClippingBounds = LvcRectangle.Empty,
            Width = (float)MiniatureShapeSize,
            Height = (float)MiniatureShapeSize,
            RotateTransform = sampled?.RotateTransform ?? 0,
        };
        return m;
    }

    /// <inheritdoc cref="Series{TModel, TVisual, TLabel}.FindPointsInPosition(Chart, LvcPoint, FindingStrategy, FindPointFor)"/>
    protected override IEnumerable<ChartPoint> FindPointsInPosition(
        Chart chart, LvcPoint pointerPosition, FindingStrategy strategy, FindPointFor findPointFor)
    {
        // ExactMatch hit-tests either marker. The band area itself is reachable via
        // the rectangular HoverArea attached during Invalidate, so X/Y/CompareAll
        // tooltip strategies (which use HoverArea, not the visual) work out of the box.
        bool Contains(BoundedDrawnGeometry v)
        {
            var x = v.X + v.TranslateTransform.X;
            var y = v.Y + v.TranslateTransform.Y;
            return pointerPosition.X > x && pointerPosition.X < x + v.Width &&
                   pointerPosition.Y > y && pointerPosition.Y < y + v.Height;
        }

        bool VisualContains(ChartPoint p)
        {
            if (p.Context.AdditionalVisuals is not RangeCubicSegmentVisualPoint v) return false;
            return Contains(v.HighGeometry) || Contains(v.LowGeometry);
        }

        return strategy switch
        {
            FindingStrategy.ExactMatch => Fetch(chart).Where(VisualContains),
            FindingStrategy.ExactMatchTakeClosest => Fetch(chart)
                .Where(VisualContains)
                .Select(p => new { distance = p.DistanceTo(pointerPosition, strategy), point = p })
                .OrderBy(p => p.distance)
                .SelectFirst(p => p.point),
            _ => base.FindPointsInPosition(chart, pointerPosition, strategy, findPointFor),
        };
    }

    /// <inheritdoc cref="GetRequestedGeometrySize"/>
    protected override double GetRequestedGeometrySize() =>
        (GeometrySize + (GeometryStroke?.StrokeThickness ?? 0)) * 0.5f;

    /// <inheritdoc cref="CartesianSeries{TModel, TVisual, TLabel}.GetBounds(Chart, ICartesianAxis, ICartesianAxis)"/>
    public override SeriesBounds GetBounds(Chart chart, ICartesianAxis secondaryAxis, ICartesianAxis primaryAxis)
    {
        var sb = base.GetBounds(chart, secondaryAxis, primaryAxis);
        if (sb.HasData) return sb;

        // PrimaryBounds is High, TertiaryBounds is Low — merge so an auto value axis
        // covers the full span. Mirrors CoreRangeColumnSeries.GetBounds.
        var b = sb.Bounds;
        b.PrimaryBounds.AppendValue(b.TertiaryBounds);
        b.VisiblePrimaryBounds.AppendValue(b.VisibleTertiaryBounds);
        return sb;
    }

    /// <inheritdoc cref="ISeries.GetPrimaryToolTipText(ChartPoint)"/>
    public override string? GetPrimaryToolTipText(ChartPoint point)
    {
        if (YToolTipLabelFormatter is not null)
            return YToolTipLabelFormatter(new ChartPoint<TModel, TVisual, TLabel>(point));

        var chart = (CartesianChartEngine)point.Context.Chart.CoreChart;
        var series = (ICartesianSeries)point.Context.Series;
        var valueAxis = chart.YAxes[series.ScalesYAt];

        var low = valueAxis.Labels is not null
            ? Labelers.BuildNamedLabeler(valueAxis.Labels)(point.Coordinate.TertiaryValue)
            : valueAxis.Labeler(point.Coordinate.TertiaryValue);
        var high = valueAxis.Labels is not null
            ? Labelers.BuildNamedLabeler(valueAxis.Labels)(point.Coordinate.PrimaryValue)
            : valueAxis.Labeler(point.Coordinate.PrimaryValue);

        return $"{low} → {high}";
    }

    /// <inheritdoc cref="Series{TModel, TVisual, TLabel}.SoftDeleteOrDispose(IChartView)"/>
    public override void SoftDeleteOrDispose(IChartView chart)
    {
        base.SoftDeleteOrDispose(chart);
        var canvas = ((ICartesianChartView)chart).CoreCanvas;

        if (Stroke is not null)
        {
            foreach (var c in _highStrokePathDictionary.ToArray())
                foreach (var p in c.Value.ToArray())
                    Stroke.RemoveGeometryFromPaintTask(canvas, p);

            foreach (var c in _lowStrokePathDictionary.ToArray())
                foreach (var p in c.Value.ToArray())
                    Stroke.RemoveGeometryFromPaintTask(canvas, p);
        }

        if (Fill is not null)
        {
            foreach (var c in _bandFillPathDictionary.ToArray())
                foreach (var p in c.Value.ToArray())
                    Fill.RemoveGeometryFromPaintTask(canvas, p);
        }

        if (GeometryFill is not null) canvas.RemovePaintTask(GeometryFill);
        if (GeometryStroke is not null) canvas.RemovePaintTask(GeometryStroke);
    }

    /// <inheritdoc/>
    public override void RemoveFromUI(Chart chart)
    {
        base.RemoveFromUI(chart);
        _ = _highStrokePathDictionary.Remove(chart.Canvas.Sync);
        _ = _lowStrokePathDictionary.Remove(chart.Canvas.Sync);
        _ = _bandFillPathDictionary.Remove(chart.Canvas.Sync);
    }

    /// <inheritdoc cref="ChartElement.GetPaintTasks"/>
    protected internal override Paint?[] GetPaintTasks() =>
        [Stroke, Fill, GeometryFill, GeometryStroke, DataLabelsPaint];

    /// <inheritdoc cref="Series{TModel, TVisual, TLabel}.SetDefaultPointTransitions(ChartPoint)"/>
    protected override void SetDefaultPointTransitions(ChartPoint chartPoint)
    {
        var chart = chartPoint.Context.Chart;

        if (chartPoint.Context.AdditionalVisuals is not RangeCubicSegmentVisualPoint visual)
            throw new Exception("Unable to initialize the point instance.");

        var animation = GetAnimation(chart.CoreChart);

        visual.HighGeometry.Animate(animation);
        visual.LowGeometry.Animate(animation);
        visual.HighSegment.Animate(animation);
        visual.LowSegment.Animate(animation);
    }

    /// <inheritdoc cref="CartesianSeries{TModel, TVisual, TLabel}.SoftDeleteOrDisposePoint(ChartPoint, Scaler, Scaler)"/>
    protected internal override void SoftDeleteOrDisposePoint(ChartPoint point, Scaler primaryScale, Scaler secondaryScale)
    {
        var visual = (RangeCubicSegmentVisualPoint?)point.Context.AdditionalVisuals;
        if (visual is null) return;
        if (DataFactory is null) throw new Exception("Data provider not found");

        var c = point.Coordinate;
        var x = secondaryScale.ToPixels(c.SecondaryValue);
        var highY = primaryScale.ToPixels(c.PrimaryValue);
        var lowY = primaryScale.ToPixels(c.TertiaryValue);
        var midY = (highY + lowY) * 0.5f;

        // Collapse both markers to the midpoint with zero size — mirrors the
        // EnsureVisualForPoint entry seed so dispose animates symmetrically.
        visual.HighGeometry.X = x + visual.HighGeometry.Width * 0.5f;
        visual.HighGeometry.Y = midY + visual.HighGeometry.Height * 0.5f;
        visual.HighGeometry.Height = 0;
        visual.HighGeometry.Width = 0;
        visual.HighGeometry.Opacity = 0;
        visual.HighGeometry.RemoveOnCompleted = true;

        visual.LowGeometry.X = x + visual.LowGeometry.Width * 0.5f;
        visual.LowGeometry.Y = midY + visual.LowGeometry.Height * 0.5f;
        visual.LowGeometry.Height = 0;
        visual.LowGeometry.Width = 0;
        visual.LowGeometry.Opacity = 0;
        visual.LowGeometry.RemoveOnCompleted = true;

        foreach (var coll in _highStrokePathDictionary.Values)
            foreach (var p in coll)
                _ = p.Commands.Remove(visual.HighSegment);
        foreach (var coll in _lowStrokePathDictionary.Values)
            foreach (var p in coll)
                _ = p.Commands.Remove(visual.LowSegment);
        foreach (var coll in _bandFillPathDictionary.Values)
            foreach (var p in coll)
            {
                _ = p.Commands.Remove(visual.HighSegment);
                _ = p.LowCommands.Remove(visual.LowSegment);
            }

        DataFactory.DisposePoint(point);

        var label = (TLabel?)point.Context.Label;
        if (label is null) return;
        label.RemoveOnCompleted = true;
    }

    private void DeleteNullPoint(ChartPoint point, Scaler xScale, Scaler yScale)
    {
        if (point.Context.AdditionalVisuals is not RangeCubicSegmentVisualPoint visual) return;

        var c = point.Coordinate;
        var x = xScale.ToPixels(c.SecondaryValue);
        var highY = yScale.ToPixels(c.PrimaryValue);
        var lowY = yScale.ToPixels(c.TertiaryValue);
        var midY = (highY + lowY) * 0.5f;
        var gs = _geometrySize;
        var hgs = gs / 2f;

        visual.HighGeometry.X = x - hgs;
        visual.HighGeometry.Y = midY - hgs;
        visual.HighGeometry.Width = gs;
        visual.HighGeometry.Height = gs;
        visual.HighGeometry.RemoveOnCompleted = true;

        visual.LowGeometry.X = x - hgs;
        visual.LowGeometry.Y = midY - hgs;
        visual.LowGeometry.Width = gs;
        visual.LowGeometry.Height = gs;
        visual.LowGeometry.RemoveOnCompleted = true;

        point.Context.Visual = null;
        point.Context.AdditionalVisuals = null;
    }

    private void AttachSegmentPaths(
        int segmentI,
        List<TStrokePathGeometry> highStrokeContainer,
        List<TStrokePathGeometry> lowStrokeContainer,
        List<TBandPathGeometry> bandFillContainer,
        in RangeLineMeasureContext ctx,
        out VectorManager highStrokeVm,
        out VectorManager lowStrokeVm,
        out VectorManager bandHighVm,
        out VectorManager bandLowVm)
    {
        var highStroke = GetOrCreate(highStrokeContainer, segmentI, VectorClosingMethod.NotClosed);
        var lowStroke = GetOrCreate(lowStrokeContainer, segmentI, VectorClosingMethod.NotClosed);
        var bandFill = GetOrCreate(bandFillContainer, segmentI, VectorClosingMethod.NotClosed);

        var isNew = highStroke.IsNew || lowStroke.IsNew || bandFill.IsNew;

        highStrokeVm = new VectorManager(highStroke.Path.Commands);
        lowStrokeVm = new VectorManager(lowStroke.Path.Commands);
        bandHighVm = new VectorManager(bandFill.Path.Commands);
        bandLowVm = new VectorManager(bandFill.Path.LowCommands);

        var chart = ctx.Chart;

        if (Fill is not null && Fill != Paint.Default)
        {
            Fill.AddGeometryToPaintTask(chart.Canvas, bandFill.Path);
            chart.Canvas.AddDrawableTask(Fill, zone: CanvasZone.DrawMargin);
            Fill.ZIndex = ctx.ActualZIndex + PaintConstants.SeriesFillZIndexOffset;
            if (isNew) bandFill.Path.Animate(GetAnimation(chart));
        }

        if (Stroke is not null && Stroke != Paint.Default)
        {
            Stroke.AddGeometryToPaintTask(chart.Canvas, highStroke.Path);
            Stroke.AddGeometryToPaintTask(chart.Canvas, lowStroke.Path);
            chart.Canvas.AddDrawableTask(Stroke, zone: CanvasZone.DrawMargin);
            Stroke.ZIndex = ctx.ActualZIndex + PaintConstants.SeriesStrokeZIndexOffset;
            if (isNew)
            {
                var animation = GetAnimation(chart);
                highStroke.Path.Animate(animation);
                lowStroke.Path.Animate(animation);
            }
        }

        highStroke.Path.Opacity = IsVisible ? 1 : 0;
        lowStroke.Path.Opacity = IsVisible ? 1 : 0;
        bandFill.Path.Opacity = IsVisible ? 1 : 0;
    }

    private void CleanupOrphanSegmentPaths(
        int segmentI,
        List<TStrokePathGeometry> highStrokeContainer,
        List<TStrokePathGeometry> lowStrokeContainer,
        List<TBandPathGeometry> bandFillContainer,
        CartesianChartEngine chart)
    {
        var maxSegment = Math.Max(
            Math.Max(highStrokeContainer.Count, lowStrokeContainer.Count),
            bandFillContainer.Count);

        for (var i = maxSegment - 1; i >= segmentI; i--)
        {
            if (i < highStrokeContainer.Count)
            {
                var p = highStrokeContainer[i];
                Stroke?.RemoveGeometryFromPaintTask(chart.Canvas, p);
                p.Commands.Clear();
                highStrokeContainer.RemoveAt(i);
            }
            if (i < lowStrokeContainer.Count)
            {
                var p = lowStrokeContainer[i];
                Stroke?.RemoveGeometryFromPaintTask(chart.Canvas, p);
                p.Commands.Clear();
                lowStrokeContainer.RemoveAt(i);
            }
            if (i < bandFillContainer.Count)
            {
                var p = bandFillContainer[i];
                Fill?.RemoveGeometryFromPaintTask(chart.Canvas, p);
                p.Commands.Clear();
                p.LowCommands.Clear();
                bandFillContainer.RemoveAt(i);
            }
        }
    }

    private void MeasureDataLabel(ChartPoint point, float x, float y, in RangeLineMeasureContext ctx)
    {
        if (!ShowDataLabels || DataLabelsPaint is null || DataLabelsPaint == Paint.Default) return;

        var coordinate = point.Coordinate;
        var hgs = ctx.HalfGeometrySize;
        var gs = ctx.GeometrySize;
        var chart = ctx.Chart;
        var label = (TLabel?)point.Context.Label;

        if (label is null)
        {
            var l = new TLabel
            {
                X = x - hgs,
                Y = y - hgs,
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
            x - hgs, y - hgs, gs, gs, m, DataLabelsPosition,
            SeriesProperties, coordinate.PrimaryValue > Pivot, ctx.DrawLocation, ctx.DrawMarginSize);
        if (DataLabelsTranslate is not null)
            label.TranslateTransform = new LvcPoint(
                m.Width * DataLabelsTranslate.Value.X, m.Height * DataLabelsTranslate.Value.Y);

        label.X = labelPosition.X;
        label.Y = labelPosition.Y;
    }

    private IEnumerable<(BezierData high, BezierData low)> GetPairedSplines(IEnumerable<ChartPoint> points)
    {
        // Two reusable buffers (same trick as CoreLineSeries.GetSpline): the references
        // never escape a single MoveNext step on the consumer side, so mutating them
        // between iterations is safe.
        BezierData? highData = null;
        BezierData? lowData = null;

        foreach (var item in points.AsSplineData())
        {
            if (item.IsFirst)
            {
                highData ??= new BezierData(item.Next);
                lowData ??= new BezierData(item.Next);
                highData.TargetPoint = item.Next;
                lowData.TargetPoint = item.Next;

                LineSplineMath.SeedFirstSegment(highData, item.Current.Coordinate, 0);
                LineSplineMath.SeedFirstSegment(lowData, ToLowCoordinate(item.Current.Coordinate), 0);
                highData.IsNextEmpty = item.IsNextEmpty;
                lowData.IsNextEmpty = item.IsNextEmpty;

                yield return (highData, lowData);
                continue;
            }

            highData ??= new BezierData(item.Next);
            lowData ??= new BezierData(item.Next);
            highData.TargetPoint = item.Next;
            lowData.TargetPoint = item.Next;

            LineSplineMath.ComputeSegment(
                highData,
                item.Previous.Coordinate, 0,
                item.Current.Coordinate, 0,
                item.Next.Coordinate, 0,
                item.AfterNext.Coordinate, 0,
                _lineSmoothness);

            LineSplineMath.ComputeSegment(
                lowData,
                ToLowCoordinate(item.Previous.Coordinate), 0,
                ToLowCoordinate(item.Current.Coordinate), 0,
                ToLowCoordinate(item.Next.Coordinate), 0,
                ToLowCoordinate(item.AfterNext.Coordinate), 0,
                _lineSmoothness);

            highData.IsNextEmpty = false;
            lowData.IsNextEmpty = false;
            yield return (highData, lowData);
        }
    }

    // Build a Coordinate whose PrimaryValue is the source's TertiaryValue (Low),
    // so the spline helper (which always reads from PrimaryValue) generates the
    // low curve without needing a "which side" flag.
    private static Coordinate ToLowCoordinate(in Coordinate c) =>
        new(c.TertiaryValue, c.SecondaryValue, 0, 0, 0, 0, c.PointError);

    private static SegmentVisual<TPath> GetOrCreate<TPath>(List<TPath> container, int index, VectorClosingMethod method)
        where TPath : BaseVectorGeometry, new()
    {
        TPath path;
        var isNew = false;
        if (index >= container.Count)
        {
            isNew = true;
            path = new TPath { ClosingMethod = method };
            container.Add(path);
        }
        else
        {
            path = container[index];
        }
        path.IsValid = false;
        return new SegmentVisual<TPath>(isNew, path);
    }

    private readonly struct SegmentVisual<TPath>(bool isNew, TPath path)
        where TPath : BaseVectorGeometry
    {
        public bool IsNew { get; } = isNew;
        public TPath Path { get; } = path;
    }

    private static SeriesProperties GetProperties() =>
        SeriesProperties.Line | SeriesProperties.PrimaryAxisVerticalOrientation |
        SeriesProperties.Sketch | SeriesProperties.PrefersXStrategyTooltips;
}
