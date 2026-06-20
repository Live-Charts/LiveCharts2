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
/// orchestration on <see cref="CoreFinancialSeries{TModel, TVisual, TLabel, TMiniatureGeometry}"/>
/// and the per-point <c>MeasureFinancialLayout</c> hook.
/// </summary>
public readonly ref struct FinancialMeasureContext
{
    /// <summary>Initializes a new instance of <see cref="FinancialMeasureContext"/>.</summary>
    public FinancialMeasureContext(
        CartesianChartEngine chart,
        ICartesianAxis primaryAxis,
        ICartesianAxis secondaryAxis,
        Scaler primaryScale,
        Scaler secondaryScale,
        Scaler? previousPrimaryScale,
        Scaler? previousSecondaryScale,
        float candleWidth,
        float previousCandleWidth,
        float halfCandleWidth,
        float categoryWidth,
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
        CandleWidth = candleWidth;
        PreviousCandleWidth = previousCandleWidth;
        HalfCandleWidth = halfCandleWidth;
        CategoryWidth = categoryWidth;
        TooltipPosition = tooltipPosition;
        IsFirstDraw = isFirstDraw;
        DrawLocation = drawLocation;
        DrawMarginSize = drawMarginSize;
        DataLabelsSize = dataLabelsSize;
    }

    /// <summary>The chart engine.</summary>
    public CartesianChartEngine Chart { get; }
    /// <summary>Primary (Y) axis = value axis for financial.</summary>
    public ICartesianAxis PrimaryAxis { get; }
    /// <summary>Secondary (X) axis = time axis for financial.</summary>
    public ICartesianAxis SecondaryAxis { get; }
    /// <summary>Y-axis pixel scaler for the current frame.</summary>
    public Scaler PrimaryScale { get; }
    /// <summary>X-axis pixel scaler for the current frame.</summary>
    public Scaler SecondaryScale { get; }
    /// <summary>Y-axis pixel scaler for the previous frame; null on first draw.</summary>
    public Scaler? PreviousPrimaryScale { get; }
    /// <summary>X-axis pixel scaler for the previous frame; null on first draw.</summary>
    public Scaler? PreviousSecondaryScale { get; }
    /// <summary>Candle body width in pixels (clamped to MaxBarWidth).</summary>
    public float CandleWidth { get; }
    /// <summary>Previous-frame candle width (animation source).</summary>
    public float PreviousCandleWidth { get; }
    /// <summary>Half the candle width (precomputed centering offset).</summary>
    public float HalfCandleWidth { get; }
    /// <summary>Axis-unit slot width in pixels (unclamped by MaxBarWidth). Used for the category-wide hover area.</summary>
    public float CategoryWidth { get; }
    /// <summary>Tooltip anchor position resolved from the chart at the start of measure.</summary>
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
