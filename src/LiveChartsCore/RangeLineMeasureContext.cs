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
/// orchestration on <see cref="CoreRangeLineSeries{TModel, TVisual, TLabel, TStrokePathGeometry, TBandPathGeometry}"/>
/// and the per-point hooks. Ref struct so passing it by <c>in</c> to hooks
/// costs nothing and the contents cannot escape into a longer-lived closure.
/// Range line has no stacker (ranges don't stack) and no pivot (the band
/// midpoint takes its place as the entry / exit baseline), so this context
/// omits both compared with <see cref="LineMeasureContext"/>.
/// </summary>
public readonly ref struct RangeLineMeasureContext
{
    /// <summary>Initializes a new instance of <see cref="RangeLineMeasureContext"/>.</summary>
    public RangeLineMeasureContext(
        CartesianChartEngine chart,
        ICartesianAxis primaryAxis,
        ICartesianAxis secondaryAxis,
        Scaler primaryScale,
        Scaler secondaryScale,
        LvcPoint drawLocation,
        LvcSize drawMarginSize,
        int actualZIndex,
        float unitWidthX,
        float geometrySize,
        float halfGeometrySize,
        float dataLabelsSize,
        bool isFirstDraw)
    {
        Chart = chart;
        PrimaryAxis = primaryAxis;
        SecondaryAxis = secondaryAxis;
        PrimaryScale = primaryScale;
        SecondaryScale = secondaryScale;
        DrawLocation = drawLocation;
        DrawMarginSize = drawMarginSize;
        ActualZIndex = actualZIndex;
        UnitWidthX = unitWidthX;
        GeometrySize = geometrySize;
        HalfGeometrySize = halfGeometrySize;
        DataLabelsSize = dataLabelsSize;
        IsFirstDraw = isFirstDraw;
    }

    /// <summary>The chart engine.</summary>
    public CartesianChartEngine Chart { get; }
    /// <summary>Primary (Y) axis.</summary>
    public ICartesianAxis PrimaryAxis { get; }
    /// <summary>Secondary (X) axis.</summary>
    public ICartesianAxis SecondaryAxis { get; }
    /// <summary>Primary (Y) pixel scaler.</summary>
    public Scaler PrimaryScale { get; }
    /// <summary>Secondary (X) pixel scaler.</summary>
    public Scaler SecondaryScale { get; }
    /// <summary>Top-left of the draw margin region in chart pixel coordinates.</summary>
    public LvcPoint DrawLocation { get; }
    /// <summary>Size of the draw margin region.</summary>
    public LvcSize DrawMarginSize { get; }
    /// <summary>Resolved Z-index for this series (honors user override).</summary>
    public int ActualZIndex { get; }
    /// <summary>Unit-width on the secondary axis, in pixels (clamped to GeometrySize floor so hover areas never go below marker size).</summary>
    public float UnitWidthX { get; }
    /// <summary>Configured geometry (marker) size in pixels.</summary>
    public float GeometrySize { get; }
    /// <summary>Half of <see cref="GeometrySize"/> (precomputed for marker centering).</summary>
    public float HalfGeometrySize { get; }
    /// <summary>Pre-cast data-label text size.</summary>
    public float DataLabelsSize { get; }
    /// <summary>True if this is the first draw of the series.</summary>
    public bool IsFirstDraw { get; }
}
