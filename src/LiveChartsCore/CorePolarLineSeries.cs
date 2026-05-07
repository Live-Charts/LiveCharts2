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
/// Defines the data to plot as a polar line.
/// </summary>
/// <typeparam name="TModel">The type of the model to plot.</typeparam>
/// <typeparam name="TVisual">The type of the visual point.</typeparam>
/// <typeparam name="TLabel">The type of the data label.</typeparam>
/// <typeparam name="TPathGeometry">The type of the path geometry.</typeparam>
/// <typeparam name="TLineGeometry">The type of the line geometry</typeparam>
public abstract class CorePolarLineSeries<TModel, TVisual, TLabel, TPathGeometry, TLineGeometry>
    : Series<TModel, TVisual, TLabel>, IPolarLineSeries, IPolarSeries
        where TPathGeometry : BaseVectorGeometry, new()
        where TVisual : BoundedDrawnGeometry, new()
        where TLabel : BaseLabelGeometry, new()
        where TLineGeometry : BaseLineGeometry, new()
{
    private readonly Dictionary<object, List<TPathGeometry>> _fillPathHelperDictionary = [];
    private readonly Dictionary<object, List<TPathGeometry>> _strokePathHelperDictionary = [];
    private float _lineSmoothness = 0.65f;
    private float _geometrySize = 14f;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorePolarLineSeries{TModel, TVisual, TLabel, TPathGeometry, TLineGeometry}"/> class.
    /// </summary>
    /// <param name="isStacked">if set to <c>true</c> [is stacked].</param>
    /// <param name="values">The values.</param>
    public CorePolarLineSeries(IReadOnlyCollection<TModel>? values, bool isStacked = false)
        : base(GetProperties(isStacked), values)
    {
        DataPadding = new LvcPoint(1f, 1.5f);
    }

    /// <summary>
    /// Gets or sets the stroke.
    /// </summary>
    /// <value>
    /// The stroke.
    /// </value>
    public Paint? Stroke
    {
        get;
        set => SetPaintProperty(ref field, value, PaintStyle.Stroke);
    } = Paint.Default;

    /// <summary>
    /// Gets or sets the fill.
    /// </summary>
    /// <value>
    /// The fill.
    /// </value>
    public Paint? Fill
    {
        get;
        set => SetPaintProperty(ref field, value);
    } = Paint.Default;

    /// <inheritdoc cref="ILineSeries.GeometrySize"/>
    public double GeometrySize { get => _geometrySize; set => SetProperty(ref _geometrySize, (float)value); }

    /// <inheritdoc cref="IPolarSeries.ScalesAngleAt"/>
    public int ScalesAngleAt { get; set => SetProperty(ref field, value); }

    /// <inheritdoc cref="IPolarSeries.ScalesRadiusAt"/>
    public int ScalesRadiusAt { get; set => SetProperty(ref field, value); }

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

    /// <inheritdoc cref="IPolarLineSeries.IsClosed"/>
    public bool IsClosed { get; set => SetProperty(ref field, value); } = true;

    /// <inheritdoc cref="IPolarSeries.DataLabelsPosition"/>
    public PolarLabelsPosition DataLabelsPosition { get; set => SetProperty(ref field, value); }

    /// <summary>
    /// Gets or sets the tool tip label formatter for the X axis, this function will build the label when a point in this series 
    /// is shown inside a tool tip.
    /// </summary>
    /// <value>
    /// The tool tip label formatter.
    /// </value>
    public Func<ChartPoint<TModel, TVisual, TLabel>, string>? AngleToolTipLabelFormatter
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    /// Gets or sets the tool tip label formatter for the Y axis, this function will build the label when a point in this series 
    /// is shown inside a tool tip.
    /// </summary>
    /// <value>
    /// The tool tip label formatter.
    /// </value>
    public Func<ChartPoint<TModel, TVisual, TLabel>, string>? RadiusToolTipLabelFormatter
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc cref="ChartElement.Invalidate(Chart)"/>
    public override void Invalidate(Chart chart)
    {
        var polarChart = (PolarChartEngine)chart;
        var angleAxis = polarChart.GetAngleAxis(this);
        var radiusAxis = polarChart.GetRadiusAxis(this);

        var drawLocation = polarChart.DrawMarginLocation;
        var drawMarginSize = polarChart.DrawMarginSize;

        var scaler = new PolarScaler(
            drawLocation, drawMarginSize, angleAxis, radiusAxis,
            polarChart.InnerRadius, polarChart.InitialRotation, polarChart.TotalAnge);

        var gs = _geometrySize;
        var hgs = gs / 2f;
        var sw = Stroke?.StrokeThickness ?? 0;

        var fetched = Fetch(polarChart);
        if (fetched is not ChartPoint[] points) points = [.. fetched];

        var segments = EnableNullSplitting
            ? SplitEachNull(points, scaler)
            : [points];

        var stacker = (SeriesProperties & SeriesProperties.Stacked) == SeriesProperties.Stacked
            ? polarChart.SeriesContext.GetStackPosition(this, GetStackGroup())
            : null;

        // #1923: see CoreLineSeries.Invalidate for the rationale.
        var actualZIndex = ZIndex != 0
            ? ZIndex
            : stacker is not null
                ? stacker.Stacker.MaxSeriesId - stacker.Position
                : ((ISeries)this).SeriesId;

        var dls = unchecked((float)DataLabelsSize);

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

        var r = (float)DataLabelsRotation;
        var isTangent = false;
        var isCotangent = false;

        if (((int)r & (int)LiveCharts.TangentAngle) != 0)
        {
            r -= (int)LiveCharts.TangentAngle;
            isTangent = true;
        }

        if (((int)r & (int)LiveCharts.CotangentAngle) != 0)
        {
            r -= (int)LiveCharts.CotangentAngle;
            isCotangent = true;
        }

        var segmentI = 0;
        var hasSvg = this.HasVariableSvgGeometry();

        var isFirstDraw = !chart.IsDrawn(((ISeries)this).SeriesId);

        foreach (var segment in segments)
        {
            var hasPaths = false;
            var isSegmentEmpty = true;
            VectorManager? strokeVector = null, fillVector = null;

            foreach (var data in GetSpline(segment, scaler, stacker))
            {
                if (!hasPaths)
                {
                    hasPaths = true;

                    var fillLookup = GetSegmentVisual(segmentI, fillPathHelperContainer, VectorClosingMethod.NotClosed);
                    var strokeLookup = GetSegmentVisual(segmentI, strokePathHelperContainer, VectorClosingMethod.NotClosed);

                    // See CoreLineSeries for why the old Count==1 cleanup is gone.

                    var isNew = fillLookup.IsNew || strokeLookup.IsNew;
                    var fillPath = fillLookup.Path;
                    var strokePath = strokeLookup.Path;

                    strokeVector = new VectorManager(strokePath.Commands);
                    fillVector = new VectorManager(fillPath.Commands);

                    if (Fill is not null && Fill != Paint.Default)
                    {
                        Fill.AddGeometryToPaintTask(polarChart.Canvas, fillPath);
                        polarChart.Canvas.AddDrawableTask(Fill, zone: CanvasZone.DrawMargin);
                        Fill.ZIndex = actualZIndex + PaintConstants.SeriesFillZIndexOffset;
                        if (isNew)
                        {
                            fillPath.Animate(EasingFunction ?? polarChart.ActualEasingFunction, AnimationsSpeed ?? polarChart.ActualAnimationsSpeed);
                        }
                    }
                    if (Stroke is not null && Stroke != Paint.Default)
                    {
                        Stroke.AddGeometryToPaintTask(polarChart.Canvas, strokePath);
                        polarChart.Canvas.AddDrawableTask(Stroke, zone: CanvasZone.DrawMargin);
                        Stroke.ZIndex = actualZIndex + PaintConstants.SeriesStrokeZIndexOffset;
                        if (isNew)
                        {
                            strokePath.Animate(EasingFunction ?? polarChart.ActualEasingFunction, AnimationsSpeed ?? polarChart.ActualAnimationsSpeed);
                        }
                    }

                    strokePath.Opacity = IsVisible ? 1 : 0;
                    fillPath.Opacity = IsVisible ? 1 : 0;
                }

                var coordinate = data.TargetPoint.Coordinate;

                isSegmentEmpty = false;
                var s = 0d;
                if (stacker is not null)
                {
                    s = stacker.GetStack(data.TargetPoint).CumulativeStart;
                }

                var cp = scaler.ToPixels(coordinate.SecondaryValue, coordinate.PrimaryValue + s);

                var visual = (CubicSegmentVisualPoint?)data.TargetPoint.Context.AdditionalVisuals;
                // See CoreLineSeries — drives AddConsecutiveSegment's Follows/Copy decision.
                var isVisualNew = visual is null;

                if (visual is null)
                {
                    var v = new CubicSegmentVisualPoint(new TVisual());

                    visual = v;

                    var x0b = scaler.CenterX - hgs;
                    var x1b = scaler.CenterX - hgs;
                    var x2b = scaler.CenterX - hgs;
                    var y0b = scaler.CenterY - hgs;
                    var y1b = scaler.CenterY - hgs;
                    var y2b = scaler.CenterY - hgs;

                    v.Geometry.X = scaler.CenterX;
                    v.Geometry.Y = scaler.CenterY;
                    v.Geometry.Width = gs;
                    v.Geometry.Height = gs;

                    v.Segment.Xi = (float)x0b;
                    v.Segment.Yi = y0b;
                    v.Segment.Xm = (float)x1b;
                    v.Segment.Ym = y1b;
                    v.Segment.Xj = (float)x2b;
                    v.Segment.Yj = y2b;

                    data.TargetPoint.Context.Visual = v.Geometry;
                    data.TargetPoint.Context.AdditionalVisuals = v;
                    OnPointCreated(data.TargetPoint);
                }

                if (hasSvg)
                {
                    var svgVisual = (IVariableSvgPath)visual.Geometry;
                    if (_geometrySvgChanged || svgVisual.SVGPath is null)
                        svgVisual.SVGPath = GeometrySvg ?? throw new Exception("svg path is not defined");
                }

                _ = everFetched.Add(data.TargetPoint);

                if (GeometryFill is not null && GeometryFill != Paint.Default)
                    GeometryFill.AddGeometryToPaintTask(polarChart.Canvas, visual.Geometry);
                if (GeometryStroke is not null && GeometryStroke != Paint.Default)
                    GeometryStroke.AddGeometryToPaintTask(polarChart.Canvas, visual.Geometry);

                visual.Segment.Id = data.TargetPoint.Context.Entity.MetaData!.EntityIndex;

                if (Fill is not null && Fill != Paint.Default)
                    fillVector!.AddConsecutiveSegment(visual.Segment, isVisualNew && !isFirstDraw);
                if (Stroke is not null && Stroke != Paint.Default)
                    strokeVector!.AddConsecutiveSegment(visual.Segment, isVisualNew && !isFirstDraw);

                visual.Segment.Xi = (float)data.X0;
                visual.Segment.Yi = (float)data.Y0;
                visual.Segment.Xm = (float)data.X1;
                visual.Segment.Ym = (float)data.Y1;
                visual.Segment.Xj = (float)data.X2;
                visual.Segment.Yj = (float)data.Y2;

                var x = cp.X;
                var y = cp.Y;

                visual.Geometry.X = x - hgs;
                visual.Geometry.Y = y - hgs;
                visual.Geometry.Width = gs;
                visual.Geometry.Height = gs;
                visual.Geometry.RemoveOnCompleted = false;

                var hags = gs < 16 ? 16 : gs;
                if (data.TargetPoint.Context.HoverArea is not RectangleHoverArea ha)
                    data.TargetPoint.Context.HoverArea = ha = new RectangleHoverArea();
                _ = ha.SetDimensions(x - hags * 0.5f, y - hags * 0.5f, hags, hags).CenterXToolTip().CenterYToolTip();

                pointsCleanup.Clean(data.TargetPoint);

                if (ShowDataLabels && DataLabelsPaint is not null && DataLabelsPaint != Paint.Default)
                {
                    var label = (TLabel?)data.TargetPoint.Context.Label;

                    var actualRotation = r +
                        (isTangent ? scaler.GetAngle(coordinate.SecondaryValue) - 90 : 0) +
                        (isCotangent ? scaler.GetAngle(coordinate.SecondaryValue) : 0);

                    if ((isTangent || isCotangent) && ((actualRotation + 90) % 360) > 180)
                        actualRotation += 180;

                    if (label is null)
                    {
                        var l = new TLabel { X = x - hgs, Y = scaler.CenterY - hgs, RotateTransform = (float)actualRotation, MaxWidth = (float)DataLabelsMaxWidth };
                        l.Animate(
                            EasingFunction ?? chart.ActualEasingFunction,
                            AnimationsSpeed ?? chart.ActualAnimationsSpeed,
                            BaseLabelGeometry.XProperty,
                            BaseLabelGeometry.YProperty);
                        label = l;
                        data.TargetPoint.Context.Label = l;
                    }

                    DataLabelsPaint.AddGeometryToPaintTask(polarChart.Canvas, label);
                    label.Text = DataLabelsFormatter(new ChartPoint<TModel, TVisual, TLabel>(data.TargetPoint));
                    label.TextSize = dls;
                    label.Padding = DataLabelsPadding;
                    label.RotateTransform = actualRotation;
                    label.Paint = DataLabelsPaint;

                    var rad = Math.Sqrt(Math.Pow(cp.X - scaler.CenterX, 2) + Math.Pow(cp.Y - scaler.CenterY, 2));

                    if (isFirstDraw)
                        label.CompleteTransition(
                            BaseLabelGeometry.TextSizeProperty,
                            BaseLabelGeometry.XProperty,
                            BaseLabelGeometry.YProperty,
                            BaseLabelGeometry.RotateTransformProperty);

                    var labelPosition = GetLabelPolarPosition(
                        scaler.CenterX, scaler.CenterY, (float)rad, scaler.GetAngle(coordinate.SecondaryValue),
                        label.Measure(), (float)GeometrySize, DataLabelsPosition);

                    label.X = labelPosition.X;
                    label.Y = labelPosition.Y;
                }

                OnPointMeasured(data.TargetPoint);
            }

            if (GeometryFill is not null && GeometryFill != Paint.Default)
            {
                polarChart.Canvas.AddDrawableTask(GeometryFill, zone: CanvasZone.DrawMargin);
                GeometryFill.ZIndex = actualZIndex + PaintConstants.SeriesGeometryFillZIndexOffset;
            }
            if (GeometryStroke is not null && GeometryStroke != Paint.Default)
            {
                polarChart.Canvas.AddDrawableTask(GeometryStroke, zone: CanvasZone.DrawMargin);
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
                Fill?.RemoveGeometryFromPaintTask(polarChart.Canvas, segmentFill);
                segmentFill.Commands.Clear();
                fillPathHelperContainer.RemoveAt(i);
            }

            if (i < strokePathHelperContainer.Count)
            {
                var segmentStroke = strokePathHelperContainer[i];
                Stroke?.RemoveGeometryFromPaintTask(polarChart.Canvas, segmentStroke);
                segmentStroke.Commands.Clear();
                strokePathHelperContainer.RemoveAt(i);
            }
        }

        if (ShowDataLabels && DataLabelsPaint is not null && DataLabelsPaint != Paint.Default)
        {
            polarChart.Canvas.AddDrawableTask(DataLabelsPaint, zone: CanvasZone.DrawMargin);
            DataLabelsPaint.ZIndex = actualZIndex + PaintConstants.SeriesDataLabelsZIndexOffset;
        }

        pointsCleanup.CollectPoints(
            everFetched, polarChart.View, scaler, SoftDeleteOrDisposePoint);

        _geometrySvgChanged = false;
    }

    /// <inheritdoc cref="IPolarSeries.GetBounds(Chart, IPolarAxis, IPolarAxis)"/>
    public virtual SeriesBounds GetBounds(
        Chart chart, IPolarAxis angleAxis, IPolarAxis radiusAxis)
    {
        var baseSeriesBounds = DataFactory is null
            ? throw new Exception("A data provider is required")
            : DataFactory.GetCartesianBounds(chart, this, angleAxis, radiusAxis);

        if (baseSeriesBounds.HasData) return baseSeriesBounds;
        var baseBounds = baseSeriesBounds.Bounds;

        var tickPrimary = radiusAxis.GetTick((PolarChartEngine)chart, baseBounds.VisiblePrimaryBounds);

        var tp = tickPrimary.Value * DataPadding.Y;

        if (baseBounds.VisiblePrimaryBounds.Delta == 0)
        {
            var mp = baseBounds.VisiblePrimaryBounds.Min == 0 ? 1 : baseBounds.VisiblePrimaryBounds.Min;
            tp = 0.1 * mp * DataPadding.Y;
        }

        var rgs = GeometrySize * 0.5f + (Stroke?.StrokeThickness ?? 0);

        return
            new SeriesBounds(
                new DimensionalBounds
                {
                    SecondaryBounds = new Bounds
                    {
                        Max = baseBounds.SecondaryBounds.Max,
                        Min = baseBounds.SecondaryBounds.Min,
                        MinDelta = baseBounds.SecondaryBounds.MinDelta,
                        PaddingMax = 1,
                        PaddingMin = 0,
                        RequestedGeometrySize = rgs
                    },
                    PrimaryBounds = new Bounds
                    {
                        Max = baseBounds.PrimaryBounds.Max,
                        Min = baseBounds.PrimaryBounds.Min,
                        MinDelta = baseBounds.PrimaryBounds.MinDelta,
                        PaddingMax = tp,
                        PaddingMin = tp,
                        RequestedGeometrySize = rgs
                    },
                    VisibleSecondaryBounds = new Bounds
                    {
                        Max = baseBounds.VisibleSecondaryBounds.Max,
                        Min = baseBounds.VisibleSecondaryBounds.Min,
                    },
                    VisiblePrimaryBounds = new Bounds
                    {
                        Max = baseBounds.VisiblePrimaryBounds.Max,
                        Min = baseBounds.VisiblePrimaryBounds.Min
                    }
                },
                false);
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
            Fill = v?.Fill ?? Fill,
            Stroke = v?.Stroke ?? Stroke,
            StrokeThickness = (float)MiniatureStrokeThickness,
            ClippingBounds = LvcRectangle.Empty,
            Width = (float)MiniatureShapeSize,
            Height = (float)MiniatureShapeSize,
            RotateTransform = v?.RotateTransform ?? 0
        };

        if (m is IVariableSvgPath svg) svg.SVGPath = GeometrySvg;

        return m;
    }

    /// <inheritdoc cref="ISeries.GetPrimaryToolTipText(ChartPoint)"/>
    public override string? GetPrimaryToolTipText(ChartPoint point)
    {
        string? label = null;

        if (RadiusToolTipLabelFormatter is not null)
            label = RadiusToolTipLabelFormatter(new ChartPoint<TModel, TVisual, TLabel>(point));

        if (label is null)
        {
            var cc = (PolarChartEngine)point.Context.Chart.CoreChart;
            var cs = (IPolarSeries)point.Context.Series;

            var ax = cc.RadiusAxes[cs.ScalesRadiusAt];

            label = ax.Labels is not null
                ? Labelers.BuildNamedLabeler(ax.Labels)(point.Coordinate.PrimaryValue)
                : ax.Labeler(point.Coordinate.PrimaryValue);
        }

        return label;
    }

    /// <inheritdoc cref="ISeries.GetSecondaryToolTipText(ChartPoint)"/>
    public override string? GetSecondaryToolTipText(ChartPoint point)
    {
        string? label = null;

        if (AngleToolTipLabelFormatter is not null)
            label = AngleToolTipLabelFormatter(new ChartPoint<TModel, TVisual, TLabel>(point));

        if (label is null)
        {
            var cc = (PolarChartEngine)point.Context.Chart.CoreChart;
            var cs = (IPolarSeries)point.Context.Series;

            var ax = cc.AngleAxes[cs.ScalesAngleAt];

            label = ax.Labels is not null
                ? Labelers.BuildNamedLabeler(ax.Labels)(point.Coordinate.SecondaryValue)
                : (ax.Labeler != Labelers.Default
                    ? ax.Labeler(point.Coordinate.SecondaryValue)
                    : LiveCharts.IgnoreToolTipLabel);
        }

        return label;
    }

    /// <summary>
    /// Builds an spline from the given points.
    /// </summary>
    /// <param name="points"></param>
    /// <param name="scaler"></param>
    /// <param name="stacker"></param>
    /// <returns></returns>
    protected internal IEnumerable<BezierData> GetSpline(
        ChartPoint[] points,
        PolarScaler scaler,
        StackPosition? stacker)
    {
        if (points.Length == 0) yield break;

        LvcPoint previous, current, next, next2;

        for (var i = 0; i < points.Length; i++)
        {
            var isClosed = IsClosed && points.Length > 2;

            var a1 = i + 1 - points.Length;
            var a2 = i + 2 - points.Length;

            var p0 = points[i - 1 < 0 ? (isClosed ? points.Length - 1 : 0) : i - 1];
            var p1 = points[i];
            var p2 = points[i + 1 > points.Length - 1 ? (isClosed ? a1 : points.Length - 1) : i + 1];
            var p3 = points[i + 2 > points.Length - 1 ? (isClosed ? a2 : points.Length - 1) : i + 2];

            var p0c = p0.Coordinate;
            var p1c = p1.Coordinate;
            var p2c = p2.Coordinate;
            var p3c = p3.Coordinate;

            previous = scaler.ToPixels(p0c.SecondaryValue, p0c.PrimaryValue);
            current = scaler.ToPixels(p1c.SecondaryValue, p1c.PrimaryValue);
            next = scaler.ToPixels(p2c.SecondaryValue, p2c.PrimaryValue);
            next2 = scaler.ToPixels(p3c.SecondaryValue, p3c.PrimaryValue);

            var pys = 0d;
            var cys = 0d;
            var nys = 0d;
            var nnys = 0d;

            if (stacker is not null)
            {
                pys = scaler.ToPixels(0, stacker.GetStack(p0).CumulativeStart).Y;
                cys = scaler.ToPixels(0, stacker.GetStack(p1).CumulativeStart).Y;
                nys = scaler.ToPixels(0, stacker.GetStack(p2).CumulativeStart).Y;
                nnys = scaler.ToPixels(0, stacker.GetStack(p3).CumulativeStart).Y;
            }

            var xc1 = (previous.X + current.X) / 2.0f;
            var yc1 = (previous.Y + pys + current.Y + cys) / 2.0f;
            var xc2 = (current.X + next.X) / 2.0f;
            var yc2 = (current.Y + cys + next.Y + nys) / 2.0f;
            var xc3 = (next.X + next2.X) / 2.0f;
            var yc3 = (next.Y + nys + next2.Y + nnys) / 2.0f;

            var len1 = (float)Math.Sqrt(
                (current.X - previous.X) *
                (current.X - previous.X) +
                (current.Y + cys - previous.Y + pys) * (current.Y + cys - previous.Y + pys));
            var len2 = (float)Math.Sqrt(
                (next.X - current.X) *
                (next.X - current.X) +
                (next.Y + nys - current.Y + cys) * (next.Y + nys - current.Y + cys));
            var len3 = (float)Math.Sqrt(
                (next2.X - next.X) *
                (next2.X - next.X) +
                (next2.Y + nnys - next.Y + nys) * (next2.Y + nnys - next.Y + nys));

            var k1 = len1 / (len1 + len2);
            var k2 = len2 / (len2 + len3);

            if (float.IsNaN(k1)) k1 = 0f;
            if (float.IsNaN(k2)) k2 = 0f;

            var xm1 = xc1 + (xc2 - xc1) * k1;
            var ym1 = yc1 + (yc2 - yc1) * k1;
            var xm2 = xc2 + (xc3 - xc2) * k2;
            var ym2 = yc2 + (yc3 - yc2) * k2;

            var c1X = xm1 + (xc2 - xm1) * _lineSmoothness + current.X - xm1;
            var c1Y = ym1 + (yc2 - ym1) * _lineSmoothness + current.Y + cys - ym1;
            var c2X = xm2 + (xc2 - xm2) * _lineSmoothness + next.X - xm2;
            var c2Y = ym2 + (yc2 - ym2) * _lineSmoothness + next.Y + nys - ym2;

            double x0, y0;

            if (i == 0)
            {
                x0 = current.X;
                y0 = current.Y + cys;
            }
            else
            {
                x0 = c1X;
                y0 = c1Y;
            }

            yield return new BezierData(points[i])
            {
                X0 = x0,
                Y0 = y0,
                X1 = c2X,
                Y1 = c2Y,
                X2 = next.X,
                Y2 = next.Y
            };
        }
    }
    /// <inheritdoc cref="Series{TModel, TVisual, TLabel}.SetDefaultPointTransitions(ChartPoint)"/>
    protected override void SetDefaultPointTransitions(ChartPoint chartPoint)
    {
        var chart = chartPoint.Context.Chart;

        if (chartPoint.Context.AdditionalVisuals is not CubicSegmentVisualPoint visual)
            throw new Exception("Unable to initialize the point instance.");

        visual.Geometry.Animate(EasingFunction ?? chart.CoreChart.ActualEasingFunction, AnimationsSpeed ?? chart.CoreChart.ActualAnimationsSpeed);
        visual.Segment.Animate(EasingFunction ?? chart.CoreChart.ActualEasingFunction, AnimationsSpeed ?? chart.CoreChart.ActualAnimationsSpeed);
    }

    /// <summary>
    /// Softs the delete point.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="scaler">The scaler.</param>
    protected virtual void SoftDeleteOrDisposePoint(ChartPoint point, PolarScaler scaler)
    {
        var visual = (CubicSegmentVisualPoint?)point.Context.AdditionalVisuals;
        if (visual is null) return;
        if (DataFactory is null) throw new Exception("Data provider not found");

        var p = scaler.ToPixels(point);
        var x = p.X;
        var y = p.Y;

        visual.Geometry.X = x;
        visual.Geometry.Y = y;
        visual.Geometry.Height = 0;
        visual.Geometry.Width = 0;
        visual.Geometry.RemoveOnCompleted = true;

        DataFactory.DisposePoint(point);

        var label = (TLabel?)point.Context.Label;
        if (label is null) return;

        label.TextSize = 1;
        label.RemoveOnCompleted = true;
    }

    /// <inheritdoc cref="Series{TModel, TVisual, TLabel}.SoftDeleteOrDispose(IChartView)"/>
    public override void SoftDeleteOrDispose(IChartView chart)
    {
        var core = ((IPolarChartView)chart).Core;

        var scale = new PolarScaler(
            core.DrawMarginLocation, core.DrawMarginSize, core.AngleAxes[ScalesAngleAt], core.RadiusAxes[ScalesRadiusAt],
            core.InnerRadius, core.InitialRotation, core.TotalAnge);

        var deleted = new List<ChartPoint>();
        foreach (var point in everFetched)
        {
            if (point.Context.Chart != chart) continue;

            SoftDeleteOrDisposePoint(point, scale);
            deleted.Add(point);
        }

        foreach (var pt in GetPaintTasks())
        {
            if (pt is not null) core.Canvas.RemovePaintTask(pt);
        }

        foreach (var item in deleted) _ = everFetched.Remove(item);

        var canvas = ((IPolarChartView)chart).CoreCanvas;

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

    /// <inheritdoc cref="ChartElement.GetPaintTasks"/>
    protected internal override Paint?[] GetPaintTasks() =>
        [Stroke, Fill, GeometryFill, GeometryStroke, DataLabelsPaint];

    /// <summary>
    /// Gets the label polar position.
    /// </summary>
    /// <param name="centerX">The center x.</param>
    /// <param name="centerY">The center y.</param>
    /// <param name="radius">The radius.</param>
    /// <param name="angle">The start angle.</param>
    /// <param name="labelSize">Size of the label.</param>
    /// <param name="geometrySize">The geometry size.</param>
    /// <param name="position">The position.</param>
    /// <returns></returns>
    protected virtual LvcPoint GetLabelPolarPosition(
        float centerX,
        float centerY,
        float radius,
        float angle,
        LvcSize labelSize,
        float geometrySize,
        PolarLabelsPosition position)
    {
        const float toRadians = (float)(Math.PI / 180);
        float actualAngle = 0;

        switch (position)
        {
            case PolarLabelsPosition.End:
                actualAngle = angle;
                radius += (float)Math.Sqrt(
                    Math.Pow(labelSize.Width + geometrySize * 0.5f, 2) +
                    Math.Pow(labelSize.Height + geometrySize * 0.5f, 2)) * 0.5f;
                break;
            case PolarLabelsPosition.Start:
                actualAngle = angle;
                radius -= (float)Math.Sqrt(
                    Math.Pow(labelSize.Width + geometrySize * 0.5f, 2) +
                    Math.Pow(labelSize.Height + geometrySize * 0.5f, 2)) * 0.5f;
                break;
            case PolarLabelsPosition.Outer:
                actualAngle = angle;
                radius *= 2;
                break;
            case PolarLabelsPosition.Middle:
                actualAngle = angle;
                break;
            case PolarLabelsPosition.ChartCenter:
                return new LvcPoint(centerX, centerY);
            default:
                break;
        }

        actualAngle %= 360;
        if (actualAngle < 0) actualAngle += 360;
        actualAngle *= toRadians;

        return new LvcPoint(
             (float)(centerX + Math.Cos(actualAngle) * radius),
             (float)(centerY + Math.Sin(actualAngle) * radius));
    }

    private IEnumerable<ChartPoint[]> SplitEachNull(
        ChartPoint[] points,
        PolarScaler scaler)
    {
        var l = new List<ChartPoint>(points.Length);

        foreach (var point in points)
        {
            if (point.IsEmpty || !IsVisible)
            {
                if (point.Context.Visual is CubicSegmentVisualPoint visual)
                {
                    var s = scaler.ToPixels(point);
                    var x = s.X;
                    var y = s.Y;
                    var gs = _geometrySize;
                    var hgs = gs / 2f;
                    var sw = Stroke?.StrokeThickness ?? 0;
                    visual.Geometry.X = x - hgs;
                    visual.Geometry.Y = y - hgs;
                    visual.Geometry.Width = gs;
                    visual.Geometry.Height = gs;
                    visual.Geometry.RemoveOnCompleted = true;
                    point.Context.Visual = null;
                }

                if (l.Count > 0) yield return l.ToArray();
                l = new List<ChartPoint>(points.Length);
                continue;
            }

            l.Add(point);
        }

        if (l.Count > 0) yield return l.ToArray();
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

    private static SeriesProperties GetProperties(bool isStacked = false)
    {
        return SeriesProperties.Polar | SeriesProperties.PolarLine |
            SeriesProperties.Sketch | SeriesProperties.PrefersXYStrategyTooltips |
            (isStacked ? SeriesProperties.Stacked : 0);
    }
}
