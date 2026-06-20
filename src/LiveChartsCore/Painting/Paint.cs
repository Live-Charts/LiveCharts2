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

#pragma warning disable IDE0290 // Use primary ctor... why suggest this?? you cant do it in this case

using System;
using System.Collections.Generic;
using LiveChartsCore.Drawing;
using LiveChartsCore.Motion;

namespace LiveChartsCore.Painting;

/// <summary>
/// Defines a set of geometries that will be drawn according to this instance specifications.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="Paint"/> class.
/// </remarks>
public abstract partial class Paint : Animatable
{
    private readonly Dictionary<object, HashSet<IDrawnElement>> _geometriesByCanvas = [];
    internal event Action? ZIndexChanged;

    /// <param name="strokeThickness">The stroke thickness.</param>
    /// <param name="strokeMiter">The stroke miter.</param>
    public Paint(float strokeThickness = 1f, float strokeMiter = 0)
    {
        StrokeThickness = strokeThickness;
        StrokeMiter = strokeMiter;
    }

    /// <summary>
    /// Gets the default paint.
    /// </summary>
    public static Paint Default { get; } = new DefaultPaint();



    /// <summary>
    /// Gets or sets the index of the z.
    /// </summary>
    public double ZIndex
    {
        get => field;
        set
        {
            if (field == value) return;
            field = value;
            ZIndexChanged?.Invoke();
        }
    }

    internal PaintStyle PaintStyle { get; set; }

    /// <summary>
    /// Obsolete.
    /// </summary>
    [Obsolete("This is not accesible now at this point, instead set the Fill or Stroke properties of the desired geomemtry.")]
    public bool IsStroke
    {
        get => PaintStyle.HasFlag(PaintStyle.Stroke);
        set => PaintStyle = value ? PaintStyle.Stroke : PaintStyle.Fill;
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is antialias.
    /// </summary>
    public bool IsAntialias { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether this instance is paused.
    /// </summary>
    public bool IsPaused { get; set; }

    /// <summary>
    /// Gets or sets the stroke thickness.
    /// </summary>
    public float StrokeThickness { get; set; }

    /// <summary>
    /// Gets or sets the stroke miter.
    /// </summary>
    public float StrokeMiter { get; set; }

    /// <summary>
    /// Gets a value indicating whether this instance is empty.
    /// </summary>
    public bool IsEmpty => _geometriesByCanvas.Count == 0;

    /// <summary>
    /// Sets the geometries for the given canvas.
    /// </summary>
    /// <param name="canvas">The canvas.</param>
    /// <param name="geometries">The geometries.</param>
    public void SetGeometries(CoreMotionCanvas canvas, HashSet<IDrawnElement> geometries)
    {
        if (this == Default) return;

        _geometriesByCanvas[canvas] = geometries;
        IsValid = false;
    }

    /// <summary>
    /// Adds a geometry to the paint task for the given canvas.
    /// </summary>
    /// <param name="canvas">The canvas.</param>
    /// <param name="geometry">The geometry.</param>
    public void AddGeometryToPaintTask(CoreMotionCanvas canvas, IDrawnElement geometry)
    {
        if (this == Default) return;

        var g = GetGeometriesByCanvas(canvas);

        if (g is null)
        {
            g = [];
            _geometriesByCanvas[canvas] = g;
        }

        _ = g.Add(geometry);
        IsValid = false;
    }

    /// <summary>
    /// Removes the given geometry from paint task.
    /// </summary>
    /// <param name="canvas">The canvas.</param>
    /// <param name="geometry">The geometry.</param>
    public void RemoveGeometryFromPaintTask(CoreMotionCanvas canvas, IDrawnElement geometry)
    {
        geometry.DisposePaints();
        _ = GetGeometriesByCanvas(canvas)?.Remove(geometry);
        ((Animatable)geometry)._statesTracker = null;

        IsValid = false;
    }

    /// <summary>
    /// Removes all geometry from paint task.
    /// </summary>
    /// <param name="canvas">The canvas.</param>
    public void ClearGeometriesFromPaintTask(CoreMotionCanvas canvas)
    {
        var geometries = GetGeometriesByCanvas(canvas);
        if (geometries is null || geometries.Count == 0) return;

        // DisposePaints fires the OnDisposed hook on the geometry — needed for geometries
        // that hold cached native resources (e.g. VectorGeometry's SKPath) even if they
        // don't own their own paints. DisposePaints null-checks the paints internally.
        foreach (var geometry in geometries)
            geometry.DisposePaints();

        GetGeometriesByCanvas(canvas)?.Clear();

        IsValid = false;
    }

    /// <summary>
    /// Clones the task.
    /// </summary>
    /// <returns>A new instance with the same properties.</returns>
    public abstract Paint CloneTask();

    /// <summary>
    /// Parses a hexadecimal color string.
    /// </summary>
    /// <param name="hexString">the hex string.</param>
    /// <returns>A solid color paint with the color.</returns>
    /// <exception cref="ArgumentException">Invalid hex color.</exception>
    public static Paint? Parse(string hexString)
    {
        return !LvcColor.TryParse(hexString, out var color)
            ? throw new ArgumentException("Invalid hexadecimal color string.", nameof(hexString))
            : LiveCharts.DefaultSettings.GetProvider().GetSolidColorPaint(color);
    }

    internal void ReleaseCanvas(CoreMotionCanvas canvas) =>
        _ = _geometriesByCanvas.Remove(canvas);

    internal abstract Paint Transitionate(float progress, Paint target);

    internal HashSet<IDrawnElement> GetGeometries(CoreMotionCanvas canvas) =>
        GetGeometriesByCanvas(canvas) ?? [];

    internal abstract void OnPaintStarted(DrawingContext drawingContext, IDrawnElement? drawnElement);

    internal abstract void OnPaintFinished(DrawingContext drawingContext, IDrawnElement? drawnElement);

    internal abstract void ApplyOpacityMask(DrawingContext context, float opacity, IDrawnElement? drawnElement);

    internal abstract void RestoreOpacityMask(DrawingContext context, float opacity, IDrawnElement? drawnElement);

    internal abstract void DisposeTask();

    private HashSet<IDrawnElement>? GetGeometriesByCanvas(object canvas)
    {
        return _geometriesByCanvas.TryGetValue(canvas, out var geometries)
            ? geometries
            : null;
    }

    private class DefaultPaint : Paint
    {
        public override Paint CloneTask() => this;
        internal override void ApplyOpacityMask(DrawingContext context, float opacity, IDrawnElement? drawnElement) { }
        internal override void OnPaintFinished(DrawingContext context, IDrawnElement? drawnElement) { }
        internal override void OnPaintStarted(DrawingContext drawingContext, IDrawnElement? drawnElement) { }
        internal override void RestoreOpacityMask(DrawingContext context, float opacity, IDrawnElement? drawnElement) { }
        internal override Paint Transitionate(float progress, Paint target) => this;
        internal override void DisposeTask() { }
    }
}
