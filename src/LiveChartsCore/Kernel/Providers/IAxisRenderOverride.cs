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
using System.Collections.Generic;
using LiveChartsCore.Kernel.Sketches;

namespace LiveChartsCore.Kernel.Providers;

/// <summary>
/// An optional hook a <see cref="ChartEngine"/> can supply to take over how a cartesian axis lays out
/// its separators and labels. The engine decides which axes are overridden (e.g. by the axis'
/// concrete type and its own opt-in flags, like GroupTimeUnits on the DateTime axis); the axis caches
/// the result by the visible range, so <see cref="TryGroup"/> re-runs when you zoom or pan but is
/// skipped while the range is unchanged. The default engine returns none, so the axis lays itself out
/// as usual.
/// </summary>
public interface IAxisRenderOverride
{
    /// <summary>
    /// Supplies the separator positions and the labeler the axis should use for the current visible
    /// range. The values are applied for this measure only — the override must not mutate the axis'
    /// own <see cref="IPlane.CustomSeparators"/> / <see cref="IPlane.Labeler"/> (doing so each frame
    /// would re-trigger a measure). Return <see langword="true"/> to take over, or
    /// <see langword="false"/> to let the axis lay itself out normally.
    /// </summary>
    /// <param name="axis">The axis being measured.</param>
    /// <param name="chart">The chart being measured.</param>
    /// <param name="min">The effective visible minimum, in axis units.</param>
    /// <param name="max">The effective visible maximum, in axis units.</param>
    /// <param name="separators">The separator positions to draw (in axis units), or null.</param>
    /// <param name="labeler">The labeler to format each separator, or null to keep the axis' own.</param>
    bool TryGroup(
        ICartesianAxis axis,
        Chart chart,
        double min,
        double max,
        out IEnumerable<double>? separators,
        out Func<double, string>? labeler);
}
