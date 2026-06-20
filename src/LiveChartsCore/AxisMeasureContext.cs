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

namespace LiveChartsCore;

/// <summary>
/// Bundle of per-Invalidate state shared between the orchestration on
/// <see cref="CoreAxis{TTextGeometry, TLineGeometry}"/> and the per-separator
/// helpers. Ref struct so passing by <c>in</c> costs nothing and the contents
/// cannot escape into a longer-lived closure.
/// </summary>
public readonly ref struct AxisMeasureContext
{
    /// <summary>Initializes a new instance of <see cref="AxisMeasureContext"/>.</summary>
    public AxisMeasureContext(
        CartesianChartEngine chart,
        Scaler scale,
        Scaler actualScale,
        Func<double, string> labeler,
        LvcPoint drawLocation,
        LvcSize drawMarginSize,
        LvcSize controlSize,
        float lxi, float lxj, float lyi, float lyj,
        float xoo, float yoo,
        float labelTextSize,
        float labelsRotation,
        bool hasRotation,
        bool hasActivePaint,
        double min, double max,
        double step, double start,
        float ticksXOffset, float ticksYOffset,
        float separatorsXOffset, float separatorsYOffset,
        AxisOrientation orientation)
    {
        Chart = chart;
        Scale = scale;
        ActualScale = actualScale;
        Labeler = labeler;
        DrawLocation = drawLocation;
        DrawMarginSize = drawMarginSize;
        ControlSize = controlSize;
        LeftX = lxi; RightX = lxj; TopY = lyi; BottomY = lyj;
        OffsetX = xoo; OffsetY = yoo;
        LabelTextSize = labelTextSize;
        LabelsRotation = labelsRotation;
        HasRotation = hasRotation;
        HasActivePaint = hasActivePaint;
        Min = min; Max = max;
        Step = step; Start = start;
        TicksXOffset = ticksXOffset; TicksYOffset = ticksYOffset;
        SeparatorsXOffset = separatorsXOffset; SeparatorsYOffset = separatorsYOffset;
        Orientation = orientation;
    }

    /// <summary>The chart engine.</summary>
    public CartesianChartEngine Chart { get; }
    /// <summary>Next-frame pixel scaler (target of any in-flight transition).</summary>
    public Scaler Scale { get; }
    /// <summary>Current-frame pixel scaler (source of any in-flight transition); falls back to <see cref="Scale"/> when no transition is active.</summary>
    public Scaler ActualScale { get; }
    /// <summary>Resolved labeler (honors <c>Labels</c> dictionary override before the user's <c>Labeler</c>).</summary>
    public Func<double, string> Labeler { get; }
    /// <summary>Top-left of the draw margin region.</summary>
    public LvcPoint DrawLocation { get; }
    /// <summary>Size of the draw margin region.</summary>
    public LvcSize DrawMarginSize { get; }
    /// <summary>Full control size (used to position the axis line at Start/End).</summary>
    public LvcSize ControlSize { get; }
    /// <summary>Left edge of the draw margin region (pixel).</summary>
    public float LeftX { get; }
    /// <summary>Right edge of the draw margin region (pixel).</summary>
    public float RightX { get; }
    /// <summary>Top edge of the draw margin region (pixel).</summary>
    public float TopY { get; }
    /// <summary>Bottom edge of the draw margin region (pixel).</summary>
    public float BottomY { get; }
    /// <summary>X offset of the axis line (zero for an X axis; resolved from Position + control size for a Y axis).</summary>
    public float OffsetX { get; }
    /// <summary>Y offset of the axis line (zero for a Y axis; resolved from Position + control size for an X axis).</summary>
    public float OffsetY { get; }
    /// <summary>Pre-cast label TextSize.</summary>
    public float LabelTextSize { get; }
    /// <summary>Pre-cast label rotation (in degrees).</summary>
    public float LabelsRotation { get; }
    /// <summary>True when <see cref="LabelsRotation"/> is non-negligible (|r| > 0.01).</summary>
    public bool HasRotation { get; }
    /// <summary>True when at least one paint (Name, Separators, Labels, Ticks, Subticks, Subseparators) is configured to draw — gates the membership of separators in the "measured" set.</summary>
    public bool HasActivePaint { get; }
    /// <summary>Effective axis minimum (MinLimit override or visible data bounds).</summary>
    public double Min { get; }
    /// <summary>Effective axis maximum (MaxLimit override or visible data bounds).</summary>
    public double Max { get; }
    /// <summary>Tick step (snapped to MinStep floor and ForceStepToMin where applicable).</summary>
    public double Step { get; }
    /// <summary>First separator value (largest multiple of <see cref="Step"/> not greater than <see cref="Min"/>).</summary>
    public double Start { get; }
    /// <summary>Half-unit-width X offset applied to tick positions when <c>TicksAtCenter</c> is false (X axis only).</summary>
    public float TicksXOffset { get; }
    /// <summary>Half-unit-width Y offset applied to tick positions when <c>TicksAtCenter</c> is false (Y axis only).</summary>
    public float TicksYOffset { get; }
    /// <summary>Half-unit-width X offset applied to separator positions when <c>SeparatorsAtCenter</c> is false (X axis only).</summary>
    public float SeparatorsXOffset { get; }
    /// <summary>Half-unit-width Y offset applied to separator positions when <c>SeparatorsAtCenter</c> is false (Y axis only).</summary>
    public float SeparatorsYOffset { get; }
    /// <summary>X or Y orientation of this axis.</summary>
    public AxisOrientation Orientation { get; }
}
