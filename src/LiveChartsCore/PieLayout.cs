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
/// Final per-frame geometry of a single pie slice, returned from
/// <c>MeasurePieLayout</c>. Pie is angular (StartAngle / SweepAngle drive the
/// arc) so the X/Y/Width/Height fields describe the bounding box used by the
/// underlying doughnut geometry for centering rather than a literal slice rect.
/// </summary>
public readonly struct PieLayout
{
    /// <summary>Initializes a new instance of <see cref="PieLayout"/>.</summary>
    public PieLayout(
        float centerX, float centerY,
        float x, float y, float width, float height,
        float startAngle, float sweepAngle,
        float pushOut, float innerRadius, float outerRadius,
        float cornerRadius)
    {
        CenterX = centerX;
        CenterY = centerY;
        X = x;
        Y = y;
        Width = width;
        Height = height;
        StartAngle = startAngle;
        SweepAngle = sweepAngle;
        PushOut = pushOut;
        InnerRadius = innerRadius;
        OuterRadius = outerRadius;
        CornerRadius = cornerRadius;
    }

    /// <summary>Chart-center X (slice geometric center).</summary>
    public float CenterX { get; }
    /// <summary>Chart-center Y.</summary>
    public float CenterY { get; }
    /// <summary>Doughnut bounding-box top-left X.</summary>
    public float X { get; }
    /// <summary>Doughnut bounding-box top-left Y.</summary>
    public float Y { get; }
    /// <summary>Doughnut bounding-box width (== outer-radius x2 in pixel space).</summary>
    public float Width { get; }
    /// <summary>Doughnut bounding-box height.</summary>
    public float Height { get; }
    /// <summary>Slice starting angle (degrees, after initial rotation + clockwise correction).</summary>
    public float StartAngle { get; }
    /// <summary>Slice sweep angle (degrees, possibly clamped to ~360 to avoid the broken-arc bug from issue #2131).</summary>
    public float SweepAngle { get; }
    /// <summary>Pushout offset for hover effect.</summary>
    public float PushOut { get; }
    /// <summary>Inner radius (for doughnut hole).</summary>
    public float InnerRadius { get; }
    /// <summary>Outer radius (for outer arc).</summary>
    public float OuterRadius { get; }
    /// <summary>Corner-radius for rounded slice corners.</summary>
    public float CornerRadius { get; }
}
