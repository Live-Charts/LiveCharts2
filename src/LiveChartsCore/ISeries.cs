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
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Drawing;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.Painting;
using LiveChartsCore.VisualStates;

namespace LiveChartsCore;

/// <summary>
/// Defines a chart series.
/// </summary>
public interface ISeries : IChartElement
{
    /// <summary>
    /// Gets or sets a series unique identifier, the library handles this id automatically.
    /// </summary>
    int SeriesId { get; set; }

    /// <summary>
    /// Gets the properties of the series.
    /// </summary>
    SeriesProperties SeriesProperties { get; }

    /// <summary>
    /// Gets whether the series requires to find the closest point when the pointer goes down.
    /// </summary>
    bool RequiresFindClosestOnPointerDown { get; }

    /// <summary>
    /// Occurs when a point in this series is measured. This is the non-generic counterpart of
    /// <see cref="Series{TModel, TVisual, TLabel}.PointMeasured"/>, exposed so code that only has
    /// an <see cref="ISeries"/> reference (e.g. a theme rule) can react per point.
    /// </summary>
    event Action<ChartPoint>? PointMeasured;

    /// <summary>
    /// Occurs when a point in this series is created. This is the non-generic counterpart of
    /// <see cref="Series{TModel, TVisual, TLabel}.PointCreated"/>, exposed so code that only has
    /// an <see cref="ISeries"/> reference (e.g. a theme rule) can react per point.
    /// </summary>
    event Action<ChartPoint>? PointCreated;

    /// <summary>
    /// Occurs when the pointer goes down over a chart point(s). This is the non-generic counterpart
    /// of <see cref="Series{TModel, TVisual, TLabel}.DataPointerDown"/>.
    /// </summary>
    event ChartPointsHandler? DataPointerDown;

    /// <summary>
    /// Occurs when the pointer is over a chart point. This is the non-generic counterpart of
    /// <see cref="Series{TModel, TVisual, TLabel}.ChartPointPointerHover"/>.
    /// </summary>
    event ChartPointHandler? ChartPointPointerHover;

    /// <summary>
    /// Occurs when the pointer left a chart point. This is the non-generic counterpart of
    /// <see cref="Series{TModel, TVisual, TLabel}.ChartPointPointerHoverLost"/>.
    /// </summary>
    event ChartPointHandler? ChartPointPointerHoverLost;

    /// <summary>
    /// Occurs when the pointer goes down over a chart point, if there are multiple points the closest
    /// one is selected. This is the non-generic counterpart of
    /// <see cref="Series{TModel, TVisual, TLabel}.ChartPointPointerDown"/>.
    /// </summary>
    event ChartPointHandler? ChartPointPointerDown;

    /// <summary>
    /// Gets or sets the name of the series, the name is normally used by <see cref="IChartTooltip"/> or 
    /// <see cref="IChartLegend"/>, the default value is set automatically by the library.
    /// </summary>
    string? Name { get; set; }

    /// <summary>
    /// Gets or sets the values.
    /// </summary>
    /// <value>
    /// The values.
    /// </value>
    IEnumerable? Values { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance will show up in tool tips when the pointer is over a point.
    /// default value is <c>true</c>.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is hover-able; otherwise, <c>false</c>.
    /// </value>
    bool IsHoverable { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance will show up in legends.
    /// default value is <c>true</c>.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is visible at legends; otherwise, <c>false</c>.
    /// </value>
    bool IsVisibleAtLegend { get; set; }

    /// <summary>
    /// Gets or sets the size of the legend shape.
    /// </summary>
    /// <value>
    /// The size of the legend shape.
    /// </value>
    double MiniatureShapeSize { get; set; }

    /// <summary>
    /// Gets or sets the miniature stroke thickness.
    /// </summary>
    double MiniatureStrokeThickness { get; set; }

    /// <summary>
    /// Gets or sets the data padding, the distance from the edge of the chart to where the series is drawn,
    /// both coordinates (X and Y) from 0 to 1, where 0 is nothing and 1 is the axis tick (the separation between every label).
    /// </summary>
    /// <value>
    /// The data padding.
    /// </value>
    LvcPoint DataPadding { get; set; }

    /// <summary>
    /// Gets or sets the z index position.
    /// </summary>
    /// <value>
    /// The index of the z.
    /// </value>
    int ZIndex { get; set; }

    /// <summary>
    /// Gets or sets the value where the direction of the axis changes, by default is 0.
    /// </summary>
    double Pivot { get; set; }

    /// <summary>
    /// Gets or sets the series geometry svg, this property requires the series visual to be
    /// an <see cref="IVariableSvgPath"/> instance.
    /// </summary>
    string? GeometrySvg { get; set; }

    /// <summary>
    /// Gets or sets the animations speed; when null the series inherits the chart-level animation
    /// (see <see cref="Chart.Animation"/>). Set it to <see cref="TimeSpan.Zero"/> to disable
    /// animations for this series only.
    /// </summary>
    /// <value>
    /// The animations speed.
    /// </value>
    TimeSpan? AnimationsSpeed { get; set; }

    /// <summary>
    /// Gets or sets the easing function used to animate the series; when null the series inherits
    /// the chart-level easing (see <see cref="Chart.Animation"/>).
    /// </summary>
    /// <value>
    /// The easing function.
    /// </value>
    Func<float, float>? EasingFunction { get; set; }

    /// <summary>
    /// Indicates whether the data labels are visible,
    /// to set the color use the <see cref="DataLabelsPaint"/> property, the <see cref="DataLabelsPaint"/> property
    /// could be defined by the theme when not explicitly set.
    /// Default is <c>false</c>.
    /// </summary>
    bool ShowDataLabels { get; set; }

    /// <summary>
    /// Gets or sets the data labels paint.
    /// </summary>
    /// <value>
    /// The data labels paint.
    /// </value>
    Paint? DataLabelsPaint { get; set; }

    /// <summary>
    /// Gets or sets the size of the data labels.
    /// </summary>
    /// <value>
    /// The size of the data labels.
    /// </value>
    double DataLabelsSize { get; set; }

    /// <summary>
    /// Gets or sets the data labels rotation in degrees.
    /// </summary>
    /// <value>
    /// The rotation of the data labels in degrees.
    /// </value>
    double DataLabelsRotation { get; set; }

    /// <summary>
    /// Gets or sets the data labels padding.
    /// </summary>
    /// <value>
    /// The data labels padding.
    /// </value>
    Padding DataLabelsPadding { get; set; }

    /// <summary>
    /// Gets or sets the max width of the data labels.
    /// </summary>
    /// <value>
    /// The max with of the data labels.
    /// </value>
    double DataLabelsMaxWidth { get; set; }

    /// <summary>
    /// Gets the visual states, states define the look of chart points when they are in a certain state, for example
    /// the hover, active, or selected states.
    /// </summary>
    VisualStatesDictionary VisualStates { get; }

    /// <summary>
    /// Gets or sets the data labels formatter, the function receives a <see cref="ChartPoint"/> instance and must return a string.
    /// </summary>
    Func<ChartPoint, string> DataLabelsFormatter { get; set; }

    /// <summary>
    /// Gets the tool tip text for a give chart point.
    /// </summary>
    /// <param name="point">The chart point.</param>
    /// <returns></returns>
    string? GetPrimaryToolTipText(ChartPoint point);

    /// <summary>
    /// Gets the tool tip text for a give chart point.
    /// </summary>
    /// <param name="point">The chart point.</param>
    /// <returns></returns>
    string? GetSecondaryToolTipText(ChartPoint point);

    /// <summary>
    /// Gets the data label content for a given chart point.
    /// </summary>
    /// <param name="point">The chart point.</param>
    /// <returns></returns>
    string? GetDataLabelText(ChartPoint point);

    /// <summary>
    /// Gets a <see cref="ChartPoint"/> IEnumerable with the points used to generate the plot.
    /// </summary>
    /// <param name="chart">the chart</param>
    /// <returns>The IEnumerable of <see cref="ChartPoint"/>.</returns>
    IEnumerable<ChartPoint> Fetch(Chart chart);

    /// <summary>
    /// Gets the <see cref="ChartPoint"/> instances which contain the <paramref name="pointerPosition"/>, according 
    /// to the chart's <see cref="FindingStrategy"/> property.
    /// </summary>
    /// <param name="chart">the chart.</param>
    /// <param name="pointerPosition">the pointer position.</param>
    /// <param name="strategy">the strategy.</param>
    /// <param name="findPointFor">the trigger that fired the search.</param>
    /// <returns></returns>
    IEnumerable<ChartPoint> FindHitPoints(Chart chart, LvcPoint pointerPosition, FindingStrategy strategy, FindPointFor findPointFor);

    /// <summary>
    /// Called when the pointer enters a chart point.
    /// </summary>
    /// <param name="point"></param>
    void OnPointerEnter(ChartPoint point);

    /// <summary>
    /// Called when the pointer leaves a chart point.
    /// </summary>
    /// <param name="point"></param>
    void OnPointerLeft(ChartPoint point);

    /// <summary>
    /// Clears the visuals in the cache and re-starts animations.
    /// </summary>
    void RestartAnimations();

    /// <summary>
    /// Deletes the series from the user interface.
    /// </summary>
    /// <param name="chart">The chart that draws the series.</param>
    void SoftDeleteOrDispose(IChartView chart);

    /// <summary>
    /// Gets the stack group, normally used internally to handled the stacked series.
    /// </summary>
    /// <returns></returns>
    int GetStackGroup();

    /// <summary>
    /// Returns a geometry that represents the series in a tooltip or legend.
    /// </summary>
    /// <param name="point">The target point.</param>
    IDrawnElement GetMiniatureGeometry(ChartPoint? point);

    /// <summary>
    /// Called when the pointer goes down on a data point or points.
    /// </summary>
    /// <param name="chart">The chart.</param>
    /// <param name="points">The found points.</param>
    /// <param name="pointerLocation">The pointer location.</param>
    void OnDataPointerDown(IChartView chart, IEnumerable<ChartPoint> points, LvcPoint pointerLocation);
}

/// <summary>
/// Defines a series.
/// </summary>
/// <typeparam name="TModel">The type of the model.</typeparam>
public interface ISeries<TModel> : ISeries
{
    /// <summary>
    /// Gets or sets the values.
    /// </summary>
    /// <value>
    /// The values.
    /// </value>
    new IReadOnlyCollection<TModel>? Values { get; set; }

    /// <summary>
    /// Gets or sets the mapping.
    /// </summary>
    /// <value>
    /// The mapping.
    /// </value>
    Func<TModel, int, Coordinate>? Mapping { get; set; }
}
