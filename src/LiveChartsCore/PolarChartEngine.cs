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
using System.Diagnostics;
using System.Linq;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.Motion;

namespace LiveChartsCore;

/// <summary>
/// Defines a Polar chart.
/// </summary>
/// <seealso cref="Chart" />
/// <remarks>
/// Initializes a new instance of the <see cref="PolarChartEngine"/> class.
/// </remarks>
/// <param name="view">The view.</param>
/// <param name="canvas">The canvas.</param>
public class PolarChartEngine(
    IPolarChartView view,
    CoreMotionCanvas canvas)
        : Chart(canvas, view, ChartKind.Polar)
{
    /// <summary>
    /// Gets the angle axes.
    /// </summary>
    /// <value>
    /// The x axes.
    /// </value>
    public IPolarAxis[] AngleAxes { get; private set; } = [];

    /// <summary>
    /// Gets the radius axes.
    /// </summary>
    /// <value>
    /// The y axes.
    /// </value>
    public IPolarAxis[] RadiusAxes { get; private set; } = [];

    ///<inheritdoc cref="Chart.Series"/>
    public override IEnumerable<ISeries> Series =>
        view.Series?.Select(x => x.ChartElementSource).Cast<ISeries>() ?? [];

    ///<inheritdoc cref="Chart.VisibleSeries"/>
    public override IEnumerable<ISeries> VisibleSeries =>
        Series.Where(x => x.IsVisible);

    /// <summary>
    /// Gets whether the series fit to bounds or not.
    /// </summary>
    public bool FitToBounds { get; private set; }

    /// <summary>
    /// Gets the total circumference angle.
    /// </summary>
    /// <value>
    /// The total angle.
    /// </value>
    public float TotalAnge { get; private set; }

    /// <summary>
    /// Gets the Inner radius.
    /// </summary>
    /// <value>
    /// The inner radius.
    /// </value>
    public float InnerRadius { get; private set; }

    /// <summary>
    /// Gets the Initial rotation.
    /// </summary>
    /// /// <value>
    /// The inner radius.
    /// </value>
    public float InitialRotation { get; private set; }

    /// <summary>
    /// Gets the view.
    /// </summary>
    /// <value>
    /// The view.
    /// </value>
    public override IChartView View => view;

    /// <summary>
    /// Finds the points near to the specified point.
    /// </summary>
    /// <param name="pointerPosition">The pointer position.</param>
    /// <returns></returns>
    public override IEnumerable<ChartPoint> FindHoveredPointsBy(LvcPoint pointerPosition)
    {
        return VisibleSeries
            .Where(series => series.IsHoverable)
            .SelectMany(series => series.FindHitPoints(this, pointerPosition, FindingStrategy.CompareAll, FindPointFor.HoverEvent));
    }

    /// <inheritdoc cref="IPolarChartView.ScalePixelsToData(LvcPointD, int, int)"/>
    public LvcPointD ScalePixelsToData(LvcPointD point, int angleAxisIndex = 0, int radiusAxisIndex = 0)
    {
        var scaler = new PolarScaler(
            DrawMarginLocation, DrawMarginSize, AngleAxes[angleAxisIndex], RadiusAxes[radiusAxisIndex],
            InnerRadius, InitialRotation, TotalAnge);

        return scaler.ToChartValues(point.X, point.Y);
    }

    /// <inheritdoc cref="IPolarChartView.ScaleDataToPixels(LvcPointD, int, int)"/>
    public LvcPointD ScaleDataToPixels(LvcPointD point, int angleAxisIndex = 0, int radiusAxisIndex = 0)
    {
        var scaler = new PolarScaler(
            DrawMarginLocation, DrawMarginSize, AngleAxes[angleAxisIndex], RadiusAxes[radiusAxisIndex],
            InnerRadius, InitialRotation, TotalAnge);

        var r = scaler.ToPixels(point.X, point.Y);

        return new LvcPointD { X = (float)r.X, Y = (float)r.Y };
    }

    /// <summary>
    /// Measures this chart.
    /// </summary>
    /// <returns></returns>
    protected internal override void Measure()
    {
#if DEBUG
        if (LiveCharts.EnableLogging)
        {
            Trace.WriteLine(
                $"[Polar chart measured]".PadRight(60) +
                $"geometries: {Canvas.CountGeometries()}    " +
                $"thread: {Environment.CurrentManagedThreadId}");
        }
#endif
        if (!IsLoaded || !IsRendering())
            return;

        InvokeOnMeasuring();

        if (_preserveFirstDraw)
        {
            _isFirstDraw = true;
            _preserveFirstDraw = false;
        }

        #region copy the current data in the view

        var viewDrawMargin = view.DrawMargin;
        ControlSize = view.ControlSize;

        var a = view.AngleAxes;
        var r = view.RadiusAxes;

        if (a is null || !a.Any())
        {
            var provider = LiveCharts.DefaultSettings.GetProvider();
            a = [provider.GetDefaultPolarAxis()];
        }

        if (r is null || !r.Any())
        {
            var provider = LiveCharts.DefaultSettings.GetProvider();
            r = [provider.GetDefaultPolarAxis()];
        }

        AngleAxes = [.. a.Select(x => x.ChartElementSource).Cast<IPolarAxis>()];
        RadiusAxes = [.. r.Select(x => x.ChartElementSource).Cast<IPolarAxis>()];

        var theme = GetTheme();

        LegendPosition = view.LegendPosition;
        Legend = view.Legend;

        TooltipPosition = view.TooltipPosition;
        Tooltip = view.Tooltip;

        ActualAnimationsSpeed = view.AnimationsSpeed == TimeSpan.MaxValue
            ? theme.AnimationsSpeed
            : view.AnimationsSpeed;
        ActualEasingFunction = view.EasingFunction == EasingFunctions.Unset
            ? theme.EasingFunction
            : view.EasingFunction;

        FitToBounds = view.FitToBounds;
        TotalAnge = (float)view.TotalAngle;
        InnerRadius = (float)view.InnerRadius;
        InitialRotation = (float)view.InitialRotation;

        VisualElements = view.VisualElements ?? [];

        #endregion

        SeriesContext = new SeriesContext(VisibleSeries, this);
        var themeId = theme.ThemeId;

        // restart axes bounds and meta data
        foreach (var axis in AngleAxes)
        {
            var ce = axis.ChartElementSource;
            ce._isInternalSet = true;
            axis.Initialize(PolarAxisOrientation.Angle);
            if (ce._theme != themeId)
            {
                theme.ApplyStyleToAxis(axis);
                ce._theme = themeId;
            }
            ce._isInternalSet = false;
        }
        foreach (var axis in RadiusAxes)
        {
            var ce = axis.ChartElementSource;
            ce._isInternalSet = true;
            axis.Initialize(PolarAxisOrientation.Radius);
            if (ce._theme != themeId)
            {
                theme.ApplyStyleToAxis(axis);
                ce._theme = themeId;
            }
            ce._isInternalSet = false;
        }

        // get seriesBounds
        SetDrawMargin(ControlSize, new Margin());
        foreach (var series in VisibleSeries.Cast<IPolarSeries>())
        {
            if (series.SeriesId == -1) series.SeriesId = GetNextSeriesId();

            // #1923: see CartesianChartEngine for rationale — pre-register stack
            // positions so Stacker.MaxSeriesId is final before any series renders.
            if ((series.SeriesProperties & SeriesProperties.Stacked) == SeriesProperties.Stacked)
                _ = SeriesContext.GetStackPosition(series, series.GetStackGroup());

            var ce = series.ChartElementSource;
            ce._isInternalSet = true;
            if (ce._theme != themeId)
            {
                theme.ApplyStyleToSeries(series);
                ce._theme = themeId;
            }

            var secondaryAxis = GetAngleAxis(series);
            var primaryAxis = GetRadiusAxis(series);

            var seriesBounds = series.GetBounds(this, secondaryAxis, primaryAxis).Bounds;

            if (seriesBounds.IsEmpty)
            {
                ce._isInternalSet = false;
                continue;
            }

            secondaryAxis.DataBounds.AppendValue(seriesBounds.SecondaryBounds);
            primaryAxis.DataBounds.AppendValue(seriesBounds.PrimaryBounds);
            secondaryAxis.VisibleDataBounds.AppendValue(seriesBounds.SecondaryBounds);
            primaryAxis.VisibleDataBounds.AppendValue(seriesBounds.PrimaryBounds);

            ce._isInternalSet = false;
        }

        #region empty bounds

        // prevent the bounds are not empty...

        foreach (var axis in AngleAxes)
        {
            var ce = axis.ChartElementSource;
            ce._isInternalSet = true;

            if (!axis.DataBounds.IsEmpty)
            {
                ce._isInternalSet = false;
                continue;
            }

            var min = 0;
            var max = 10d * axis.UnitWidth;

            axis.DataBounds.AppendValue(max);
            axis.DataBounds.AppendValue(min);
            axis.VisibleDataBounds.AppendValue(max);
            axis.VisibleDataBounds.AppendValue(min);

            if (axis.DataBounds.MinDelta < max) axis.DataBounds.MinDelta = max;

            ce._isInternalSet = false;
        }
        foreach (var axis in RadiusAxes)
        {
            var ce = axis.ChartElementSource;
            ce._isInternalSet = true;

            if (!axis.DataBounds.IsEmpty)
            {
                ce._isInternalSet = false;
                continue;
            }

            var min = 0;
            var max = 10d * axis.UnitWidth;

            axis.DataBounds.AppendValue(max);
            axis.DataBounds.AppendValue(min);
            axis.VisibleDataBounds.AppendValue(max);
            axis.VisibleDataBounds.AppendValue(min);

            if (axis.DataBounds.MinDelta < max) axis.DataBounds.MinDelta = max;

            ce._isInternalSet = false;
        }

        #endregion

        InitializeVisualsCollector();

        if (FitToBounds)
        {
            float mt = 0, mb = 0, ml = 0, mr = 0;

            foreach (var series in VisibleSeries.Cast<IPolarSeries>())
            {
                var scaler = new PolarScaler(
                    DrawMarginLocation, DrawMarginSize, AngleAxes[series.ScalesAngleAt], RadiusAxes[series.ScalesRadiusAt],
                    InnerRadius, InitialRotation, TotalAnge);

                foreach (var point in series.Fetch(this))
                {
                    var p = scaler.ToPixels(point);

                    var dx = p.X - scaler.CenterX;
                    var dy = p.Y - scaler.CenterY;

                    if (dx > 0)
                    {
                        if (dx > mr)
                            mr = dx;
                    }
                    else
                    {
                        dx *= -1;
                        if (dx > ml)
                            ml = dx;
                    }

                    if (dy > 0)
                    {
                        if (dy > mb)
                            mb = dy;
                    }
                    else
                    {
                        dy *= -1;
                        if (dy > mt)
                            mt = dy;
                    }
                }
            }

            var cs = ControlSize;
            var cx = cs.Width * 0.5f;
            var cy = cs.Height * 0.5f;

            var dl = cx - ml;
            var dr = cx - mr;
            var dt = cy - mt;
            var db = cy - mb;

            // so the idea is...

            // we know the distance of the most left point to the left border (dl)
            // the most right point to the right border (dr)
            // the most bottom point to the bottom border (db)
            // the most top point to the top border (dt)

            // then to "easily" fit the plot to the data bounds, we create a negative margin for our draw margin
            // then the scaler will luckily handle it.

            var fitMargin = new Margin(-dl, -dt, -dr, -db);
            SetDrawMargin(ControlSize, fitMargin);
        }
        else
        {
            // calculate draw margin
            var m = new Margin();
            float ts = 0f, bs = 0f, ls = 0f, rs = 0f;
            if (View.Title is not null)
            {
                var titleSize = MeasureTitle();
                m.Top = titleSize.Height;
                ts = titleSize.Height;
                _titleHeight = titleSize.Height;
            }

            DrawLegend(ref ts, ref bs, ref ls, ref rs);

            m.Top = ts;
            m.Bottom = bs;
            m.Left = ls;
            m.Right = rs;

            foreach (var axis in AngleAxes)
            {
                if (!axis.IsVisible) continue;

                if (axis.DataBounds.Max == axis.DataBounds.Min)
                {
                    var c = axis.UnitWidth * 0.5;
                    axis.DataBounds.Min = axis.DataBounds.Min - c;
                    axis.DataBounds.Max = axis.DataBounds.Max + c;
                    axis.VisibleDataBounds.Min = axis.VisibleDataBounds.Min - c;
                    axis.VisibleDataBounds.Max = axis.VisibleDataBounds.Max + c;
                }

                var ns = axis.GetNameLabelSize(this);
                var s = axis.GetPossibleSize(this);

                var radius = s.Height; // <- this type needs to be changed... it is not the height it is the radius.

                axis.Ro = m.Top + radius;

                m.Top += radius;
                m.Bottom += radius;
                m.Left += radius;
                m.Right += radius;
            }
            foreach (var axis in RadiusAxes)
            {
                if (!axis.IsVisible) continue;

                if (axis.DataBounds.Max == axis.DataBounds.Min)
                {
                    var c = axis.UnitWidth * 0.5;
                    axis.DataBounds.Min = axis.DataBounds.Min - c;
                    axis.DataBounds.Max = axis.DataBounds.Max + c;
                    axis.VisibleDataBounds.Min = axis.VisibleDataBounds.Min - c;
                    axis.VisibleDataBounds.Max = axis.VisibleDataBounds.Max + c;
                }

                // the angle axis does not require padding?? I think it does not
            }

            var rm = viewDrawMargin ?? new Margin(Margin.Auto);
            var actualMargin = new Margin(
                Margin.IsAuto(rm.Left) ? m.Left : rm.Left,
                Margin.IsAuto(rm.Top) ? m.Top : rm.Top,
                Margin.IsAuto(rm.Right) ? m.Right : rm.Right,
                Margin.IsAuto(rm.Bottom) ? m.Bottom : rm.Bottom);

            SetDrawMargin(ControlSize, m);
        }

        // invalid dimensions, probably the chart is too small
        // or it is initializing in the UI and has no dimensions yet
        if (DrawMarginSize.Width <= 0 || DrawMarginSize.Height <= 0) return;

        if (View.Title is not null) AddTitleToChart();

        var totalAxes = RadiusAxes.Concat(AngleAxes).ToArray();
        foreach (var axis in totalAxes)
        {
            if (axis.DataBounds.Max == axis.DataBounds.Min)
            {
                var c = axis.DataBounds.Min * 0.3;
                axis.DataBounds.Min = axis.DataBounds.Min - c;
                axis.DataBounds.Max = axis.DataBounds.Max + c;
            }

            // apply padding
            if (axis.MinLimit is null)
            {
                // correction by geometry size
                var p = 0d;
                if (axis.DataBounds.PaddingMin > p) p = axis.DataBounds.PaddingMin;
                var ce = axis.ChartElementSource;
                ce._isInternalSet = true;
                axis.DataBounds.Min = axis.DataBounds.Min - p;
                axis.VisibleDataBounds.Min = axis.VisibleDataBounds.Min - p;
                ce._isInternalSet = false;
            }

            // apply padding
            if (axis.MaxLimit is null)
            {
                // correction by geometry size
                var p = 0d; // Math.Abs(s.ToChartValues(axis.DataBounds.RequestedGeometrySize) - s.ToChartValues(0));
                if (axis.DataBounds.PaddingMax > p) p = axis.DataBounds.PaddingMax;
                var ce = axis.ChartElementSource;
                ce._isInternalSet = true;
                axis.DataBounds.Max = axis.DataBounds.Max + p;
                axis.VisibleDataBounds.Max = axis.VisibleDataBounds.Max + p;
                ce._isInternalSet = false;
            }

            if (axis.IsVisible) AddVisual(axis.ChartElementSource);
            axis.ChartElementSource.RemoveOldPaints(View); // <- this is probably obsolete.
            // the probable issue is the "IsVisible" property
        }

        // we draw all the series even invisible because it animates the series when hidden.
        // Sections and Visuals are not animated when hidden, thus we just skip them.
        // it means that invisible series have a performance impact, it should not be a big deal
        // but ideally, do not keep invisible series in the chart, instead, add/remove them when needed.

        foreach (var visual in VisualElements.Where(x => x.IsVisible)) AddVisual(visual);
        foreach (var series in Series)
        {
            AddVisual(series.ChartElementSource);
            _drawnSeries.Add(series.SeriesId);
        }

        CollectVisuals();

        foreach (var axis in totalAxes)
        {
            if (!axis.IsVisible) continue;

            var ce = axis.ChartElementSource;
            ce._isInternalSet = true;
            ce._isInternalSet = false;
        }

        InvokeOnUpdateStarted();

        if (_isToolTipOpen) DrawToolTip();
        _isFirstDraw = false;

        if (IsLoaded)
            Canvas.Invalidate();

        _isFirstDraw = false;
    }

    /// <summary>
    /// Gets the x axis for the specified series.
    /// </summary>
    /// <param name="series">The series.</param>
    public IPolarAxis GetAngleAxis(IPolarSeries series)
        // we ensure it is in the axes collection bounds, this is just to
        // prevent crashes on hot-reload scenarios.
        => AngleAxes[series.ScalesAngleAt > AngleAxes.Length - 1 ? 0 : series.ScalesAngleAt];

    /// <summary>
    /// Gets the y axis for the specified series.
    /// </summary>
    /// <param name="series">The series.</param>
    public IPolarAxis GetRadiusAxis(IPolarSeries series)
        // we ensure it is in the axes collection bounds, this is just to
        // prevent crashes on hot-reload scenarios.
        => RadiusAxes[series.ScalesRadiusAt > RadiusAxes.Length - 1 ? 0 : series.ScalesRadiusAt];

    /// <inheritdoc cref="Chart.Unload"/>
    public override void Unload()
    {
        base.Unload();
        _isFirstDraw = true;
    }
}
