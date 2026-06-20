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

namespace LiveChartsCore;

/// <summary>
/// The final per-frame geometry of a single scatter marker, returned from
/// <c>MeasureScatterLayout</c>. The marker is centered on (DataX, DataY); the
/// rect's top-left is offset by half the geometry size so the centered hover
/// area + visual line up.
/// </summary>
public readonly struct ScatterLayout
{
    /// <summary>
    /// Initializes a new instance of <see cref="ScatterLayout"/>.
    /// </summary>
    /// <param name="x">Visual rect top-left X (marker center X minus half-size).</param>
    /// <param name="y">Visual rect top-left Y.</param>
    /// <param name="width">Marker geometry width.</param>
    /// <param name="height">Marker geometry height.</param>
    /// <param name="dataX">Data-point pixel X (marker center).</param>
    /// <param name="dataY">Data-point pixel Y (marker center).</param>
    public ScatterLayout(float x, float y, float width, float height, float dataX, float dataY)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        DataX = dataX;
        DataY = dataY;
    }

    /// <summary>Visual rect top-left X.</summary>
    public float X { get; }
    /// <summary>Visual rect top-left Y.</summary>
    public float Y { get; }
    /// <summary>Marker width.</summary>
    public float Width { get; }
    /// <summary>Marker height.</summary>
    public float Height { get; }
    /// <summary>Pixel X of the data point (marker geometric center).</summary>
    public float DataX { get; }
    /// <summary>Pixel Y of the data point (marker geometric center).</summary>
    public float DataY { get; }
}
