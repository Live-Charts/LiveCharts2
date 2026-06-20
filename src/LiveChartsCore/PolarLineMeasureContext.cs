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

using LiveChartsCore.Measure;

namespace LiveChartsCore;

/// <summary>
/// Bundle of per-Invalidate state shared between the template-method
/// orchestration on <see cref="CorePolarLineSeries{TModel, TVisual, TLabel, TPathGeometry, TLineGeometry}"/>
/// and the per-point hooks. Polar series collapse the (drawLocation, drawMarginSize,
/// angleAxis, radiusAxis, innerRadius, initialRotation, totalAngle) tuple into a
/// single <see cref="PolarScaler"/>, so the context is slimmer than its
/// cartesian counterpart. Ref struct so the contents cannot escape into a
/// longer-lived closure.
/// </summary>
public readonly ref struct PolarLineMeasureContext
{
    /// <summary>Initializes a new instance of <see cref="PolarLineMeasureContext"/>.</summary>
    public PolarLineMeasureContext(
        PolarChartEngine chart,
        PolarScaler scaler,
        int actualZIndex,
        float geometrySize,
        float halfGeometrySize,
        float dataLabelsSize,
        float baseLabelRotation,
        bool isTangent,
        bool isCotangent,
        bool isFirstDraw,
        bool hasSvg,
        StackPosition? stacker)
    {
        Chart = chart;
        Scaler = scaler;
        ActualZIndex = actualZIndex;
        GeometrySize = geometrySize;
        HalfGeometrySize = halfGeometrySize;
        DataLabelsSize = dataLabelsSize;
        BaseLabelRotation = baseLabelRotation;
        IsTangent = isTangent;
        IsCotangent = isCotangent;
        IsFirstDraw = isFirstDraw;
        HasSvg = hasSvg;
        Stacker = stacker;
    }

    /// <summary>The polar chart engine.</summary>
    public PolarChartEngine Chart { get; }
    /// <summary>The polar pixel scaler (combines draw region, angle/radius axes, inner radius, initial rotation, and sweep).</summary>
    public PolarScaler Scaler { get; }
    /// <summary>Resolved Z-index for this series (honors user override and stacked-area anchor from issue #1923).</summary>
    public int ActualZIndex { get; }
    /// <summary>Configured geometry (marker) size in pixels.</summary>
    public float GeometrySize { get; }
    /// <summary>Half of <see cref="GeometrySize"/>.</summary>
    public float HalfGeometrySize { get; }
    /// <summary>Pre-cast data-label text size.</summary>
    public float DataLabelsSize { get; }
    /// <summary>DataLabelsRotation with the LiveCharts.TangentAngle / CotangentAngle flag bits stripped.</summary>
    public float BaseLabelRotation { get; }
    /// <summary>True when LiveCharts.TangentAngle was set on DataLabelsRotation; label rotation adds (angle − 90).</summary>
    public bool IsTangent { get; }
    /// <summary>True when LiveCharts.CotangentAngle was set on DataLabelsRotation; label rotation adds the angle.</summary>
    public bool IsCotangent { get; }
    /// <summary>True if this is the first draw of the series.</summary>
    public bool IsFirstDraw { get; }
    /// <summary>True if the marker visual carries a variable SVG path.</summary>
    public bool HasSvg { get; }
    /// <summary>Stacker for this series, or null when the series is not stacked.</summary>
    public StackPosition? Stacker { get; }
}
