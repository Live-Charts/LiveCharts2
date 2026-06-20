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
using LiveChartsCore.Drawing;
using SkiaSharp;

namespace LiveChartsCore.SkiaSharpView.Drawing.Geometries;

/// <summary>
/// SkiaSharp implementation of a sankey chord ribbon (used in BipartiteArc
/// layout). Draws a closed band from a source-arc chord to a target-arc chord,
/// with both cubic curves' control points coincident at
/// (<see cref="BaseSankeyChordRibbonGeometry.CenterX"/>,
/// <see cref="BaseSankeyChordRibbonGeometry.CenterY"/>) — this is the
/// d3-chord convention: control-point-at-center collapses the cubic to a
/// quadratic through the center, producing the chord-diagram "bowtie" curve.
/// </summary>
public class SankeyChordRibbonGeometry : BaseSankeyChordRibbonGeometry, IDrawnElement<SkiaSharpDrawingContext>
{
    private SKPath? _cachedPath;

    /// <inheritdoc cref="IDrawnElement{TDrawingContext}.Draw(TDrawingContext)" />
    public void Draw(SkiaSharpDrawingContext context)
    {
        var cx = CenterX;
        var cy = CenterY;

        // Inner-arc radius: chord endpoints lie on the same circle (the
        // layout computes them via cos/sin at innerR from the chart center),
        // so any endpoint's distance to (cx, cy) is the radius. SourceP0 is
        // the cheapest probe.
        var dx = SourceP0X - cx;
        var dy = SourceP0Y - cy;
        var r = (float)Math.Sqrt(dx * dx + dy * dy);
        var arcRect = new SKRect(cx - r, cy - r, cx + r, cy + r);

        const float toDeg = (float)(180.0 / Math.PI);
        var sp0Angle = (float)Math.Atan2(SourceP0Y - cy, SourceP0X - cx) * toDeg;
        var sp1Angle = (float)Math.Atan2(SourceP1Y - cy, SourceP1X - cx) * toDeg;
        var tp0Angle = (float)Math.Atan2(TargetP0Y - cy, TargetP0X - cx) * toDeg;
        var tp1Angle = (float)Math.Atan2(TargetP1Y - cy, TargetP1X - cx) * toDeg;

        // Shortest-direction sweep across each chord — band sweeps are well
        // under 180° in practice (a single node never occupies the full arc),
        // so normalizing to (-180, 180] picks the inside-the-node direction.
        var targetSweep = _NormalizeSweep(tp1Angle - tp0Angle);
        var sourceCloseSweep = _NormalizeSweep(sp0Angle - sp1Angle);

        var path = _cachedPath ??= new SKPath();
        path.Reset();

        // Untwisted band: source-top → target-top via cubic; trace the inner
        // arc along the target chord (rounded end); target-bottom →
        // source-bottom via cubic; trace the inner arc back along the source
        // chord (rounded end). Control points coincident at chart center
        // collapse the cubic to a curve through (cx, cy) — d3-chord convention.
        path.MoveTo(SourceP0X, SourceP0Y);
        path.CubicTo(cx, cy, cx, cy, TargetP0X, TargetP0Y);
        path.ArcTo(arcRect, tp0Angle, targetSweep, forceMoveTo: false);
        path.CubicTo(cx, cy, cx, cy, SourceP1X, SourceP1Y);
        path.ArcTo(arcRect, sp1Angle, sourceCloseSweep, forceMoveTo: false);
        path.Close();

        // Save/restore paint color around the per-instance override so a
        // non-Empty Color doesn't bleed into the next geometry sharing the
        // same paint task.
        var c = Color;
        var activePaint = context.ActiveSkiaPaint;
        var previousColor = activePaint.Color;
        var hasOverride = !c.Equals(LvcColor.Empty);
        if (hasOverride)
            activePaint.Color = new SKColor(c.R, c.G, c.B, c.A);

        context.Canvas.DrawPath(path, activePaint);

        if (hasOverride) activePaint.Color = previousColor;
    }

    /// <inheritdoc cref="DrawnGeometry.OnDisposed()" />
    internal override void OnDisposed()
    {
        _cachedPath?.Dispose();
        _cachedPath = null;
        base.OnDisposed();
    }

    private static float _NormalizeSweep(float sweep)
    {
        while (sweep > 180f) sweep -= 360f;
        while (sweep < -180f) sweep += 360f;
        return sweep;
    }

    /// <inheritdoc cref="DrawnGeometry.Measure()" />
    public override LvcSize Measure()
    {
        // Loose bounds: span across all 4 anchors. Sufficient for paint-task
        // dispatch; no fine-grained hit-testing consumes Measure() here yet.
        var minX = SourceP0X; var maxX = SourceP0X;
        if (SourceP1X < minX) minX = SourceP1X; if (SourceP1X > maxX) maxX = SourceP1X;
        if (TargetP0X < minX) minX = TargetP0X; if (TargetP0X > maxX) maxX = TargetP0X;
        if (TargetP1X < minX) minX = TargetP1X; if (TargetP1X > maxX) maxX = TargetP1X;
        var minY = SourceP0Y; var maxY = SourceP0Y;
        if (SourceP1Y < minY) minY = SourceP1Y; if (SourceP1Y > maxY) maxY = SourceP1Y;
        if (TargetP0Y < minY) minY = TargetP0Y; if (TargetP0Y > maxY) maxY = TargetP0Y;
        if (TargetP1Y < minY) minY = TargetP1Y; if (TargetP1Y > maxY) maxY = TargetP1Y;
        return new LvcSize(maxX - minX, maxY - minY);
    }
}
