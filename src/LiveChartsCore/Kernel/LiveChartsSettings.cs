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
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Providers;
using LiveChartsCore.Measure;
using LiveChartsCore.Painting;
using LiveChartsCore.Themes;

namespace LiveChartsCore.Kernel;

/// <summary>
/// LiveCharts global settings
/// </summary>
public class LiveChartsSettings
{
    private bool _isRightToLeftDefined;
    private object? _currentProvider;
    private readonly Dictionary<Type, object> _mappers = [];
    private object _theme = new();

    internal bool HasBackedDefined => _currentProvider is not null;
    internal bool HasThemeDefined => _theme is not null and Theme;
    internal bool HasMappersDefined => _mappers.Count > 0;
    internal bool IsRightToLeft
    {
        get
        {
            if (_isRightToLeftDefined)
                return field;

            // if the user has not defined the right to left mode, we will use the system culture.
            var culture = System.Globalization.CultureInfo.CurrentCulture;
            field = culture.TextInfo.IsRightToLeft;
            _isRightToLeftDefined = true;

            return field;
        }
        set
        {
            _isRightToLeftDefined = true;
            field = value;
        }
    }

    /// <summary>
    /// Gets or sets the default easing function.
    /// </summary>
    /// <value>
    /// The default easing function.
    /// </value>
    public Func<float, float> EasingFunction { get; set; } = EasingFunctions.Unset;

    /// <summary>
    /// Gets or sets the default animations speed.
    /// </summary>
    /// <value>
    /// The default animations speed.
    /// </value>
    public TimeSpan AnimationsSpeed { get; set; } = TimeSpan.MaxValue;

    /// <summary>
    /// Gets or sets the default zoom speed.
    /// </summary>
    /// <value>
    /// The default zoom speed.
    /// </value>
    public double ZoomSpeed { get; set; } = 0.2;

    /// <summary>
    /// Gets or sets the default zoom mode.
    /// </summary>
    /// <value>
    /// The default zoom mode.
    /// </value>
    public ZoomAndPanMode ZoomMode { get; set; } = ZoomAndPanMode.None;

    /// <summary>
    /// Gets or sets the default legend position.
    /// </summary>
    /// <value>
    /// The default legend position.
    /// </value>
    public LegendPosition LegendPosition { get; set; } = LegendPosition.Hidden;

    /// <summary>
    /// Gets or sets the default legend background paint.
    /// </summary>
    public Paint? LegendBackgroundPaint { get; set; }

    /// <summary>
    /// Gets or sets the default legend text paint.
    /// </summary>
    public Paint? LegendTextPaint { get; set; }

    /// <summary>
    /// Gets or sets the default legend text size.
    /// </summary>
    public double LegendTextSize { get; set; } = -1;

    /// <summary>
    /// Gets or sets the default tooltip position.
    /// </summary>
    /// <value>
    /// The default tooltip position.
    /// </value>
    public TooltipPosition TooltipPosition { get; set; } = TooltipPosition.Auto;

    /// <summary>
    /// Gets or sets the default tooltip background paint.
    /// </summary>
    public Paint? TooltipBackgroundPaint { get; set; }

    /// <summary>
    /// Gets or sets the default tooltip text paint.
    /// </summary>
    public Paint? TooltipTextPaint { get; set; }

    /// <summary>
    /// Gets or sets the default tooltip text size.
    /// </summary>
    public double TooltipTextSize { get; set; } = -1;

    /// <summary>
    /// Gets or sets the default max with for labels inside tooltips and legends.
    /// </summary>
    public double MaxTooltipsAndLegendsLabelsWidth { get; set; } = 170;

    /// <summary>
    /// Gets or sets the default finding strategy.
    /// </summary>
    /// <value>
    /// The default finding strategy.
    /// </value>
    public FindingStrategy FindingStrategy { get; set; } = FindingStrategy.Automatic;

    /// <summary>
    /// Gets or sets the default polar initial rotation.
    /// </summary>
    /// <value>
    /// The default animations speed.
    /// </value>
    public double PolarInitialRotation { get; set; } = -90;

    /// <summary>
    /// Gets or sets the default update throttling timeout
    /// </summary>
    /// <value>
    /// The default update throttling timeout
    /// </value>
    public TimeSpan UpdateThrottlingTimeout { get; set; } = TimeSpan.FromMilliseconds(50);

    /// <summary>
    /// Gets or sets a value indicating whether the pie slices are clockwise.
    /// </summary>
    public bool PieIsClockwise { get; set; } = true;

    /// <summary>
    /// Gets or sets the initial rotation for pie charts, this value is used to calculate the pie slices.
    /// </summary>
    public double PieInitialRotation { get; set; } = 0;

    /// <summary>
    /// Gets or sets the maximum angle for pie charts, this value is used to calculate the pie slices.
    /// </summary>
    public double PieMaxAngle { get; set; } = 360;

    /// <summary>
    /// Gets or sets the maximum value for pie charts, this value is used to calculate the pie slices.
    /// </summary>
    public double PieMaxValue { get; set; } = double.NaN;

    /// <summary>
    /// Gets or sets the minimum value for pie charts, this value is used to calculate the pie slices.
    /// </summary>
    public double PieMinValue { get; set; } = 0d;

    /// <summary>
    /// Gets or sets a value indicating whether the polar series should fit to bounds.
    /// </summary>
    public bool PolarFitToBounds { get; set; } = false;

    /// <summary>
    /// Gets or sets the total angle for polar series, this value is used to calculate the polar chart.
    /// </summary>
    public double PolarTotalAngle { get; set; } = 360d;

    /// <summary>
    /// Gets or sets the outer radius for polar series, this value is used to calculate the polar chart.
    /// </summary>
    public double PolarInnerRadius { get; set; } = 0d;

    /// <summary>
    /// Adds or replaces a mapping for a given type, the mapper defines how a type is mapped to a <see cref="Coordinate"/>
    /// in the chart.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <param name="mapper">The mapper.</param>
    /// <returns></returns>
    public LiveChartsSettings HasMap<TModel>(Func<TModel, int, Coordinate> mapper)
    {
        var t = typeof(TModel);
        _mappers[t] = mapper;
        return this;
    }

    /// <summary>
    /// Indicates that the library should render tooltips and legends in the right to left mode, if not defined,
    /// the library will use the system culture to determine if it should render in right to left mode or not.
    /// </summary>
    /// <returns>The current settings.</returns>
    public LiveChartsSettings UseRightToLeftSettings()
    {
        IsRightToLeft = true;
        return this;
    }

    /// <summary>
    /// Gets the map for a given type.
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Func<TModel, int, Coordinate> GetMap<TModel>()
    {
        if (_mappers.TryGetValue(typeof(TModel), out var mapper))
            return (Func<TModel, int, Coordinate>)mapper;

        var type = typeof(TModel);

        // most likely a xaml series
        if (type == typeof(object))
            throw new Exception(
                "A mapper for the type to plot was not found. " +
                "When using custom types in XAML, you must define the series data type, " +
                "for example to plot a collection of double add the x:TypeArguments=\"x:Double\" attribute " +
                "to the series XAML, the mapper must also be configured, alternatively, you can also " +
                "implement IChartEntity in the type your are plotting, for more info see: " +
                "https://livecharts.dev/docs/select/latest/Overview.Mappers");

        throw new NotImplementedException(
            $"A mapper for type {typeof(TModel)} is not implemented yet, consider using " +
            $"{nameof(LiveCharts)}.{nameof(LiveCharts.Configure)}() " +
            $"method to call {nameof(HasMap)}() with the type you are trying to plot.");
    }

    internal LiveChartsSettings HasProvider(ChartEngine factory)
    {
        _currentProvider = factory;
        return this;
    }

    internal ChartEngine GetProvider()
    {
        return _currentProvider is null
            ? throw new NotImplementedException(
                $"There is no a {nameof(ChartEngine)} registered.")
            : (ChartEngine)_currentProvider;
    }

    /// <summary>
    /// Sets the default animations speed.
    /// </summary>
    /// <param name="animationsSpeed">The animations speed.</param>
    /// <returns>The current settings</returns>
    public LiveChartsSettings WithAnimationsSpeed(TimeSpan animationsSpeed)
    {
        AnimationsSpeed = animationsSpeed;
        return this;
    }

    /// <summary>
    /// Sets the default the default easing function.
    /// </summary>
    /// <param name="easingFunction">The easing function.</param>
    /// <returns>The current settings</returns>
    public LiveChartsSettings WithEasingFunction(Func<float, float> easingFunction)
    {
        EasingFunction = easingFunction;
        return this;
    }

    /// <summary>
    /// Sets the default the default zoom speed.
    /// </summary>
    /// <param name="speed">The speed.</param>
    /// <returns>The current settings</returns>
    public LiveChartsSettings WithZoomSpeed(double speed)
    {
        ZoomSpeed = speed;
        return this;
    }

    /// <summary>
    /// Sets the default the default zoom mode.
    /// </summary>
    /// <param name="zoomMode">The zoom mode.</param>
    /// <returns>The current settings</returns>
    public LiveChartsSettings WithZoomMode(ZoomAndPanMode zoomMode)
    {
        ZoomMode = zoomMode;
        return this;
    }

    /// <summary>
    /// Sets the default the default update throttling timeout
    /// </summary>
    /// <param name="timeout">The update throttling timeout.</param>
    /// <returns>The current settings</returns>
    public LiveChartsSettings WithUpdateThrottlingTimeout(TimeSpan timeout)
    {
        UpdateThrottlingTimeout = timeout;
        return this;
    }

    /// <summary>
    /// Sets the default legend background paint.
    /// </summary>
    /// <param name="paint">The paint.</param>
    /// <returns>The current settings.</returns>
    public LiveChartsSettings WithLegendBackgroundPaint(Paint paint)
    {
        LegendBackgroundPaint = paint;
        return this;
    }

    /// <summary>
    /// Sets the default legend text paint.
    /// </summary>
    /// <param name="paint">The paint.</param>
    /// <returns>The current settings.</returns>
    public LiveChartsSettings WithLegendTextPaint(Paint paint)
    {
        LegendTextPaint = paint;
        return this;
    }

    /// <summary>
    /// Sets the default legend text size.
    /// </summary>
    /// <param name="size">The size.</param>
    /// <returns>The current settings.</returns>
    public LiveChartsSettings WithLegendTextSize(double size)
    {
        LegendTextSize = size;
        return this;
    }

    /// <summary>
    /// Sets the default tooltip background paint.
    /// </summary>
    /// <param name="paint">The paint.</param>
    /// <returns>The current settings.</returns>
    public LiveChartsSettings WithTooltipBackgroundPaint(Paint paint)
    {
        TooltipBackgroundPaint = paint;
        return this;
    }

    /// <summary>
    /// Sets the default tooltip text paint.
    /// </summary>
    /// <param name="paint">The paint.</param>
    /// <returns>The current settings.</returns>
    public LiveChartsSettings WithTooltipTextPaint(Paint paint)
    {
        TooltipTextPaint = paint;
        return this;
    }

    /// <summary>
    /// Sets the default tooltip text size.
    /// </summary>
    /// <param name="size">The size.</param>
    /// <returns>The current settings.</returns>
    public LiveChartsSettings WithTooltipTextSize(double size)
    {
        TooltipTextSize = size;
        return this;
    }

    /// <summary>
    /// Removes a map from the settings.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <returns>The current settings.</returns>
    public LiveChartsSettings RemoveMap<TModel>()
    {
        _ = _mappers.Remove(typeof(TModel));
        return this;
    }

    /// <summary>
    /// Adds the default styles.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>The current settings.</returns>
    public LiveChartsSettings HasTheme(Action<Theme> builder)
    {
        Theme t;
        _theme = t = new Theme();

        builder(t);

        return this;
    }

    /// <summary>
    /// Gets the styles builder.
    /// </summary>
    public Theme GetTheme() =>
        (Theme?)_theme ?? throw new Exception("A theme is required.");

    /// <summary>
    /// Obsolete.
    /// </summary>
    /// <typeparam name="TDrawingContext"></typeparam>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [Obsolete("Replaced by the non generic GetThememe method.")]
    public Theme GetTheme<TDrawingContext>()
        where TDrawingContext : DrawingContext
            => (Theme?)_theme ?? throw new Exception("A theme is required.");

    /// <summary>
    /// Enables LiveCharts to be able to plot short, int, long, float, double, decimal, short?, int?, long?, float?, double?, decimal?.
    /// </summary>
    /// <returns>The current settings.</returns>
    public LiveChartsSettings AddDefaultMappers()
    {
        return
            HasMap<short>((model, index) => new(index, model))
            .HasMap<int>((model, index) => new(index, model))
            .HasMap<long>((model, index) => new(index, model))
            .HasMap<float>((model, index) => new(index, model))
            .HasMap<double>((model, index) => new(index, model))
            .HasMap<decimal>((model, index) => new(index, (double)model))
            .HasMap<short?>((model, index) => new(index, model!.Value))
            .HasMap<int?>((model, index) => new(index, model!.Value))
            .HasMap<long?>((model, index) => new(index, model!.Value))
            .HasMap<float?>((model, index) => new(index, model!.Value))
            .HasMap<double?>((model, index) => new(index, model!.Value))
            .HasMap<decimal?>((model, index) => new(index, (double)model!.Value));
    }

    /// <summary>
    /// Configures the rendering settings for LiveCharts.
    /// </summary>
    /// <param name="builder">The rendering settings builder.</param>
    /// <returns>The current settings.</returns>
    public LiveChartsSettings HasRenderingSettings(Action<RenderingSettings> builder)
    {
        builder(LiveCharts.RenderingSettings);

        return this;
    }
}
