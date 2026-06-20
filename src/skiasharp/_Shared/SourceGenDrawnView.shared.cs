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

using LiveChartsCore.Kernel.Sketches;

namespace LiveChartsGeneratedCode;

// =================================================================================
// Cross-platform base for any control that hosts a MotionCanvas and renders through
// the LiveCharts engine. Owns just the bits that are identical for cartesian, pie,
// polar AND geo views: MotionCanvas hosting, Size/Load/Unload event wiring, and the
// IDrawnView contract. Chart-specific plumbing (series template inflation, command
// binding, pointer-capture recovery, observer lifecycle) lives in SourceGenChart;
// map-specific plumbing (wheel-to-zoom) lives in SourceGenMapChart.
// =================================================================================

/// <inheritdoc cref="IDrawnView" />
public abstract partial class SourceGenDrawnView : IDrawnView
{
    /// <summary>
    /// Called when the drawn view is attached to the visual tree. Subclasses
    /// chain their own Load logic on top (StartObserving for chart, etc).
    /// </summary>
    protected abstract void OnDrawnViewLoaded();

    /// <summary>
    /// Called when the drawn view is detached from the visual tree.
    /// </summary>
    protected abstract void OnDrawnViewUnloaded();

    /// <summary>
    /// Called when the control's size changes. The default implementation invokes
    /// an update on the core chart; subclasses may override if they need different
    /// behavior.
    /// </summary>
    protected abstract void OnDrawnViewSizeChanged();
}
