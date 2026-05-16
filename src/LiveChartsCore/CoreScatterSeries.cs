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

    /// <inheritdoc cref="ChartElement.Invalidate(Chart)"/>
    public override void Invalidate(Chart chart)
    {
        var cartesianChart = (CartesianChartEngine)chart;
        _ = GetAnimation(cartesianChart);

        var primaryAxis = cartesianChart.GetYAxis(this);
        var secondaryAxis = cartesianChart.GetXAxis(this);

        var drawLocation = cartesianChart.DrawMarginLocation;
        var drawMarginSize = cartesianChart.DrawMarginSize;
        var xScale = new Scaler(drawLocation, drawMarginSize, secondaryAxis);
        var yScale = new Scaler(drawLocation, drawMarginSize, primaryAxis);

        var actualZIndex = ZIndex == 0 ? ((ISeries)this).SeriesId : ZIndex;

        var weightStackIndex = StackGroup ?? ((ISeries)this).SeriesId;
        var weightBounds = chart.SeriesContext.GetWeightBounds(weightStackIndex);

        if (Fill is not null && Fill != Paint.Default)
        {
            Fill.ZIndex = actualZIndex + PaintConstants.SeriesFillZIndexOffset;
            cartesianChart.Canvas.AddDrawableTask(Fill, zone: CanvasZone.DrawMargin);
        }
        if (Stroke is not null && Stroke != Paint.Default)
        {
            Stroke.ZIndex = actualZIndex + PaintConstants.SeriesStrokeZIndexOffset;
            cartesianChart.Canvas.AddDrawableTask(Stroke, zone: CanvasZone.DrawMargin);
        }
        if (ShowError && ErrorPaint is not null && ErrorPaint != Paint.Default)
        {
            ErrorPaint.ZIndex = actualZIndex + PaintConstants.SeriesGeometryFillZIndexOffset;
            cartesianChart.Canvas.AddDrawableTask(ErrorPaint, zone: CanvasZone.DrawMargin);
        }
        if (ShowDataLabels && DataLabelsPaint is not null && DataLabelsPaint != Paint.Default)
        {
            DataLabelsPaint.ZIndex = actualZIndex + PaintConstants.SeriesGeometryStrokeZIndexOffset;
            cartesianChart.Canvas.AddDrawableTask(DataLabelsPaint, zone: CanvasZone.DrawMargin);
        }

        var dls = (float)DataLabelsSize;
        var pointsCleanup = ChartPointCleanupContext.For(everFetched);

        var gs = (float)GeometrySize;
        var hgs = gs / 2f;
        var sw = Stroke?.StrokeThickness ?? 0;
        IsWeighted = weightBounds.Max - weightBounds.Min > 0;
        var wm = -(GeometrySize - MinGeometrySize) / (weightBounds.Max - weightBounds.Min);

        var hy = chart.ControlSize.Height * .5f;
        var hasSvg = this.HasVariableSvgGeometry();

        var isFirstDraw = !chart.IsDrawn(((ISeries)this).SeriesId);

        foreach (var point in Fetch(cartesianChart))
        {
            var coordinate = point.Coordinate;

            var visual = (TVisual?)point.Context.Visual;
            var e = point.Context.AdditionalVisuals as ErrorVisual<TErrorGeometry>;

            var x = xScale.ToPixels(coordinate.SecondaryValue);
            var y = yScale.ToPixels(coordinate.PrimaryValue);

            if (point.IsEmpty || !IsVisible)
            {
                if (visual is not null)
                {
                    visual.X = x - hgs;
                    visual.Y = y - hgs;
                    visual.Width = 0;
                    visual.Height = 0;
                    visual.RemoveOnCompleted = true;
                    point.Context.Visual = null;
                }

                if (point.Context.Label is not null)
                {
                    var label = (TLabel)point.Context.Label;

                    label.X = x - hgs;
                    label.Y = x - hgs;
                    label.Opacity = 0;
                    label.RemoveOnCompleted = true;

                    point.Context.Label = null;
                }

                pointsCleanup.Clean(point);

                continue;
            }

            if (IsWeighted)
            {
                gs = (float)(wm * (weightBounds.Max - coordinate.TertiaryValue) + GeometrySize);
                hgs = gs / 2f;
            }

            if (visual is null)
            {
                var r = new TVisual
                {
                    X = x,
                    Y = y,
                    Width = 0,
                    Height = 0
                };

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

            if (Fill is not null && Fill != Paint.Default)
                Fill.AddGeometryToPaintTask(cartesianChart.Canvas, visual);
            if (Stroke is not null && Stroke != Paint.Default)
                Stroke.AddGeometryToPaintTask(cartesianChart.Canvas, visual);
            if (ErrorPaint is not null && ErrorPaint != Paint.Default)
            {
                ErrorPaint.AddGeometryToPaintTask(cartesianChart.Canvas, e!.YError);
                ErrorPaint.AddGeometryToPaintTask(cartesianChart.Canvas, e!.XError);
            }

            var sizedGeometry = visual;

            sizedGeometry.X = x - hgs;
            sizedGeometry.Y = y - hgs;
            sizedGeometry.Width = gs;
            sizedGeometry.Height = gs;

            if (!coordinate.PointError.IsEmpty && ShowError && ErrorPaint is not null && ErrorPaint != Paint.Default)
            {
                var pe = coordinate.PointError;

                e!.YError!.X = x;
                e.YError.X1 = x;
                e.YError.Y = y + yScale.MeasureInPixels(pe.Yi);
                e.YError.Y1 = y - yScale.MeasureInPixels(pe.Yj);
                e.YError.RemoveOnCompleted = false;

                e.XError!.X = x - xScale.MeasureInPixels(pe.Xi);
                e.XError.X1 = x + xScale.MeasureInPixels(pe.Xj);
                e.XError.Y = y;
                e.XError.Y1 = y;
                e.XError.RemoveOnCompleted = false;
            }

            sizedGeometry.RemoveOnCompleted = false;

            if (point.Context.HoverArea is not RectangleHoverArea ha)
                point.Context.HoverArea = ha = new RectangleHoverArea();
            _ = ha.SetDimensions(x - hgs, y - hgs, gs, gs).CenterXToolTip().CenterYToolTip();

            pointsCleanup.Clean(point);

            if (ShowDataLabels && DataLabelsPaint is not null && DataLabelsPaint != Paint.Default)
            {
                if (point.Context.Label is not TLabel label)
                {
                    var l = new TLabel { X = x - hgs, Y = y - hgs, RotateTransform = (float)DataLabelsRotation, MaxWidth = (float)DataLabelsMaxWidth };
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

                var m = label.Measure();
                var labelPosition = GetLabelPosition(
                    x - hgs, y - hgs, gs, gs, m, DataLabelsPosition,
                    SeriesProperties, coordinate.PrimaryValue > 0, drawLocation, drawMarginSize);
                if (DataLabelsTranslate is not null) label.TranslateTransform =
                        new LvcPoint(m.Width * DataLabelsTranslate.Value.X, m.Height * DataLabelsTranslate.Value.Y);
                label.X = labelPosition.X;
                label.Y = labelPosition.Y;
            }

            OnPointMeasured(point);
        }

        pointsCleanup.CollectPoints(everFetched, cartesianChart.View, yScale, xScale, SoftDeleteOrDisposePoint);
        _geometrySvgChanged = false;
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
