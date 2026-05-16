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
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.Motion;
using LiveChartsCore.Painting;

namespace LiveChartsCore;

/// <summary>
/// Defines a visual section in a chart.
/// </summary>
/// <seealso cref="ChartElement" />
public abstract class CoreSection : ChartElement, INotifyPropertyChanged
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

    /// <summary>
    /// Gets or sets the label paint.
    /// </summary>
    /// <value>
    /// The fill.
    /// </value>
    public Paint? LabelPaint
    {
        get;
        set => SetPaintProperty(ref field, value, PaintStyle.Text);
    } = null;

    /// <summary>
    /// Gets or sets the xi, the value where the section starts at the X axis,
    /// set the property to null (or double.NaN) to indicate that the section must start at the beginning of the X axis, default is null.
    /// </summary>
    /// <value>
    /// The xi.
    /// </value>
    public double? Xi { get; set => SetProperty(ref field, filterNaN(value)); }

    /// <summary>
    /// Gets or sets the xj, the value where the section ends and the X axis.
    /// set the property to null (or double.NaN) to indicate that the section must go to the end of the X axis, default is null.
    /// </summary>
    /// <value>
    /// The xj.
    /// </value>
    public double? Xj { get; set => SetProperty(ref field, filterNaN(value)); }

    /// <summary>
    /// Gets or sets the yi, the value where the section starts and the Y axis.
    /// set the property to null (or double.NaN) to indicate that the section must start at the beginning of the Y axis, default is null.
    /// </summary>
    /// <value>
    /// The yi.
    /// </value>
    public double? Yi { get; set => SetProperty(ref field, filterNaN(value)); }

    /// <summary>
    /// Gets or sets the yj, the value where the section ends and the Y axis.
    /// set the property to null (or double.NaN) to indicate that the section must go to the end of the Y axis, default is null.
    /// </summary>
    /// <value>
    /// The yj.
    /// </value>
    public double? Yj { get; set => SetProperty(ref field, filterNaN(value)); }

    /// <summary>
    /// Gets or sets the axis index where the section is scaled in the X plane, the index must exist 
    /// in the <see cref="ICartesianChartView.XAxes"/> collection.
    /// </summary>
    /// <value>
    /// The index of the axis.
    /// </value>
    public int ScalesXAt { get; set => SetProperty(ref field, value); }

    /// <summary>
    /// Gets or sets the axis index where the section is scaled in the Y plane, the index must exist 
    /// in the <see cref="ICartesianChartView.YAxes"/> collection.
    /// </summary>
    /// <value>
    /// The index of the axis.
    /// </value>
    public int ScalesYAt { get; set => SetProperty(ref field, value); }

    /// <summary>
    /// Gets or sets the index of the z axis.
    /// </summary>
    /// <value>
    /// The index of the z.
    /// </value>
    public int? ZIndex { get; set => SetProperty(ref field, value); }

    /// <inheritdoc cref="ChartElement.GetPaintTasks"/>
    protected internal override Paint?[] GetPaintTasks() =>
        [Stroke, Fill, LabelPaint];

    /// <summary>
    /// Called when the fill changes.
    /// </summary>
    /// <param name="propertyName"></param>
    protected override void OnPaintChanged(string? propertyName)
    {
        base.OnPaintChanged(propertyName);
        OnPropertyChanged(propertyName);
    }

    private static double? filterNaN(double? value)
    {
        if (value is not null && double.IsNaN(value.Value))
            value = null;

        return value;
    }
}

/// <summary>
/// Defines a visual section in a chart.
/// </summary>
/// <typeparam name="TSizedGeometry">The type of the sized geometry.</typeparam>
/// <typeparam name="TLabelGeometry">The type of the label geometry.</typeparam>
/// <seealso cref="ChartElement" />
public abstract class CoreSection<TSizedGeometry, TLabelGeometry> : CoreSection
    where TSizedGeometry : BoundedDrawnGeometry, new()
    where TLabelGeometry : BaseLabelGeometry, new()
{
    private float _labelSize = 12;

    /// <summary>
    /// The fill sized geometry
    /// </summary>
    protected internal TSizedGeometry? _fillSizedGeometry;

    /// <summary>
    /// The stroke sized geometry
    /// </summary>
    protected internal TSizedGeometry? _strokeSizedGeometry;

    /// <summary>
    /// The label geometry.
    /// </summary>
    protected internal TLabelGeometry? _labelGeometry;

    /// <summary>
    /// Gets or sets the label, a string to be displayed within the section.
    /// </summary>
    public string Label { get; set => SetProperty(ref field, value); } = string.Empty;

    /// <summary>
    /// Gets or sets the label size.
    /// </summary>
    public double LabelSize { get => _labelSize; set => SetProperty(ref _labelSize, (float)value); }

    /// <summary>
    /// Measures the specified chart.
    /// </summary>
    /// <param name="chart">The chart.</param>
    public override void Invalidate(Chart chart)
    {
        var drawLocation = chart.DrawMarginLocation;
        var drawMarginSize = chart.DrawMarginSize;

        var cartesianChart = (CartesianChartEngine)chart;
        var primaryAxis = cartesianChart.GetYAxis(ScalesYAt);
        var secondaryAxis = cartesianChart.GetXAxis(ScalesXAt);

        var secondaryScale = new Scaler(drawLocation, drawMarginSize, secondaryAxis);
        var primaryScale = new Scaler(drawLocation, drawMarginSize, primaryAxis);

        var xi = Xi is null ? drawLocation.X : secondaryScale.ToPixels(Xi.Value);
        var xj = Xj is null ? drawLocation.X + drawMarginSize.Width : secondaryScale.ToPixels(Xj.Value);

        var yi = Yi is null ? drawLocation.Y : primaryScale.ToPixels(Yi.Value);
        var yj = Yj is null ? drawLocation.Y + drawMarginSize.Height : primaryScale.ToPixels(Yj.Value);

        if (Fill is not null)
        {
            Fill.ZIndex = ZIndex ?? PaintConstants.SectionFillZIndex;

            if (_fillSizedGeometry is null)
            {
                _fillSizedGeometry = new TSizedGeometry
                {
                    X = xi,
                    Y = yi,
                    Width = xj - xi,
                    Height = yj - yi
                };

                _fillSizedGeometry.Animate(cartesianChart);
            }

            _fillSizedGeometry.X = xi;
            _fillSizedGeometry.Y = yi;
            _fillSizedGeometry.Width = xj - xi;
            _fillSizedGeometry.Height = yj - yi;

            Fill.AddGeometryToPaintTask(chart.Canvas, _fillSizedGeometry);
            chart.Canvas.AddDrawableTask(Fill, zone: CanvasZone.DrawMargin);
        }

        if (Stroke is not null)
        {
            Stroke.ZIndex = ZIndex ?? PaintConstants.SectionStrokeZIndex;

            if (_strokeSizedGeometry is null)
            {
                _strokeSizedGeometry = new TSizedGeometry
                {
                    X = xi,
                    Y = yi,
                    Width = xj - xi,
                    Height = yj - yi
                };

                _strokeSizedGeometry.Animate(chart);
            }

            _strokeSizedGeometry.X = xi;
            _strokeSizedGeometry.Y = yi;
            _strokeSizedGeometry.Width = xj - xi;
            _strokeSizedGeometry.Height = yj - yi;

            Stroke.AddGeometryToPaintTask(chart.Canvas, _strokeSizedGeometry);
            chart.Canvas.AddDrawableTask(Stroke, zone: CanvasZone.DrawMargin);
        }

        if (LabelPaint is not null)
        {
            LabelPaint.ZIndex = ZIndex ?? PaintConstants.SectionLabelsZIndex;

            var xil = Xi is null ? drawLocation.X : secondaryScale.ToPixels(Xi.Value);
            var yil = Yi is null ? drawLocation.Y : primaryScale.ToPixels(Yi.Value);

            if (_labelGeometry is null)
            {
                _labelGeometry = new TLabelGeometry
                {
                    X = xi,
                    Y = yi,
                    Padding = new Padding(6)
                };

                _labelGeometry.Animate(
                    cartesianChart,
                    BaseLabelGeometry.XProperty, BaseLabelGeometry.YProperty);

                _labelGeometry.VerticalAlign = Align.Start;
                _labelGeometry.HorizontalAlign = Align.Start;
            }

            _labelGeometry.X = xi;
            _labelGeometry.Y = yi;
            _labelGeometry.Text = Label;
            _labelGeometry.TextSize = _labelSize;
            _labelGeometry.Paint = LabelPaint;

            LabelPaint.AddGeometryToPaintTask(chart.Canvas, _labelGeometry);
            chart.Canvas.AddDrawableTask(LabelPaint, zone: CanvasZone.DrawMargin);
        }
    }
}
