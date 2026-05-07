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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.Kernel.Observers;
using LiveChartsCore.Kernel.Providers;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.Painting;
using LiveChartsCore.VisualStates;

namespace LiveChartsCore;

/// <summary>
/// Defines a series in a Cartesian chart.
/// </summary>
/// <typeparam name="TModel">The type of the model.</typeparam>
/// <typeparam name="TVisual">The type of the visual.</typeparam>
/// <typeparam name="TLabel">The type of the label.</typeparam>
/// <seealso cref="ISeries" />
/// <seealso cref="ISeries{TModel}" />
/// <seealso cref="IDisposable" />
/// <seealso cref="INotifyPropertyChanged" />
public abstract class Series<TModel, TVisual, TLabel>
    : ChartElement, ISeries, ISeries<TModel>, IInternalSeries, INotifyPropertyChanged
        where TVisual : DrawnGeometry, new()
        where TLabel : BaseLabelGeometry, new()
{
    /// <summary>
    /// The subscribed to
    /// </summary>
    protected readonly HashSet<Chart> subscribedTo = [];

    /// <summary>
    /// The implements icp
    /// </summary>
    protected readonly bool implementsICP;

    /// <summary>
    /// The pivot
    /// </summary>
    protected float pivot = 0f;

    /// <summary>
    /// The max series stroke.
    /// </summary>
    protected const float MAX_MINIATURE_STROKE_WIDTH = 3.5f;

    /// <summary>
    /// The ever fetched points.
    /// </summary>
    protected internal HashSet<ChartPoint> everFetched = [];

    /// <summary>
    /// Indicates whether the custom measure handler was requested already.
    /// </summary>
    protected bool _requestedCustomMeasureHandler = false;

    /// <summary>
    /// The custom measure handler.
    /// </summary>
    protected Action<Chart>? _customMeasureHandler = null;

    /// <summary>
    /// Indicates whether the geometry svg changed.
    /// </summary>
    protected bool _geometrySvgChanged = false;

    private readonly CollectionDeepObserver _observer;
    private IEnumerable? _values;
    private Func<ChartPoint, string> _dataLabelsFormatter = x => x.Coordinate.PrimaryValue.ToString();
    private LvcPoint _dataPadding = new(0.5f, 0.5f);
    private bool _showDataLabels;

    // Shared by every geometry this series animates. Mutated in-place on each Invalidate so
    // AnimationsSpeed/EasingFunction changes reach already-created visuals — every MotionProperty
    // holds a reference to this same instance, not a copy.
    private readonly Animation _animation = new(EasingFunctions.QuadraticOut, TimeSpan.Zero);

    /// <summary>
    /// Initializes a new instance of the <see cref="Series{TModel, TVisual, TLabel}"/> class.
    /// </summary>
    /// <param name="properties">The properties.</param>
    /// <param name="values">The values.</param>
    protected Series(SeriesProperties properties, IReadOnlyCollection<TModel>? values)
    {
        SeriesProperties = properties;

        if (typeof(IVariableSvgPath).IsAssignableFrom(typeof(TVisual)))
            SeriesProperties |= SeriesProperties.IsSVGPath;

        _observer = new CollectionDeepObserver(NotifySubscribers);

        Values = values;
    }

    /// <inheritdoc cref="ISeries.SeriesProperties"/>
    public SeriesProperties SeriesProperties { get; internal set; }

    SeriesProperties IInternalSeries.SeriesProperties
    {
        get => SeriesProperties;
        set => SeriesProperties = value;
    }

    /// <inheritdoc cref="ISeries.Name"/>
    public string? Name { get; set => SetProperty(ref field, value); }

    /// <summary>
    /// Gets or sets the data set to draw in the chart.
    /// </summary>
    public IReadOnlyCollection<TModel>? Values
    {
        get => (IReadOnlyCollection<TModel>?)((ISeries)this).Values;
        set => ((ISeries)this).Values = value;
    }

    IEnumerable? ISeries.Values
    {
        get => _values;
        set
        {
            _observer?.Dispose();
            _observer?.Initialize(value);
            _values = value;
            OnPropertyChanged();
        }
    }

    /// <inheritdoc cref="ISeries.Pivot"/>
    public double Pivot { get => pivot; set => SetProperty(ref pivot, (float)value); }

    /// <inheritdoc cref="ISeries{TModel}.Mapping"/>
    public Func<TModel, int, Coordinate>? Mapping { get; set => SetProperty(ref field, value); }

    /// <inheritdoc cref="ISeries.GeometrySvg"/>
    public string? GeometrySvg
    {
        get;
        set
        {
            _geometrySvgChanged = true;
            SetProperty(ref field, value);

            if (!this.HasVariableSvgGeometry())
            {
                throw new Exception(
                    $"You must use a geometry that implements {nameof(IVariableSvgPath)}, " +
                    $"{nameof(TVisual)} does not satisfies the constrait.");
            }
        }
    }

    int ISeries.SeriesId { get; set; } = -1;

    bool ISeries.RequiresFindClosestOnPointerDown => DataPointerDown is not null || ChartPointPointerDown is not null;

    /// <inheritdoc cref="ISeries.ZIndex" />
    public int ZIndex { get; set => SetProperty(ref field, value); }

    /// <summary>
    /// Gets or sets the data label formatter, this function will build the label when a point in this series 
    /// is shown as data label.
    /// </summary>
    /// <value>
    /// The data label formatter.
    /// </value>
    public Func<ChartPoint<TModel, TVisual, TLabel>, string> DataLabelsFormatter
    {
        get => _dataLabelsFormatter;
        // hack #040425...
        //
        // 1. this property maybe should have never be of type Func<ChartPoint<...>, string>
        //    instead of Func<ChartPoint, string>, it is nice to know the type of the point
        //    but maybe only casting would be enough, this was a design decision to improve
        //    developer experience.
        // 2. now when xaml support was introduced:
        //    In c# the compiler knows the type of the generic args and it is a "great" experience
        //    but in xaml it is not, we would need to write the type of the 3 generic args,
        //    it is unessesary complex.
        //
        // Solution:
        //    Lets instead use the Func<ChartPoint, string> type, this also lets us expose this
        //    property in not strongly typed interfaces like ISeries.
        //    To avoid breaking changes we will keep the public property as
        //    Func<ChartPoint<TModel, TVisual, TLabel>, string>, but internally we store it as
        //    Func<ChartPoint, string>... the problem is that we are changing the reference passed in
        //    the setter... this could lead to unexpected behavior, but since this function is
        //    just a formatter, it should not be a problem, lets take the risk and see the feedback.
        set => ((ISeries)this).DataLabelsFormatter = p => value.Invoke(ConvertToTypedChartPoint(p));
    }

    Func<ChartPoint, string> ISeries.DataLabelsFormatter
    {
        get => _dataLabelsFormatter;
        set => SetProperty(ref _dataLabelsFormatter, value);
    }

    /// <inheritdoc cref="ISeries.IsHoverable" />
    public bool IsHoverable { get; set; } = true;

    /// <inheritdoc cref="ISeries.IsVisibleAtLegend" />
    public bool IsVisibleAtLegend { get; set => SetProperty(ref field, value); } = true;

    /// <inheritdoc cref="ISeries.DataPadding" />
    public LvcPoint DataPadding { get => _dataPadding; set => SetProperty(ref _dataPadding, value); }

    /// <inheritdoc cref="ISeries.AnimationsSpeed" />
    public TimeSpan? AnimationsSpeed { get => field; set => SetProperty(ref field, value); }

    /// <inheritdoc cref="ISeries.EasingFunction" />
    public Func<float, float>? EasingFunction { get; set => SetProperty(ref field, value); }

    /// <summary>
    /// Gets the data factory.
    /// </summary>
    public DataFactory<TModel> DataFactory
    {
        get
        {
            if (field is null)
            {
                var factory = LiveCharts.DefaultSettings.GetProvider();
                field = factory.GetDefaultDataFactory<TModel>();
            }

            return field;
        }

        private set;
    }

    /// <summary>
    /// Gets or sets the size of the miniature, the miniature shape is used to draw the series representation in
    /// tooltips and legends, defaults to 12.
    /// </summary>
    public double MiniatureShapeSize
    {
        get;
        set
        {
            field = value;
            SetProperty(ref field, value);
        }
    } = 12;

    /// <summary>
    /// Gets or sets the size of the miniature stroke, the miniature shape is used to draw the series representation in
    /// tooltips and legends, defaults to 2.
    /// </summary>
    public double MiniatureStrokeThickness
    {
        get;
        set
        {
            field = value;
            SetProperty(ref field, value);
        }
    } = 2;

    /// <inheritdoc cref="ISeries.ShowDataLabels"/>
    public bool ShowDataLabels
    {
        get => _showDataLabels;
        set
        {
            SetProperty(ref _showDataLabels, value);
            DataLabelsPaint?.IsPaused = !value;
        }
    }

    /// <inheritdoc cref="ISeries.DataLabelsPaint"/>
    public Paint? DataLabelsPaint
    {
        get;
        set
        {
            SetPaintProperty(ref field, value, PaintStyle.Text);
            _showDataLabels = value is not null && value != Paint.Default;
        }
    } = Paint.Default;

    /// <inheritdoc cref="ISeries.DataLabelsSize"/>
    public double DataLabelsSize { get; set => SetProperty(ref field, value); } = 16;

    /// <inheritdoc cref="ISeries.DataLabelsRotation"/>
    public double DataLabelsRotation { get; set => SetProperty(ref field, value); } = 0;

    /// <inheritdoc cref="ISeries.DataLabelsPadding"/>
    public Padding DataLabelsPadding { get; set => SetProperty(ref field, value); } = new() { Left = 6, Top = 8, Right = 6, Bottom = 8 };

    /// <inheritdoc cref="ISeries.DataLabelsMaxWidth"/>
    public double DataLabelsMaxWidth { get; set => SetProperty(ref field, value); } = LiveCharts.DefaultSettings.MaxTooltipsAndLegendsLabelsWidth;

    /// <inheritdoc cref="ISeries.VisualStates"/>
    public VisualStatesDictionary VisualStates { get; } = [];

    /// <inheritdoc cref="ISeries.GetStackGroup"/>
    public virtual int GetStackGroup() => 0;

    /// <summary>
    /// Occurs when an instance of <see cref="ChartPoint"/> is measured.
    /// </summary>
    public event Action<ChartPoint<TModel, TVisual, TLabel>>? PointMeasured;

    /// <summary>
    /// Occurs when an instance of <see cref="ChartPoint"/> is created.
    /// </summary>
    public event Action<ChartPoint<TModel, TVisual, TLabel>>? PointCreated;

    /// <summary>
    /// Occurs when the pointer goes down over a chart point(s).
    /// </summary>
    public event ChartPointsHandler<TModel, TVisual, TLabel>? DataPointerDown;

    /// <summary>
    /// Occurs when the pointer is over a chart point.
    /// </summary>
    public event ChartPointHandler<TModel, TVisual, TLabel>? ChartPointPointerHover;

    /// <summary>
    /// Occurs when the pointer left a chart point.
    /// </summary>
    public event ChartPointHandler<TModel, TVisual, TLabel>? ChartPointPointerHoverLost;

    /// <summary>
    /// Occurs when the pointer goes down over a chart point, if there are multiple points, the closest one will be selected.
    /// </summary>
    public event ChartPointHandler<TModel, TVisual, TLabel>? ChartPointPointerDown;

    /// <inheritdoc cref="ISeries.Fetch(Chart)"/>
    protected IEnumerable<ChartPoint> Fetch(Chart chart)
    {
        _ = subscribedTo.Add(chart);
        return DataFactory.Fetch(this, chart);
    }

    /// <inheritdoc cref="ISeries.OnDataPointerDown(IChartView, IEnumerable{ChartPoint}, LvcPoint)"/>
    protected virtual void OnDataPointerDown(IChartView chart, IEnumerable<ChartPoint> points, LvcPoint pointer)
    {
        DataPointerDown?.Invoke(chart, points.Select(point => new ChartPoint<TModel, TVisual, TLabel>(point)));
        ChartPointPointerDown?.Invoke(chart, new ChartPoint<TModel, TVisual, TLabel>(points.FindClosestTo<TModel, TVisual, TLabel>(pointer)!));
    }

    ///<inheritdoc cref="ISeries.FindHitPoints(Chart, LvcPoint, FindingStrategy, FindPointFor)"/>
    protected virtual IEnumerable<ChartPoint> FindPointsInPosition(
        Chart chart, LvcPoint pointerPosition, FindingStrategy strategy, FindPointFor findPointFor)
    {
        var query =
            Fetch(chart)
            .Where(x =>
                x.Context.HoverArea is not null &&
                x.Context.HoverArea.IsPointerOver(pointerPosition, strategy));

        var s = (int)strategy;
        if (s is (>= 4 and <= 6) or 8)
        {
            // if select closest...
            query = query
                .Select(x => new { distance = x.DistanceTo(pointerPosition, strategy), point = x })
                .OrderBy(x => x.distance)
                .SelectFirst(x => x.point);
        }

        return query;
    }

    IEnumerable<ChartPoint> ISeries.Fetch(Chart chart) => Fetch(chart);

    IEnumerable<ChartPoint> ISeries.FindHitPoints(
        Chart chart,
        LvcPoint pointerPosition,
        FindingStrategy strategy,
        FindPointFor findPointFor) =>
            FindPointsInPosition(chart, pointerPosition, strategy, findPointFor);

    void ISeries.OnPointerEnter(ChartPoint point) =>
        OnPointerEnter(point);

    void ISeries.OnPointerLeft(ChartPoint point) =>
        OnPointerLeft(point);

    /// <inheritdoc cref="ISeries.RestartAnimations"/>
    public void RestartAnimations()
    {
        if (DataFactory is null) throw new Exception("Data provider not found");
        DataFactory.RestartVisuals();
    }

    /// <inheritdoc cref="ISeries.GetPrimaryToolTipText(ChartPoint)"/>
    public abstract string? GetPrimaryToolTipText(ChartPoint point);

    /// <inheritdoc cref="ISeries.GetSecondaryToolTipText(ChartPoint)"/>
    public abstract string? GetSecondaryToolTipText(ChartPoint point);

    /// <inheritdoc cref="ISeries.GetDataLabelText(ChartPoint)"/>
    public string? GetDataLabelText(ChartPoint point) =>
        DataLabelsFormatter is null
        ? null
        : DataLabelsFormatter(new ChartPoint<TModel, TVisual, TLabel>(point));

    /// <inheritdoc cref="ChartElement.RemoveFromUI(Chart)"/>
    public override void RemoveFromUI(Chart chart)
    {
        base.RemoveFromUI(chart);
        DataFactory?.Dispose(chart);
        DataFactory = null!;
        everFetched = [];
    }

    /// <summary>
    /// Converts a chart to a strong-typed version of it.
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public ChartPoint<TModel, TVisual, TLabel> ConvertToTypedChartPoint(ChartPoint point) =>
        new(point);

    /// <inheritdoc cref="ISeries.SoftDeleteOrDispose"/>
    public abstract void SoftDeleteOrDispose(IChartView chart);

    /// <inheritdoc cref="ISeries.GetMiniatureGeometry"/>
    public abstract IDrawnElement GetMiniatureGeometry(ChartPoint? point);

    void ISeries.OnDataPointerDown(IChartView chart, IEnumerable<ChartPoint> points, LvcPoint pointer) =>
        OnDataPointerDown(chart, points, pointer);

    /// <summary>
    /// Returns the <see cref="Animation"/> instance that every geometry this series animates
    /// should reference. With no per-series override, the chart's shared <see cref="Chart.Animation"/>
    /// is returned directly so chart-level <see cref="Chart.ActualAnimationsSpeed"/> /
    /// <see cref="Chart.ActualEasingFunction"/> changes propagate to already-created visuals.
    /// With a per-series override, a series-owned instance is mutated in-place from the active
    /// settings.
    /// </summary>
    /// <param name="chart">The chart whose <see cref="Chart.Animation"/> is used as the fallback.</param>
    protected Animation GetAnimation(Chart chart)
    {
        if (AnimationsSpeed is null && EasingFunction is null)
            return chart.Animation;

        _animation.Duration = (long)(AnimationsSpeed ?? chart.ActualAnimationsSpeed).TotalMilliseconds;
        _animation.EasingFunction = EasingFunction ?? chart.ActualEasingFunction;
        return _animation;
    }

    /// <summary>
    /// Called when a point was measured.
    /// </summary>
    /// <param name="chartPoint">The chart point.</param>
    protected internal virtual void OnPointMeasured(ChartPoint chartPoint) =>
        PointMeasured?.Invoke(new ChartPoint<TModel, TVisual, TLabel>(chartPoint));

    /// <summary>
    /// Called when a point is created.
    /// </summary>
    /// <param name="chartPoint">The chart point.</param>
    protected internal virtual void OnPointCreated(ChartPoint chartPoint)
    {
        SetDefaultPointTransitions(chartPoint);
        PointCreated?.Invoke(new ChartPoint<TModel, TVisual, TLabel>(chartPoint));
    }

    /// <summary>
    /// Sets the default point transitions.
    /// </summary>
    /// <param name="chartPoint">The chart point.</param>
    /// <returns></returns>
    protected abstract void SetDefaultPointTransitions(ChartPoint chartPoint);

    /// <summary>
    /// Called when the pointer enters a point.
    /// </summary>
    /// <param name="point">The chart point.</param>
    protected virtual void OnPointerEnter(ChartPoint point)
    {
        point.Context.Series.VisualStates.SetState("Hover", point.Context.Visual);

        if (ChartPointPointerHover is null || point.IsPointerOver) return;
        point.IsPointerOver = true;
        ChartPointPointerHover.Invoke(point.Context.Chart, ConvertToTypedChartPoint(point));
    }

    /// <summary>
    /// Called when the pointer leaves a point.
    /// </summary>
    /// <param name="point">The chart point.</param>
    protected virtual void OnPointerLeft(ChartPoint point)
    {
        point.Context.Series.VisualStates.ClearState("Hover", point.Context.Visual);

        if (ChartPointPointerHoverLost is null || !point.IsPointerOver) return;
        point.IsPointerOver = false;
        ChartPointPointerHoverLost.Invoke(point.Context.Chart, ConvertToTypedChartPoint(point));
    }

    /// <inheritdoc cref="ChartElement.OnPaintChanged(string?)"/>
    protected override void OnPaintChanged(string? propertyName) => base.OnPaintChanged(propertyName);

    private void NotifySubscribers()
    {
        foreach (var chart in subscribedTo) chart.Update();
    }
}
