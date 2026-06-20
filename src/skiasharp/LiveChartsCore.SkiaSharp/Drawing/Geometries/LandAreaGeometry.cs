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

using LiveChartsCore.Drawing;
using LiveChartsCore.Drawing.Segments;
using SkiaSharp;

namespace LiveChartsCore.SkiaSharpView.Drawing.Geometries;

/// <summary>
/// Defines a land area geometry with cached path rendering for smooth zoom/pan.
/// Re-implements the draw interface to bypass per-frame path rebuilding.
/// Uses canvas matrix transform (GPU-accelerated) — zero allocations during zoom/pan.
/// </summary>
public class LandAreaGeometry : VectorGeometry, IDrawnElement<SkiaSharpDrawingContext>
{
    private SKPath? _basePath;
    private bool _pathDirty = true;

    /// <summary>
    /// Gets or sets the shared viewport transform applied during rendering.
    /// </summary>
    public MapViewportTransform? ViewportTransform { get; set; }

    /// <summary>
    /// Marks the cached path as dirty, forcing a rebuild on the next draw.
    /// Call this after modifying the Commands collection.
    /// </summary>
    public void MarkPathDirty()
    {
        _pathDirty = true;
    }

    /// <summary>
    /// Sets a pre-built base path directly, bypassing the Commands-based path building.
    /// Used for combined geometries where multiple sub-paths need proper MoveTo calls.
    /// </summary>
    /// <param name="path">The pre-built SKPath. Ownership is transferred to this geometry.</param>
    public void SetBasePath(SKPath path)
    {
        _basePath?.Dispose();
        _basePath = path;
        _pathDirty = false;
    }

    /// <summary>
    /// Returns the cached base path, reset and ready for in-place rebuild.
    /// Caller populates with MoveTo/LineTo. Hot path for animated projections
    /// (orthographic rotation) — avoids the native alloc + dispose of a fresh
    /// <see cref="SKPath"/> per land per frame.
    /// </summary>
    public SKPath GetOrResetBasePath()
    {
        _basePath ??= new SKPath();
        _basePath.Reset();
        _pathDirty = false;
        return _basePath;
    }

    /// <summary>
    /// Draws the land area using a cached base path with GPU canvas matrix transform.
    /// Zero allocations during zoom/pan — just Save/Concat/DrawPath/Restore.
    /// </summary>
    void IDrawnElement<SkiaSharpDrawingContext>.Draw(SkiaSharpDrawingContext context)
    {
        if (_basePath is null && Commands.Count == 0) return;

        // Rebuild base path only when geometry data changes (map load, resize)
        if (_pathDirty || _basePath is null)
        {
            _basePath?.Dispose();
            _basePath = new SKPath();

            var isFirst = true;
            foreach (var segment in Commands)
            {
                if (isFirst)
                {
                    _basePath.MoveTo(segment.Xi, segment.Yi);
                    isFirst = false;
                }
                _basePath.LineTo(segment.Xi, segment.Yi);
            }

            _pathDirty = false;
        }

        // Apply viewport transform via canvas matrix (GPU) and draw cached path
        var vt = ViewportTransform;
        if (vt is not null && vt.IsActive)
        {
            var state = context.Canvas.Save();
            var matrix = vt.GetMatrix();
            context.Canvas.Concat(ref matrix);
            context.Canvas.DrawPath(_basePath, context.ActiveSkiaPaint);
            context.Canvas.RestoreToCount(state);
        }
        else
        {
            context.Canvas.DrawPath(_basePath, context.ActiveSkiaPaint);
        }
    }

    /// <inheritdoc cref="VectorGeometry.OnDrawSegment(SkiaSharpDrawingContext, SKPath, Segment)"/>
    protected override void OnDrawSegment(SkiaSharpDrawingContext context, SKPath path, Segment segment) =>
        path.LineTo(segment.Xi, segment.Yi);

    /// <inheritdoc cref="VectorGeometry.OnOpen(SkiaSharpDrawingContext, SKPath, Segment)"/>
    protected override void OnOpen(SkiaSharpDrawingContext context, SKPath path, Segment segment) =>
        path.MoveTo(segment.Xi, segment.Yi);

    /// <inheritdoc cref="VectorGeometry.OnClose(SkiaSharpDrawingContext, SKPath, Segment)"/>
    protected override void OnClose(SkiaSharpDrawingContext context, SKPath path, Segment segment)
    { }

    /// <summary>
    /// Determines whether the specified point is inside this land polygon.
    /// </summary>
    public override bool ContainsPoint(float x, float y)
    {
        var vt = ViewportTransform;
        if (vt is not null && vt.IsActive)
            (x, y) = vt.InverseTransform(x, y);

        if (_basePath is not null && !_pathDirty)
            return _basePath.Contains(x, y);

        if (Commands.Count == 0) return false;

        using var path = new SKPath();
        var isFirst = true;

        foreach (var segment in Commands)
        {
            if (isFirst)
            {
                path.MoveTo(segment.Xi, segment.Yi);
                isFirst = false;
            }
            path.LineTo(segment.Xi, segment.Yi);
        }

        path.Close();
        return path.Contains(x, y);
    }
}
