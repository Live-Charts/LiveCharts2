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
using System.ComponentModel;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.Themes;

namespace LiveChartsCore.VisualElements;

/// <summary>
/// Defines the base visual element class, inheriting from this class makes it easy to implement a visual element.
/// </summary>
public abstract class VisualElement : ChartElement, INotifyPropertyChanged, IInternalInteractable
{
    internal double _x;
    internal double _y;
    internal LvcPoint _translate = new();
    internal double _rotation;
    private Chart? _chart;

    /// <summary>
    /// Gets the primary scaler.
    /// </summary>
    protected Scaler? PrimaryScaler { get; private set; }

    /// <summary>
    /// Gets the secondary scaler.
    /// </summary>
    protected Scaler? SecondaryScaler { get; private set; }

    /// <summary>
    /// Gets or sets the X coordinate [in Pixels or ChartValues, see <see cref="LocationUnit"/>].
    /// </summary>
    public double X { get => _x; set => SetProperty(ref _x, value); }

    /// <summary>
    /// Gets or sets the Y coordinate [in Pixels or ChartValues, see <see cref="LocationUnit"/>].
    /// </summary>
    public double Y { get => _y; set => SetProperty(ref _y, value); }

    /// <summary>
    /// Gets or sets the rotation.
    /// </summary>
    public double Rotation { get => _rotation; set { _rotation = value; OnPropertyChanged(); } }

    /// <summary>
    /// Gets or sets the translate transform.
    /// </summary>
    public LvcPoint Translate { get => _translate; set => SetProperty(ref _translate, value); }

    /// <summary>
    /// Gets or sets the unit of the <see cref="X"/> and <see cref="Y"/> properties.
    /// </summary>
    public MeasureUnit LocationUnit { get; set => SetProperty(ref field, value); } = MeasureUnit.Pixels;

    /// <summary>
    /// Gets or sets the axis index where the series is scaled in the X plane, the index must exist 
    /// in the <see cref="ICartesianChartView.YAxes"/> collection.
    /// </summary>
    /// <value>
    /// The index of the axis.
    /// </value>
    public int ScalesXAt { get; set => SetProperty(ref field, value); }

    /// <summary>
    /// Gets or sets the axis index where the series is scaled in the Y plane, the index must exist 
    /// in the <see cref="ICartesianChartView.YAxes"/> collection.
    /// </summary>
    /// <value>
    /// The index of the axis.
    /// </value>
    public int ScalesYAt { get; set => SetProperty(ref field, value); }

    /// <summary>
    /// Gets or sets the z-index applied to every paint produced by this visual element.
    /// When 0 (default) the underlying paint z-index is left untouched, which means the
    /// visual is drawn behind series whose paints sit at <c>SeriesId + offset</c>.
    /// Set a positive value (e.g. 1000) to render the visual on top of series.
    /// </summary>
    public int ZIndex { get; set => SetProperty(ref field, value); }

    /// <inheritdoc cref="IInteractable.PointerDown"/>
    public event VisualElementHandler? PointerDown;

    /// <inheritdoc cref="ChartElement.Invalidate(Chart)"/>
    public override void Invalidate(Chart chart)
    {
        _chart = chart;

        if (chart is CartesianChartEngine cc)
        {
            var primaryAxis = cc.YAxes[ScalesYAt];
            var secondaryAxis = cc.XAxes[ScalesXAt];

            SecondaryScaler = secondaryAxis.GetNextScaler(cc);
            PrimaryScaler = primaryAxis.GetNextScaler(cc);
        }

        foreach (var paintTask in GetPaintTasks())
        {
            if (paintTask is null) continue;
            // 0 means "do not override"; gauge subclasses bump untouched paints in
            // OnInvalidated, so leaving 0-valued paints alone keeps that path working.
            if (ZIndex != 0) paintTask.ZIndex = ZIndex;
            chart.Canvas.AddDrawableTask(paintTask);
        }

        OnInvalidated(chart);
    }

    /// <summary>
    /// Measures the element and returns the size.
    /// </summary>
    /// <param name = "chart" > The chart.</param>
    public abstract LvcSize Measure(Chart chart);

    /// <summary>
    /// Called when [paint changed].
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns></returns>
    protected override void OnPaintChanged(string? propertyName)
    {
        base.OnPaintChanged(propertyName);
        OnPropertyChanged(propertyName);
    }

    /// <summary>
    /// Called when the visual is drawn.
    /// </summary>
    /// <param name="chart">The chart.</param>
    protected internal abstract void OnInvalidated(Chart chart);

    /// <summary>
    /// Sets the parent to all the geometries in the visual.
    /// </summary>
    protected internal abstract void SetParent(DrawnGeometry parent);

    /// <summary>
    /// Gets the acdtual coordinate of the visual.
    /// </summary>
    /// <returns></returns>
    protected LvcPoint GetActualCoordinate()
    {
        var x = (float)X;
        var y = (float)Y;

        if (LocationUnit == MeasureUnit.ChartValues)
        {
            if (PrimaryScaler is null || SecondaryScaler is null)
                throw new Exception($"You can not use {MeasureUnit.ChartValues} scale at this element.");

            x = SecondaryScaler.ToPixels(X);
            y = PrimaryScaler.ToPixels(Y);
        }

        return new(x, y);
    }

    /// <summary>
    /// Determines whether the given point hits visuals in the chart.
    /// </summary>
    /// <param name="chart">The chart.</param>
    /// <param name="point">The point in the UI.</param>
    /// <returns>The visual or visuals touched by the point.</returns>
    protected internal virtual IEnumerable<VisualElement> IsHitBy(Chart chart, LvcPoint point)
    {
        var location = GetActualCoordinate();

        // Note #241104
        // we only translate the location for now, scale and rotation are not supported yet.
        // do visual elements need to be that complex?
        // actually rotation is not working properly when labels are used.
        // this for sure needs a review.
        location.X += _translate.X;
        location.Y += _translate.Y;

        var size = Measure(chart);

        // it returns an enumerable because there are more complex types where a visual can contain more than one element
        if (point.X >= location.X && point.X <= location.X + size.Width &&
            point.Y >= location.Y && point.Y <= location.Y + size.Height)
        {
            yield return this;
        }
    }

    /// <inheritdoc cref="IInteractable.GetHitBox"/>
    public LvcRectangle GetHitBox()
    {
        if (_chart is null) return new();

        var location = GetActualCoordinate();
        var translatedLocation = new LvcPoint(location.X + _translate.X, location.Y + Translate.Y);
        var size = Measure(_chart);

        return new(translatedLocation, size);
    }

    /// <summary>
    /// Gets the geometries to draw.
    /// </summary>
    /// <returns>The geometries.</returns>
    protected internal abstract Animatable?[] GetDrawnGeometries();

    /// <summary>
    /// Applies the theme to the visual.
    /// </summary>
    /// <typeparam name="T">The type of the visual.</typeparam>
    /// <param name="theme">The theme.</param>
    protected virtual void ApplyTheme<T>(Theme theme)
        where T : VisualElement
    {
        _isInternalSet = true;
        if (_theme != theme.ThemeId)
        {
            theme.ApplyStyleTo<T>(this);
            _theme = theme.ThemeId;
        }
        _isInternalSet = false;
    }

    /// <inheritdoc cref="ChartElement.RemoveFromUI(Chart)"/>
    public override void RemoveFromUI(Chart chart)
    {
        base.RemoveFromUI(chart);
        _chart = null;
    }

    internal virtual void AlignToTopLeftCorner()
    {
        // just a workaround to align labels as the rest of the geometries.
    }

    void IInternalInteractable.InvokePointerDown(VisualElementEventArgs args) =>
        PointerDown?.Invoke(this, args);
}
