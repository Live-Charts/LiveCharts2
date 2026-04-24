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
/// Defines a rectangle hover area.
/// </summary>
/// <seealso cref="HoverArea" />
public class RectangleHoverArea : HoverArea
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RectangleHoverArea"/> class.
    /// </summary>
    public RectangleHoverArea() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RectangleHoverArea"/> class.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public RectangleHoverArea(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Gets or sets the x location.
    /// </summary>
    /// <value>
    /// The x.
    /// </value>
    public float X { get; set; }

    /// <summary>
    /// Gets or sets the y location.
    /// </summary>
    /// <value>
    /// The y.
    /// </value>
    public float Y { get; set; }

    /// <summary>
    /// Gets or sets the width.
    /// </summary>
    /// <value>
    /// The width.
    /// </value>
    public float Width { get; set; }

    /// <summary>
    /// Gets or sets the height.
    /// </summary>
    /// <value>
    /// The height.
    /// </value>
    public float Height { get; set; }

    /// <summary>
    /// Gets or sets the suggested tool tip location
    /// </summary>
    public LvcPoint SuggestedTooltipLocation { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the primary value is less than pivot.
    /// </summary>
    public bool LessThanPivot { get; set; }

    /// <summary>
    /// Sets the area dimensions.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <returns>The current instance.</returns>
    public RectangleHoverArea SetDimensions(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        return this;
    }

    /// <summary>
    /// Sets the <see cref="SuggestedTooltipLocation"/> to the center of the <see cref="RectangleHoverArea"/> in the X axis.
    /// </summary>
    /// <returns>The current instance.</returns>
    public RectangleHoverArea CenterXToolTip()
    {
        SuggestedTooltipLocation = new LvcPoint(X + Width * 0.5f, SuggestedTooltipLocation.Y);
        return this;
    }

    /// <summary>
    /// Sets the <see cref="SuggestedTooltipLocation"/> to the start of the <see cref="RectangleHoverArea"/> in the X axis.
    /// </summary>
    /// <returns>The current instance.</returns>
    public RectangleHoverArea StartXToolTip()
    {
        SuggestedTooltipLocation = new LvcPoint(X, SuggestedTooltipLocation.Y);
        return this;
    }

    /// <summary>
    /// Sets the <see cref="SuggestedTooltipLocation"/> to the center of the <see cref="RectangleHoverArea"/> in the X axis.
    /// </summary>
    /// <returns>The current instance.</returns>
    public RectangleHoverArea EndXToolTip()
    {
        SuggestedTooltipLocation = new LvcPoint(X + Width, SuggestedTooltipLocation.Y);
        return this;
    }

    /// <summary>
    /// Sets the <see cref="SuggestedTooltipLocation"/> to the center of the <see cref="RectangleHoverArea"/> in the Y axis.
    /// </summary>
    /// <returns>The current instance.</returns>
    public RectangleHoverArea CenterYToolTip()
    {
        SuggestedTooltipLocation = new LvcPoint(SuggestedTooltipLocation.X, Y + Height * 0.5f);
        return this;
    }

    /// <summary>
    /// Sets the <see cref="SuggestedTooltipLocation"/> to the start of the <see cref="RectangleHoverArea"/> in the Y axis.
    /// </summary>
    /// <returns>The current instance.</returns>
    public RectangleHoverArea StartYToolTip()
    {
        SuggestedTooltipLocation = new LvcPoint(SuggestedTooltipLocation.X, Y);
        return this;
    }

    /// <summary>
    /// Sets the <see cref="SuggestedTooltipLocation"/> to the center of the <see cref="RectangleHoverArea"/> in the Y axis.
    /// </summary>
    /// <returns>The current instance.</returns>
    public RectangleHoverArea EndYToolTip()
    {
        SuggestedTooltipLocation = new LvcPoint(SuggestedTooltipLocation.X, Y + Height);
        return this;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the primary value is less than pivot.
    /// </summary>
    /// <returns>The current instance.</returns>
    public RectangleHoverArea IsLessThanPivot()
    {
        LessThanPivot = true;
        return this;
    }

    /// <inheritdoc cref="HoverArea.DistanceTo(LvcPoint, FindingStrategy)"/>
    public override double DistanceTo(LvcPoint point, FindingStrategy strategy)
    {
        var midX = X + Width * 0.5;
        var midY = Y + Height * 0.5;

        return strategy switch
        {
            FindingStrategy.CompareOnlyX or FindingStrategy.CompareOnlyXTakeClosest
                => Math.Abs(point.X - midX),
            FindingStrategy.CompareOnlyY or FindingStrategy.CompareOnlyYTakeClosest
                => Math.Abs(point.Y - midY),
            FindingStrategy.CompareAll or FindingStrategy.CompareAllTakeClosest or
            FindingStrategy.ExactMatch or FindingStrategy.ExactMatchTakeClosest
                => Math.Sqrt(Math.Pow(point.X - midX, 2) + Math.Pow(point.Y - midY, 2)),
            FindingStrategy.Automatic
                => throw new Exception($"The strategy {strategy} is not supported."),
            _ => throw new NotImplementedException()
        };
    }

    /// <inheritdoc cref="HoverArea.IsPointerOver(LvcPoint, FindingStrategy)"/>
    public override bool IsPointerOver(LvcPoint pointerLocation, FindingStrategy strategy)
    {
        // at least one pixel to fire the tooltip.
        var w = Width switch
        {
            > 1 => Width,
            >= 0 => 1,
            > -1 => -1,
            _ => Width
        };

        var h = Height switch
        {
            > 1 => Height,
            >= 0 => 1,
            > -1 => -1,
            _ => Height
        };

        var isInX = IsBetween(pointerLocation.X, X, X + w);
        var isInY = IsBetween(pointerLocation.Y, Y, Y + h);

        return strategy switch
        {
            FindingStrategy.CompareOnlyX or FindingStrategy.CompareOnlyXTakeClosest
                => isInX,
            FindingStrategy.CompareOnlyY or FindingStrategy.CompareOnlyYTakeClosest
                => isInY,
            FindingStrategy.CompareAll or FindingStrategy.CompareAllTakeClosest or
            FindingStrategy.ExactMatch or FindingStrategy.ExactMatchTakeClosest
                => isInX && isInY,
            FindingStrategy.Automatic
                => throw new Exception($"The strategy {strategy} is not supported."),
            _ => throw new NotImplementedException()
        };
    }

    private static bool IsBetween(float value, float firstBound, float secondBound) => firstBound > secondBound
        ? value > secondBound && value < firstBound
        : value > firstBound && value < secondBound;

    /// <inheritdoc cref="HoverArea.SuggestTooltipPlacement(TooltipPlacementContext, LvcSize)"/>
    public override void SuggestTooltipPlacement(TooltipPlacementContext ctx, LvcSize tooltipSize)
    {
        ctx.AreAllLessThanPivot = LessThanPivot && ctx.AreAllLessThanPivot;
        var autoY = (LessThanPivot ? 1 : 0) * tooltipSize.Height;

        if (Y < ctx.MostTop) ctx.MostTop = SuggestedTooltipLocation.Y;
        if (Y + Height > ctx.MostBottom) ctx.MostBottom = SuggestedTooltipLocation.Y;
        if (X + Width > ctx.MostRight) ctx.MostRight = SuggestedTooltipLocation.X;
        if (X < ctx.MostLeft) ctx.MostLeft = SuggestedTooltipLocation.X;

        if (Y < ctx.MostAutoTop)
        {
            ctx.MostAutoTop = SuggestedTooltipLocation.Y + autoY;
            ctx.AutoPopPupPlacement = ctx.AreAllLessThanPivot ? PopUpPlacement.Bottom : PopUpPlacement.Top;
        }
        if (Y + Height > ctx.MostAutoBottom)
        {
            ctx.MostAutoBottom = SuggestedTooltipLocation.Y + autoY;
            ctx.AutoPopPupPlacement = ctx.AreAllLessThanPivot ? PopUpPlacement.Bottom : PopUpPlacement.Top;
        }
    }
}
