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

using System.ComponentModel;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Motion;
using LiveChartsCore.Painting;

namespace LiveChartsCore;

/// <summary>
/// Defines a draw margin frame visual in a chart.
/// </summary>
public abstract class CoreDrawMarginFrame : ChartElement, INotifyPropertyChanged
{

    /// <summary>
    /// Gets or sets the stroke.
    /// </summary>
    /// <value>
    /// The stroke.
    /// </value>
    public Paint? Stroke
    {
        get;
        set => SetPaintProperty(ref field, value, PaintStyle.Stroke);
    } = null;

    /// <summary>
    /// Gets or sets the fill.
    /// </summary>
    /// <value>
    /// The fill.
    /// </value>
    public Paint? Fill
    {
        get;
        set => SetPaintProperty(ref field, value);
    } = null;

    /// <inheritdoc cref="ChartElement.GetPaintTasks"/>
    protected internal override Paint?[] GetPaintTasks() =>
        [Stroke, Fill];

    /// <summary>
    /// Called when the fill changes.
    /// </summary>
    /// <param name="propertyName"></param>
    protected override void OnPaintChanged(string? propertyName)
    {
        base.OnPaintChanged(propertyName);
        OnPropertyChanged(propertyName);
    }
}

/// <summary>
/// Defines a draw margin frame visual in a chart.
/// </summary>
/// <typeparam name="TSizedGeometry">The type of the sized geometry.</typeparam>
public abstract class CoreDrawMarginFrame<TSizedGeometry> : CoreDrawMarginFrame
    where TSizedGeometry : BoundedDrawnGeometry, new()
{
    private TSizedGeometry? _fillSizedGeometry;
    private TSizedGeometry? _strokeSizedGeometry;
    private bool _isInitialized = false;

    /// <summary>
    /// Measures the specified chart.
    /// </summary>
    /// <param name="chart">The chart.</param>
    public override void Invalidate(Chart chart)
    {
        var drawLocation = chart.DrawMarginLocation;
        var drawMarginSize = chart.DrawMarginSize;

        if (Fill is not null)
        {
            if (Fill.ZIndex == 0) Fill.ZIndex = PaintConstants.DrawMarginFrameFillZIndex;

            _fillSizedGeometry ??= new TSizedGeometry();

            _fillSizedGeometry.X = drawLocation.X;
            _fillSizedGeometry.Y = drawLocation.Y;
            _fillSizedGeometry.Width = drawMarginSize.Width;
            _fillSizedGeometry.Height = drawMarginSize.Height;

            Fill.AddGeometryToPaintTask(chart.Canvas, _fillSizedGeometry);
            chart.Canvas.AddDrawableTask(Fill, zone: CanvasZone.DrawMargin);
        }

        if (Stroke is not null)
        {
            if (Stroke.ZIndex == 0) Stroke.ZIndex = PaintConstants.DrawMarginFrameStrokeZIndex;

            _strokeSizedGeometry ??= new TSizedGeometry();

            _strokeSizedGeometry.X = drawLocation.X;
            _strokeSizedGeometry.Y = drawLocation.Y;
            _strokeSizedGeometry.Width = drawMarginSize.Width;
            _strokeSizedGeometry.Height = drawMarginSize.Height;

            Stroke.AddGeometryToPaintTask(chart.Canvas, _strokeSizedGeometry);
            chart.Canvas.AddDrawableTask(Stroke, zone: CanvasZone.NoClip);
        }

        if (!_isInitialized)
        {
            _fillSizedGeometry?.Animate(chart);
            _strokeSizedGeometry?.Animate(chart);
            _isInitialized = true;
        }
    }
}
