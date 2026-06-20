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

namespace LiveChartsCore;

/// <summary>
/// Defines internal members of a cartesian axis that are not part of the public
/// <see cref="ICartesianAxis"/> contract.
/// </summary>
internal interface IInternalCartesianAxis
{
    /// <summary>
    /// Gets the user-pinned <see cref="IPlane.MinLimit"/>, independent of the current view min the chart
    /// engine mutates via SetLimits during zoom/pan. <c>null</c> when the user never pinned a min.
    /// </summary>
    double? UserSetMinLimit { get; }

    /// <summary>
    /// Gets the user-pinned <see cref="IPlane.MaxLimit"/>, independent of the current view max the chart
    /// engine mutates via SetLimits during zoom/pan. <c>null</c> when the user never pinned a max.
    /// </summary>
    double? UserSetMaxLimit { get; }
}
