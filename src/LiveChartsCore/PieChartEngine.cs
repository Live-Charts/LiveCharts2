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
/// Defines a pie chart.
/// </summary>
/// <seealso cref="Chart" />
/// <remarks>
/// Initializes a new instance of the <see cref="PieChartEngine"/> class.
/// </remarks>
/// <param name="view">The view.</param>
/// <param name="canvas">The canvas.</param>
public class PieChartEngine(
    IPieChartView view,
    CoreMotionCanvas canvas)
        : Chart(canvas, view, ChartKind.Pie)
{
    ///<inheritdoc cref="Chart.Series"/>
    public override IEnumerable<ISeries> Series =>
        view.Series?.Select(x => x.ChartElementSource).Cast<ISeries>() ?? [];

    ///<inheritdoc cref="Chart.VisibleSeries"/>
    public override IEnumerable<ISeries> VisibleSeries =>
        Series.Where(x => x.IsVisible);

    /// <summary>
    /// Gets the view.
    /// </summary>
    /// <value>
    /// The view.
    /// </value>
    public override IChartView View => view;

    /// <summary>
    /// Gets the value bounds.
    /// </summary>
    /// <value>
    /// The value bounds.
    /// </value>
    public Bounds ValueBounds { get; private set; } = new();

    /// <summary>
    /// Gets the index bounds.
    /// </summary>
    /// <value>
    /// The index bounds.
    /// </value>
    public Bounds IndexBounds { get; private set; } = new();

    /// <summary>
    /// Gets the pushout bounds.
    /// </summary>
    /// <value>
    /// The pushout bounds.
    /// </value>
    public Bounds PushoutBounds { get; private set; } = new();

    /// <summary>
    /// Finds the points near to the specified point.
    /// </summary>
    /// <param name="pointerPosition">The pointer position.</param>
    /// <returns></returns>
    public override IEnumerable<ChartPoint> FindHoveredPointsBy(LvcPoint pointerPosition)
    {
        return VisibleSeries
            .Where(series => (series is IPieSeries pieSeries) && !pieSeries.IsFillSeries)
            .Where(series => series.IsHoverable)
            .SelectMany(series => series.FindHitPoints(this, pointerPosition, FindingStrategy.CompareAll, FindPointFor.HoverEvent));
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
                $"[Pie chart measured]".PadRight(60) +
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

        var theme = GetTheme();

        var viewDrawMargin = view.DrawMargin;
        ControlSize = view.ControlSize;

        VisualElements = view.VisualElements ?? [];

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

        SeriesContext = new SeriesContext(VisibleSeries, this);
        var themeId = theme.ThemeId;

        ValueBounds = new Bounds();
        IndexBounds = new Bounds();
        PushoutBounds = new Bounds();

        // iterate Series (not VisibleSeries) so invisible series still get their theme
        // applied; the legend can still request their miniature when IsVisibleAtLegend is
        // true (its default), and a missing paint would otherwise crash on draw.
        foreach (var series in Series.Cast<IPieSeries>())
        {
            if (series.SeriesId == -1) series.SeriesId = GetNextSeriesId();

            var ce = series.ChartElementSource;
            ce._isInternalSet = true;
            if (ce._theme != themeId)
            {
                theme.ApplyStyleToSeries(series);
                ce._theme = themeId;
            }

            if (!series.IsVisible)
            {
                ce._isInternalSet = false;
                continue;
            }

            var seriesBounds = series.GetBounds(this);

            ValueBounds.AppendValue(seriesBounds.PrimaryBounds.Max);
            ValueBounds.AppendValue(seriesBounds.PrimaryBounds.Min);
            IndexBounds.AppendValue(seriesBounds.SecondaryBounds.Max);
            IndexBounds.AppendValue(seriesBounds.SecondaryBounds.Min);
            PushoutBounds.AppendValue(seriesBounds.TertiaryBounds.Max);
            PushoutBounds.AppendValue(seriesBounds.TertiaryBounds.Min);

            ce._isInternalSet = false;
        }

        InitializeVisualsCollector();

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

        var rm = viewDrawMargin ?? new Margin(Margin.Auto);
        var actualMargin = new Margin(
            Margin.IsAuto(rm.Left) ? m.Left : rm.Left,
            Margin.IsAuto(rm.Top) ? m.Top : rm.Top,
            Margin.IsAuto(rm.Right) ? m.Right : rm.Right,
            Margin.IsAuto(rm.Bottom) ? m.Bottom : rm.Bottom);

        SetDrawMargin(ControlSize, actualMargin);

        // invalid dimensions, probably the chart is too small
        // or it is initializing in the UI and has no dimensions yet
        if (DrawMarginSize.Width <= 0 || DrawMarginSize.Height <= 0) return;

        if (View.Title is not null) AddTitleToChart();

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

        if (_isToolTipOpen) _ = DrawToolTip();
        InvokeOnUpdateStarted();
        _isFirstDraw = false;

        if (IsLoaded)
            Canvas.Invalidate();

        _isFirstDraw = false;
    }

    /// <inheritdoc cref="Chart.Unload"/>
    public override void Unload()
    {
        base.Unload();
        _isFirstDraw = true;
    }
}
