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
using LiveChartsCore.Measure;

namespace LiveChartsCore.Kernel;

/// <summary>
/// Decides, measure after measure, whether a series with <see cref="GeometryVisibility.Auto"/> draws the geometries
/// on its points: they are hidden when the points are closer than a geometry (the geometries would overlap), and shown
/// again only when they are 1.3 geometries apart, so a chart near the limit does not flicker while it is resized or
/// zoomed. A point alone between two gaps has no line, its geometry is the only thing that shows it, so it is always
/// drawn.
/// </summary>
internal sealed class AutoGeometryVisibility
{
    private const float HideBelow = 1f;
    private const float ShowAbove = 1.3f;
    private const string HoverState = "Hover";

    private bool _hidden;
    private float _min, _max, _length, _lastX, _lastY;
    private int _count;
    private readonly List<Animatable> _isolated = [];

    /// <summary>
    /// Gets whether the geometries are hidden because the points are too close (only in Auto).
    /// </summary>
    public bool IsHidden => _hidden;

    /// <summary>
    /// The opacity of a geometry for the given mode.
    /// </summary>
    public float GetOpacity(GeometryVisibility mode) =>
        mode == GeometryVisibility.Never || (mode == GeometryVisibility.Auto && _hidden) ? 0f : 1f;

    /// <summary>
    /// Starts a measure.
    /// </summary>
    public void Begin()
    {
        _min = float.MaxValue;
        _max = float.MinValue;
        _length = 0;
        _count = 0;
        _isolated.Clear();
    }

    /// <summary>
    /// Adds a point; on a cartesian chart the spacing is horizontal, on a polar one it is measured along the path.
    /// </summary>
    public void Add(float x, float y, bool alongPath)
    {
        if (alongPath && _count > 0)
        {
            var dx = x - _lastX;
            var dy = y - _lastY;
            _length += (float)Math.Sqrt(dx * dx + dy * dy);
        }

        if (x < _min) _min = x;
        if (x > _max) _max = x;
        _lastX = x;
        _lastY = y;
        _count++;
    }

    /// <summary>
    /// Ends a measure, returns true when the geometries changed from shown to hidden or back.
    /// </summary>
    public bool End(GeometryVisibility mode, float geometrySize, bool alongPath)
    {
        var hidden = false;

        if (mode == GeometryVisibility.Auto && geometrySize > 0 && _count > 1)
        {
            var spacing = (alongPath ? _length : _max - _min) / (_count - 1);
            hidden = _hidden
                ? spacing < ShowAbove * geometrySize
                : spacing < HideBelow * geometrySize;
        }

        if (hidden == _hidden) return false;
        _hidden = hidden;
        return true;
    }

    /// <summary>
    /// Sets the opacity of a geometry, a geometry under the pointer keeps the opacity of its hover state.
    /// </summary>
    public void Apply(Animatable? geometry, GeometryVisibility mode)
    {
        if (geometry is null || IsHovered(geometry) || geometry is not IDrawnElement drawn) return;
        if (_isolated.Count > 0 && _isolated.Contains(geometry)) return;
        drawn.Opacity = GetOpacity(mode);
    }

    /// <summary>
    /// Draws the geometry of a point that is alone between two gaps, whatever the spacing (not in Never).
    /// </summary>
    public void KeepVisible(Animatable? geometry, GeometryVisibility mode)
    {
        if (geometry is null || mode == GeometryVisibility.Never) return;
        _isolated.Add(geometry);
        if (!IsHovered(geometry) && geometry is IDrawnElement drawn) drawn.Opacity = 1f;
    }

    /// <summary>
    /// Shows the geometry of the point under the pointer when the geometries are hidden because they are too close.
    /// </summary>
    public void OnPointerEnter(Animatable? geometry, GeometryVisibility mode)
    {
        if (mode != GeometryVisibility.Auto || !_hidden || geometry is not IDrawnElement drawn) return;
        drawn.Opacity = 1f;
    }

    /// <summary>
    /// Hides again the geometry of the point the pointer left, when the geometries are hidden because they are too
    /// close; the hover state does not restore it, it may not hold the opacity (the default theme's line hover only
    /// scales the geometry).
    /// </summary>
    public void OnPointerLeft(Animatable? geometry, GeometryVisibility mode)
    {
        if (mode != GeometryVisibility.Auto || !_hidden || geometry is not IDrawnElement drawn) return;
        if (_isolated.Count > 0 && _isolated.Contains(geometry)) return;
        drawn.Opacity = 0f;
    }

    private static bool IsHovered(Animatable geometry) =>
        geometry._statesTracker?.ActiveStates.Contains(HoverState) == true;
}
