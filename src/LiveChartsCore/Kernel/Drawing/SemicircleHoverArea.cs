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
using LiveChartsCore.Drawing;
using LiveChartsCore.Measure;

namespace LiveChartsCore.Kernel.Drawing;

/// <summary>
/// Defines a semicircle hover area.
/// </summary>
/// <seealso cref="HoverArea" />
public class SemicircleHoverArea : HoverArea
{
    /// <summary>
    /// Gets or sets the center x.
    /// </summary>
    /// <value>
    /// The center x coordinate.
    /// </value>
    public float CenterX { get; set; }

    /// <summary>
    /// Gets or sets the center y.
    /// </summary>
    /// <value>
    /// The center y coordinate.
    /// </value>
    public float CenterY { get; set; }

    /// <summary>
    /// Gets or sets the start angle in degrees.
    /// </summary>
    public float StartAngle { get; set; }

    /// <summary>
    /// Gets or sets the and angle in degrees.
    /// </summary>
    public float EndAngle { get; set; }

    /// <summary>
    /// Gets or sets the inner radius.
    /// </summary>
    public float InnerRadius { get; set; }

    /// <summary>
    /// Gets or sets the radius.
    /// </summary>
    public float Radius { get; set; }

    /// <summary>
    /// Sets the area dimensions.
    /// </summary>
    /// <param name="centerX">The center x.</param>
    /// <param name="centerY">The center y.</param>
    /// <param name="startAngle">The start angle.</param>
    /// <param name="endAngle">The end angle.</param>
    /// <param name="innerRadius">The inner radius.</param>
    /// <param name="radius">The radius.</param>
    /// <returns></returns>
    public SemicircleHoverArea SetDimensions(
        float centerX, float centerY, float startAngle, float endAngle, float innerRadius, float radius)
    {
        CenterX = centerX;
        CenterY = centerY;
        StartAngle = startAngle;
        EndAngle = endAngle;
        InnerRadius = innerRadius;
        Radius = radius;
        return this;
    }

    /// <inheritdoc cref="HoverArea.DistanceTo(LvcPoint, FindingStrategy)"/>
    public override double DistanceTo(LvcPoint point, FindingStrategy strategy)
    {
        // Pixel midpoint of the slice = pie center + (mid-radius * mid-angle).
        // The strategy is intentionally ignored: a pie slice's containment is
        // angular + radial, neither of which decomposes into X-only / Y-only,
        // so TakeClosest just uses Euclidean distance to the slice midpoint.
        var midAngleRad = (StartAngle + EndAngle) * 0.5 * Math.PI / 180d;
        var midRadius = (InnerRadius + Radius * 0.5f) * 0.5;

        var midX = CenterX + midRadius * Math.Cos(midAngleRad);
        var midY = CenterY + midRadius * Math.Sin(midAngleRad);

        return Math.Sqrt(Math.Pow(point.X - midX, 2) + Math.Pow(point.Y - midY, 2));
    }

    /// <inheritdoc cref="HoverArea.IsPointerOver(LvcPoint, FindingStrategy)"/>
    public override bool IsPointerOver(LvcPoint pointerLocation, FindingStrategy strategy)
    {
        var startAngle = GetActualStartAngle();
        var endAngle = GetActualEndAngle();

        var dx = CenterX - pointerLocation.X;
        var dy = CenterY - pointerLocation.Y;
        var beta = Math.Atan(dy / dx) * (180 / Math.PI);

        if ((dx > 0 && dy < 0) || (dx > 0 && dy > 0)) beta += 180;
        if (dx < 0 && dy > 0) beta += 360;

        var r = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));

        // NOTE #230206
        // angles are normalized (from 0 to 360)
        // so in cases where (for example) angles start in 350 and end in 370,
        // then 370 is actually 10 because degrees are normalized from 0-360.
        if (endAngle > startAngle)
        {
            return
                startAngle <= beta &&
                endAngle >= beta &&
                r >= InnerRadius && r <= Radius * 0.5f;
        }

        if (beta < startAngle) beta += 360;

        return
            startAngle <= beta &&
            endAngle + 360 >= beta &&
            r >= InnerRadius && r <= Radius * 0.5f;
    }

    /// <inheritdoc cref="HoverArea.SuggestTooltipPlacement(TooltipPlacementContext, LvcSize)"/>
    public override void SuggestTooltipPlacement(TooltipPlacementContext ctx, LvcSize tooltipSize)
    {
        const double toRadians = Math.PI / 180;

        var startAngle = GetActualStartAngle();
        var endAngle = GetActualEndAngle();

        var angle = (startAngle + endAngle) / 2d;

        var r = Radius * 0.5f * 0.95f;

        // place it according to the furthest point to the center.
        if (r <= ctx.PieMostR) return;

        ctx.PieMostR = r;

        // NOTE #230206
        // angles are normalized (from 0 to 360)
        // so in cases where (for example) angles start in 350 and end in 370,
        // then 370 is actually 10 because degrees are normalized from 0-360.
        if (endAngle > startAngle)
        {
            ctx.PieX = CenterX + (float)Math.Cos(angle * toRadians) * r;
            ctx.PieY = CenterY + (float)Math.Sin(angle * toRadians) * r;
            return;
        }

        angle = (startAngle + endAngle + 360f) / 2d;
        ctx.PieX = CenterX + (float)Math.Cos(angle * toRadians) * r;
        ctx.PieY = CenterY + (float)Math.Sin(angle * toRadians) * r;
    }

    private float GetActualStartAngle()
    {
        var startAngle = StartAngle;
        startAngle %= 360;
        if (startAngle < 0) startAngle += 360;
        return startAngle;
    }

    private float GetActualEndAngle()
    {
        // -0.01 is a work around to avoid the case where the last slice (360) would be converted to 0 also
        // UPDATE: this should not be necessary anymore? based on: https://github.com/beto-rodriguez/LiveCharts2/issues/623
        var endAngle = EndAngle - 0.01f;
        endAngle %= 360;
        if (endAngle < 0) endAngle += 360;
        return endAngle;
    }
}
