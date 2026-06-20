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
using SkiaSharp;

namespace LiveChartsCore.SkiaSharpView.Drawing.Geometries;

/// <summary>
/// SkiaSharp implementation of a sankey ribbon. Draws a closed Bezier band
/// from the source node's right edge (at the SourceY0..SourceY1 band) to the
/// target node's left edge (at the TargetY0..TargetY1 band), with the two
/// cubic curves' control points placed at the horizontal midpoint — the
/// classic d3-sankey ribbon shape.
/// </summary>
public class SankeyRibbonGeometry : BaseSankeyRibbonGeometry, IDrawnElement<SkiaSharpDrawingContext>
{
    private SKPath? _cachedPath;

    /// <inheritdoc cref="IDrawnElement{TDrawingContext}.Draw(TDrawingContext)" />
    public void Draw(SkiaSharpDrawingContext context)
    {
        var sx = SourceX;
        var sy0 = SourceY0;
        var sy1 = SourceY1;
        var tx = TargetX;
        var ty0 = TargetY0;
        var ty1 = TargetY1;

        // Control points at the horizontal midpoint give the standard
        // S-curve shape that doesn't overshoot either node's interior.
        var mx = (sx + tx) * 0.5f;

        var path = _cachedPath ??= new SKPath();
        path.Reset();

        path.MoveTo(sx, sy0);
        path.CubicTo(mx, sy0, mx, ty0, tx, ty0); // top edge
        path.LineTo(tx, ty1);
        path.CubicTo(mx, ty1, mx, sy1, sx, sy1); // bottom edge (reversed)
        path.Close();

        // Per-instance Color override (mirrors ColoredRectangleGeometry).
        // IsEmpty is the canonical "no override" sentinel — when set, the
        // shared paint's color flows through unchanged. Save/restore the
        // paint's color so a non-Empty override doesn't bleed into the next
        // geometry sharing the same paint task.
        var c = Color;
        var activePaint = context.ActiveSkiaPaint;
        var previousColor = activePaint.Color;
        var hasOverride = !c.Equals(LvcColor.Empty);
        if (hasOverride)
            activePaint.Color = new SKColor(c.R, c.G, c.B, c.A);

        context.Canvas.DrawPath(path, activePaint);

        if (hasOverride) activePaint.Color = previousColor;
    }

    /// <inheritdoc cref="DrawnGeometry.Measure()" />
    public override LvcSize Measure()
    {
        var w = TargetX - SourceX;
        var top = SourceY0 < TargetY0 ? SourceY0 : TargetY0;
        var bottom = SourceY1 > TargetY1 ? SourceY1 : TargetY1;
        return new LvcSize(w > 0 ? w : 0, bottom - top);
    }

    /// <inheritdoc cref="DrawnGeometry.OnDisposed()" />
    internal override void OnDisposed()
    {
        _cachedPath?.Dispose();
        _cachedPath = null;
        base.OnDisposed();
    }
}
