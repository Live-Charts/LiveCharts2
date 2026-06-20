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
/// A sankey ribbon used in BipartiteArc layout — a closed Bezier band whose
/// anchors are two chord endpoints on each arc (instead of vertical chords on
/// rectangle edges, as <see cref="BaseSankeyRibbonGeometry"/> assumes). The
/// platform-specific implementation builds two cubic curves with control
/// points biased toward <see cref="CenterX"/>/<see cref="CenterY"/>, producing
/// the chord-diagram "bowtie" shape. Implements
/// <see cref="IColoredGeometry"/> so each ribbon can carry its own tint while
/// sharing a single Paint task.
/// </summary>
public abstract partial class BaseSankeyChordRibbonGeometry : DrawnGeometry, IColoredGeometry, IDrawnElement
{
    /// <summary>Initializes <see cref="Color"/> to the
    /// <see cref="LvcColor.Empty"/> sentinel so the ribbon defaults to "use
    /// the shared paint color." See <see cref="BaseSankeyRibbonGeometry"/>
    /// for the rationale (otherwise the source-gen default would draw
    /// invisible transparent-black).</summary>
    protected BaseSankeyChordRibbonGeometry()
    {
        _ColorMotionProperty = new(LvcColor.Empty);
    }

    /// <summary>Per-instance color override — see <see cref="BaseSankeyRibbonGeometry.Color"/>.</summary>
    [MotionProperty]
    public partial LvcColor Color { get; set; }

    /// <summary>X of the first source-arc chord endpoint.</summary>
    [MotionProperty]
    public partial float SourceP0X { get; set; }

    /// <summary>Y of the first source-arc chord endpoint.</summary>
    [MotionProperty]
    public partial float SourceP0Y { get; set; }

    /// <summary>X of the second source-arc chord endpoint.</summary>
    [MotionProperty]
    public partial float SourceP1X { get; set; }

    /// <summary>Y of the second source-arc chord endpoint.</summary>
    [MotionProperty]
    public partial float SourceP1Y { get; set; }

    /// <summary>X of the first target-arc chord endpoint.</summary>
    [MotionProperty]
    public partial float TargetP0X { get; set; }

    /// <summary>Y of the first target-arc chord endpoint.</summary>
    [MotionProperty]
    public partial float TargetP0Y { get; set; }

    /// <summary>X of the second target-arc chord endpoint.</summary>
    [MotionProperty]
    public partial float TargetP1X { get; set; }

    /// <summary>Y of the second target-arc chord endpoint.</summary>
    [MotionProperty]
    public partial float TargetP1Y { get; set; }

    /// <summary>X of the chord-attractor center (typically the chart ellipse center). Control points are biased toward this point so ribbons curve inward.</summary>
    [MotionProperty]
    public partial float CenterX { get; set; }

    /// <summary>Y of the chord-attractor center.</summary>
    [MotionProperty]
    public partial float CenterY { get; set; }

    void IDrawnElement.DisposePaints()
    {
        Stroke?.DisposeTask();
        Fill?.DisposeTask();
        ((IDrawnElement)this).Paint?.DisposeTask();

        OnDisposed();
    }

    /// <summary>
    /// Subclass hook called when the geometry is being removed from a canvas. Use it to
    /// release any cached native resources (e.g. <c>SankeyChordRibbonGeometry</c>'s
    /// cached <c>SKPath</c>) that aren't covered by paint disposal.
    /// </summary>
    internal virtual void OnDisposed() { }
}
