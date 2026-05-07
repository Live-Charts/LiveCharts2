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

    /// <inheritdoc cref="ChartElement.Invalidate(Chart)"/>
    public override void Invalidate(Chart chart)
    {
        var cartesianChart = (CartesianChartEngine)chart;
        var primaryAxis = cartesianChart.GetYAxis(this);
        var secondaryAxis = cartesianChart.GetXAxis(this);

        var drawLocation = cartesianChart.DrawMarginLocation;
        var drawMarginSize = cartesianChart.DrawMarginSize;
        var secondaryScale = secondaryAxis.GetNextScaler(cartesianChart);
        var primaryScale = primaryAxis.GetNextScaler(cartesianChart);
        var actualSecondaryScale = secondaryAxis.GetActualScaler(cartesianChart);
        var actualPrimaryScale = primaryAxis.GetActualScaler(cartesianChart);

        var gs = _geometrySize;
        var hgs = gs / 2f;
        var sw = Stroke?.StrokeThickness ?? 0;
        var p = primaryScale.ToPixels(pivot);

        // Note #240222
        // the following cases probably have a similar performance impact
        // this options were necessary at some older point when _enableNullSplitting = false could improve performance
        // ToDo: Check this out, maybe this is unnecessary now and we should just go for the first approach all the times.
        var segments = EnableNullSplitting
            ? Fetch(cartesianChart).SplitByNullGaps(point => DeleteNullPoint(point, secondaryScale, primaryScale)) // calling this method is probably as expensive as the line bellow
            : [Fetch(cartesianChart)];
        var stacker = (SeriesProperties & SeriesProperties.Stacked) == SeriesProperties.Stacked
            ? cartesianChart.SeriesContext.GetStackPosition(this, GetStackGroup())
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

        var dls = (float)DataLabelsSize;
        var pointsCleanup = ChartPointCleanupContext.For(everFetched);

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

        var uwx = secondaryScale.MeasureInPixels(secondaryAxis.UnitWidth);
        uwx = uwx < gs ? gs : uwx;
        var tooltipPositon = chart.TooltipPosition;

        var segmentI = 0;
        var hasSvg = this.HasVariableSvgGeometry();

        var isFirstDraw = !chart.IsDrawn(((ISeries)this).SeriesId);

        foreach (var segment in segments)
        {
            var hasPaths = false;
            var isSegmentEmpty = true;
            VectorManager? strokeVector = null, fillVector = null;

            foreach (var data in GetSpline(segment, stacker))
            {
                if (!hasPaths)
                {
                    hasPaths = true;

                    var fillLookup = GetSegmentVisual(segmentI, fillPathHelperContainer, VectorClosingMethod.CloseToPivot);
                    var strokeLookup = GetSegmentVisual(segmentI, strokePathHelperContainer, VectorClosingMethod.NotClosed);

                    // The previous "Count == 1 && !IsNextEmpty" branch tried to discard a
                    // stale single-segment path left behind when this slot used to host a
                    // one-point sub-segment. Its implementation called
                    // container.RemoveAt(segmentI) and then re-fetched the path, which
                    // shifted every subsequent sub-segment onto the WRONG path in the
                    // container — the root of #2132's resize/yellow-region regression.
                    // VectorManager.TrimTail now drops stale remnants without touching
                    // the container, so this cleanup is no longer necessary.

                    var isNew = fillLookup.IsNew || strokeLookup.IsNew;
                    var fillPath = fillLookup.Path;
                    var strokePath = strokeLookup.Path;

                    strokeVector = new VectorManager(strokePath.Commands);
                    fillVector = new VectorManager(fillPath.Commands);

                    if (Fill is not null && Fill != Paint.Default)
                    {
                        Fill.AddGeometryToPaintTask(cartesianChart.Canvas, fillPath);
                        cartesianChart.Canvas.AddDrawableTask(Fill, zone: CanvasZone.DrawMargin);
                        Fill.ZIndex = actualZIndex + PaintConstants.SeriesFillZIndexOffset;
                        fillPath.Pivot = p;
                        if (isNew)
                        {
                            fillPath.Animate(EasingFunction ?? cartesianChart.ActualEasingFunction, AnimationsSpeed ?? cartesianChart.ActualAnimationsSpeed);
                        }
                    }
                    if (Stroke is not null && Stroke != Paint.Default)
                    {
                        Stroke.AddGeometryToPaintTask(cartesianChart.Canvas, strokePath);
                        cartesianChart.Canvas.AddDrawableTask(Stroke, zone: CanvasZone.DrawMargin);
                        Stroke.ZIndex = actualZIndex + PaintConstants.SeriesStrokeZIndexOffset;
                        strokePath.Pivot = p;
                        if (isNew)
                        {
                            strokePath.Animate(EasingFunction ?? cartesianChart.ActualEasingFunction, AnimationsSpeed ?? cartesianChart.ActualAnimationsSpeed);
                        }
                    }

                    strokePath.Opacity = IsVisible ? 1 : 0;
                    fillPath.Opacity = IsVisible ? 1 : 0;
                }

                var coordinate = data.TargetPoint.Coordinate;

                isSegmentEmpty = false;
                var s = 0d;
                if (stacker is not null)
                    s = stacker.GetStack(data.TargetPoint).CumulativeStart;

                var visual = (CubicSegmentVisualPoint?)data.TargetPoint.Context.AdditionalVisuals;
                // Captured before the null check reassigns. Drives AddConsecutiveSegment's
                // decision to Follows/Copy — only new visuals need a motion-state seed;
                // preserved visuals already carry live state from last frame.
                var isVisualNew = visual is null;

                if (!IsVisible)
                {
                    if (visual is not null)
                    {
                        visual.Geometry.X = secondaryScale.ToPixels(coordinate.SecondaryValue);
                        visual.Geometry.Y = p;
                        visual.Geometry.Opacity = 0;
                        visual.Geometry.RemoveOnCompleted = true;

                        visual.Segment.Xi = secondaryScale.ToPixels(data.X0);
                        visual.Segment.Xm = secondaryScale.ToPixels(data.X1);
                        visual.Segment.Xj = secondaryScale.ToPixels(data.X2);
                        visual.Segment.Yi = p;
                        visual.Segment.Ym = p;
                        visual.Segment.Yj = p;

                        data.TargetPoint.Context.Visual = null;
                        data.TargetPoint.Context.AdditionalVisuals = null;
                    }

                    if (data.TargetPoint.Context.Label is not null)
                    {
                        var label = (TLabel)data.TargetPoint.Context.Label;

                        label.X = secondaryScale.ToPixels(coordinate.SecondaryValue);
                        label.Y = p;
                        label.Opacity = 0;
                        label.RemoveOnCompleted = true;

                        data.TargetPoint.Context.Label = null;
                    }

                    pointsCleanup.Clean(data.TargetPoint);

                    continue;
                }

                if (visual is null)
                {
                    var v = new CubicSegmentVisualPoint(new TVisual());

                    if (ShowError && ErrorPaint is not null && ErrorPaint != Paint.Default)
                    {
                        v.YError = new TErrorGeometry();
                        v.XError = new TErrorGeometry();

                        v.YError.X = secondaryScale.ToPixels(coordinate.SecondaryValue);
                        v.YError.X1 = secondaryScale.ToPixels(coordinate.SecondaryValue);
                        v.YError.Y = p;
                        v.YError.Y1 = p;

                        v.XError.X = secondaryScale.ToPixels(coordinate.SecondaryValue);
                        v.XError.X1 = secondaryScale.ToPixels(coordinate.SecondaryValue);
                        v.XError.Y = p;
                        v.XError.Y1 = p;
                    }

                    visual = v;

                    // Seed motion state so the real-value setter below animates from a
                    // sensible place instead of the default (0, 0). Runs for every new
                    // visual — not just isFirstDraw — because a mid-life new visual that
                    // happens to be the first point of a brand-new sub-segment path will
                    // skip Follows() in AddConsecutiveSegment (list.Last is null), and
                    // without this init the segment would swoop in from the top-left
                    // corner (#2132). For non-first points in a sub-segment, Follows()
                    // overrides these values with the previous tail, so the "grow from
                    // previous" animation is preserved.
                    v.Geometry.X = secondaryScale.ToPixels(coordinate.SecondaryValue);
                    v.Geometry.Y = p;
                    v.Geometry.Width = 0;
                    v.Geometry.Height = 0;

                    v.Segment.Xi = secondaryScale.ToPixels(data.X0);
                    v.Segment.Xm = secondaryScale.ToPixels(data.X1);
                    v.Segment.Xj = secondaryScale.ToPixels(data.X2);
                    v.Segment.Yi = p;
                    v.Segment.Ym = p;
                    v.Segment.Yj = p;

                    data.TargetPoint.Context.Visual = v.Geometry;
                    data.TargetPoint.Context.AdditionalVisuals = v;
                    OnPointCreated(data.TargetPoint);
                }

                visual.Geometry.Opacity = 1;

                if (hasSvg)
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

                if (Fill is not null) fillVector!.AddConsecutiveSegment(visual.Segment, isVisualNew && !isFirstDraw);
                if (Stroke is not null) strokeVector!.AddConsecutiveSegment(visual.Segment, isVisualNew && !isFirstDraw);

                visual.Segment.Xi = secondaryScale.ToPixels(data.X0);
                visual.Segment.Xm = secondaryScale.ToPixels(data.X1);
                visual.Segment.Xj = secondaryScale.ToPixels(data.X2);
                visual.Segment.Yi = primaryScale.ToPixels(data.Y0);
                visual.Segment.Ym = primaryScale.ToPixels(data.Y1);
                visual.Segment.Yj = primaryScale.ToPixels(data.Y2);

                var x = secondaryScale.ToPixels(coordinate.SecondaryValue);
                var y = primaryScale.ToPixels(coordinate.PrimaryValue + s);

                DrawnGeometry.XProperty.GetMotion(visual.Geometry)!
                    .CopyFrom(Segment.XjProperty.GetMotion(visual.Segment)!);
                DrawnGeometry.YProperty.GetMotion(visual.Geometry)!
                    .CopyFrom(Segment.YjProperty.GetMotion(visual.Segment)!);

                visual.Geometry.TranslateTransform = new LvcPoint(-hgs, -hgs);

                visual.Geometry.Width = gs;
                visual.Geometry.Height = gs;
                visual.Geometry.RemoveOnCompleted = false;

                if (!coordinate.PointError.IsEmpty && ShowError && ErrorPaint is not null && ErrorPaint != Paint.Default)
                {
                    var e = coordinate.PointError;

                    visual.YError!.X = secondaryScale.ToPixels(data.X2);
                    visual.YError.X1 = secondaryScale.ToPixels(data.X2);
                    visual.YError.Y = primaryScale.ToPixels(data.Y2 + e.Yi);
                    visual.YError.Y1 = primaryScale.ToPixels(data.Y2 - e.Yj);
                    visual.YError.RemoveOnCompleted = false;

                    visual.XError!.X = secondaryScale.ToPixels(data.X2 - e.Xi);
                    visual.XError.X1 = secondaryScale.ToPixels(data.X2 + e.Xj);
                    visual.XError.Y = primaryScale.ToPixels(data.Y2);
                    visual.XError.Y1 = primaryScale.ToPixels(data.Y2);
                    visual.XError.RemoveOnCompleted = false;
                }

                var hags = gs < 8 ? 8 : gs;

                if (data.TargetPoint.Context.HoverArea is not RectangleHoverArea ha)
                    data.TargetPoint.Context.HoverArea = ha = new RectangleHoverArea();

                _ = ha
                    .SetDimensions(x - uwx * 0.5f, y - hgs, uwx, gs)
                    .CenterXToolTip();

                _ = coordinate.PrimaryValue >= pivot
                    ? ha.StartYToolTip()
                    : ha.EndYToolTip().IsLessThanPivot();

                pointsCleanup.Clean(data.TargetPoint);

                if (ShowDataLabels && DataLabelsPaint is not null && DataLabelsPaint != Paint.Default)
                {
                    var label = (TLabel?)data.TargetPoint.Context.Label;

                    if (label is null)
                    {
                        var l = new TLabel { X = x - hgs, Y = p - hgs, RotateTransform = (float)DataLabelsRotation, MaxWidth = (float)DataLabelsMaxWidth };
                        l.Animate(
                            EasingFunction ?? cartesianChart.ActualEasingFunction,
                            AnimationsSpeed ?? cartesianChart.ActualAnimationsSpeed,
                            BaseLabelGeometry.XProperty,
                            BaseLabelGeometry.YProperty);
                        label = l;
                        data.TargetPoint.Context.Label = l;
                    }

                    DataLabelsPaint.AddGeometryToPaintTask(cartesianChart.Canvas, label);
                    label.Text = DataLabelsFormatter(new ChartPoint<TModel, TVisual, TLabel>(data.TargetPoint));
                    label.TextSize = dls;
                    label.Padding = DataLabelsPadding;
                    label.Paint = DataLabelsPaint;

                    if (isFirstDraw)
                        label.CompleteTransition(
                            BaseLabelGeometry.TextSizeProperty,
                            BaseLabelGeometry.XProperty,
                            BaseLabelGeometry.YProperty,
                            BaseLabelGeometry.RotateTransformProperty);

                    var m = label.Measure();
                    var labelPosition = GetLabelPosition(
                        x - hgs, y - hgs, gs, gs, m, DataLabelsPosition,
                        SeriesProperties, coordinate.PrimaryValue > Pivot, drawLocation, drawMarginSize);
                    if (DataLabelsTranslate is not null) label.TranslateTransform =
                        new LvcPoint(m.Width * DataLabelsTranslate.Value.X, m.Height * DataLabelsTranslate.Value.Y);

                    label.X = labelPosition.X;
                    label.Y = labelPosition.Y;
                }

                OnPointMeasured(data.TargetPoint);
            }

            if (GeometryFill is not null && GeometryFill != Paint.Default)
            {
                cartesianChart.Canvas.AddDrawableTask(GeometryFill, zone: CanvasZone.DrawMargin);
                GeometryFill.ZIndex = actualZIndex + PaintConstants.SeriesGeometryFillZIndexOffset;
            }
            if (GeometryStroke is not null && GeometryStroke != Paint.Default)
            {
                cartesianChart.Canvas.AddDrawableTask(GeometryStroke, zone: CanvasZone.DrawMargin);
                GeometryStroke.ZIndex = actualZIndex + PaintConstants.SeriesGeometryStrokeZIndexOffset;
            }

            if (!isSegmentEmpty) segmentI++;

            fillVector?.TrimTail();
            strokeVector?.TrimTail();
        }

        var maxSegment = fillPathHelperContainer.Count > strokePathHelperContainer.Count
            ? fillPathHelperContainer.Count
            : strokePathHelperContainer.Count;

        for (var i = maxSegment - 1; i >= segmentI; i--)
        {
            if (i < fillPathHelperContainer.Count)
            {
                var segmentFill = fillPathHelperContainer[i];
                Fill?.RemoveGeometryFromPaintTask(cartesianChart.Canvas, segmentFill);
                segmentFill.Commands.Clear();
                fillPathHelperContainer.RemoveAt(i);
            }

            if (i < strokePathHelperContainer.Count)
            {
                var segmentStroke = strokePathHelperContainer[i];
                Stroke?.RemoveGeometryFromPaintTask(cartesianChart.Canvas, segmentStroke);
                segmentStroke.Commands.Clear();
                strokePathHelperContainer.RemoveAt(i);
            }
        }

        if (ShowDataLabels && DataLabelsPaint is not null && DataLabelsPaint != Paint.Default)
        {
            cartesianChart.Canvas.AddDrawableTask(DataLabelsPaint, zone: CanvasZone.DrawMargin);
            DataLabelsPaint.ZIndex = actualZIndex + PaintConstants.SeriesDataLabelsZIndexOffset;
        }
        if (ShowError && ErrorPaint is not null && ErrorPaint != Paint.Default)
        {
            cartesianChart.Canvas.AddDrawableTask(ErrorPaint, zone: CanvasZone.DrawMargin);
            ErrorPaint.ZIndex = actualZIndex + PaintConstants.SeriesGeometryFillZIndexOffset;
        }

        pointsCleanup.CollectPoints(
            everFetched, cartesianChart.View, primaryScale, secondaryScale, SoftDeleteOrDisposePoint);

        _geometrySvgChanged = false;
    }

    /// <inheritdoc cref="Series{TModel, TVisual, TLabel}.FindPointsInPosition(Chart, LvcPoint, FindingStrategy, FindPointFor)"/>
    protected override IEnumerable<ChartPoint> FindPointsInPosition(
        Chart chart, LvcPoint pointerPosition, FindingStrategy strategy, FindPointFor findPointFor)
    {
        return strategy switch
        {
            FindingStrategy.ExactMatch => Fetch(chart)
                .Where(point =>
                {
                    var v = (TVisual?)point.Context.Visual;
                    if (v is null) return false;

                    var x = v.X + v.TranslateTransform.X;
                    var y = v.Y + v.TranslateTransform.Y;

                    return
                        pointerPosition.X > x && pointerPosition.X < x + v.Width &&
                        pointerPosition.Y > y && pointerPosition.Y < y + v.Height;
                }),
            FindingStrategy.ExactMatchTakeClosest => Fetch(chart)
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
            FindingStrategy.ExactMatchTakeClosest or
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
        foreach (var item in points.AsSplineData())
        {
            if (item.IsFirst)
            {
                var c = item.Current.Coordinate;

                var sc = stacker?.GetStack(item.Current).CumulativeStart ?? 0;

                yield return new BezierData(item.Next)
                {
                    X0 = c.SecondaryValue,
                    Y0 = c.PrimaryValue + sc,
                    X1 = c.SecondaryValue,
                    Y1 = c.PrimaryValue + sc,
                    X2 = c.SecondaryValue,
                    Y2 = c.PrimaryValue + sc,
                    IsNextEmpty = item.IsNextEmpty
                };

                continue;
            }

            var pys = 0d;
            var cys = 0d;
            var nys = 0d;
            var nnys = 0d;

            var previous = item.Previous.Coordinate;
            var current = item.Current.Coordinate;
            var next = item.Next.Coordinate;
            var afterNext = item.AfterNext.Coordinate;

            if (stacker is not null)
            {
                pys = stacker.GetStack(item.Previous).CumulativeStart;
                cys = stacker.GetStack(item.Current).CumulativeStart;
                nys = stacker.GetStack(item.Next).CumulativeStart;
                nnys = stacker.GetStack(item.AfterNext).CumulativeStart;
            }

            var xc1 = (previous.SecondaryValue + current.SecondaryValue) / 2.0f;
            var yc1 = (previous.PrimaryValue + pys + current.PrimaryValue + cys) / 2.0f;
            var xc2 = (current.SecondaryValue + next.SecondaryValue) / 2.0f;
            var yc2 = (current.PrimaryValue + cys + next.PrimaryValue + nys) / 2.0f;
            var xc3 = (next.SecondaryValue + afterNext.SecondaryValue) / 2.0f;
            var yc3 = (next.PrimaryValue + nys + afterNext.PrimaryValue + nnys) / 2.0f;

            var len1 = (float)Math.Sqrt(
                (current.SecondaryValue - previous.SecondaryValue) *
                (current.SecondaryValue - previous.SecondaryValue) +
                (current.PrimaryValue + cys - previous.PrimaryValue + pys) * (current.PrimaryValue + cys - previous.PrimaryValue + pys));
            var len2 = (float)Math.Sqrt(
                (next.SecondaryValue - current.SecondaryValue) *
                (next.SecondaryValue - current.SecondaryValue) +
                (next.PrimaryValue + nys - current.PrimaryValue + cys) * (next.PrimaryValue + nys - current.PrimaryValue + cys));
            var len3 = (float)Math.Sqrt(
                (afterNext.SecondaryValue - next.SecondaryValue) *
                (afterNext.SecondaryValue - next.SecondaryValue) +
                (afterNext.PrimaryValue + nnys - next.PrimaryValue + nys) * (afterNext.PrimaryValue + nnys - next.PrimaryValue + nys));

            var k1 = len1 / (len1 + len2);
            var k2 = len2 / (len2 + len3);

            if (float.IsNaN(k1)) k1 = 0f;
            if (float.IsNaN(k2)) k2 = 0f;

            var xm1 = xc1 + (xc2 - xc1) * k1;
            var ym1 = yc1 + (yc2 - yc1) * k1;
            var xm2 = xc2 + (xc3 - xc2) * k2;
            var ym2 = yc2 + (yc3 - yc2) * k2;

            var c1X = xm1 + (xc2 - xm1) * _lineSmoothness + current.SecondaryValue - xm1;
            var c1Y = ym1 + (yc2 - ym1) * _lineSmoothness + current.PrimaryValue + cys - ym1;
            var c2X = xm2 + (xc2 - xm2) * _lineSmoothness + next.SecondaryValue - xm2;
            var c2Y = ym2 + (yc2 - ym2) * _lineSmoothness + next.PrimaryValue + nys - ym2;

            yield return new BezierData(item.Next)
            {
                X0 = c1X,
                Y0 = c1Y,
                X1 = c2X,
                Y1 = c2Y,
                X2 = next.SecondaryValue,
                Y2 = next.PrimaryValue + nys
            };
        }
    }

    /// <inheritdoc cref="Series{TModel, TVisual, TLabel}.SetDefaultPointTransitions(ChartPoint)"/>
    protected override void SetDefaultPointTransitions(ChartPoint chartPoint)
    {
        var chart = chartPoint.Context.Chart;

        if (chartPoint.Context.AdditionalVisuals is not CubicSegmentVisualPoint visual)
            throw new Exception("Unable to initialize the point instance.");

        var easing = EasingFunction ?? chart.CoreChart.ActualEasingFunction;
        var speed = AnimationsSpeed ?? chart.CoreChart.ActualAnimationsSpeed;

        visual.Geometry.Animate(easing, speed);
        visual.Segment.Animate(easing, speed);
        visual.YError?.Animate(easing, speed);
        visual.XError?.Animate(easing, speed);
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
