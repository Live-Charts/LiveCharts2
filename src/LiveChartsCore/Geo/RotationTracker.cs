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
using LiveChartsCore.Generators;

namespace LiveChartsCore.Geo;

/// <summary>
/// Holds the orthographic rotation as motion properties so the LiveCharts
/// engine handles per-frame interpolation. <see cref="GeoMapChart"/> reads
/// <see cref="X"/> / <see cref="Y"/> in <c>Measure</c> (each read interpolates
/// the current value and flips <see cref="Animatable.IsValid"/> to false while
/// the animation is still in flight) and subscribes to the canvas
/// <c>Validated</c> event to queue another measure while !IsValid — naturally
/// rate-matching rotation to paint capacity instead of queueing Timer ticks
/// on a UI thread that is already busy painting.
/// </summary>
internal partial class RotationTracker : Animatable
{
    /// <summary>Rotation around the longitude axis (degrees).</summary>
    [MotionProperty]
    public partial double X { get; set; }

    /// <summary>Rotation around the latitude axis (degrees).</summary>
    [MotionProperty]
    public partial double Y { get; set; }
}
