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

using LiveChartsCore.Generators;

namespace LiveChartsCore.Drawing;

/// <summary>
/// A node "tile" for sankey series in BipartiteArc layout — an annular sector
/// (donut wedge) defined by a center, inner/outer radius, and angular span.
/// Carries a per-instance <see cref="Color"/> so a single Paint task can render
/// N node tiles each in its own color (parallel to
/// <see cref="BaseSankeyRibbonGeometry"/>'s convention).
/// </summary>
public abstract partial class BaseSankeyArcSegmentGeometry : DrawnGeometry, IColoredGeometry, IDrawnElement
{
    /// <summary>Initializes <see cref="Color"/> to the
    /// <see cref="LvcColor.Empty"/> sentinel so the segment defaults to "use
    /// the shared paint color." Without this, the source-gen would default
    /// to <c>default(LvcColor)</c> = (0,0,0,0,IsEmpty=false), which the
    /// Draw's <c>!Equals(Empty)</c> check treats as a real (transparent
    /// black) override — same trap <see cref="BaseSankeyRibbonGeometry"/>
    /// documents.</summary>
    protected BaseSankeyArcSegmentGeometry()
    {
        _ColorMotionProperty = new(LvcColor.Empty);
    }

    /// <inheritdoc cref="IColoredGeometry.Color" />
    [MotionProperty]
    public partial LvcColor Color { get; set; }

    /// <summary>X coordinate of the ellipse center the arc sweeps around.</summary>
    [MotionProperty]
    public partial float CenterX { get; set; }

    /// <summary>Y coordinate of the ellipse center the arc sweeps around.</summary>
    [MotionProperty]
    public partial float CenterY { get; set; }

    /// <summary>Inner radius (px). Ribbons attach to this edge.</summary>
    [MotionProperty]
    public partial float InnerRadius { get; set; }

    /// <summary>Outer radius (px). Labels sit just outside this edge.</summary>
    [MotionProperty]
    public partial float OuterRadius { get; set; }

    /// <summary>Start angle (degrees, 0° = east, clockwise).</summary>
    [MotionProperty]
    public partial float StartAngle { get; set; }

    /// <summary>Angular sweep (degrees). Positive sweeps clockwise from <see cref="StartAngle"/>.</summary>
    [MotionProperty]
    public partial float SweepAngle { get; set; }

    /// <summary>Corner rounding (px) applied to the arc's straight (radial) edges. 0 = sharp.</summary>
    [MotionProperty]
    public partial float CornerRadius { get; set; }

    void IDrawnElement.DisposePaints()
    {
        Stroke?.DisposeTask();
        Fill?.DisposeTask();
        ((IDrawnElement)this).Paint?.DisposeTask();

        OnDisposed();
    }

    /// <summary>
    /// Subclass hook called when the geometry is being removed from a canvas. Use it to
    /// release any cached native resources (e.g. <c>SankeyArcSegmentGeometry</c>'s
    /// cached <c>SKPath</c>) that aren't covered by paint disposal.
    /// </summary>
    internal virtual void OnDisposed() { }
}
