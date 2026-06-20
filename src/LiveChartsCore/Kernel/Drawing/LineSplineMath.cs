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

namespace LiveChartsCore.Kernel.Drawing;

/// <summary>
/// Shared math used to build cubic-bezier spline segments from a sequence of
/// chart coordinates. Used by line-shaped series (line, area, range line).
/// </summary>
internal static class LineSplineMath
{
    /// <summary>
    /// Writes the seed bezier into <paramref name="data"/> for the first point of a
    /// spline: a degenerate cubic with all three control points collapsed onto the
    /// point itself, so the path can "open" without a visible segment.
    /// </summary>
    /// <param name="data">The target buffer (mutated in place).</param>
    /// <param name="current">The first point's coordinate.</param>
    /// <param name="yOffset">Stacker offset to add to the primary value, or 0.</param>
    public static void SeedFirstSegment(BezierData data, Coordinate current, double yOffset)
    {
        var x = current.SecondaryValue;
        var y = current.PrimaryValue + yOffset;

        data.X0 = x;
        data.Y0 = y;
        data.X1 = x;
        data.Y1 = y;
        data.X2 = x;
        data.Y2 = y;
    }

    /// <summary>
    /// Computes the two cubic-bezier control points for the segment ending at
    /// <paramref name="next"/>, using a Catmull-Rom-style tangent built from the
    /// four surrounding points and blended by <paramref name="smoothness"/>.
    /// Writes the result (and the endpoint) into <paramref name="data"/>.
    /// </summary>
    /// <param name="data">The target buffer (mutated in place).</param>
    /// <param name="previous">Point before the segment start.</param>
    /// <param name="pys">Stacker offset for <paramref name="previous"/>, or 0.</param>
    /// <param name="current">Segment start.</param>
    /// <param name="cys">Stacker offset for <paramref name="current"/>, or 0.</param>
    /// <param name="next">Segment end.</param>
    /// <param name="nys">Stacker offset for <paramref name="next"/>, or 0.</param>
    /// <param name="afterNext">Point after the segment end.</param>
    /// <param name="nnys">Stacker offset for <paramref name="afterNext"/>, or 0.</param>
    /// <param name="smoothness">Tangent blend factor in [0, 1] (0 = straight, 1 = full bezier).</param>
    public static void ComputeSegment(
        BezierData data,
        Coordinate previous, double pys,
        Coordinate current, double cys,
        Coordinate next, double nys,
        Coordinate afterNext, double nnys,
        float smoothness)
    {
        var xc1 = (previous.SecondaryValue + current.SecondaryValue) / 2.0f;
        var yc1 = (previous.PrimaryValue + pys + current.PrimaryValue + cys) / 2.0f;
        var xc2 = (current.SecondaryValue + next.SecondaryValue) / 2.0f;
        var yc2 = (current.PrimaryValue + cys + next.PrimaryValue + nys) / 2.0f;
        var xc3 = (next.SecondaryValue + afterNext.SecondaryValue) / 2.0f;
        var yc3 = (next.PrimaryValue + nys + afterNext.PrimaryValue + nnys) / 2.0f;

        var len1 = (float)Math.Sqrt(
            (current.SecondaryValue - previous.SecondaryValue) *
            (current.SecondaryValue - previous.SecondaryValue) +
            (current.PrimaryValue + cys - previous.PrimaryValue + pys) * (current.PrimaryValue + cys - previous.PrimaryValue + pys));
        var len2 = (float)Math.Sqrt(
            (next.SecondaryValue - current.SecondaryValue) *
            (next.SecondaryValue - current.SecondaryValue) +
            (next.PrimaryValue + nys - current.PrimaryValue + cys) * (next.PrimaryValue + nys - current.PrimaryValue + cys));
        var len3 = (float)Math.Sqrt(
            (afterNext.SecondaryValue - next.SecondaryValue) *
            (afterNext.SecondaryValue - next.SecondaryValue) +
            (afterNext.PrimaryValue + nnys - next.PrimaryValue + nys) * (afterNext.PrimaryValue + nnys - next.PrimaryValue + nys));

        var k1 = len1 / (len1 + len2);
        var k2 = len2 / (len2 + len3);

        if (float.IsNaN(k1)) k1 = 0f;
        if (float.IsNaN(k2)) k2 = 0f;

        var xm1 = xc1 + (xc2 - xc1) * k1;
        var ym1 = yc1 + (yc2 - yc1) * k1;
        var xm2 = xc2 + (xc3 - xc2) * k2;
        var ym2 = yc2 + (yc3 - yc2) * k2;

        data.X0 = xm1 + (xc2 - xm1) * smoothness + current.SecondaryValue - xm1;
        data.Y0 = ym1 + (yc2 - ym1) * smoothness + current.PrimaryValue + cys - ym1;
        data.X1 = xm2 + (xc2 - xm2) * smoothness + next.SecondaryValue - xm2;
        data.Y1 = ym2 + (yc2 - ym2) * smoothness + next.PrimaryValue + nys - ym2;
        data.X2 = next.SecondaryValue;
        data.Y2 = next.PrimaryValue + nys;
    }
}
