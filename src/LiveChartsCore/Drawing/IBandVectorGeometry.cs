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

using System.Collections.Generic;
using LiveChartsCore.Drawing.Segments;

namespace LiveChartsCore.Drawing;

/// <summary>
/// A vector geometry that draws a closed band between two cubic-bezier curves.
/// The high curve is supplied via the inherited <see cref="BaseVectorGeometry.Commands"/>;
/// the low curve goes here. Implementations close the band by walking the low
/// list in reverse.
/// </summary>
public interface IBandVectorGeometry
{
    /// <summary>
    /// Gets the low-curve segments, in the same forward (ascending Id) order as
    /// <see cref="BaseVectorGeometry.Commands"/>.
    /// </summary>
    LinkedList<Segment> LowCommands { get; }
}
