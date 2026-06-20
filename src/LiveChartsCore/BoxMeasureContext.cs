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
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;

namespace LiveChartsCore;

/// <summary>
/// Bundle of per-Invalidate state shared between the template-method
/// orchestration on <see cref="CoreBoxSeries{TModel, TVisual, TLabel, TMiniatureGeometry}"/>
/// and the per-point <c>MeasureBoxLayout</c> hook.
/// </summary>
public readonly ref struct BoxMeasureContext
{
    /// <summary>Initializes a new instance of <see cref="BoxMeasureContext"/>.</summary>
    public BoxMeasureContext(
        CartesianChartEngine chart,
        ICartesianAxis primaryAxis,
        ICartesianAxis secondaryAxis,
        Scaler primaryScale,
        Scaler secondaryScale,
        Scaler? previousPrimaryScale,
        Scaler? previousSecondaryScale,
        BoxMeasureHelper helper,
        float categoryUnitWidth,
        float previousCategoryUnitWidth,
        float halfCategoryUnitWidth,
        TooltipPosition tooltipPosition,
        bool isFirstDraw,
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
        Helper = helper;
        CategoryUnitWidth = categoryUnitWidth;
        PreviousCategoryUnitWidth = previousCategoryUnitWidth;
        HalfCategoryUnitWidth = halfCategoryUnitWidth;
        TooltipPosition = tooltipPosition;
        IsFirstDraw = isFirstDraw;
        DrawLocation = drawLocation;
        DrawMarginSize = drawMarginSize;
        DataLabelsSize = dataLabelsSize;
    }

    /// <summary>The chart engine.</summary>
    public CartesianChartEngine Chart { get; }
    /// <summary>Primary (Y) value axis.</summary>
    public ICartesianAxis PrimaryAxis { get; }
    /// <summary>Secondary (X) category axis.</summary>
    public ICartesianAxis SecondaryAxis { get; }
    /// <summary>Y-axis pixel scaler for the current frame.</summary>
    public Scaler PrimaryScale { get; }
    /// <summary>X-axis pixel scaler for the current frame.</summary>
    public Scaler SecondaryScale { get; }
    /// <summary>Y-axis pixel scaler for the previous frame; null on first draw.</summary>
    public Scaler? PreviousPrimaryScale { get; }
    /// <summary>X-axis pixel scaler for the previous frame; null on first draw.</summary>
    public Scaler? PreviousSecondaryScale { get; }
    /// <summary>Per-series box position primitives (uw, uwm, cp, p, actualUw).</summary>
    public BoxMeasureHelper Helper { get; }
    /// <summary>Raw axis unit-width in pixels (pre-MaxBarWidth clamp).</summary>
    public float CategoryUnitWidth { get; }
    /// <summary>Previous-frame raw axis unit-width.</summary>
    public float PreviousCategoryUnitWidth { get; }
    /// <summary>Half the raw axis unit-width.</summary>
    public float HalfCategoryUnitWidth { get; }
    /// <summary>Tooltip anchor position resolved from the chart.</summary>
    public TooltipPosition TooltipPosition { get; }
    /// <summary>True if this is the first draw of the series.</summary>
    public bool IsFirstDraw { get; }
    /// <summary>Top-left of the draw margin region in chart pixel coordinates.</summary>
    public LvcPoint DrawLocation { get; }
    /// <summary>Size of the draw margin region.</summary>
    public LvcSize DrawMarginSize { get; }
    /// <summary>Pre-cast data-label text size.</summary>
    public float DataLabelsSize { get; }
}
