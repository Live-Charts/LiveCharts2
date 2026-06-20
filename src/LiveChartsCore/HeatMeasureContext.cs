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
using System.Collections.Generic;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;

namespace LiveChartsCore;

/// <summary>
/// Bundle of per-Invalidate state shared between the template-method orchestration
/// on <see cref="CoreHeatSeries{TModel, TVisual, TLabel}"/> and the per-cell
/// <c>MeasureHeatLayout</c> hook. Ref struct so passing it by <c>in</c> to hooks
/// costs nothing.
/// </summary>
public readonly ref struct HeatMeasureContext
{
    /// <summary>Initializes a new instance of <see cref="HeatMeasureContext"/>.</summary>
    public HeatMeasureContext(
        CartesianChartEngine chart,
        ICartesianAxis primaryAxis,
        ICartesianAxis secondaryAxis,
        Scaler primaryScale,
        Scaler secondaryScale,
        Scaler? previousPrimaryScale,
        Scaler? previousSecondaryScale,
        float cellWidth,
        float cellHeight,
        Padding pointPadding,
        Bounds weightBounds,
        LvcColor[] heatMap,
        List<Tuple<double, LvcColor>> heatStops,
        bool isFirstDraw,
        bool hasSvg,
        LvcPoint drawLocation,
        LvcSize drawMarginSize,
        float dataLabelsSize)
    {
        Chart = chart;
        PrimaryAxis = primaryAxis;
        SecondaryAxis = secondaryAxis;
        PrimaryScale = primaryScale;
        SecondaryScale = secondaryScale;
        PreviousPrimaryScale = previousPrimaryScale;
        PreviousSecondaryScale = previousSecondaryScale;
        CellWidth = cellWidth;
        CellHeight = cellHeight;
        PointPadding = pointPadding;
        WeightBounds = weightBounds;
        HeatMap = heatMap;
        HeatStops = heatStops;
        IsFirstDraw = isFirstDraw;
        HasSvg = hasSvg;
        DrawLocation = drawLocation;
        DrawMarginSize = drawMarginSize;
        DataLabelsSize = dataLabelsSize;
    }

    /// <summary>The chart engine.</summary>
    public CartesianChartEngine Chart { get; }
    /// <summary>Primary (Y) axis.</summary>
    public ICartesianAxis PrimaryAxis { get; }
    /// <summary>Secondary (X) axis.</summary>
    public ICartesianAxis SecondaryAxis { get; }
    /// <summary>Y-axis pixel scaler for the current frame.</summary>
    public Scaler PrimaryScale { get; }
    /// <summary>X-axis pixel scaler for the current frame.</summary>
    public Scaler SecondaryScale { get; }
    /// <summary>Y-axis pixel scaler for the previous frame; null on first draw.</summary>
    public Scaler? PreviousPrimaryScale { get; }
    /// <summary>X-axis pixel scaler for the previous frame; null on first draw.</summary>
    public Scaler? PreviousSecondaryScale { get; }
    /// <summary>Heat cell width in pixels (data-step-driven; see issue #1511).</summary>
    public float CellWidth { get; }
    /// <summary>Heat cell height in pixels.</summary>
    public float CellHeight { get; }
    /// <summary>Per-cell padding applied inside the grid square.</summary>
    public Padding PointPadding { get; }
    /// <summary>Weight bounds across the series (Min..Max of TertiaryValue).</summary>
    public Bounds WeightBounds { get; }
    /// <summary>Configured gradient stops (cold .. hot).</summary>
    public LvcColor[] HeatMap { get; }
    /// <summary>Pre-built gradient stop list for HeatFunctions.InterpolateColor.</summary>
    public List<Tuple<double, LvcColor>> HeatStops { get; }
    /// <summary>True if this is the first draw of the series.</summary>
    public bool IsFirstDraw { get; }
    /// <summary>True if the visual carries a variable SVG path.</summary>
    public bool HasSvg { get; }
    /// <summary>Top-left of the draw margin region in chart pixel coordinates.</summary>
    public LvcPoint DrawLocation { get; }
    /// <summary>Size of the draw margin region.</summary>
    public LvcSize DrawMarginSize { get; }
    /// <summary>Pre-cast data-label text size.</summary>
    public float DataLabelsSize { get; }
}
