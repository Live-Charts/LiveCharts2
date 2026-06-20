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
/// Sankey chart engine. No axes — each series lays out its own graph in the
/// full draw-margin rectangle. Multi-series support is intentionally
/// minimal in v1: each series gets the full draw region (typical sankey
/// charts have a single series).
/// </summary>
public class SankeyChartEngine(
    ISankeyChartView view,
    CoreMotionCanvas canvas)
        : Chart(canvas, view, ChartKind.Sankey)
{
    /// <inheritdoc cref="Chart.Series"/>
    public override IEnumerable<ISeries> Series =>
        view.Series?.Select(x => x.ChartElementSource).Cast<ISeries>() ?? [];

    /// <inheritdoc cref="Chart.VisibleSeries"/>
    public override IEnumerable<ISeries> VisibleSeries =>
        Series.Where(x => x.IsVisible);

    /// <inheritdoc cref="Chart.View"/>
    public override IChartView View => view;

    /// <inheritdoc cref="Chart.FindHoveredPointsBy(LvcPoint)"/>
    public override IEnumerable<ChartPoint> FindHoveredPointsBy(LvcPoint pointerPosition) =>
        VisibleSeries
            .Where(s => s.IsHoverable)
            .SelectMany(s => s.FindHitPoints(this, pointerPosition, FindingStrategy.CompareAll, FindPointFor.HoverEvent));

    /// <inheritdoc cref="Chart.Measure"/>
    protected internal override void Measure()
    {
#if DEBUG
        if (LiveCharts.EnableLogging)
        {
            Trace.WriteLine(
                $"[Sankey chart measured]".PadRight(60) +
                $"geometries: {Canvas.CountGeometries()}    " +
                $"thread: {Environment.CurrentManagedThreadId}");
        }
#endif

        if (!IsLoaded || !IsRendering()) return;

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

        foreach (var series in Series)
        {
            if (series.SeriesId == -1) series.SeriesId = GetNextSeriesId();

            var ce = series.ChartElementSource;
            ce._isInternalSet = true;
            if (ce._theme != themeId)
            {
                theme.ApplyStyleToSeries(series);
                ce._theme = themeId;
            }
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

        // invalid dimensions (too small, or initializing with no size yet). Don't just return — the
        // canvas would keep painting the previous frame's geometry at its old transform. Hide the
        // plot instead; a resize back to a valid size resets the clips below and re-measures.
        if (DrawMarginSize.Width <= 0 || DrawMarginSize.Height <= 0)
        {
            HidePlotZones();
            Canvas.Invalidate();
            return;
        }

        // sankey doesn't clip its plot zone, so undo a previous collapse-hide before drawing.
        ResetPlotZoneClips();

        if (View.Title is not null) AddTitleToChart();

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

        Canvas.Invalidate();
    }

    /// <inheritdoc cref="Chart.Unload"/>
    public override void Unload()
    {
        base.Unload();
        _isFirstDraw = true;
    }
}
