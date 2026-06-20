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
using LiveChartsCore.Geo;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Motion;
using LiveChartsCore.Painting;

namespace LiveChartsCore.Kernel.Providers;

/// <summary>
/// Defines the <see cref="ChartEngine"/> class.
/// </summary>
public abstract class ChartEngine
{
    /// <summary>
    /// Gets a new instance of the default data factory.
    /// </summary>
    public virtual DataFactory<TModel> GetDefaultDataFactory<TModel>() => new();

    /// <summary>
    /// Optionally returns an override that takes over drawing, bounds and hit-testing
    /// for the given <paramref name="series"/> (bypassing its per-point pipeline). Returns
    /// <see langword="null"/> (the default) to let the series handle itself.
    /// <para>
    /// Consulted frequently — once per measure pass for rendering and bounds, and
    /// again on every pointer move/down for hit-testing, plus on series removal — and
    /// not cached by the engine. Implementations should make this cheap (e.g. a
    /// dictionary keyed by series) rather than allocating per call.
    /// </para>
    /// </summary>
    public virtual ISeriesRenderOverride? GetRenderOverride(ISeries series) => null;

    /// <summary>
    /// Gets the render override for the given cartesian axis, if any. The override takes over how the
    /// axis lays out its separators and labels; the engine decides which axes are overridden — e.g.
    /// by the axis' concrete type and its own opt-in flags, like GroupTimeUnits on the DateTime axis
    /// (consulted while measuring; the axis caches the result by the visible range, so it re-runs on
    /// zoom/pan but is skipped while the range is unchanged). Return <see langword="null"/> (the
    /// default) to let the axis lay itself out as usual. Consulted on every measure pass, so
    /// implementations should be cheap (e.g. a type check + flag read) rather than allocating per call.
    /// </summary>
    public virtual IAxisRenderOverride? GetAxisRenderOverride(ICartesianAxis axis) => null;

    /// <summary>
    /// Gets a new instance of the default map factory.
    /// </summary>
    /// <returns></returns>
    public abstract IMapFactory GetDefaultMapFactory();

    /// <summary>
    /// Gets a new instance of the default Cartesian axis.
    /// </summary>
    /// <returns></returns>
    public abstract ICartesianAxis GetDefaultCartesianAxis();

    /// <summary>
    /// Gets a new instance of the default polar axis.
    /// </summary>
    /// <returns></returns>
    public abstract IPolarAxis GetDefaultPolarAxis();

    /// <summary>
    /// Gets a new paint of the given color.
    /// </summary>
    /// <returns></returns>
    public abstract Paint GetSolidColorPaint(LvcColor color = new());

    /// <summary>
    /// Initializes the zooming section for a cartesian chart in a given canvas.
    /// </summary>
    /// <param name="canvas">The canvas.</param>
    /// <returns>The created geometry.</returns>
    public abstract BoundedDrawnGeometry InitializeZoommingSection(CoreMotionCanvas canvas);
}
