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
using LiveChartsCore.Generators;
using SkiaSharp;

namespace LiveChartsCore.SkiaSharpView.Drawing.Geometries;

/// <summary>
/// A rounded rectangle that ALSO carries a per-instance <see cref="Color"/>.
/// Combines <see cref="RoundedRectangleGeometry"/>'s BorderRadius with
/// <see cref="ColoredRectangleGeometry"/>'s color-override convention so a
/// single Paint task can render N geometries each in its own color
/// (used by SankeySeries for per-node fills).
/// </summary>
public partial class ColoredRoundedRectangleGeometry
    : BaseRoundedRectangleGeometry, IColoredGeometry, IDrawnElement<SkiaSharpDrawingContext>
{
    /// <summary>Initializes a new instance of the <see cref="ColoredRoundedRectangleGeometry"/> class.</summary>
    public ColoredRoundedRectangleGeometry()
    {
        _ColorMotionProperty = new(LvcColor.Empty);
    }

    /// <inheritdoc cref="IColoredGeometry.Color" />
    [MotionProperty]
    public partial LvcColor Color { get; set; }

    /// <inheritdoc cref="IDrawnElement{TDrawingContext}.Draw(TDrawingContext)" />
    public void Draw(SkiaSharpDrawingContext context)
    {
        var activePaint = context.ActiveSkiaPaint;
        var c = Color;
        // Sentinel LvcColor.Empty means "no per-instance override" — leave the
        // paint task's own color alone (the typical "all-N-nodes-same-color"
        // path). Any non-Empty value overrides per-draw, which is the path
        // taken by NodeColorMapper. Save/restore the paint color so a
        // non-Empty override doesn't bleed into the next geometry sharing
        // the same paint task.
        var previousColor = activePaint.Color;
        var hasOverride = !c.Equals(LvcColor.Empty);
        if (hasOverride)
            activePaint.Color = new SKColor(c.R, c.G, c.B, c.A);

        var br = BorderRadius;
        context.Canvas.DrawRoundRect(
            new SKRect { Top = Y, Left = X, Size = new SKSize { Height = Height, Width = Width } },
            br.X, br.Y, activePaint);

        if (hasOverride) activePaint.Color = previousColor;
    }
}
