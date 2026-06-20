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
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;

namespace LiveChartsCore;

/// <summary>
/// Bundle of per-Invalidate state shared between the template-method
/// orchestration on <see cref="BarSeries{TModel, TVisual, TLabel}"/> and the
/// per-orientation <c>MeasureBarLayout</c> hook. Ref struct so passing it by
/// <c>in</c> to hooks costs nothing and the contents cannot accidentally escape
/// into a longer-lived closure.
/// </summary>
public readonly ref struct BarMeasureContext
{
    /// <summary>Initializes a new instance of <see cref="BarMeasureContext"/>.</summary>
    public BarMeasureContext(
        CartesianChartEngine chart,
        ICartesianAxis primaryAxis,
        ICartesianAxis secondaryAxis,
        Scaler primaryScale,
        Scaler secondaryScale,
        Scaler previousPrimaryScale,
        Scaler previousSecondaryScale,
        BarMeasureHelper helper,
        BarMeasureHelper previousHelper,
        StackPosition? stacker,
        bool isFirstDraw,
        bool hasSvg,
        LvcPoint drawLocation,
        LvcSize drawMarginSize,
        float rx,
        float ry,
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
        PreviousHelper = previousHelper;
        Stacker = stacker;
        IsFirstDraw = isFirstDraw;
        HasSvg = hasSvg;
        DrawLocation = drawLocation;
        DrawMarginSize = drawMarginSize;
        Rx = rx;
        Ry = ry;
        DataLabelsSize = dataLabelsSize;
    }

    /// <summary>The chart engine.</summary>
    public CartesianChartEngine Chart { get; }
    /// <summary>The axis carrying value (primary direction).</summary>
    public ICartesianAxis PrimaryAxis { get; }
    /// <summary>The axis carrying category (secondary direction).</summary>
    public ICartesianAxis SecondaryAxis { get; }
    /// <summary>Scaler for value-direction pixels in the current frame.</summary>
    public Scaler PrimaryScale { get; }
    /// <summary>Scaler for category-direction pixels in the current frame.</summary>
    public Scaler SecondaryScale { get; }
    /// <summary>Scaler for value-direction pixels in the previous frame (animation source).</summary>
    public Scaler PreviousPrimaryScale { get; }
    /// <summary>Scaler for category-direction pixels in the previous frame.</summary>
    public Scaler PreviousSecondaryScale { get; }
    /// <summary>Bar position primitives for the current frame.</summary>
    public BarMeasureHelper Helper { get; }
    /// <summary>Bar position primitives for the previous frame (animation source).</summary>
    public BarMeasureHelper PreviousHelper { get; }
    /// <summary>Stack-position context when the series is stacked; otherwise null.</summary>
    public StackPosition? Stacker { get; }
    /// <summary>True if this is the first draw of the series — controls animation source.</summary>
    public bool IsFirstDraw { get; }
    /// <summary>True if the visual carries a variable SVG path.</summary>
    public bool HasSvg { get; }
    /// <summary>Top-left of the draw margin region in chart pixel coordinates.</summary>
    public LvcPoint DrawLocation { get; }
    /// <summary>Size of the draw margin region.</summary>
    public LvcSize DrawMarginSize { get; }
    /// <summary>Pre-cast Rx corner radius.</summary>
    public float Rx { get; }
    /// <summary>Pre-cast Ry corner radius.</summary>
    public float Ry { get; }
    /// <summary>Pre-cast data-label text size.</summary>
    public float DataLabelsSize { get; }
}
