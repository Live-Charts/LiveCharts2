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
/// Hit-test region for an annular sector — used by sankey nodes in the
/// BipartiteArc layout. Polar bounds are stored in Skia angle convention
/// (0° = east, positive sweeps clockwise) so they round-trip directly with
/// <see cref="LiveChartsCore.Drawing.BaseSankeyArcSegmentGeometry"/>.
/// </summary>
public class AnnularSectorHoverArea : HoverArea
{
    /// <summary>X of the arc center.</summary>
    public float CenterX { get; set; }

    /// <summary>Y of the arc center.</summary>
    public float CenterY { get; set; }

    /// <summary>Inner radius (px).</summary>
    public float InnerRadius { get; set; }

    /// <summary>Outer radius (px).</summary>
    public float OuterRadius { get; set; }

    /// <summary>Start angle (degrees, Skia convention — 0° = east, clockwise positive).</summary>
    public float StartAngle { get; set; }

    /// <summary>Sweep angle (degrees).</summary>
    public float SweepAngle { get; set; }

    /// <summary>Suggested tooltip anchor (typically the midpoint of the outer arc).</summary>
    public LvcPoint SuggestedTooltipLocation { get; set; }

    /// <summary>Sets all sector dimensions in one call.</summary>
    public AnnularSectorHoverArea SetDimensions(
        float centerX, float centerY,
        float innerRadius, float outerRadius,
        float startAngle, float sweepAngle)
    {
        CenterX = centerX;
        CenterY = centerY;
        InnerRadius = innerRadius;
        OuterRadius = outerRadius;
        StartAngle = startAngle;
        SweepAngle = sweepAngle;
        return this;
    }

    /// <summary>Pins the tooltip anchor at the midpoint of the outer arc edge
    /// — the "natural" spot to point a tooltip at a node-arc-tile.</summary>
    public AnnularSectorHoverArea AnchorAtOuterMidpoint()
    {
        const float toRad = (float)(Math.PI / 180);
        var midAngle = (StartAngle + SweepAngle * 0.5f) * toRad;
        SuggestedTooltipLocation = new LvcPoint(
            CenterX + (float)Math.Cos(midAngle) * OuterRadius,
            CenterY + (float)Math.Sin(midAngle) * OuterRadius);
        return this;
    }

    /// <inheritdoc cref="HoverArea.DistanceTo(LvcPoint, FindingStrategy)"/>
    public override double DistanceTo(LvcPoint point, FindingStrategy strategy)
    {
        const float toRad = (float)(Math.PI / 180);
        var midAngle = (StartAngle + SweepAngle * 0.5f) * toRad;
        var midR = (InnerRadius + OuterRadius) * 0.5f;
        var midX = CenterX + Math.Cos(midAngle) * midR;
        var midY = CenterY + Math.Sin(midAngle) * midR;
        return Math.Sqrt(Math.Pow(point.X - midX, 2) + Math.Pow(point.Y - midY, 2));
    }

    /// <inheritdoc cref="HoverArea.IsPointerOver(LvcPoint, FindingStrategy)"/>
    public override bool IsPointerOver(LvcPoint pointerLocation, FindingStrategy strategy)
    {
        // Convert to polar relative to (CenterX, CenterY). atan2 returns
        // (-180, 180]; we normalize to [0, 360) so the angular range check
        // is straightforward.
        var dx = pointerLocation.X - CenterX;
        var dy = pointerLocation.Y - CenterY;
        var r = Math.Sqrt(dx * dx + dy * dy);
        if (r < InnerRadius || r > OuterRadius) return false;

        const float toDeg = (float)(180.0 / Math.PI);
        var ang = (float)(Math.Atan2(dy, dx) * toDeg);

        // Walk both bounds + the pointer angle to a common 0..360 frame.
        var start = StartAngle;
        var end = StartAngle + SweepAngle;
        while (ang < start) ang += 360f;
        while (ang > start + 360f) ang -= 360f;
        return ang <= end;
    }

    /// <inheritdoc cref="HoverArea.SuggestTooltipPlacement(TooltipPlacementContext, LvcSize)"/>
    public override void SuggestTooltipPlacement(TooltipPlacementContext ctx, LvcSize tooltipSize)
    {
        // Populate the cartesian-tooltip context using the bounding box of the
        // sector — close enough for tooltip layout, exact polar reasoning isn't
        // needed at the tooltip-positioning stage (the inner hit-test already
        // verified the sector). Mirrors RectangleHoverArea's shape so the
        // shared cartesian tooltip layout works without a polar-specific path.
        const float toRad = (float)(Math.PI / 180);
        var midAngle = (StartAngle + SweepAngle * 0.5f) * toRad;
        var anchorX = CenterX + (float)Math.Cos(midAngle) * OuterRadius;
        var anchorY = CenterY + (float)Math.Sin(midAngle) * OuterRadius;

        // Sankey arc sectors don't carry a "less than pivot" notion (unlike
        // bars whose value can be signed). Force false so AutoPopPupPlacement
        // doesn't bias to Bottom on the first sector seen — RectangleHoverArea
        // does the same dance (`AreAllLessThanPivot = LessThanPivot && …`),
        // and with LessThanPivot defaulting false this collapses to `= false`.
        ctx.AreAllLessThanPivot = false;

        if (anchorY < ctx.MostTop) ctx.MostTop = anchorY;
        if (anchorY > ctx.MostBottom) ctx.MostBottom = anchorY;
        if (anchorX > ctx.MostRight) ctx.MostRight = anchorX;
        if (anchorX < ctx.MostLeft) ctx.MostLeft = anchorX;

        if (anchorY < ctx.MostAutoTop)
        {
            ctx.MostAutoTop = anchorY;
            ctx.AutoPopPupPlacement = ctx.AreAllLessThanPivot ? PopUpPlacement.Bottom : PopUpPlacement.Top;
        }
        if (anchorY > ctx.MostAutoBottom)
        {
            ctx.MostAutoBottom = anchorY;
            ctx.AutoPopPupPlacement = ctx.AreAllLessThanPivot ? PopUpPlacement.Bottom : PopUpPlacement.Top;
        }
    }
}
