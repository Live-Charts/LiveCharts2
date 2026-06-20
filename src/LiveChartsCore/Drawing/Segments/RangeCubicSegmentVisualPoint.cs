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

namespace LiveChartsCore.Drawing.Segments;

/// <summary>
/// Per-point visual carried by a range line series: two markers (one at the
/// high endpoint, one at the low endpoint) and two cubic-bezier segments (one
/// in the high stroke path, one in the low stroke path / band fill).
/// </summary>
/// <param name="highGeometry">The drawn geometry for the high endpoint marker.</param>
/// <param name="lowGeometry">The drawn geometry for the low endpoint marker.</param>
public class RangeCubicSegmentVisualPoint(BoundedDrawnGeometry highGeometry, BoundedDrawnGeometry lowGeometry)
{
    /// <summary>
    /// Gets the marker geometry sitting at the high endpoint.
    /// </summary>
    public BoundedDrawnGeometry HighGeometry { get; } = highGeometry;

    /// <summary>
    /// Gets the marker geometry sitting at the low endpoint.
    /// </summary>
    public BoundedDrawnGeometry LowGeometry { get; } = lowGeometry;

    /// <summary>
    /// Gets the segment in the high curve that this point belongs to.
    /// </summary>
    public CubicBezierSegment HighSegment { get; } = new();

    /// <summary>
    /// Gets the segment in the low curve that this point belongs to.
    /// </summary>
    public CubicBezierSegment LowSegment { get; } = new();
}
