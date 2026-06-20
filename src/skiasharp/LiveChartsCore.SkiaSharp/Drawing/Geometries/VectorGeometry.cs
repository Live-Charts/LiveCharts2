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

using System.Collections.Generic;
using LiveChartsCore.Drawing;
using LiveChartsCore.Drawing.Segments;
using SkiaSharp;

namespace LiveChartsCore.SkiaSharpView.Drawing.Geometries;

/// <summary>
/// Defines an area geometry.
/// </summary>
public abstract class VectorGeometry : BaseVectorGeometry, IDrawnElement<SkiaSharpDrawingContext>
{
    // Cached path reused across Draw calls — SkPath::reset() is cheap on the native side,
    // and skipping the per-call SKPath managed alloc + Dispose adds up in perf-sensitive
    // chart paths where a vector is redrawn every invalidation.
    private SKPath? _cachedPath;

    /// <summary>
    /// Called when the area begins the draw.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="path">The path.</param>
    /// <param name="segment">The segment.</param>
    protected abstract void OnOpen(SkiaSharpDrawingContext context, SKPath path, Segment segment);

    /// <summary>
    /// Called to close the area.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="path">The path.</param>
    /// <param name="segment">The segment.</param>
    protected abstract void OnClose(SkiaSharpDrawingContext context, SKPath path, Segment segment);

    /// <summary>
    /// Called to draw the segment.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="path">The path.</param>
    /// <param name="segment">The segment.</param>
    protected abstract void OnDrawSegment(SkiaSharpDrawingContext context, SKPath path, Segment segment);

    /// <inheritdoc cref="IDrawnElement{TDrawingContext}.Draw(TDrawingContext)" />
    public void Draw(SkiaSharpDrawingContext context)
    {
        if (Commands.Count == 0) return;

        var path = _cachedPath ??= new SKPath();
        path.Reset();

        var isValid = true;
        List<Segment>? toRemoveSegments = null;

        var isFirst = true;
        Segment? last = null;

        foreach (var segment in Commands)
        {
            segment.IsValid = true;

            if (isFirst)
            {
                isFirst = false;
                OnOpen(context, path, segment);
            }

            OnDrawSegment(context, path, segment);
            isValid = isValid && segment.IsValid;

            if (segment.IsValid && segment.RemoveOnCompleted)
                (toRemoveSegments ??= []).Add(segment);
            last = segment;
        }

        if (toRemoveSegments is not null)
        {
            foreach (var segment in toRemoveSegments)
            {
                _ = Commands.Remove(segment);
                isValid = false;
            }
        }

        if (last is not null) OnClose(context, path, last);

        context.Canvas.DrawPath(path, context.ActiveSkiaPaint);

        if (!isValid) IsValid = false;
    }

    internal override void OnDisposed()
    {
        // Release the cached native SKPath deterministically when the geometry is removed
        // from its paint task. GC finalization is the safety net if a geometry is dropped
        // without going through the normal removal path.
        _cachedPath?.Dispose();
        _cachedPath = null;

        base.OnDisposed();
    }
}
