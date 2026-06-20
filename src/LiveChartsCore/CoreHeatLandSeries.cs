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
using System.ComponentModel;
using System.Runtime.CompilerServices;
#if NET462
using System.Linq;
#endif
using LiveChartsCore.Drawing;
using LiveChartsCore.Geo;
using LiveChartsCore.Kernel.Observers;
using LiveChartsCore.Kernel.Providers;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.Motion;
using LiveChartsCore.Painting;

namespace LiveChartsCore;

/// <summary>
/// Defines the heat land series class.
/// </summary>
/// <typeparam name="TModel">The type fo the model.</typeparam>
public abstract class CoreHeatLandSeries<TModel> : IGeoSeries, IHeatLegendSource, INotifyPropertyChanged
    where TModel : IWeigthedMapLand
{
    private Paint? _heatPaint;
    private bool _isHeatInCanvas = false;
    private readonly HashSet<GeoMapChart> _subscribedTo = [];
    private readonly CollectionDeepObserver _observer;
    private readonly HashSet<LandDefinition> _everUsed = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="CoreHeatLandSeries{TModel}"/> class.
    /// </summary>
    /// <param name="lands">The lands.</param>
    public CoreHeatLandSeries(ICollection<TModel>? lands)
    {
        Lands = lands;
        _observer = new CollectionDeepObserver(
            NotifySubscribers, null, null, CollectionDeepObserver.MayContainTrackableItems<TModel>());
    }

    /// <summary>
    /// Called when a property changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the heat map.
    /// </summary>
    public LvcColor[] HeatMap { get; set { field = value; OnPropertyChanged(); } } = [];

    /// <summary>
    /// Gets or sets the color stops.
    /// </summary>
    public double[]? ColorStops { get; set { field = value; OnPropertyChanged(); } }

    /// <summary>
    /// Gets or sets the lands.
    /// </summary>
    public ICollection<TModel>? Lands
    {
        get;
        set
        {
            _observer?.Dispose();
            _observer?.Initialize(value);
            field = value;
            _cachedBounds = null;
            OnPropertyChanged();
        }
    }

    /// <inheritdoc cref="IGeoSeries.IsVisible"/>
    public bool IsVisible { get; set { field = value; OnPropertyChanged(); } }

    private Bounds? _cachedBounds;

    /// <summary>
    /// Gets the data weight bounds (min/max) over <see cref="Lands"/>, honoring
    /// any <see cref="MinValue"/> / <see cref="MaxValue"/> overrides. Cached
    /// after the first read; the cache is invalidated when <see cref="Lands"/>
    /// (or any item within it via the collection observer),
    /// <see cref="MinValue"/>, or <see cref="MaxValue"/> change. SKHeatLegend
    /// reads this during every layout pass, so the cache keeps that hot path
    /// allocation-free.
    /// </summary>
    public Bounds WeightBounds
    {
        get
        {
            if (_cachedBounds is not null) return _cachedBounds;

            var bounds = new Bounds();
            foreach (var land in Lands ?? [])
                bounds.AppendValue(land.Value);
            if (MinValue is not null) bounds.Min = MinValue.Value;
            if (MaxValue is not null) bounds.Max = MaxValue.Value;
            _cachedBounds = bounds;
            return bounds;
        }
    }

    /// <summary>
    /// Gets or sets the minimum value override used for color mapping. When
    /// null, the observed minimum value across <see cref="Lands"/> is used.
    /// </summary>
    public double? MinValue
    {
        get;
        set { field = value; _cachedBounds = null; OnPropertyChanged(); }
    }

    /// <summary>
    /// Gets or sets the maximum value override used for color mapping. When
    /// null, the observed maximum value across <see cref="Lands"/> is used.
    /// </summary>
    public double? MaxValue
    {
        get;
        set { field = value; _cachedBounds = null; OnPropertyChanged(); }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this series contributes to the
    /// chart's legend. Defaults to true; set false to keep a series rendered
    /// on the map but suppress its row in the legend.
    /// </summary>
    public bool IsVisibleAtLegend { get; set { field = value; OnPropertyChanged(); } } = true;

    /// <inheritdoc cref="IGeoSeries.Measure(MapContext)"/>
    public void Measure(MapContext context)
    {
        _ = _subscribedTo.Add(context.CoreMap);

        InitializeHeatPaint(context);

        // Pull bounds via the computed property so the heat ramp and the
        // legend agree on the gradient endpoints (both honor Min/MaxValue).
        var bounds = WeightBounds;
        var heatStops = HeatFunctions.BuildColorStops(HeatMap, ColorStops);

        // MapShapeContext registers the gradient on context.View so the legend
        // can read it back without re-deriving — fire-and-forget constructor.
        _ = new MapShapeContext(context.View, _heatPaint!, heatStops, bounds);

        var toRemove = new HashSet<LandDefinition>(_everUsed);
        var provider = LiveCharts.DefaultSettings.GetProvider();

        foreach (var land in Lands ?? [])
        {
            _ = Maps.BuildProjector(
                context.View.MapProjection, [context.View.ControlSize.Width, context.View.ControlSize.Height]);

            var heat = HeatFunctions.InterpolateColor((float)land.Value, bounds, HeatMap, heatStops);

            var mapLand = context.View.ActiveMap.FindLand(land.Name);
            if (mapLand is null) continue;

            ApplyHeatToLand(mapLand, heat, provider);
            _ = _everUsed.Add(mapLand);
            _ = toRemove.Remove(mapLand);
        }

        ClearHeat(toRemove);
    }

    /// <summary>
    /// Lazy-attaches the heat paint to the chart canvas (once per chart) and
    /// updates its Z-index so heat fills sit just above the base land fill on
    /// each pass. The paint itself is created by a derived class via
    /// <see cref="IntitializeSeries(Paint)"/> in its constructor.
    /// </summary>
    private void InitializeHeatPaint(MapContext context)
    {
        if (_heatPaint is null) throw new Exception("Default paint not found");

        if (!_isHeatInCanvas)
        {
            // DrawMargin zone so heat fills clip to the projection's rendering
            // rectangle (matches the base GeoMapChart paints — see Measure()).
            context.View.CoreCanvas.AddDrawableTask(_heatPaint, zone: CanvasZone.DrawMargin);
            _isHeatInCanvas = true;
        }

        var baseZ = context.View.Fill?.ZIndex ?? 0;
        _heatPaint.ZIndex = baseZ + 1;
    }

    /// <summary>
    /// Replaces the Fill paint on every shape under the given land definition
    /// with a fresh <c>SolidColorPaint</c> in the interpolated heat color. A
    /// land may carry multiple shapes (e.g. island chains modeled as separate
    /// polygons under the same name).
    /// </summary>
    private static void ApplyHeatToLand(LandDefinition mapLand, LvcColor heat, ChartEngine provider)
    {
        foreach (var data in mapLand.Data)
        {
            var shape = data.Shape;
            if (shape is null) continue;

            shape.Fill = provider.GetSolidColorPaint(heat);
        }
    }

    /// <inheritdoc cref="IGeoSeries.TryGetValue(string, out double)"/>
    public bool TryGetValue(string landShortName, out double value)
    {
        if (Lands is not null)
        {
            foreach (var land in Lands)
            {
                if (string.Equals(land.Name, landShortName, StringComparison.OrdinalIgnoreCase))
                {
                    value = land.Value;
                    return true;
                }
            }
        }

        value = 0;
        return false;
    }

    /// <inheritdoc cref="IGeoSeries.Delete(MapContext)"/>
    public void Delete(MapContext context)
    {
        // ClearHeat removes each item from _everUsed during iteration; passing
        // _everUsed directly throws "Collection was modified" on .NET Framework's
        // HashSet enumerator, so snapshot it there (matches CollectionDeepObserver).
#if NET462
        ClearHeat(_everUsed.ToArray());
#else
        ClearHeat(_everUsed);
#endif
        _ = _subscribedTo.Remove(context.CoreMap);
    }

    /// <summary>
    /// Initializes the series.
    /// </summary>
    protected void IntitializeSeries(Paint heatPaint)
    {
        heatPaint.PaintStyle = PaintStyle.Fill;
        _heatPaint = heatPaint;
    }

    /// <summary>
    /// Called to invoke the property changed event.
    /// </summary>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void NotifySubscribers()
    {
        // The observer fires when items inside Lands change (e.g. a HeatLand.Value
        // is mutated in place). Drop the cached bounds so the legend re-reads
        // the new extrema on the next layout pass.
        _cachedBounds = null;
        foreach (var chart in _subscribedTo) chart.Update();
    }

    private void ClearHeat(IEnumerable<LandDefinition> toRemove)
    {
        foreach (var mapLand in toRemove)
        {
            foreach (var data in mapLand.Data)
            {
                if (data.Shape is not null)
                    data.Shape.Fill = null;
            }

            _ = _everUsed.Remove(mapLand);
        }
    }
}
