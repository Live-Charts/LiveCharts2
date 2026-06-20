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

namespace LiveChartsCore.Measure;

/// <summary>
/// Defines the axis limit structure.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AxisLimit"/> struct.
/// </remarks>
/// <param name="min">The min value.</param>
/// <param name="max">The masx value.</param>
/// <param name="minDelta">The min delta.</param>
/// <param name="dataMin">The data min value.</param>
/// <param name="dataMax">The data max value.</param>
public struct AxisLimit(double min, double max, double minDelta, double dataMin, double dataMax)
{

    /// <summary>
    /// Gets or sets the minimum value.
    /// </summary>
    public double Min { get; set; } = min;

    /// <summary>
    /// Gets or sets the maximum value.
    /// </summary>
    public double Max { get; set; } = max;

    /// <summary>
    /// Gets or sets the minimum value.
    /// </summary>
    public double DataMin { get; set; } = dataMin;

    /// <summary>
    /// Gets or sets the maximum value.
    /// </summary>
    public double DataMax { get; set; } = dataMax;

    /// <summary>
    /// Gets or sets the min delta.
    /// </summary>
    public double MinDelta { get; set; } = minDelta;

    /// <summary>
    /// Gets or sets the user-set <see cref="LiveChartsCore.Kernel.Sketches.IPlane.MinLimit"/>
    /// pinning, independent of the current view min the chart engine may have mutated via
    /// SetLimits during zoom/pan. <c>null</c> when the user never pinned a min.
    /// </summary>
    public double? UserSetMin { get; set; }

    /// <summary>
    /// Gets or sets the user-set <see cref="LiveChartsCore.Kernel.Sketches.IPlane.MaxLimit"/>
    /// pinning, independent of the current view max the chart engine may have mutated via
    /// SetLimits during zoom/pan. <c>null</c> when the user never pinned a max.
    /// </summary>
    public double? UserSetMax { get; set; }

    /// <summary>
    /// Validates the limits of the axis, if the limits are not defined.
    /// </summary>
    /// <param name="min">The possible min value.</param>
    /// <param name="max">The possible max value.</param>
    /// <param name="step">The axis step, if greather than 0, the range will be based on this value.</param>
    public static void ValidateLimits(ref double min, ref double max, double step)
    {
        var isMinDefined = min is not (double.MaxValue or double.MinValue);
        var isMaxDefined = max is not (double.MaxValue or double.MinValue);

        if (isMinDefined && isMaxDefined)
            return; // both limits are defined, nothing to do.

        const double minDefault = 0;
        const double maxDefault = 10;
        var scale = step == 0 ? 1 : step;

        if (isMinDefined && !isMaxDefined)
        {
            // only min is defined, we need to set max.
            max = min + scale * maxDefault;
            return;
        }

        if (!isMinDefined && isMaxDefined)
        {
            // only max is defined, we need to set min.
            min = max - scale * maxDefault;
            return;
        }

        // both limits are not defined, we need to set them to default values.
        min = scale * minDefault;
        max = scale * maxDefault;
    }
}
