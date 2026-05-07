
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
using System.Linq;
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
/// <typeparam name="TLineGeometry">The type of the line geometry</typeparam>
public abstract class CoreStepLineSeries<TModel, TVisual, TLabel, TPathGeometry, TLineGeometry>
    : StrokeAndFillCartesianSeries<TModel, TVisual, TLabel>, IStepLineSeries
        where TPathGeometry : BaseVectorGeometry, new()
        where TVisual : BoundedDrawnGeometry, new()
        where TLabel : BaseLabelGeometry, new()
        where TLineGeometry : BaseLineGeometry, new()
{
    internal readonly Dictionary<object, List<TPathGeometry>> _fillPathHelperDictionary = [];
    internal readonly Dictionary<object, List<TPathGeometry>> _strokePathHelperDictionary = [];
    private float _geometrySize = 14f;

    /// <summary>
    /// Initializes a new instance of the <see cref="CoreStepLineSeries{TModel, TVisual, TLabel, TPathGeometry, TLineGeometry}"/> class.
    /// </summary>
    /// <param name="isStacked">if set to <c>true</c> [is stacked].</param>
    /// <param name="values">The values.</param>
    public CoreStepLineSeries(IReadOnlyCollection<TModel>? values, bool isStacked = false)
        : base(GetProperties(isStacked), values)
    {
        DataPadding = new LvcPoint(0.5f, 1f);
    }

    /// <inheritdoc cref="IStepLineSeries.EnableNullSplitting"/>
    public bool EnableNullSplitting { get; set => SetProperty(ref field, value); } = true;

    /// <inheritdoc cref="IStepLineSeries.GeometrySize"/>
    public double GeometrySize { get => _geometrySize; set => SetProperty(ref _geometrySize, (float)value); }

    /// <inheritdoc cref="IStepLineSeries.GeometryFill"/>
    public Paint? GeometryFill
    {
        get;
        set => SetPaintProperty(ref field, value);
    } = Paint.Default;

    /// <inheritdoc cref="IStepLineSeries.GeometrySize"/>
    public Paint? GeometryStroke
    {
        get;
        set => SetPaintProperty(ref field, value, PaintStyle.Stroke);
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

        // see note #240222
        var segments = EnableNullSplitting
            ? Fetch(cartesianChart).SplitByNullGaps(point => DeleteNullPoint(point, secondaryScale, primaryScale))
            : [Fetch(cartesianChart)];

        var stacker = (SeriesProperties & SeriesProperties.Stacked) == SeriesProperties.Stacked
            ? cartesianChart.SeriesContext.GetStackPosition(this, GetStackGroup())
            : null;

        // #1923: see CoreLineSeries.Invalidate for the rationale.
        var actualZIndex = ZIndex != 0
            ? ZIndex
            : stacker is not null
                ? stacker.Stacker.MaxSeriesId - stacker.Position
                : ((ISeries)this).SeriesId;

        var dls = (float)DataLabelsSize;

        var segmentI = 0;
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
        var hasSvg = this.HasVariableSvgGeometry();

        var isFirstDraw = !chart.IsDrawn(((ISeries)this).SeriesId);

        foreach (var segment in segments)
        {
            var hasPaths = false;
            var isSegmentEmpty = true;
            VectorManager? strokeVector = null, fillVector = null;

            double previousPrimary = 0, previousSecondary = 0;

            foreach (var point in segment)
            {
                if (!hasPaths)
                {
                    hasPaths = true;

                    var fillLookup = GetSegmentVisual(segmentI, fillPathHelperContainer, VectorClosingMethod.CloseToPivot);
                    var strokeLookup = GetSegmentVisual(segmentI, strokePathHelperContainer, VectorClosingMethod.NotClosed);

                    // See CoreLineSeries for why the old Count==1 cleanup is gone.

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

                var coordinate = point.Coordinate;

                isSegmentEmpty = false;
                var s = 0d;
                if (stacker is not null)
                    s = stacker.GetStack(point).CumulativeStart;

                var visual = (SegmentVisualPoint?)point.Context.AdditionalVisuals;
                // See CoreLineSeries for the rationale — drives AddConsecutiveSegment's
                // Follows/Copy decision.
                var isVisualNew = visual is null;
                var dp = coordinate.PrimaryValue + s - previousPrimary;
                var ds = coordinate.SecondaryValue - previousSecondary;

                if (!IsVisible)
                {
                    if (visual is not null)
                    {
                        visual.Geometry.X = secondaryScale.ToPixels(coordinate.SecondaryValue);
                        visual.Geometry.Y = p;
                        visual.Geometry.Opacity = 0;
                        visual.Geometry.RemoveOnCompleted = true;

                        visual.Segment.Xi = secondaryScale.ToPixels(coordinate.SecondaryValue - ds);
                        visual.Segment.Xj = secondaryScale.ToPixels(coordinate.SecondaryValue);
                        visual.Segment.Yi = p;
                        visual.Segment.Yj = p;

                        point.Context.Visual = null;
                        point.Context.AdditionalVisuals = null;
                    }

                    if (point.Context.Label is not null)
                    {
                        var label = (TLabel)point.Context.Label;

                        label.X = secondaryScale.ToPixels(coordinate.SecondaryValue);
                        label.Y = p;
                        label.Opacity = 0;
                        label.RemoveOnCompleted = true;

                        point.Context.Label = null;
                    }

                    pointsCleanup.Clean(point);

                    continue;
                }

                if (visual is null)
                {
                    var v = new SegmentVisualPoint(new TVisual());
                    visual = v;

                    // See CoreLineSeries for the rationale — seed motion state for every
                    // new visual, not just isFirstDraw, so a mid-life new visual that
                    // becomes the first point of a brand-new sub-segment path doesn't
                    // swoop in from (0, 0).
                    v.Geometry.X = secondaryScale.ToPixels(coordinate.SecondaryValue);
                    v.Geometry.Y = p;
                    v.Geometry.Width = 0;
                    v.Geometry.Height = 0;

                    v.Segment.Xi = secondaryScale.ToPixels(coordinate.SecondaryValue - ds);
                    v.Segment.Xj = secondaryScale.ToPixels(coordinate.SecondaryValue);
                    v.Segment.Yi = p;
                    v.Segment.Yj = p;

                    point.Context.Visual = v.Geometry;
                    point.Context.AdditionalVisuals = v;
                    OnPointCreated(point);
                }

                visual.Geometry.Opacity = 1;

                if (hasSvg)
                {
                    var svgVisual = (IVariableSvgPath)visual.Geometry;
                    if (_geometrySvgChanged || svgVisual.SVGPath is null)
                        svgVisual.SVGPath = GeometrySvg ?? throw new Exception("svg path is not defined");
                }

                _ = everFetched.Add(point);

                if (GeometryFill is not null && GeometryFill != Paint.Default)
                    GeometryFill.AddGeometryToPaintTask(cartesianChart.Canvas, visual.Geometry);
                if (GeometryStroke is not null && GeometryStroke != Paint.Default)
                    GeometryStroke.AddGeometryToPaintTask(cartesianChart.Canvas, visual.Geometry);

                visual.Segment.Id = point.Context.Entity.MetaData!.EntityIndex;

                if (Fill is not null && Fill != Paint.Default)
                    fillVector!.AddConsecutiveSegment(visual.Segment, isVisualNew && !isFirstDraw);
                if (Stroke is not null && Stroke != Paint.Default)
                    strokeVector!.AddConsecutiveSegment(visual.Segment, isVisualNew && !isFirstDraw);

                visual.Segment.Xi = secondaryScale.ToPixels(coordinate.SecondaryValue - ds);
                visual.Segment.Xj = secondaryScale.ToPixels(coordinate.SecondaryValue);
                visual.Segment.Yi = primaryScale.ToPixels(coordinate.PrimaryValue + s - dp);
                visual.Segment.Yj = primaryScale.ToPixels(coordinate.PrimaryValue + s);

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

                var hags = gs < 8 ? 8 : gs;

                if (point.Context.HoverArea is not RectangleHoverArea ha)
                    point.Context.HoverArea = ha = new RectangleHoverArea();

                _ = ha
                    .SetDimensions(x - uwx * 0.5f, y - hgs, uwx, gs)
                    .CenterXToolTip();

                _ = coordinate.PrimaryValue >= pivot
                    ? ha.StartYToolTip()
                    : ha.EndYToolTip().IsLessThanPivot();

                pointsCleanup.Clean(point);

                if (ShowDataLabels && DataLabelsPaint is not null && DataLabelsPaint != Paint.Default)
                {
                    var label = (TLabel?)point.Context.Label;

                    if (label is null)
                    {
                        var l = new TLabel { X = x - hgs, Y = p - hgs, RotateTransform = (float)DataLabelsRotation, MaxWidth = (float)DataLabelsMaxWidth };
                        l.Animate(
                            EasingFunction ?? cartesianChart.ActualEasingFunction,
                            AnimationsSpeed ?? cartesianChart.ActualAnimationsSpeed,
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

                    var m = label.Measure();
                    var labelPosition = GetLabelPosition(
                        x - hgs, y - hgs, gs, gs, m, DataLabelsPosition,
                        SeriesProperties, coordinate.PrimaryValue > Pivot, drawLocation, drawMarginSize);
                    if (DataLabelsTranslate is not null) label.TranslateTransform =
                        new LvcPoint(m.Width * DataLabelsTranslate.Value.X, m.Height * DataLabelsTranslate.Value.Y);

                    label.X = labelPosition.X;
                    label.Y = labelPosition.Y;
                }

                OnPointMeasured(point);

                previousPrimary = coordinate.PrimaryValue + s;
                previousSecondary = coordinate.SecondaryValue;
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

    /// <inheritdoc cref="Series{TModel, TVisual, TLabel}.GetMiniatureGeometry(ChartPoint?)"/>
    public override IDrawnElement GetMiniatureGeometry(ChartPoint? point)
    {
        var noGeometryPaint = GeometryStroke is null && GeometryFill is null;
        var usesLine = (GeometrySize < 1 || noGeometryPaint) && Stroke is not null;

        var v = point?.Context.Visual;

        if (usesLine)
        {
            return new TLineGeometry
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

    /// <inheritdoc cref="Series{TModel, TVisual, TLabel}.OnPointerLeft(ChartPoint)"/>
    protected override void OnPointerLeft(ChartPoint point)
    {
        var visual = (TVisual?)point.Context.Visual;
        if (visual is null) return;
        visual.ScaleTransform = new LvcPoint(1f, 1f);

        base.OnPointerLeft(point);
    }

    /// <inheritdoc cref="Series{TModel, TVisual, TLabel}.SetDefaultPointTransitions(ChartPoint)"/>
    protected override void SetDefaultPointTransitions(ChartPoint chartPoint)
    {
        var chart = chartPoint.Context.Chart;

        if (chartPoint.Context.AdditionalVisuals is not SegmentVisualPoint visual)
            throw new Exception("Unable to initialize the point instance.");

        visual.Geometry.Animate(EasingFunction ?? chart.CoreChart.ActualEasingFunction, AnimationsSpeed ?? chart.CoreChart.ActualAnimationsSpeed);
        visual.Segment.Animate(EasingFunction ?? chart.CoreChart.ActualEasingFunction, AnimationsSpeed ?? chart.CoreChart.ActualAnimationsSpeed);
    }

    /// <inheritdoc cref="CartesianSeries{TModel, TVisual, TLabel}.SoftDeleteOrDisposePoint(ChartPoint, Scaler, Scaler)"/>
    protected internal override void SoftDeleteOrDisposePoint(ChartPoint point, Scaler primaryScale, Scaler secondaryScale)
    {
        var visual = (SegmentVisualPoint?)point.Context.AdditionalVisuals;
        if (visual is null) return;
        if (DataFactory is null) throw new Exception("Data provider not found");

        var coordinate = point.Coordinate;

        var x = secondaryScale.ToPixels(coordinate.SecondaryValue);
        var y = primaryScale.ToPixels(coordinate.PrimaryValue);

        visual.Geometry.X = x + visual.Geometry.Width * 0.5f;
        visual.Geometry.Y = y + visual.Geometry.Height * 0.5f;
        visual.Geometry.Height = 0;
        visual.Geometry.Width = 0;
        visual.Geometry.Opacity = 0;
        visual.Geometry.RemoveOnCompleted = true;

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

    /// <inheritdoc cref="IChartElement.RemoveFromUI(Chart)"/>
    public override void RemoveFromUI(Chart chart)
    {
        base.RemoveFromUI(chart);

        _ = _fillPathHelperDictionary.Remove(chart.Canvas.Sync);
        _ = _strokePathHelperDictionary.Remove(chart.Canvas.Sync);
    }

    /// <summary>
    /// Gets the paint tasks.
    /// </summary>
    /// <returns></returns>
    protected internal override Paint?[] GetPaintTasks() =>
        [Stroke, Fill, GeometryFill, GeometryStroke, DataLabelsPaint];

    private void DeleteNullPoint(ChartPoint point, Scaler xScale, Scaler yScale)
    {
        if (point.Context.Visual is not SegmentVisualPoint visual) return;

        var coordinate = point.Coordinate;

        var x = xScale.ToPixels(coordinate.SecondaryValue);
        var y = yScale.ToPixels(coordinate.PrimaryValue);
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

    private class SegmentVisual(bool isNew, TPathGeometry path)
    {
        public bool IsNew { get; set; } = isNew;

        public TPathGeometry Path { get; set; } = path;
    }

    private static SeriesProperties GetProperties(bool isStacked)
    {
        return SeriesProperties.StepLine | SeriesProperties.PrimaryAxisVerticalOrientation |
            SeriesProperties.Sketch | SeriesProperties.PrefersXStrategyTooltips |
            (isStacked ? SeriesProperties.Stacked : 0);
    }
}
