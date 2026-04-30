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
using System.Threading.Tasks;
using LiveChartsCore.Drawing;
using LiveChartsCore.Geo;
using LiveChartsCore.Kernel;
using LiveChartsCore.Painting;

namespace LiveChartsCore;

/// <summary>
/// Defines a geo map chart.
/// </summary>
public class GeoMapChart
{
    private readonly HashSet<IGeoSeries> _everMeasuredSeries = [];
    private readonly ActionThrottler _updateThrottler;
    private readonly ActionThrottler _panningThrottler;
    private bool _isHeatInCanvas = false;
    private Paint _heatPaint;
    private Paint? _previousStroke;
    private Paint? _previousFill;
    private LvcPoint _pointerPanningPosition = new(-10, -10);
    private LvcPoint _pointerPreviousPanningPosition = new(-10, -10);
    private bool _isPanning = false;
    private IMapFactory _mapFactory;
    private DrawnMap? _activeMap;
    private bool _isUnloaded = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="GeoMapChart"/> class.
    /// </summary>
    /// <param name="mapView"></param>
    public GeoMapChart(IGeoMapView mapView)
    {
        View = mapView;
        _updateThrottler = mapView.DesignerMode
                ? new ActionThrottler(() => Task.CompletedTask, TimeSpan.FromMilliseconds(50))
                : new ActionThrottler(UpdateThrottlerUnlocked, TimeSpan.FromMilliseconds(100));
        _heatPaint = LiveCharts.DefaultSettings.GetProvider().GetSolidColorPaint();
        _mapFactory = LiveCharts.DefaultSettings.GetProvider().GetDefaultMapFactory();

        PointerDown += Chart_PointerDown;
        PointerMove += Chart_PointerMove;
        PointerUp += Chart_PointerUp;
        PointerLeft += Chart_PointerLeft;

        _panningThrottler = new ActionThrottler(PanningThrottlerUnlocked, TimeSpan.FromMilliseconds(30));
    }

    internal event Action<LvcPoint> PointerDown;
    internal event Action<LvcPoint> PointerMove;
    internal event Action<LvcPoint> PointerUp;
    internal event Action PointerLeft;

    /// <summary>
    /// Gets the chart view.
    /// </summary>
    public IGeoMapView View { get; private set; }

    /// <inheritdoc cref="IMapFactory.ViewTo(GeoMapChart, object)"/>
    public virtual void ViewTo(object? command) => _mapFactory.ViewTo(this, command);

    /// <inheritdoc cref="IMapFactory.Pan(GeoMapChart, LvcPoint)"/>
    public virtual void Pan(LvcPoint delta) => _mapFactory.Pan(this, delta);

    /// <summary>
    /// Queues a measure request to update the chart.
    /// </summary>
    /// <param name="chartUpdateParams"></param>
    public virtual void Update(ChartUpdateParams? chartUpdateParams = null)
    {
        chartUpdateParams ??= new ChartUpdateParams();

        if (chartUpdateParams.IsAutomaticUpdate && !View.AutoUpdateEnabled) return;

        if (!chartUpdateParams.Throttling)
        {
            _updateThrottler.ForceCall();
            return;
        }

        _updateThrottler.Call();
    }

    /// <summary>
    /// Loads (or reloads) the map resources after a previous <see cref="Unload"/>,
    /// then queues a measure. Safe to call when the chart has not been unloaded —
    /// it will simply queue an update.
    /// </summary>
    public void Load()
    {
        if (_isUnloaded)
        {
            _heatPaint = LiveCharts.DefaultSettings.GetProvider().GetSolidColorPaint();
            _mapFactory = LiveCharts.DefaultSettings.GetProvider().GetDefaultMapFactory();
            _isHeatInCanvas = false;
            _isUnloaded = false;
        }
        Update();
    }

    /// <summary>
    /// Unload the map resources. Calling this method on an already-unloaded chart
    /// is a no-op.
    /// </summary>
    public void Unload()
    {
        if (_isUnloaded) return;

        // Hide the tooltip and clear hover state so that a subsequent Load +
        // Measure does not re-show a tooltip from a stale _hoveredLand.
        if (_isToolTipOpen)
        {
            View.Tooltip?.Hide(this);
            _isToolTipOpen = false;
        }
        _hoveredLand = null;

        if (View.Stroke is not null) View.CoreCanvas.RemovePaintTask(View.Stroke);
        if (View.Fill is not null) View.CoreCanvas.RemovePaintTask(View.Fill);

        _everMeasuredSeries.Clear();
        _heatPaint = null!;
        _previousStroke = null!;
        _previousFill = null!;
        _isUnloaded = true;
        _mapFactory.Dispose();

        // Do NOT dispose _activeMap: DrawnMap.Dispose clears its Layers dictionary,
        // and the same instance is referenced by View.ActiveMap. Disposing it here
        // would make the chart unrenderable on a subsequent Load (issue #1417).
        // The View owns the map's lifetime.
        _activeMap = null!;
        _mapFactory = null!;

        View.CoreCanvas.Dispose();
    }

    /// <summary>
    /// Invokes the pointer down event.
    /// </summary>
    /// <param name="point">The pointer position.</param>
    protected internal void InvokePointerDown(LvcPoint point) => PointerDown?.Invoke(point);

    /// <summary>
    /// Invokes the pointer move event.
    /// </summary>
    /// <param name="point">The pointer position.</param>
    protected internal void InvokePointerMove(LvcPoint point) => PointerMove?.Invoke(point);

    /// <summary>
    /// Invokes the pointer up event.
    /// </summary>
    /// <param name="point">The pointer position.</param>
    protected internal void InvokePointerUp(LvcPoint point) => PointerUp?.Invoke(point);

    /// <summary>
    /// Invokes the pointer left event.
    /// </summary>
    protected internal void InvokePointerLeft() => PointerLeft?.Invoke();

    /// <summary>
    /// Called to measure the chart.
    /// </summary>
    /// <returns>The update task.</returns>
    protected virtual Task UpdateThrottlerUnlocked()
    {
        return Task.Run(() =>
        {
            View.InvokeOnUIThread(() =>
            {
                lock (View.CoreCanvas.Sync)
                {
                    if (_isUnloaded) return;
                    Measure();
                }
            });
        });
    }

    /// <summary>
    /// Measures the chart.
    /// </summary>
    protected internal void Measure()
    {
        if (_activeMap is not null && _activeMap != View.ActiveMap)
        {
            _previousStroke?.ClearGeometriesFromPaintTask(View.CoreCanvas);
            _previousFill?.ClearGeometriesFromPaintTask(View.CoreCanvas);

            _previousFill = null;
            _previousStroke = null;

            View.CoreCanvas.Clear();
        }
        _activeMap = View.ActiveMap;

        if (!_isHeatInCanvas)
        {
            View.CoreCanvas.AddDrawableTask(_heatPaint);
            _isHeatInCanvas = true;
        }

        if (_previousStroke != View.Stroke)
        {
            if (_previousStroke is not null)
                View.CoreCanvas.RemovePaintTask(_previousStroke);

            if (View.Stroke is not null)
            {
                if (View.Stroke.ZIndex == 0) View.Stroke.ZIndex = PaintConstants.GeoMapStrokeZIndex;
                View.Stroke.PaintStyle = PaintStyle.Stroke;
                View.CoreCanvas.AddDrawableTask(View.Stroke);
            }

            _previousStroke = View.Stroke;
        }

        if (_previousFill != View.Fill)
        {
            if (_previousFill is not null)
                View.CoreCanvas.RemovePaintTask(_previousFill);

            if (View.Fill is not null)
            {
                View.Fill.PaintStyle = PaintStyle.Fill;
                View.CoreCanvas.AddDrawableTask(View.Fill);
            }

            _previousFill = View.Fill;
        }

        var i = _previousFill?.ZIndex ?? 0;
        _heatPaint.ZIndex = i + 1;

        var context = new MapContext(
            this, View, View.ActiveMap,
            Maps.BuildProjector(View.MapProjection, [View.ControlSize.Width, View.ControlSize.Height]));

        _mapFactory.GenerateLands(context);

        // Departed series must be deleted BEFORE measuring the new series.
        // Otherwise CoreHeatLandSeries.Delete -> ClearHeat would null the Shape.Fill
        // on lands shared with the new series AFTER the new series painted them,
        // making shared lands appear blank on series swap (issue #962).
        var currentSeries = View.Series?.Cast<IGeoSeries>().ToArray() ?? [];
        var currentSet = new HashSet<IGeoSeries>(currentSeries);
        foreach (var series in _everMeasuredSeries)
        {
            if (currentSet.Contains(series)) continue;
            series.Delete(context);
        }
        _everMeasuredSeries.RemoveWhere(s => !currentSet.Contains(s));

        foreach (var series in currentSeries)
        {
            series.Measure(context);
            _ = _everMeasuredSeries.Add(series);
        }

        View.CoreCanvas.Invalidate();
    }

    private Task PanningThrottlerUnlocked()
    {
        return Task.Run(() =>
            View.InvokeOnUIThread(() =>
            {
                lock (View.CoreCanvas.Sync)
                {
                    Pan(
                        new LvcPoint(
                            (float)(_pointerPanningPosition.X - _pointerPreviousPanningPosition.X),
                            (float)(_pointerPanningPosition.Y - _pointerPreviousPanningPosition.Y)));
                    _pointerPreviousPanningPosition = new LvcPoint(_pointerPanningPosition.X, _pointerPanningPosition.Y);
                }
            }));
    }

    private void Chart_PointerDown(LvcPoint pointerPosition)
    {
        _isPanning = true;
        _pointerPreviousPanningPosition = pointerPosition;
    }

    private void Chart_PointerMove(LvcPoint pointerPosition)
    {
        if (!_isPanning) return;
        _pointerPanningPosition = pointerPosition;
        _panningThrottler.Call();
    }

    private void Chart_PointerLeft()
    {
        // ...?
    }

    private void Chart_PointerUp(LvcPoint pointerPosition)
    {
        if (!_isPanning) return;
        _isPanning = false;
        _panningThrottler.Call();
    }
}
