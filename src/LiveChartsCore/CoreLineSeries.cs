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
/// Defines the data to plot as a line.
/// </summary>
/// <typeparam name="TModel">The type of the model to plot.</typeparam>
/// <typeparam name="TVisual">The type of the visual point.</typeparam>
/// <typeparam name="TLabel">The type of the data label.</typeparam>
/// <typeparam name="TPathGeometry">The type of the path geometry.</typeparam>
/// <typeparam name="TErrorGeometry">The type of the error geometry.</typeparam>
public abstract class CoreLineSeries<TModel, TVisual, TLabel, TPathGeometry, TErrorGeometry>
    : StrokeAndFillCartesianSeries<TModel, TVisual, TLabel>, ILineSeries
        where TPathGeometry : BaseVectorGeometry, new()
        where TVisual : BoundedDrawnGeometry, new()
        where TLabel : BaseLabelGeometry, new()
        where TErrorGeometry : BaseLineGeometry, new()
{
    internal readonly Dictionary<object, List<TPathGeometry>> _fillPathHelperDictionary = [];
    internal readonly Dictionary<object, List<TPathGeometry>> _strokePathHelperDictionary = [];
    private float _lineSmoothness = 0.65f;
    private float _geometrySize = 14f;
    private bool _showError;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="CoreLineSeries{TModel, TVisual, TLabel, TPathGeometry, TErrorGeometry}"/>
    /// class.
    /// </summary>
    /// <param name="isStacked">if set to <c>true</c> [is stacked].</param>
    /// <param name="values">The values.</param>
    public CoreLineSeries(IReadOnlyCollection<TModel>? values, bool isStacked = false)
        : base(GetProperties(isStacked), values)
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

    // ---- template method ----------------------------------------------------

    /// <summary>
    /// Builds a per-frame measure context from the chart. Subclasses may override
    /// to refine context construction (e.g. additional pre-computed per-frame values).
    /// </summary>
    protected virtual LineMeasureContext BeginMeasure(CartesianChartEngine chart)
    {
        var primaryAxis = chart.GetYAxis(this);
        var secondaryAxis = chart.GetXAxis(this);

        var drawLocation = chart.DrawMarginLocation;
        var drawMarginSize = chart.DrawMarginSize;
        var secondaryScale = secondaryAxis.GetNextScaler(chart);
        var primaryScale = primaryAxis.GetNextScaler(chart);

        // GetActualScaler is called purely for its cache/registration side-effect on
        // the axis (matches the original Invalidate body, where the locals were never
        // read). Dropping the call would change the axis state on the first frame.
        _ = secondaryAxis.GetActualScaler(chart);
        _ = primaryAxis.GetActualScaler(chart);

        var stacker = (SeriesProperties & SeriesProperties.Stacked) == SeriesProperties.Stacked
            ? chart.SeriesContext.GetStackPosition(this, GetStackGroup())
            : null;

        // #1923: anchor a default-ZIndex stacked area at the largest SeriesId in
        // its stack group, then subtract Position so the bottom layer (Position 0)
        // draws ON TOP of later, larger-fill layers. Stacked area fills extend
        // from the line down to the pivot, so within-stack ordering is required
        // for the layers to be individually visible. The whole stack sits as one
        // rank against non-stacked siblings: a default-ZIndex Line added AFTER
        // the last stacked series (SeriesId > MaxSeriesId) wins; a Line added
        // BEFORE the first stacked series loses. User-set ZIndex always wins.
        var actualZIndex = ZIndex != 0
            ? ZIndex
            : stacker is not null
                ? stacker.Stacker.MaxSeriesId - stacker.Position
                : ((ISeries)this).SeriesId;

        var gs = _geometrySize;
        var hgs = gs / 2f;
        var pivotPx = primaryScale.ToPixels(pivot);

        var uwx = secondaryScale.MeasureInPixels(secondaryAxis.UnitWidth);
        if (uwx < gs) uwx = gs;

        var hasSvg = this.HasVariableSvgGeometry();
        var isFirstDraw = !chart.IsDrawn(((ISeries)this).SeriesId);

        return new LineMeasureContext(
            chart, primaryAxis, secondaryAxis,
            primaryScale, secondaryScale,
            drawLocation, drawMarginSize,
            actualZIndex: actualZIndex,
            pivotPx: pivotPx,
            unitWidthX: uwx,
            geometrySize: gs,
            halfGeometrySize: hgs,
            dataLabelsSize: (float)DataLabelsSize,
            isFirstDraw: isFirstDraw,
            hasSvg: hasSvg,
            stacker: stacker);
    }

    /// <summary>
    /// Ensures the visual + any additional visuals (error bars) exist for the
    /// point and seeds them at the data point with zero size so the marker
    /// animates from a point. On creation, the cubic segment is also seeded at
    /// the pivot baseline; see issue #2132 for why this seed must run for every
    /// new visual rather than just on first draw.
    /// </summary>
    protected virtual CubicSegmentVisualPoint EnsureLineVisualForPoint(ChartPoint point, BezierData data, in LineMeasureContext ctx)
    {
        var coordinate = point.Coordinate;
        var v = new CubicSegmentVisualPoint(new TVisual());

        if (ShowError && ErrorPaint is not null && ErrorPaint != Paint.Default)
        {
            v.YError = new TErrorGeometry();
            v.XError = new TErrorGeometry();

            v.YError.X = ctx.SecondaryScale.ToPixels(coordinate.SecondaryValue);
            v.YError.X1 = ctx.SecondaryScale.ToPixels(coordinate.SecondaryValue);
            v.YError.Y = ctx.PivotPx;
            v.YError.Y1 = ctx.PivotPx;

            v.XError.X = ctx.SecondaryScale.ToPixels(coordinate.SecondaryValue);
            v.XError.X1 = ctx.SecondaryScale.ToPixels(coordinate.SecondaryValue);
            v.XError.Y = ctx.PivotPx;
            v.XError.Y1 = ctx.PivotPx;
        }

        // Seed motion state so the real-value setter below animates from a
        // sensible place instead of the default (0, 0). Runs for every new
        // visual — not just isFirstDraw — because a mid-life new visual that
        // happens to be the first point of a brand-new sub-segment path will
        // skip Follows() in AddConsecutiveSegment (list.Last is null), and
        // without this init the segment would swoop in from the top-left
        // corner (#2132). For non-first points in a sub-segment, Follows()
        // overrides these values with the previous tail, so the "grow from
        // previous" animation is preserved.
        v.Geometry.X = ctx.SecondaryScale.ToPixels(coordinate.SecondaryValue);
        v.Geometry.Y = ctx.PivotPx;
        v.Geometry.Width = 0;
        v.Geometry.Height = 0;

        v.Segment.Xi = ctx.SecondaryScale.ToPixels(data.X0);
        v.Segment.Xm = ctx.SecondaryScale.ToPixels(data.X1);
        v.Segment.Xj = ctx.SecondaryScale.ToPixels(data.X2);
        v.Segment.Yi = ctx.PivotPx;
        v.Segment.Ym = ctx.PivotPx;
        v.Segment.Yj = ctx.PivotPx;

        point.Context.Visual = v.Geometry;
        point.Context.AdditionalVisuals = v;
        OnPointCreated(point);

        return v;
    }

    /// <summary>
    /// Collapses the point's visual and segment commands to the pivot baseline
    /// when the series becomes invisible, and removes the visual / label from
    /// the point so future paints don't redraw them.
    /// </summary>
    protected virtual void CollapseInvisibleLinePoint(ChartPoint point, BezierData data, in LineMeasureContext ctx)
    {
        if (point.Context.AdditionalVisuals is CubicSegmentVisualPoint visual)
        {
            var coordinate = point.Coordinate;

            visual.Geometry.X = ctx.SecondaryScale.ToPixels(coordinate.SecondaryValue);
            visual.Geometry.Y = ctx.PivotPx;
            visual.Geometry.Opacity = 0;
            visual.Geometry.RemoveOnCompleted = true;

            visual.Segment.Xi = ctx.SecondaryScale.ToPixels(data.X0);
            visual.Segment.Xm = ctx.SecondaryScale.ToPixels(data.X1);
            visual.Segment.Xj = ctx.SecondaryScale.ToPixels(data.X2);
            visual.Segment.Yi = ctx.PivotPx;
            visual.Segment.Ym = ctx.PivotPx;
            visual.Segment.Yj = ctx.PivotPx;

            point.Context.Visual = null;
            point.Context.AdditionalVisuals = null;
        }

        if (point.Context.Label is TLabel label)
        {
            label.X = ctx.SecondaryScale.ToPixels(point.Coordinate.SecondaryValue);
            label.Y = ctx.PivotPx;
            label.Opacity = 0;
            label.RemoveOnCompleted = true;

            point.Context.Label = null;
        }
    }

    /// <summary>
    /// Per-frame error-bar geometry update. No-op when the series carries no
    /// error visuals or no point error data.
    /// </summary>
    protected virtual void MeasureLineErrorBars(CubicSegmentVisualPoint visual, BezierData data, in LineMeasureContext ctx)
    {
        var coordinate = data.TargetPoint.Coordinate;
        if (coordinate.PointError.IsEmpty) return;
        if (!ShowError || ErrorPaint is null || ErrorPaint == Paint.Default) return;

        var e = coordinate.PointError;

        visual.YError!.X = ctx.SecondaryScale.ToPixels(data.X2);
        visual.YError.X1 = ctx.SecondaryScale.ToPixels(data.X2);
        visual.YError.Y = ctx.PrimaryScale.ToPixels(data.Y2 + e.Yi);
        visual.YError.Y1 = ctx.PrimaryScale.ToPixels(data.Y2 - e.Yj);
        visual.YError.RemoveOnCompleted = false;

        visual.XError!.X = ctx.SecondaryScale.ToPixels(data.X2 - e.Xi);
        visual.XError.X1 = ctx.SecondaryScale.ToPixels(data.X2 + e.Xj);
        visual.XError.Y = ctx.PrimaryScale.ToPixels(data.Y2);
        visual.XError.Y1 = ctx.PrimaryScale.ToPixels(data.Y2);
        visual.XError.RemoveOnCompleted = false;
    }

    /// <summary>
    /// Registers per-segment fill / stroke paths on the chart canvas, sets their
    /// Z-index and pivot, animates if newly-created, and returns fresh vector
    /// managers wrapping their command lists. Called once per segment when the
    /// first point in that segment is encountered.
    /// </summary>
    private void AttachSegmentPaths(
        int segmentI,
        List<TPathGeometry> fillContainer,
        List<TPathGeometry> strokeContainer,
        in LineMeasureContext ctx,
        out VectorManager fillVector,
        out VectorManager strokeVector)
    {
        var fillLookup = GetSegmentVisual(segmentI, fillContainer, VectorClosingMethod.CloseToPivot);
        var strokeLookup = GetSegmentVisual(segmentI, strokeContainer, VectorClosingMethod.NotClosed);

        // The previous "Count == 1 && !IsNextEmpty" branch tried to discard a
        // stale single-segment path left behind when this slot used to host a
        // one-point sub-segment. Its implementation called container.RemoveAt(segmentI)
        // and then re-fetched the path, which shifted every subsequent sub-segment
        // onto the WRONG path in the container — the root of #2132's resize/yellow-region
        // regression. VectorManager.TrimTail now drops stale remnants without touching
        // the container, so this cleanup is no longer necessary.

        var isNew = fillLookup.IsNew || strokeLookup.IsNew;
        var fillPath = fillLookup.Path;
        var strokePath = strokeLookup.Path;

        strokeVector = new VectorManager(strokePath.Commands);
        fillVector = new VectorManager(fillPath.Commands);

        var chart = ctx.Chart;

        if (Fill is not null && Fill != Paint.Default)
        {
            Fill.AddGeometryToPaintTask(chart.Canvas, fillPath);
            chart.Canvas.AddDrawableTask(Fill, zone: CanvasZone.DrawMargin);
            Fill.ZIndex = ctx.ActualZIndex + PaintConstants.SeriesFillZIndexOffset;
            fillPath.Pivot = ctx.PivotPx;
            if (isNew) fillPath.Animate(GetAnimation(chart));
        }

        if (Stroke is not null && Stroke != Paint.Default)
        {
            Stroke.AddGeometryToPaintTask(chart.Canvas, strokePath);
            chart.Canvas.AddDrawableTask(Stroke, zone: CanvasZone.DrawMargin);
            Stroke.ZIndex = ctx.ActualZIndex + PaintConstants.SeriesStrokeZIndexOffset;
            strokePath.Pivot = ctx.PivotPx;
            if (isNew) strokePath.Animate(GetAnimation(chart));
        }

        strokePath.Opacity = IsVisible ? 1 : 0;
        fillPath.Opacity = IsVisible ? 1 : 0;
    }

    /// <summary>
    /// Removes per-canvas segment paths that are no longer referenced by any
    /// active sub-segment (i.e. their index sits at or above the count of
    /// segments produced this frame). Mirrors the original tail-cleanup loop.
    /// </summary>
    private void CleanupOrphanSegmentPaths(
        int segmentI,
        List<TPathGeometry> fillContainer,
        List<TPathGeometry> strokeContainer,
        CartesianChartEngine chart)
    {
        var maxSegment = fillContainer.Count > strokeContainer.Count
            ? fillContainer.Count
            : strokeContainer.Count;

        for (var i = maxSegment - 1; i >= segmentI; i--)
        {
            if (i < fillContainer.Count)
            {
                var segmentFill = fillContainer[i];
                Fill?.RemoveGeometryFromPaintTask(chart.Canvas, segmentFill);
                segmentFill.Commands.Clear();
                fillContainer.RemoveAt(i);
            }

            if (i < strokeContainer.Count)
            {
                var segmentStroke = strokeContainer[i];
                Stroke?.RemoveGeometryFromPaintTask(chart.Canvas, segmentStroke);
                segmentStroke.Commands.Clear();
                strokeContainer.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Creates the data label visual if it doesn't exist yet (animation-sourced from
    /// the marker's top-left at the pivot baseline), updates its text + style, and
    /// positions it via <c>GetLabelPosition</c>. No-op when the series has no
    /// data-label paint configured.
    /// </summary>
    private void MeasureDataLabel(ChartPoint point, float x, float y, in LineMeasureContext ctx)
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
                Y = ctx.PivotPx - hgs,
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

        // Note #240222
        // the following cases probably have a similar performance impact
        // this options were necessary at some older point when _enableNullSplitting = false could improve performance
        // ToDo: Check this out, maybe this is unnecessary now and we should just go for the first approach all the times.
        var segments = EnableNullSplitting
            ? Fetch(cartesianChart).SplitByNullGaps(point => DeleteNullPoint(point, secondaryScale, primaryScale))
            : [Fetch(cartesianChart)];

        if (!_strokePathHelperDictionary.TryGetValue(chart.Canvas.Sync, out var strokePathHelperContainer))
        {
            strokePathHelperContainer = [];
            _strokePathHelperDictionary[chart.Canvas.Sync] = strokePathHelperContainer;
        }

        if (!_fillPathHelperDictionary.TryGetValue(chart.Canvas.Sync, out var fillPathHelperContainer))
        {
            fillPathHelperContainer = [];
            _fillPathHelperDictionary[chart.Canvas.Sync] = fillPathHelperContainer;
        }

        var segmentI = 0;

        foreach (var segment in segments)
        {
            var hasPaths = false;
            var isSegmentEmpty = true;
            VectorManager? strokeVector = null, fillVector = null;

            foreach (var data in GetSpline(segment, ctx.Stacker))
            {
                if (!hasPaths)
                {
                    hasPaths = true;
                    AttachSegmentPaths(
                        segmentI, fillPathHelperContainer, strokePathHelperContainer, in ctx,
                        out fillVector, out strokeVector);
                }

                isSegmentEmpty = false;

                var coordinate = data.TargetPoint.Coordinate;
                var s = ctx.Stacker?.GetStack(data.TargetPoint).CumulativeStart ?? 0d;

                var visual = (CubicSegmentVisualPoint?)data.TargetPoint.Context.AdditionalVisuals;
                // Captured before the null check reassigns. Drives AddConsecutiveSegment's
                // decision to Follows/Copy — only new visuals need a motion-state seed;
                // preserved visuals already carry live state from last frame.
                var isVisualNew = visual is null;

                if (!IsVisible)
                {
                    CollapseInvisibleLinePoint(data.TargetPoint, data, in ctx);
                    pointsCleanup.Clean(data.TargetPoint);
                    continue;
                }

                visual ??= EnsureLineVisualForPoint(data.TargetPoint, data, in ctx);
                visual.Geometry.Opacity = 1;

                if (ctx.HasSvg)
                {
                    var svgVisual = (IVariableSvgPath)visual.Geometry;
                    if (_geometrySvgChanged || svgVisual.SVGPath is null)
                        svgVisual.SVGPath = GeometrySvg ?? throw new Exception("svg path is not defined");
                }

                _ = everFetched.Add(data.TargetPoint);

                if (GeometryFill is not null && GeometryFill != Paint.Default)
                    GeometryFill.AddGeometryToPaintTask(cartesianChart.Canvas, visual.Geometry);
                if (GeometryStroke is not null && GeometryStroke != Paint.Default)
                    GeometryStroke.AddGeometryToPaintTask(cartesianChart.Canvas, visual.Geometry);
                if (ErrorPaint is not null && ErrorPaint != Paint.Default)
                {
                    ErrorPaint.AddGeometryToPaintTask(cartesianChart.Canvas, visual.YError!);
                    ErrorPaint.AddGeometryToPaintTask(cartesianChart.Canvas, visual.XError!);
                }

                visual.Segment.Id = data.TargetPoint.Context.Entity.MetaData!.EntityIndex;

                if (Fill is not null) fillVector!.AddConsecutiveSegment(visual.Segment, isVisualNew && !ctx.IsFirstDraw);
                if (Stroke is not null) strokeVector!.AddConsecutiveSegment(visual.Segment, isVisualNew && !ctx.IsFirstDraw);

                visual.Segment.Xi = ctx.SecondaryScale.ToPixels(data.X0);
                visual.Segment.Xm = ctx.SecondaryScale.ToPixels(data.X1);
                visual.Segment.Xj = ctx.SecondaryScale.ToPixels(data.X2);
                visual.Segment.Yi = ctx.PrimaryScale.ToPixels(data.Y0);
                visual.Segment.Ym = ctx.PrimaryScale.ToPixels(data.Y1);
                visual.Segment.Yj = ctx.PrimaryScale.ToPixels(data.Y2);

                var x = ctx.SecondaryScale.ToPixels(coordinate.SecondaryValue);
                var y = ctx.PrimaryScale.ToPixels(coordinate.PrimaryValue + s);

                DrawnGeometry.XProperty.GetMotion(visual.Geometry)!
                    .CopyFrom(Segment.XjProperty.GetMotion(visual.Segment)!);
                DrawnGeometry.YProperty.GetMotion(visual.Geometry)!
                    .CopyFrom(Segment.YjProperty.GetMotion(visual.Segment)!);

                visual.Geometry.TranslateTransform = new LvcPoint(-ctx.HalfGeometrySize, -ctx.HalfGeometrySize);
                visual.Geometry.Width = ctx.GeometrySize;
                visual.Geometry.Height = ctx.GeometrySize;
                visual.Geometry.RemoveOnCompleted = false;

                MeasureLineErrorBars(visual, data, in ctx);

                if (data.TargetPoint.Context.HoverArea is not RectangleHoverArea ha)
                    data.TargetPoint.Context.HoverArea = ha = new RectangleHoverArea();

                _ = ha
                    .SetDimensions(x - ctx.UnitWidthX * 0.5f, y - ctx.HalfGeometrySize, ctx.UnitWidthX, ctx.GeometrySize)
                    .CenterXToolTip();

                _ = coordinate.PrimaryValue >= pivot
                    ? ha.StartYToolTip()
                    : ha.EndYToolTip().IsLessThanPivot();

                pointsCleanup.Clean(data.TargetPoint);

                MeasureDataLabel(data.TargetPoint, x, y, in ctx);

                OnPointMeasured(data.TargetPoint);
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

            fillVector?.TrimTail();
            strokeVector?.TrimTail();
        }

        CleanupOrphanSegmentPaths(segmentI, fillPathHelperContainer, strokePathHelperContainer, cartesianChart);

        if (ShowDataLabels && DataLabelsPaint is not null && DataLabelsPaint != Paint.Default)
        {
            cartesianChart.Canvas.AddDrawableTask(DataLabelsPaint, zone: CanvasZone.DrawMargin);
            DataLabelsPaint.ZIndex = ctx.ActualZIndex + PaintConstants.SeriesDataLabelsZIndexOffset;
        }
        if (ShowError && ErrorPaint is not null && ErrorPaint != Paint.Default)
        {
            cartesianChart.Canvas.AddDrawableTask(ErrorPaint, zone: CanvasZone.DrawMargin);
            ErrorPaint.ZIndex = ctx.ActualZIndex + PaintConstants.SeriesGeometryFillZIndexOffset;
        }

        pointsCleanup.CollectPoints(
            everFetched, cartesianChart.View, ctx.PrimaryScale, ctx.SecondaryScale, SoftDeleteOrDisposePoint);

        _geometrySvgChanged = false;
    }

    /// <inheritdoc cref="Series{TModel, TVisual, TLabel}.FindPointsInPosition(Chart, LvcPoint, FindingStrategy, FindPointFor)"/>
    protected override IEnumerable<ChartPoint> FindPointsInPosition(
        Chart chart, LvcPoint pointerPosition, FindingStrategy strategy, FindPointFor findPointFor)
    {
        bool VisualContains(ChartPoint point)
        {
            var v = (TVisual?)point.Context.Visual;
            if (v is null) return false;

            var x = v.X + v.TranslateTransform.X;
            var y = v.Y + v.TranslateTransform.Y;

            return
                pointerPosition.X > x && pointerPosition.X < x + v.Width &&
                pointerPosition.Y > y && pointerPosition.Y < y + v.Height;
        }

        return strategy switch
        {
            FindingStrategy.ExactMatch => Fetch(chart).Where(VisualContains),
            // TakeClosest must still respect ExactMatch's visual-containment
            // filter — otherwise a probe in empty space returns the marker
            // nearest to the pointer, which is the wrong contract for "exact".
            FindingStrategy.ExactMatchTakeClosest => Fetch(chart)
                .Where(VisualContains)
                .Select(x => new { distance = x.DistanceTo(pointerPosition, strategy), point = x })
                .OrderBy(x => x.distance)
                .SelectFirst(x => x.point),
            FindingStrategy.Automatic or
            FindingStrategy.CompareAll or
            FindingStrategy.CompareOnlyX or
            FindingStrategy.CompareOnlyY or
            FindingStrategy.CompareAllTakeClosest or
            FindingStrategy.CompareOnlyXTakeClosest or
            FindingStrategy.CompareOnlyYTakeClosest or
                _ => base.FindPointsInPosition(chart, pointerPosition, strategy, findPointFor)
        };
    }

    /// <inheritdoc cref="GetRequestedGeometrySize"/>
    protected override double GetRequestedGeometrySize() =>
        (GeometrySize + (GeometryStroke?.StrokeThickness ?? 0)) * 0.5f;

    /// <inheritdoc cref="Series{TModel, TVisual, TLabel}.GetMiniatureGeometry(ChartPoint?)"/>
    public override IDrawnElement GetMiniatureGeometry(ChartPoint? point)
    {
        var noGeometryPaint = GeometryStroke is null && GeometryFill is null;
        var usesLine = (GeometrySize < 1 || noGeometryPaint) && Stroke is not null;

        var v = point?.Context.Visual;

        if (usesLine)
        {
            return new TErrorGeometry
            {
                IsRelativeToLocation = true,
                Stroke = Stroke,
                StrokeThickness = (float)MiniatureStrokeThickness,
                ClippingBounds = LvcRectangle.Empty,
                X = 0,
                Y = 0,
                X1 = (float)MiniatureShapeSize,
                Y1 = 0
            };
        }

        var m = new TVisual
        {
            Fill = v?.Fill ?? GeometryFill ?? Fill,
            Stroke = v?.Stroke ?? GeometryStroke ?? Stroke,
            StrokeThickness = (float)MiniatureStrokeThickness,
            ClippingBounds = LvcRectangle.Empty,
            Width = (float)MiniatureShapeSize,
            Height = (float)MiniatureShapeSize,
            RotateTransform = v?.RotateTransform ?? 0
        };

        if (m is IVariableSvgPath svg) svg.SVGPath = GeometrySvg;

        return m;
    }

    /// <inheritdoc cref="Series{TModel, TVisual, TLabel}.SoftDeleteOrDispose(IChartView)"/>
    public override void SoftDeleteOrDispose(IChartView chart)
    {
        base.SoftDeleteOrDispose(chart);
        var canvas = ((ICartesianChartView)chart).CoreCanvas;

        if (Fill is not null)
        {
            foreach (var activeChartContainer in _fillPathHelperDictionary.ToArray())
                foreach (var pathHelper in activeChartContainer.Value.ToArray())
                    Fill.RemoveGeometryFromPaintTask(canvas, pathHelper);
        }

        if (Stroke is not null)
        {
            foreach (var activeChartContainer in _strokePathHelperDictionary.ToArray())
                foreach (var pathHelper in activeChartContainer.Value.ToArray())
                    Stroke.RemoveGeometryFromPaintTask(canvas, pathHelper);
        }

        if (GeometryFill is not null) canvas.RemovePaintTask(GeometryFill);
        if (GeometryStroke is not null) canvas.RemovePaintTask(GeometryStroke);
    }

    /// <inheritdoc/>
    public override void RemoveFromUI(Chart chart)
    {
        base.RemoveFromUI(chart);

        _ = _fillPathHelperDictionary.Remove(chart.Canvas.Sync);
        _ = _strokePathHelperDictionary.Remove(chart.Canvas.Sync);
    }

    /// <inheritdoc cref="ChartElement.GetPaintTasks"/>
    protected internal override Paint?[] GetPaintTasks() =>
        [Stroke, Fill, GeometryFill, GeometryStroke, DataLabelsPaint, ErrorPaint];

    /// <summary>
    /// Builds an spline from the given points.
    /// </summary>
    /// <param name="points">The points.</param>
    /// <param name="stacker">The stacker.</param>
    /// <returns></returns>
    protected internal IEnumerable<BezierData> GetSpline(
        IEnumerable<ChartPoint> points,
        StackPosition? stacker)
    {
        // Single BezierData reused across yields — mutating its fields between iterations
        // is safe because Measure reads X0..Y2 into the segment via local accesses within
        // the same foreach body; the reference never escapes a single MoveNext step.
        BezierData? data = null;

        foreach (var item in points.AsSplineData())
        {
            if (item.IsFirst)
            {
                var sc = stacker?.GetStack(item.Current).CumulativeStart ?? 0;

                data ??= new BezierData(item.Next);
                data.TargetPoint = item.Next;
                LineSplineMath.SeedFirstSegment(data, item.Current.Coordinate, sc);
                data.IsNextEmpty = item.IsNextEmpty;

                yield return data;
                continue;
            }

            var pys = 0d;
            var cys = 0d;
            var nys = 0d;
            var nnys = 0d;

            if (stacker is not null)
            {
                pys = stacker.GetStack(item.Previous).CumulativeStart;
                cys = stacker.GetStack(item.Current).CumulativeStart;
                nys = stacker.GetStack(item.Next).CumulativeStart;
                nnys = stacker.GetStack(item.AfterNext).CumulativeStart;
            }

            data ??= new BezierData(item.Next);
            data.TargetPoint = item.Next;
            LineSplineMath.ComputeSegment(
                data,
                item.Previous.Coordinate, pys,
                item.Current.Coordinate, cys,
                item.Next.Coordinate, nys,
                item.AfterNext.Coordinate, nnys,
                _lineSmoothness);
            data.IsNextEmpty = false;

            yield return data;
        }
    }

    /// <inheritdoc cref="Series{TModel, TVisual, TLabel}.SetDefaultPointTransitions(ChartPoint)"/>
    protected override void SetDefaultPointTransitions(ChartPoint chartPoint)
    {
        var chart = chartPoint.Context.Chart;

        if (chartPoint.Context.AdditionalVisuals is not CubicSegmentVisualPoint visual)
            throw new Exception("Unable to initialize the point instance.");

        var animation = GetAnimation(chart.CoreChart);

        visual.Geometry.Animate(animation);
        visual.Segment.Animate(animation);
        visual.YError?.Animate(animation);
        visual.XError?.Animate(animation);
    }

    /// <inheritdoc cref="CartesianSeries{TModel, TVisual, TLabel}.SoftDeleteOrDisposePoint(ChartPoint, Scaler, Scaler)"/>
    protected internal override void SoftDeleteOrDisposePoint(ChartPoint point, Scaler primaryScale, Scaler secondaryScale)
    {
        var visual = (CubicSegmentVisualPoint?)point.Context.AdditionalVisuals;
        if (visual is null) return;
        if (DataFactory is null) throw new Exception("Data provider not found");

        var c = point.Coordinate;

        var x = secondaryScale.ToPixels(c.SecondaryValue);
        var y = primaryScale.ToPixels(c.PrimaryValue);

        visual.Geometry.X = x + visual.Geometry.Width * 0.5f;
        visual.Geometry.Y = y + visual.Geometry.Height * 0.5f;
        visual.Geometry.Height = 0;
        visual.Geometry.Width = 0;
        visual.Geometry.Opacity = 0;
        visual.Geometry.RemoveOnCompleted = true;

        if (visual.YError is not null)
        {
            visual.YError.Y = y;
            visual.YError.Y1 = y;
            visual.YError.RemoveOnCompleted = true;
        }

        if (visual.XError is not null)
        {
            visual.XError.X = x;
            visual.XError.X1 = x;
            visual.XError.RemoveOnCompleted = true;
        }

        foreach (var pathCollection in _strokePathHelperDictionary.Values)
            foreach (var path in pathCollection)
                _ = path.Commands.Remove(visual.Segment);

        foreach (var pathCollection in _fillPathHelperDictionary.Values)
            foreach (var path in pathCollection)
                _ = path.Commands.Remove(visual.Segment);

        DataFactory.DisposePoint(point);

        var label = (TLabel?)point.Context.Label;
        if (label is null) return;

        label.RemoveOnCompleted = true;
    }

    private void DeleteNullPoint(ChartPoint point, Scaler xScale, Scaler yScale)
    {
        if (point.Context.Visual is not CubicSegmentVisualPoint visual) return;

        var c = point.Coordinate;

        var x = xScale.ToPixels(c.SecondaryValue);
        var y = yScale.ToPixels(c.PrimaryValue);
        var gs = _geometrySize;
        var hgs = gs / 2f;

        visual.Geometry.X = x - hgs;
        visual.Geometry.Y = y - hgs;
        visual.Geometry.Width = gs;
        visual.Geometry.Height = gs;
        visual.Geometry.RemoveOnCompleted = true;
        point.Context.Visual = null;
    }

    private SegmentVisual GetSegmentVisual(int index, List<TPathGeometry> container, VectorClosingMethod method)
    {
        var isNew = false;
        TPathGeometry? path;

        if (index >= container.Count)
        {
            isNew = true;
            path = new TPathGeometry { ClosingMethod = method };
            container.Add(path);
        }
        else
        {
            path = container[index];
        }

        path.IsValid = false;

        return new SegmentVisual(isNew, path);
    }

    private class SplineData(ChartPoint start)
    {
        public ChartPoint Previous { get; set; } = start;

        public ChartPoint Current { get; set; } = start;

        public ChartPoint Next { get; set; } = start;

        public ChartPoint AfterNext { get; set; } = start;

        public bool IsFirst { get; set; } = true;

        public void GoNext(ChartPoint point)
        {
            Previous = Current;
            Current = Next;
            Next = AfterNext;
            AfterNext = point;
        }
    }

    private class SegmentVisual(bool isNew, TPathGeometry path)
    {
        public bool IsNew { get; set; } = isNew;

        public TPathGeometry Path { get; set; } = path;
    }

    private static SeriesProperties GetProperties(bool isStacked)
    {
        return SeriesProperties.Line | SeriesProperties.PrimaryAxisVerticalOrientation |
            SeriesProperties.Sketch | SeriesProperties.PrefersXStrategyTooltips |
            (isStacked ? SeriesProperties.Stacked : 0);
    }
}
