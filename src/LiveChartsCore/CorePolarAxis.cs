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
using System.Linq;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Helpers;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.Motion;
using LiveChartsCore.Painting;

namespace LiveChartsCore;

/// <summary>
/// Defines an Axis in a Cartesian chart.
/// </summary>
/// <typeparam name="TTextGeometry">The type of the text geometry.</typeparam>
/// <typeparam name="TCircleGeometry">The type of the circle geometry.</typeparam>
/// /// <typeparam name="TLineGeometry">The type of the line geometry.</typeparam>
public abstract class CorePolarAxis<TTextGeometry, TLineGeometry, TCircleGeometry>
    : ChartElement, IPolarAxis, IPlane, INotifyPropertyChanged
        where TTextGeometry : BaseLabelGeometry, new()
        where TLineGeometry : BaseLineGeometry, new()
        where TCircleGeometry : BoundedDrawnGeometry, new()
{
    #region fields

    /// <summary>
    /// The active separators
    /// </summary>
    protected readonly Dictionary<Chart, Dictionary<double, IVisualSeparator>> activeSeparators = [];

    internal PolarAxisOrientation _orientation;
    private double _minStep = 0;
    private Bounds? _dataBounds = null;
    private Bounds? _visibleDataBounds = null;
    private LvcColor _labelsBackground = new(255, 255, 255);

    // Shared Animation reference; mutated in-place so AnimationsSpeed/EasingFunction changes
    // reach every existing geometry without recreating it.
    private readonly Animation _animation = new(EasingFunctions.QuadraticOut, TimeSpan.Zero);

    #endregion

    #region properties

    float IPolarAxis.Ro { get; set; }

    Bounds IPlane.DataBounds => _dataBounds ?? throw new Exception("bounds not found");

    Bounds IPlane.VisibleDataBounds => _visibleDataBounds ?? throw new Exception("bounds not found");
    double IPlane.MotionMinLimit => 0;
    double IPlane.MotionMaxLimit => 0;

    /// <inheritdoc cref="IPlane.Name"/>
    public string? Name { get; set; } = null;

    /// <inheritdoc cref="IPlane.NameTextSize"/>
    public double NameTextSize { get; set => SetProperty(ref field, value); } = 20;

    /// <inheritdoc cref="IPlane.NamePadding"/>
    public Padding NamePadding { get; set => SetProperty(ref field, value); } = new(5);

    /// <inheritdoc cref="IPolarAxis.Orientation"/>
    public PolarAxisOrientation Orientation => _orientation;

    /// <inheritdoc cref="IPolarAxis.LabelsAngle"/>
    public double LabelsAngle { get; set => SetProperty(ref field, value); }

    /// <inheritdoc cref="IPlane.Labeler"/>
    public Func<double, string> Labeler { get; set => SetProperty(ref field, value); } = Labelers.Default;

    /// <inheritdoc cref="IPlane.MinStep"/>
    public double MinStep { get => _minStep; set => SetProperty(ref _minStep, value); }

    /// <inheritdoc cref="IPlane.ForceStepToMin"/>
    public bool ForceStepToMin { get; set => SetProperty(ref field, value); }

    /// <inheritdoc cref="IPlane.MinSeparators"/>
    public int MinSeparators { get; set => SetProperty(ref field, value); } = 3;

    /// <inheritdoc cref="IPlane.MinLimit"/>
    public double? MinLimit { get; set => SetProperty(ref field, value); } = null;

    /// <inheritdoc cref="IPlane.MaxLimit"/>
    public double? MaxLimit { get; set => SetProperty(ref field, value); } = null;

    /// <inheritdoc cref="IPlane.UnitWidth"/>
    public double UnitWidth { get; set => SetProperty(ref field, value); } = 1;

    /// <inheritdoc cref="IPlane.LabelsRotation"/>
    public double LabelsRotation { get; set => SetProperty(ref field, value); }

    /// <inheritdoc cref="IPlane.TextSize"/>
    public double TextSize { get; set => SetProperty(ref field, value); } = 16;

    /// <inheritdoc cref="IPlane.Labels"/>
    public IList<string>? Labels { get; set; }

    /// <inheritdoc cref="IPolarAxis.LabelsPadding"/>
    public Padding LabelsPadding { get; set => SetProperty(ref field, value); } = new(3);

    /// <inheritdoc cref="IPolarAxis.LabelsVerticalAlignment"/>
    public Align LabelsVerticalAlignment { get; set => SetProperty(ref field, value); } = Align.Middle;

    /// <inheritdoc cref="IPolarAxis.LabelsHorizontalAlignment"/>
    public Align LabelsHorizontalAlignment { get; set => SetProperty(ref field, value); } = Align.Middle;

    /// <inheritdoc cref="IPolarAxis.LabelsBackground"/>
    public LvcColor LabelsBackground { get => _labelsBackground; set => SetProperty(ref _labelsBackground, value); }

    /// <inheritdoc cref="IPlane.ShowSeparatorLines"/>
    public bool ShowSeparatorLines { get; set => SetProperty(ref field, value); } = true;

    /// <inheritdoc cref="IPlane.CustomSeparators"/>
    public IEnumerable<double>? CustomSeparators { get; set => SetProperty(ref field, value); }

    /// <inheritdoc cref="IPlane.IsInverted"/>
    public bool IsInverted { get; set => SetProperty(ref field, value); }

    /// <inheritdoc cref="IPlane.NamePaint"/>
    public Paint? NamePaint
    {
        get;
        set => SetPaintProperty(ref field, value, PaintStyle.Text);
    }

    /// <inheritdoc cref="IPlane.LabelsPaint"/>
    public Paint? LabelsPaint
    {
        get;
        set => SetPaintProperty(ref field, value, PaintStyle.Text);
    }

    /// <inheritdoc cref="IPlane.SeparatorsPaint"/>
    public Paint? SeparatorsPaint
    {
        get;
        set => SetPaintProperty(ref field, value, PaintStyle.Stroke);
    }

    /// <inheritdoc cref="IPlane.AnimationsSpeed"/>
    public TimeSpan? AnimationsSpeed { get; set; }

    /// <inheritdoc cref="IPlane.EasingFunction"/>
    public Func<float, float>? EasingFunction { get; set; }

    #endregion

    /// <inheritdoc cref="IPolarAxis.Initialized"/>
    public event Action<IPolarAxis>? Initialized;

    /// <inheritdoc cref="ChartElement.Invalidate(Chart)"/>
    public override void Invalidate(Chart chart)
    {
        var polarChart = (PolarChartEngine)chart;
        _ = GetAnimation(polarChart);

        if (_dataBounds is null) throw new Exception("DataBounds not found");

        var controlSize = polarChart.ControlSize;
        var drawLocation = polarChart.DrawMarginLocation;
        var drawMarginSize = polarChart.DrawMarginSize;

        var axisTick = this.GetTick(polarChart);

        var labeler = GetActualLabeler();

        var s = axisTick.Value;
        if (s < _minStep) s = _minStep;
        if (ForceStepToMin) s = _minStep;

        if (NamePaint is not null && NamePaint != Paint.Default)
        {
            if (NamePaint.ZIndex == 0) NamePaint.ZIndex = PaintConstants.AxisNamePaintZIndex;
            polarChart.Canvas.AddDrawableTask(NamePaint, zone: CanvasZone.DrawMargin);
        }
        if (LabelsPaint is not null && LabelsPaint != Paint.Default)
        {
            if (LabelsPaint.ZIndex == 0) LabelsPaint.ZIndex = PaintConstants.AxisLabelsPaintZIndex;
            polarChart.Canvas.AddDrawableTask(LabelsPaint, zone: CanvasZone.DrawMargin);
        }
        if (SeparatorsPaint is not null && SeparatorsPaint != Paint.Default)
        {
            if (SeparatorsPaint.ZIndex == 0) SeparatorsPaint.ZIndex = PaintConstants.AxisSeparatorsPaintZIndex;
            polarChart.Canvas.AddDrawableTask(SeparatorsPaint, zone: CanvasZone.DrawMargin);
        }

        IPolarAxis a, b;

        if (_orientation == PolarAxisOrientation.Angle)
        {
            a = this;
            b = polarChart.RadiusAxes[0];
        }
        else
        {
            a = polarChart.AngleAxes[0];
            b = this;
        }

        var scaler = new PolarScaler(
            polarChart.DrawMarginLocation, polarChart.DrawMarginSize, a, b,
            polarChart.InnerRadius, polarChart.InitialRotation, polarChart.TotalAnge);

        var size = (float)TextSize;

        var r = (float)LabelsRotation;
        var isTangent = false;
        var isCotangent = false;

        if (((int)r & (int)LiveCharts.TangentAngle) != 0)
        {
            r -= (int)LiveCharts.TangentAngle;
            isTangent = true;
        }

        if (((int)r & (int)LiveCharts.CotangentAngle) != 0)
        {
            r -= (int)LiveCharts.CotangentAngle;
            isCotangent = true;
        }

        var hasRotation = Math.Abs(r) > 0.01f;

        var max = MaxLimit is null ? (_visibleDataBounds ?? _dataBounds).Max : MaxLimit.Value;
        var min = MinLimit is null ? (_visibleDataBounds ?? _dataBounds).Min : MinLimit.Value;

        var start = Math.Truncate(min / s) * s;

        if (!activeSeparators.TryGetValue(polarChart, out var separators))
        {
            separators = [];
            activeSeparators[polarChart] = separators;
        }

        var measured = new HashSet<IVisualSeparator>();

        var separatorsSource = CustomSeparators ?? EnumerateSeparators(start, s, max);

        foreach (var i in separatorsSource)
        {
            if (i < min) continue;
            var label = labeler(i - 1d + 1d);

            if (!separators.TryGetValue(i, out var visualSeparator))
            {
                visualSeparator = _orientation == PolarAxisOrientation.Angle
                    ? new AxisVisualSeprator() { Value = i }
                    : new RadialAxisVisualSeparator() { Value = i };

                var l = _orientation == PolarAxisOrientation.Angle
                    ? scaler.ToPixels(visualSeparator.Value, scaler.MaxRadius)
                    : scaler.ToPixelsWithAngleInDegrees((float)LabelsAngle, visualSeparator.Value);

                if (LabelsPaint is not null && LabelsPaint != Paint.Default)
                {
                    var textGeometry = new TTextGeometry { TextSize = size };
                    visualSeparator.Label = textGeometry;
                    if (hasRotation) textGeometry.RotateTransform = r;

                    textGeometry.Animate(GetAnimation(chart));

                    textGeometry.X = l.X;
                    textGeometry.Y = l.Y;
                    textGeometry.Opacity = 0;
                    textGeometry.Paint = LabelsPaint;
                    textGeometry.CompleteTransition(null);
                }

                if (SeparatorsPaint is not null && SeparatorsPaint != Paint.Default && ShowSeparatorLines)
                {
                    if (visualSeparator is AxisVisualSeprator linearSeparator)
                    {
                        var lineGeometry = new TLineGeometry();

                        linearSeparator.Separator = lineGeometry;

                        lineGeometry.Animate(GetAnimation(chart));

                        lineGeometry.Opacity = 0;
                        lineGeometry.CompleteTransition(null);
                    }

                    if (visualSeparator is RadialAxisVisualSeparator polarSeparator)
                    {
                        var circleGeometry = new TCircleGeometry();

                        polarSeparator.Circle = circleGeometry;

                        circleGeometry.Animate(GetAnimation(chart));

                        var h = Math.Sqrt(Math.Pow(l.X - scaler.CenterX, 2) + Math.Pow(l.Y - scaler.CenterY, 2));
                        var radius = (float)h;
                        polarSeparator.Circle.X = scaler.CenterX - radius;
                        polarSeparator.Circle.Y = scaler.CenterY - radius;
                        polarSeparator.Circle.Width = radius * 2;
                        polarSeparator.Circle.Height = radius * 2;
                        circleGeometry.Opacity = 0;
                        circleGeometry.CompleteTransition(null);
                    }
                }

                separators.Add(i, visualSeparator);
            }

            if (SeparatorsPaint is not null && SeparatorsPaint != Paint.Default && ShowSeparatorLines && visualSeparator.Geometry is not null)
                SeparatorsPaint.AddGeometryToPaintTask(polarChart.Canvas, visualSeparator.Geometry);
            if (LabelsPaint is not null && LabelsPaint != Paint.Default && visualSeparator.Label is not null)
                LabelsPaint.AddGeometryToPaintTask(polarChart.Canvas, visualSeparator.Label);

            var location = _orientation == PolarAxisOrientation.Angle
                ? scaler.ToPixels(visualSeparator.Value, scaler.MaxRadius)
                : scaler.ToPixelsWithAngleInDegrees((float)LabelsAngle, visualSeparator.Value);

            if (visualSeparator.Label is not null)
            {
                visualSeparator.Label.Text = label;
                visualSeparator.Label.Padding = LabelsPadding;
                visualSeparator.Label.HorizontalAlign = LabelsHorizontalAlignment;
                visualSeparator.Label.VerticalAlign = LabelsVerticalAlignment;

                var actualRotation = r + (isTangent ? scaler.GetAngle(i) - 90 : 0) + (isCotangent ? scaler.GetAngle(i) : 0);

                visualSeparator.Label.X = location.X;
                visualSeparator.Label.Y = location.Y;
                visualSeparator.Label.Background = _labelsBackground;

                if (actualRotation < 0) actualRotation = 360 + actualRotation % 360;
                if (_orientation == PolarAxisOrientation.Angle && ((actualRotation + 90) % 360) > 180)
                    actualRotation += 180;

                visualSeparator.Label.RotateTransform = actualRotation;
                visualSeparator.Label.Opacity = string.IsNullOrWhiteSpace(label) ? 0 : 1; // workaround to prevent the last label overlaps the first label

                visualSeparator.Label.X = location.X;
                visualSeparator.Label.Y = location.Y;

                visualSeparator.Label.CompleteTransition(null);
            }

            if (visualSeparator.Geometry is not null)
            {
                if (visualSeparator is AxisVisualSeprator lineSeparator && lineSeparator.Separator is not null)
                {
                    var innerPos = scaler.ToPixels(visualSeparator.Value, scaler.MinRadius);

                    lineSeparator.Separator.X = innerPos.X;
                    lineSeparator.Separator.X1 = location.X;
                    lineSeparator.Separator.Y = innerPos.Y;
                    lineSeparator.Separator.Y1 = location.Y;

                    lineSeparator.Separator.CompleteTransition(null);
                }

                if (visualSeparator is RadialAxisVisualSeparator polarSeparator && polarSeparator.Circle is not null)
                {
                    var h = Math.Sqrt(Math.Pow(location.X - scaler.CenterX, 2) + Math.Pow(location.Y - scaler.CenterY, 2));
                    var radius = (float)h;
                    polarSeparator.Circle.X = scaler.CenterX - radius;
                    polarSeparator.Circle.Y = scaler.CenterY - radius;
                    polarSeparator.Circle.Width = radius * 2;
                    polarSeparator.Circle.Height = radius * 2;

                    polarSeparator.Circle.CompleteTransition(null);
                }

                visualSeparator.Geometry.Opacity = 1;
            }

            if (visualSeparator.Label is not null || visualSeparator.Geometry is not null) _ = measured.Add(visualSeparator);
        }

        foreach (var separator in separators.ToArray())
        {
            if (measured.Contains(separator.Value)) continue;

            SoftDeleteSeparator(polarChart, separator.Value, scaler);
            _ = separators.Remove(separator.Key);
        }
    }

    /// <inheritdoc cref="IPlane.GetNameLabelSize(Chart)"/>
    public LvcSize GetNameLabelSize(Chart chart)
    {
        if (NamePaint is null || string.IsNullOrWhiteSpace(Name)) return new LvcSize(0, 0);

        var textGeometry = new TTextGeometry
        {
            Text = Name ?? string.Empty,
            TextSize = (float)NameTextSize,
            Padding = LabelsPadding,
            Paint = NamePaint
        };

        return textGeometry.Measure();
    }

    /// <inheritdoc cref="IPlane.GetPossibleSize(Chart)"/>
    public virtual LvcSize GetPossibleSize(Chart chart)
    {
        if (_dataBounds is null) throw new Exception("DataBounds not found");
        if (LabelsPaint is null) return new LvcSize(0f, 0f);

        var ts = (float)TextSize;
        var labeler = GetActualLabeler();
        var polarChart = (PolarChartEngine)chart;
        IPolarAxis a, b;

        if (_orientation == PolarAxisOrientation.Angle)
        {
            a = this;
            b = polarChart.RadiusAxes[0];
        }
        else
        {
            a = polarChart.AngleAxes[0];
            b = this;
        }
        var scaler = new PolarScaler(
            polarChart.DrawMarginLocation, polarChart.DrawMarginSize, a, b,
            polarChart.InnerRadius, polarChart.InitialRotation, polarChart.TotalAnge);

        if (Labels is not null)
        {
            labeler = Labelers.BuildNamedLabeler(Labels);
            _minStep = 1;
        }

        var axisTick = this.GetTick(polarChart);
        var s = axisTick.Value;
        if (s < _minStep) s = _minStep;
        if (ForceStepToMin) s = _minStep;

        var max = MaxLimit is null ? (_visibleDataBounds ?? _dataBounds).Max : MaxLimit.Value;
        var min = MinLimit is null ? (_visibleDataBounds ?? _dataBounds).Min : MinLimit.Value;

        var start = Math.Truncate(min / s) * s;

        var totalH = 0f;
        var r = (float)LabelsRotation;

        for (var i = start; i <= max; i += s)
        {
            var textGeometry = new TTextGeometry
            {
                Text = labeler(i),
                TextSize = ts,
                RotateTransform = r + (_orientation == PolarAxisOrientation.Angle ? scaler.GetAngle(i) - 90 : 0),
                Padding = LabelsPadding,
                Paint = LabelsPaint
            };
            var m = textGeometry.Measure();

            var h = (float)Math.Sqrt(Math.Pow(m.Width * 0.5, 2) + Math.Pow(m.Height * 0.5, 2));
            if (h > totalH) totalH = h;
        }

        return new LvcSize(0, totalH);
    }

    /// <inheritdoc cref="IPolarAxis.Initialize(PolarAxisOrientation)"/>
    void IPolarAxis.Initialize(PolarAxisOrientation orientation)
    {
        _orientation = orientation;
        _dataBounds = new Bounds();
        _visibleDataBounds = new Bounds();
        Initialized?.Invoke(this);
    }

    /// <summary>
    /// Deletes the specified chart.
    /// </summary>
    /// <param name="chart">The chart.</param>
    /// <returns></returns>
    public virtual void Delete(Chart chart)
    {
        if (LabelsPaint is not null)
        {
            chart.Canvas.RemovePaintTask(LabelsPaint);
            LabelsPaint.ClearGeometriesFromPaintTask(chart.Canvas);
        }
        if (SeparatorsPaint is not null)
        {
            chart.Canvas.RemovePaintTask(SeparatorsPaint);
            SeparatorsPaint.ClearGeometriesFromPaintTask(chart.Canvas);
        }

        _ = activeSeparators.Remove(chart);
    }

    /// <inheritdoc cref="IChartElement.RemoveFromUI(Chart)"/>
    public override void RemoveFromUI(Chart chart)
    {
        base.RemoveFromUI(chart);
        _ = activeSeparators.Remove(chart);
    }

    /// <summary>
    /// Softly deletes the separator.
    /// </summary>
    /// <param name="chart">The chart.</param>
    /// <param name="separator">The separator.</param>
    /// <param name="scaler">The scale.</param>
    /// <returns></returns>
    protected virtual void SoftDeleteSeparator(
        Chart chart,
        IVisualSeparator separator,
        PolarScaler scaler)
    {
        if (separator.Geometry is null) return;

        var location = _orientation == PolarAxisOrientation.Angle
            ? scaler.ToPixels(separator.Value, scaler.MaxRadius)
            : scaler.ToPixels(0, separator.Value);

        if (separator is AxisVisualSeprator lineSeparator)
        {
            lineSeparator.Separator!.X = scaler.CenterX;
            lineSeparator.Separator.Y = scaler.CenterY;
            lineSeparator.Separator.X1 = location.X;
            lineSeparator.Separator.Y1 = location.Y;
        }

        if (separator is RadialAxisVisualSeparator polarSeparator)
        {
            polarSeparator.Circle!.X = scaler.CenterX;
            polarSeparator.Circle.Y = scaler.CenterY;
            polarSeparator.Circle.Width = 0;
            polarSeparator.Circle.Height = 0;
        }

        separator.Geometry.Opacity = 0;
        separator.Geometry.RemoveOnCompleted = true;

        if (separator.Label is not null)
        {
            //separator.Text.X = 0;
            //separator.Text.Y = 0;
            separator.Label.Opacity = 0;
            separator.Label.RemoveOnCompleted = true;
        }
    }

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

    /// <inheritdoc cref="ChartElement.GetPaintTasks"/>
    protected internal override Paint?[] GetPaintTasks() =>
        [SeparatorsPaint, LabelsPaint, NamePaint];

    private IEnumerable<double> EnumerateSeparators(double start, double s, double max)
    {
        for (var i = start; i <= max; i += s) yield return i;
    }

    private Func<double, string> GetActualLabeler()
    {
        var labeler = Labeler;

        if (Labels is not null)
        {
            labeler = Labelers.BuildNamedLabeler(Labels);
            _minStep = 1;
        }

        return labeler;
    }

    private Animation GetAnimation(Chart chart)
    {
        if (AnimationsSpeed is null && EasingFunction is null)
            return chart.Animation;

        _animation.Duration = (long)(AnimationsSpeed ?? chart.ActualAnimationsSpeed).TotalMilliseconds;
        _animation.EasingFunction = EasingFunction ?? chart.ActualEasingFunction;
        return _animation;
    }
}
