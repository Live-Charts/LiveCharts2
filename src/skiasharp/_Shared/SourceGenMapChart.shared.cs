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
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Geo;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.Kernel.Observers;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.Themes;
using LiveChartsCore.VisualElements;

namespace LiveChartsGeneratedCode;

// ==============================================================
// this file contains the shared code between all UI frameworks
// ==============================================================

/// <inheritdoc cref="IGeoMapView" />
#if SKIA_IMAGE_LVC
public partial class SourceGenSKMapChart : IGeoMapView
#else
public partial class SourceGenMapChart : IGeoMapView
#endif
{
    private CollectionDeepObserver _seriesObserver = null!;

    /// <summary>
    /// Gets the core chart.
    /// </summary>
    public GeoMapChart CoreChart { get; private set; } = null!;

    // IChartView.CoreChart resolved via covariance: GeoMapChart : Chart, so the
    // shadowed IGeoMapView.CoreChart (typed GeoMapChart) implicitly satisfies
    // IChartView.CoreChart (typed Chart). An explicit impl is provided in case
    // a consumer reflects via IChartView.
    Chart IChartView.CoreChart => CoreChart;

    /// <inheritdoc cref="IChartView.AutoUpdateEnabled" />
    public bool AutoUpdateEnabled { get; set; } = true;

    private void InitializeChartControl()
    {
        CoreChart = new GeoMapChart(this);
        _seriesObserver = new CollectionDeepObserver(() => CoreChart?.Update());

        // Forward core lifecycle events to the IChartView surface so
        // consumers subscribing through the view get notified — matches
        // SourceGenChart.InitializeChartControl wiring.
        CoreChart.Measuring += OnCoreMeasuring;
        CoreChart.UpdateStarted += OnCoreUpdateStarted;
        CoreChart.UpdateFinished += OnCoreUpdateFinished;

        ActiveMap = Maps.GetWorldMap();
        SyncContext = new object();
        Tooltip = new SKDefaultGeoTooltip();
    }

    private void OnCoreMeasuring(IChartView chart) =>
        Measuring?.Invoke(this);

    private void OnCoreUpdateStarted(IChartView chart) =>
        UpdateStarted?.Invoke(this);

    private void OnCoreUpdateFinished(IChartView chart) =>
        UpdateFinished?.Invoke(this);

    // =====================================================================
    // PHASE 1 SHIM: IChartView members that do not (yet) have a meaningful
    // implementation for the map. The map deliberately has no legend, title,
    // chart-series, or visual-element layer. These stubs exist only so the
    // type checks; future phases can either implement them or split IChartView
    // into a smaller core interface that the map can implement cleanly.
    // =====================================================================

    /// <inheritdoc cref="IGeoMapView.TooltipFormatter" />
    public Func<GeoTooltipValue, string>? TooltipFormatter { get; set; }

    /// <inheritdoc cref="IChartView.ChartTheme" />
    public Theme? ChartTheme { get; set; }

    // CoreCanvas / ControlSize / DesignerMode / IsDarkMode / InvokeOnUIThread
    // are inherited from SourceGenDrawnView. BackColor stays here because
    // WinForms' native BackColor name collides — explicit interface impl on
    // the map is the simplest way out. Each platform reads its native
    // Background brush first and falls back to the theme-driven virtual
    // background; GPURenderMode.GetBackground() relies on this — without it
    // the GL surface clears to SKColor.Empty (transparent black).
#if MAUI_LVC
    LvcColor IChartView.BackColor
    {
        get
        {
            var c = (Background as Microsoft.Maui.Controls.SolidColorBrush)?.Color ?? BackgroundColor;
            return c is not null
                ? LvcColor.FromArgb((byte)(c.Alpha * 255), (byte)(c.Red * 255), (byte)(c.Green * 255), (byte)(c.Blue * 255))
                : CoreCanvas._virtualBackgroundColor;
        }
    }
#elif WPF_LVC
    LvcColor IChartView.BackColor =>
        Background is not System.Windows.Media.SolidColorBrush b
            ? CoreCanvas._virtualBackgroundColor
            : LvcColor.FromArgb(b.Color.A, b.Color.R, b.Color.G, b.Color.B);
#elif AVALONIA_LVC
    LvcColor IChartView.BackColor =>
        Background is not Avalonia.Media.ISolidColorBrush b
            ? CoreCanvas._virtualBackgroundColor
            : LvcColor.FromArgb(b.Color.A, b.Color.R, b.Color.G, b.Color.B);
#elif WINUI_LVC
    LvcColor IChartView.BackColor =>
        Background is not Microsoft.UI.Xaml.Media.SolidColorBrush b
            ? CoreCanvas._virtualBackgroundColor
            : LvcColor.FromArgb(b.Color.A, b.Color.R, b.Color.G, b.Color.B);
#else
    // SKIA_IMAGE_LVC / WinForms / Eto / Blazor: no native Background brush
    // exposed via the partial here; theme-driven virtual background only.
    // (WinForms' own BackColor would have to be read after the System.Drawing.Color
    // → LvcColor conversion in its own partial; out of scope for this change.)
    LvcColor IChartView.BackColor => CoreCanvas._virtualBackgroundColor;
#endif

    /// <inheritdoc cref="IChartView.DrawMargin" />
    public Margin? DrawMargin { get; set; }

    /// <inheritdoc cref="IChartView.AnimationsSpeed" />
    public TimeSpan AnimationsSpeed { get; set; } = LiveCharts.DefaultSettings.AnimationsSpeed;

    /// <inheritdoc cref="IChartView.EasingFunction" />
    public Func<float, float>? EasingFunction { get; set; } = LiveCharts.DefaultSettings.EasingFunction;

    /// <inheritdoc cref="IChartView.UpdaterThrottler" />
    public TimeSpan UpdaterThrottler { get; set; } = LiveCharts.DefaultSettings.UpdateThrottlingTimeout;

    /// <inheritdoc cref="IChartView.LegendPosition" />
    public LegendPosition LegendPosition { get; set; } = LegendPosition.Hidden;

    /// <inheritdoc cref="IChartView.LegendTextPaint" />
    public Paint? LegendTextPaint { get; set; }

    /// <inheritdoc cref="IChartView.LegendBackgroundPaint" />
    public Paint? LegendBackgroundPaint { get; set; }

    /// <inheritdoc cref="IChartView.LegendTextSize" />
    // -1 matches the cartesian default from LiveCharts.DefaultSettings so
    // SKHeatLegend/SKDefaultLegend's "if (textSize < 0) textSize = theme.LegendTextSize"
    // fallback kicks in; a 0 default would render the labels at 0pt (invisible).
    public double LegendTextSize { get; set; } = LiveCharts.DefaultSettings.LegendTextSize;

    /// <inheritdoc cref="IChartView.Title" />
    public IChartElement? Title { get; set; }

    /// <inheritdoc cref="IChartView.Legend" />
    public IChartLegend? Legend { get; set; }

    /// <inheritdoc cref="IChartView.VisualElements" />
    public IEnumerable<IChartElement> VisualElements { get; set; } = [];

    // Series and Tooltip on IGeoMapView shadow the IChartView properties (different
    // element type). Explicit impls satisfy the base interface as no-ops.
    IEnumerable<ISeries> IChartView.Series { get => []; set { } }
    IChartTooltip? IChartView.Tooltip { get => null; set { } }

    /// <inheritdoc cref="IChartView.Measuring" />
    public event ChartEventHandler? Measuring;
    /// <inheritdoc cref="IChartView.UpdateStarted" />
    public event ChartEventHandler? UpdateStarted;
    /// <inheritdoc cref="IChartView.UpdateFinished" />
    public event ChartEventHandler? UpdateFinished;
    /// <inheritdoc cref="IChartView.DataPointerDown" />
    public event ChartPointsHandler? DataPointerDown;
    /// <inheritdoc cref="IChartView.HoveredPointsChanged" />
    public event ChartPointHoverHandler? HoveredPointsChanged;
    /// <inheritdoc cref="IChartView.ChartPointPointerDown" />
    [Obsolete("Use DataPointerDown.")]
    public event ChartPointHandler? ChartPointPointerDown;
    /// <inheritdoc cref="IChartView.VisualElementsPointerDown"/>
    public event VisualElementsHandler? VisualElementsPointerDown;

    /// <inheritdoc cref="IChartView.GetPointsAt(LvcPointD, FindingStrategy, FindPointFor)"/>
    public IEnumerable<ChartPoint> GetPointsAt(
        LvcPointD point,
        FindingStrategy strategy = FindingStrategy.Automatic,
        FindPointFor findPointFor = FindPointFor.HoverEvent)
    {
        var hit = CoreChart?.FindLandAt(new LvcPoint((float)point.X, (float)point.Y));
        return hit is null ? [] : CoreChart!.BuildHitPoints(hit.Value);
    }

    /// <inheritdoc cref="IChartView.GetVisualsAt"/>
    public IEnumerable<IChartElement> GetVisualsAt(LvcPointD point) => [];

    /// <inheritdoc cref="IChartView.OnDataPointerDown"/>
    public void OnDataPointerDown(IEnumerable<ChartPoint> points, LvcPoint pointer)
    {
        // The base Chart.InvokePointerDown fires this on every press with the
        // series hit-test result; for maps that's always empty (no series), so
        // the only meaningful fire is the one from GeoMapChart.InvokePointerUp
        // (a release that wasn't a drag). Drop the empty pre-fire to give the
        // map a clean "click hit a land" signal.
        if (points is null || !points.Any()) return;
        DataPointerDown?.Invoke(this, points);
    }

    /// <inheritdoc cref="IChartView.OnHoveredPointsChanged"/>
    public void OnHoveredPointsChanged(IEnumerable<ChartPoint>? newItems, IEnumerable<ChartPoint>? oldItems)
    {
        // Same reasoning as OnDataPointerDown: base.InvokePointerLeft fires
        // (null, _activePoints=[]) on every leave; for maps that's a noisy
        // no-op. Only forward when at least one side carries land points.
        var newAny = newItems is not null && newItems.Any();
        var oldAny = oldItems is not null && oldItems.Any();
        if (!newAny && !oldAny) return;
        HoveredPointsChanged?.Invoke(this, newItems, oldItems);
    }

    /// <inheritdoc cref="IChartView.OnVisualElementPointerDown"/>
    public void OnVisualElementPointerDown(IEnumerable<IInteractable> visualElements, LvcPoint pointer)
    {
        VisualElementsPointerDown?.Invoke(this, new VisualElementsEventArgs(CoreChart, visualElements, pointer));
    }

    // Invalidate is platform-specific (calls native InvalidateVisual / equivalent)
    // and lives on the per-platform partial. We define a fallback that delegates
    // to the canvas for platforms whose per-platform file hasn't been updated yet.
    /// <inheritdoc cref="IChartView.Invalidate"/>
    public virtual void Invalidate() => CoreCanvas.Invalidate();
}
