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
/// A sankey flow ribbon — a closed Bezier band connecting the right edge of a
/// source node to the left edge of a target node. Six animatable endpoints
/// (X + top-Y + bottom-Y for each end) drive the path; the platform-specific
/// implementation builds two cubic curves with control points at the
/// horizontal midpoint between source and target. Implements
/// <see cref="IColoredGeometry"/> so each ribbon can carry its own tint
/// (typically derived from its source node) while sharing a single Paint
/// task.
/// </summary>
public abstract partial class BaseSankeyRibbonGeometry : DrawnGeometry, IColoredGeometry, IDrawnElement
{
    /// <summary>Initializes <see cref="Color"/> to the
    /// <see cref="LvcColor.Empty"/> sentinel so the ribbon defaults to "use
    /// the shared paint color." Without this, the source-gen would default
    /// to <c>default(LvcColor)</c> = (0,0,0,0,IsEmpty=false), which the
    /// Draw's <c>!Equals(Empty)</c> check treats as a real (transparent
    /// black) override — and the ribbon renders invisible.</summary>
    protected BaseSankeyRibbonGeometry()
    {
        _ColorMotionProperty = new(LvcColor.Empty);
    }

    /// <summary>
    /// Per-instance color override. When <see cref="LvcColor.Empty"/>, the
    /// shared paint's own color is used — matches the
    /// <c>ColoredRectangleGeometry</c> convention.
    /// </summary>
    [MotionProperty]
    public partial LvcColor Color { get; set; }

    /// <summary>X coordinate of the source connection (right edge of source node).</summary>
    [MotionProperty]
    public partial float SourceX { get; set; }

    /// <summary>Top Y of the source connection band.</summary>
    [MotionProperty]
    public partial float SourceY0 { get; set; }

    /// <summary>Bottom Y of the source connection band.</summary>
    [MotionProperty]
    public partial float SourceY1 { get; set; }

    /// <summary>X coordinate of the target connection (left edge of target node).</summary>
    [MotionProperty]
    public partial float TargetX { get; set; }

    /// <summary>Top Y of the target connection band.</summary>
    [MotionProperty]
    public partial float TargetY0 { get; set; }

    /// <summary>Bottom Y of the target connection band.</summary>
    [MotionProperty]
    public partial float TargetY1 { get; set; }

    void IDrawnElement.DisposePaints()
    {
        Stroke?.DisposeTask();
        Fill?.DisposeTask();
        ((IDrawnElement)this).Paint?.DisposeTask();

        OnDisposed();
    }

    /// <summary>
    /// Subclass hook called when the geometry is being removed from a canvas. Use it to
    /// release any cached native resources (e.g. <c>SankeyRibbonGeometry</c>'s cached
    /// <c>SKPath</c>) that aren't covered by paint disposal. Mirrors
    /// <c>BaseVectorGeometry.OnDisposed</c>.
    /// </summary>
    internal virtual void OnDisposed() { }
}
