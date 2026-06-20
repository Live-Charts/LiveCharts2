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
/// Defines a band area drawn between two cubic-bezier curves: the inherited
/// <see cref="BaseVectorGeometry.Commands"/> traces the high curve forward,
/// <see cref="LowCommands"/> traces the low curve, and the band fill closes
/// them with the low curve reversed.
/// </summary>
public class CubicBezierBandGeometry : BaseVectorGeometry, IBandVectorGeometry, IDrawnElement<SkiaSharpDrawingContext>
{
    private SKPath? _cachedPath;

    /// <summary>
    /// Gets the low-curve segments, in the same forward (ascending Id) order as
    /// <see cref="BaseVectorGeometry.Commands"/>. The band fill walks this list
    /// in reverse to close the shape.
    /// </summary>
    public LinkedList<Segment> LowCommands { get; } = new();

    /// <inheritdoc cref="IDrawnElement{TDrawingContext}.Draw(TDrawingContext)" />
    public void Draw(SkiaSharpDrawingContext context)
    {
        if (Commands.Count == 0 || LowCommands.Count == 0) return;

        var path = _cachedPath ??= new SKPath();
        path.Reset();

        var isValid = true;
        List<Segment>? toRemoveSegments = null;

        var isFirst = true;
        foreach (var s in Commands)
        {
            s.IsValid = true;
            var cubic = (CubicBezierSegment)s;
            if (isFirst)
            {
                path.MoveTo(s.Xi, s.Yi);
                isFirst = false;
            }
            path.CubicTo(s.Xi, s.Yi, cubic.Xm, cubic.Ym, s.Xj, s.Yj);
            isValid = isValid && s.IsValid;
            if (s.IsValid && s.RemoveOnCompleted) (toRemoveSegments ??= []).Add(s);
        }

        // Bridge from the high curve's end to the low curve's end, then walk the
        // low list backward. A cubic bezier (P0, P1, P2, P3) traversed in reverse
        // is (P3, P2, P1, P0); for our segments P0 is the prior segment's Xj/Yj
        // (or the first segment's own Xi/Yi when we reach it), P1 = (Xi, Yi),
        // P2 = (Xm, Ym), P3 = (Xj, Yj).
        var lastLow = LowCommands.Last!.Value;
        path.LineTo(lastLow.Xj, lastLow.Yj);

        var node = LowCommands.Last;
        while (node is not null)
        {
            var s = node.Value;
            s.IsValid = true;
            var cubic = (CubicBezierSegment)s;
            var prev = node.Previous;
            var endX = prev?.Value.Xj ?? s.Xi;
            var endY = prev?.Value.Yj ?? s.Yi;
            path.CubicTo(cubic.Xm, cubic.Ym, s.Xi, s.Yi, endX, endY);
            isValid = isValid && s.IsValid;
            if (s.IsValid && s.RemoveOnCompleted) (toRemoveSegments ??= []).Add(s);
            node = prev;
        }

        path.Close();

        if (toRemoveSegments is not null)
        {
            foreach (var s in toRemoveSegments)
            {
                _ = Commands.Remove(s);
                _ = LowCommands.Remove(s);
                isValid = false;
            }
        }

        context.Canvas.DrawPath(path, context.ActiveSkiaPaint);

        if (!isValid) IsValid = false;
    }

    internal override void OnDisposed()
    {
        _cachedPath?.Dispose();
        _cachedPath = null;
        base.OnDisposed();
    }
}
