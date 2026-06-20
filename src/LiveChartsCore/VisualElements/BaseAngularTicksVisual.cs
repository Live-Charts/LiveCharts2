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
using System.Linq;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Painting;

namespace LiveChartsCore.VisualElements;

/// <summary>
/// Defines a visual element in a chart that draws a sized geometry in the user interface.
/// </summary>
public abstract class BaseAngularTicksVisual : VisualElement
{
    /// <summary>
    /// Gets or sets the labels paint.
    /// </summary>
    public Paint? LabelsPaint
    {
        get;
        set => SetPaintProperty(ref field, value, PaintStyle.Text);
    }

    /// <summary>
    /// Gets or sets the fill paint.
    /// </summary>
    public Paint? Stroke
    {
        get;
        set => SetPaintProperty(ref field, value, PaintStyle.Stroke);
    }

    /// <summary>
    /// Gets or sets the outer offset, the distance between the  edge of the chart and the arc and ticks.
    /// </summary>
    public double OuterOffset { get; set => SetProperty(ref field, value); }

    /// <summary>
    /// Gets or sets the labels outer offset, the distance between the edge of the chart and the labels.
    /// </summary>
    public double LabelsOuterOffset { get; set => SetProperty(ref field, value); }

    /// <summary>
    /// Gets or sets the ticks lenght.
    /// </summary>
    public double TicksLength { get; set => SetProperty(ref field, value); }

    /// <summary>
    /// Gets or sets the labels size.
    /// </summary>
    public double LabelsSize { get; set => SetProperty(ref field, value); } = 12;

    /// <summary>
    /// Gets or sets the labeler, a function that receives a number and return the label content as string.
    /// </summary>
    public Func<double, string> Labeler { get; set => SetProperty(ref field, value); } = Labelers.Default;
}

/// <summary>
/// Defines a visual element in a chart that draws a sized geometry in the user interface.
/// </summary>
/// <typeparam name="TArcGeometry">The type of the arc geometry.</typeparam>
/// <typeparam name="TLineGeometry">The type of the line geometry.</typeparam>
/// <typeparam name="TLabelGeometry">The type of the label.</typeparam>
public abstract class BaseAngularTicksVisual<TArcGeometry, TLineGeometry, TLabelGeometry> : BaseAngularTicksVisual
    where TArcGeometry : BaseArcGeometry, new()
    where TLineGeometry : BaseLineGeometry, new()
    where TLabelGeometry : BaseLabelGeometry, new()
{
    private readonly int _subSections = 5;
    private readonly Dictionary<string, TickVisual> _visuals = [];
    private TArcGeometry? _arc;

    /// <inheritdoc cref="VisualElement.OnInvalidated(Chart)"/>
    protected internal override void OnInvalidated(Chart chart)
    {
        if (chart is not PieChartEngine pieChart)
            throw new Exception("The AngularThicksVisual can only be added to a pie chart");

        ApplyTheme<BaseAngularTicksVisual>(chart.GetTheme());

        var drawLocation = pieChart.DrawMarginLocation;
        var drawMarginSize = pieChart.DrawMarginSize;

        var minDimension = drawMarginSize.Width < drawMarginSize.Height
            ? drawMarginSize.Width
            : drawMarginSize.Height;

        var view = (IPieChartView)pieChart.View;
        var initialRotation = (float)Math.Truncate(view.InitialRotation);
        var completeAngle = (float)view.MaxAngle;

        var startValue = view.MinValue;
        var endValue = view.MaxValue;

        var cx = drawLocation.X + drawMarginSize.Width * 0.5f;
        var cy = drawLocation.Y + drawMarginSize.Height * 0.5f;

        var h = minDimension;

        var outerRadius = h * 0.5f;
        var ticksDiameter = h - (float)OuterOffset;
        var ticksRadius = ticksDiameter * 0.5f;
        var innerRadius = ticksDiameter * 0.5f - (float)TicksLength;
        var subtickInnerRadius = ticksDiameter * 0.5f - (float)TicksLength * .5f;
        var labelsRadius = outerRadius - (float)LabelsOuterOffset;

        var sweep = completeAngle - 0.1f;

        _arc ??= new();

        _arc.CenterX = cx;
        _arc.CenterY = cy;
        _arc.X = drawLocation.X + (drawMarginSize.Width - ticksDiameter) * 0.5f;
        _arc.Y = drawLocation.Y + (drawMarginSize.Height - ticksDiameter) * 0.5f;
        _arc.Width = ticksDiameter;
        _arc.Height = ticksDiameter;
        _arc.StartAngle = initialRotation;
        _arc.SweepAngle = sweep;

        var max = endValue;
        var min = startValue;

        var range = max - min;
        if (range == 0) range = min;

        var separations = 10;
        var minimum = range / separations;

        var magnitude = Math.Pow(10, Math.Floor(Math.Log(minimum) / Math.Log(10)));

        var residual = minimum / magnitude;
        var tick = residual > 5
            ? 10 * magnitude :
            residual > 2
                ? 5 * magnitude
                : residual > 1
                    ? 2 * magnitude
                    : magnitude;

        var labelsSize = (float)LabelsSize;

        var updateId = new object();
        const double toRadians = Math.PI / 180d;

        for (var i = Math.Truncate(min / tick) * tick - tick; i <= max; i += tick)
        {
            var beta = (i - min) / range * sweep;
            beta += initialRotation;
            beta *= toRadians;

            var nextBeta = (i - min + tick) / range * sweep;
            nextBeta += initialRotation;
            nextBeta *= toRadians;

            if (!_visuals.TryGetValue(i.ToString(), out var visual))
            {
                visual = new TickVisual(new(), new(), new TLineGeometry[_subSections]);
                _visuals[i.ToString()] = visual;
            }

            visual.Tick.X = cx + (float)Math.Cos(beta) * innerRadius;
            visual.Tick.Y = cy + (float)Math.Sin(beta) * innerRadius;
            visual.Tick.X1 = cx + (float)Math.Cos(beta) * ticksRadius;
            visual.Tick.Y1 = cy + (float)Math.Sin(beta) * ticksRadius;

            visual.Label.Text = Labeler(i);
            visual.Label.X = cx + (float)Math.Cos(beta) * labelsRadius;
            visual.Label.Y = cy + (float)Math.Sin(beta) * labelsRadius;
            visual.Label.TextSize = labelsSize;
            visual.Label.Paint = LabelsPaint;

            if (i + tick <= max)
            {
                for (var j = 0; j < visual.Subseparator.Length - 1; j++)
                {
                    var subtick = visual.Subseparator[j];
                    subtick ??= visual.Subseparator[j] = new();

                    var alpha = beta + (nextBeta - beta) * (j + 1) / visual.Subseparator.Length;

                    subtick.X = cx + (float)Math.Cos(alpha) * ticksRadius;
                    subtick.Y = cy + (float)Math.Sin(alpha) * ticksRadius;
                    subtick.X1 = cx + (float)Math.Cos(alpha) * subtickInnerRadius;
                    subtick.Y1 = cy + (float)Math.Sin(alpha) * subtickInnerRadius;

                    Stroke?.AddGeometryToPaintTask(chart.Canvas, subtick);
                    subtick.Opacity = i + tick * (j + 1) / visual.Subseparator.Length >= min ? 1 : 0;
                }
            }

            LabelsPaint?.AddGeometryToPaintTask(chart.Canvas, visual.Label);
            Stroke?.AddGeometryToPaintTask(chart.Canvas, visual.Tick);

            var opacity = i >= min ? 1 : 0;
            visual.Label.Opacity = opacity;
            visual.Tick.Opacity = opacity;

            visual.UpdateId = updateId;
        }

        if (Stroke is not null)
        {
            Stroke.ZIndex = Stroke.ZIndex == 0 ? PaintConstants.AngularTicksStrokeZIndex : Stroke.ZIndex;
            Stroke.AddGeometryToPaintTask(chart.Canvas, _arc);
            pieChart.Canvas.AddDrawableTask(Stroke);
        }

        if (LabelsPaint is not null)
        {
            LabelsPaint.ZIndex = LabelsPaint.ZIndex == 0 ? PaintConstants.AngularTicksLabelsZIndex : LabelsPaint.ZIndex;
            pieChart.Canvas.AddDrawableTask(LabelsPaint);
        }

        foreach (var key in _visuals.Keys.ToArray())
        {
            var visual = _visuals[key];
            if (visual.UpdateId == updateId) continue;

            LabelsPaint?.RemoveGeometryFromPaintTask(chart.Canvas, visual.Label);
            Stroke?.RemoveGeometryFromPaintTask(chart.Canvas, visual.Tick);
            foreach (var subtick in visual.Subseparator)
            {
                if (subtick is null) continue;
                Stroke?.RemoveGeometryFromPaintTask(chart.Canvas, subtick);
            }
            _ = _visuals.Remove(key);
        }
    }

    /// <inheritdoc cref="VisualElement.Measure(Chart)"/>
    public override LvcSize Measure(Chart chart) => new();

    /// <inheritdoc cref="VisualElement.SetParent(DrawnGeometry)"/>
    protected internal override void SetParent(DrawnGeometry parent)
    { }

    /// <inheritdoc cref="VisualElement.GetDrawnGeometries"/>
    protected internal override Animatable?[] GetDrawnGeometries()
    {
        var count =
            _visuals.Count +                    // the ticks
            _visuals.Count +                    // the labels
            _subSections * _visuals.Count +     // the subticks
            1;                                  // the arc

        var l = new Animatable?[count];

        var i = 0;
        foreach (var visual in _visuals.Values)
        {
            l[i++] = visual.Tick;
            l[i++] = visual.Label;
            foreach (var subtick in visual.Subseparator)
                l[i++] = subtick;
        }

        l[i++] = _arc;

        return l;
    }

    /// <inheritdoc cref="ChartElement.GetPaintTasks"/>
    protected internal override Paint?[] GetPaintTasks() => [Stroke, LabelsPaint];

    private class TickVisual(TLabelGeometry label, TLineGeometry line, TLineGeometry[] subseparator)
    {
        public TLabelGeometry Label { get; set; } = label;
        public TLineGeometry Tick { get; set; } = line;
        public TLineGeometry[] Subseparator { get; set; } = subseparator;
        public object UpdateId { get; set; } = new();
    }
}
