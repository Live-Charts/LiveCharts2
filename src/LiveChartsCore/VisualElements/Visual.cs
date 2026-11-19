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
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.Motion;
using LiveChartsCore.Painting;

namespace LiveChartsCore.VisualElements;

/// <summary>
/// Defines a visual in a chart.
/// </summary>
public abstract class Visual : ChartElement, IInternalInteractable
{
    private DrawnTask? _drawnTask;

    /// <inheritdoc cref="IInteractable.PointerDown"/>
    public event VisualElementHandler? PointerDown;

    /// <summary>
    /// Gets or sets the easing function.
    /// </summary>
    public Func<float, float>? Easing { get; set; } = EasingFunctions.EaseOut;

    /// <summary>
    /// Gets or sets the animation speed.
    /// </summary>
    public TimeSpan AnimationSpeed { get; set; } = TimeSpan.FromMilliseconds(300);

    /// <summary>
    /// Gets or sets the z-index of the visual. By default (0) and with a positive value the visual
    /// is drawn on top of the series; set a negative value to draw it behind the series, for
    /// example as a background. Visuals on the same side of the series are ordered by their z-index.
    /// </summary>
    public int ZIndex { get; set => SetProperty(ref field, value); }

    /// <summary>
    /// Gets the drawn element.
    /// </summary>
    protected internal abstract IDrawnElement? DrawnElement { get; }

    /// <inheritdoc cref="ChartElement.Invalidate(Chart)"/>
    public override void Invalidate(Chart chart)
    {
        if (DrawnElement is null)
            throw new Exception($"{nameof(DrawnElement)} can not be null.");

        Measure(chart);

        // The series draw in the DrawMargin zone and a zone's ZIndex only orders the paints in it,
        // so the side of the series a visual is on is decided by its zone: Underlay is drawn before
        // the series, NoClip after them (#2364).
        var zone = ZIndex < 0 ? CanvasZone.Underlay : CanvasZone.NoClip;

        if (_drawnTask is null || _drawnTask.IsEmpty)
        {
            if (DrawnElement is Animatable animatable)
                animatable.Animate(Easing, AnimationSpeed);

            _drawnTask = chart.Canvas.AddGeometry(zone, DrawnElement);
        }
        else
        {
            // the ZIndex can change at run time, to the other side of the series.
            chart.Canvas.MoveTaskToZone(_drawnTask, zone);
        }

        _drawnTask.ZIndex = ZIndex;
    }

    /// <inheritdoc cref="IInteractable.GetHitBox"/>
    public virtual LvcRectangle GetHitBox()
    {
        if (DrawnElement is null)
            throw new Exception($"{nameof(DrawnElement)} can not be null.");

        var location = new LvcPoint(DrawnElement.X, DrawnElement.Y);
        var translate = DrawnElement.TranslateTransform;

        return new LvcRectangle(
            new LvcPoint(location.X + translate.X, location.Y + translate.Y),
            DrawnElement.Measure());
    }

    void IInternalInteractable.InvokePointerDown(VisualElementEventArgs args) =>
        PointerDown?.Invoke(this, args);

    /// <inheritdoc cref="ChartElement.GetPaintTasks"/>
    protected internal override Paint?[] GetPaintTasks() => [_drawnTask];

    /// <summary>
    /// Called when the visual is invalidated to measure the visual.
    /// </summary>
    /// <param name="chart"></param>
    protected abstract void Measure(Chart chart);
}
