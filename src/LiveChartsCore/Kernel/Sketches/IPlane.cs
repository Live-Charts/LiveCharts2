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
using LiveChartsCore.Measure;
using LiveChartsCore.Motion;
using LiveChartsCore.Painting;

namespace LiveChartsCore.Kernel.Sketches;

/// <summary>
/// Defines a plane for a dimension.
/// </summary>
public interface IPlane : IChartElement
{
    /// <summary>
    /// Gets or sets the axis name.
    /// </summary>
    string? Name { get; set; }

    /// <summary>
    /// Gets or sets the size of the name label.
    /// </summary>
    /// <value>
    /// The size of the text.
    /// </value>
    double NameTextSize { get; set; }

    /// <summary>
    /// Gets or sets the padding of the name label.
    /// </summary>
    Padding NamePadding { get; set; }

    /// <summary>
    /// Gets the motion min limit, it is the min limit at the moment of getting the property.
    /// </summary>
    double MotionMinLimit { get; }

    /// <summary>
    /// Gets the motion max limit, it is the max limit at the moment of getting the property.
    /// </summary>
    double MotionMaxLimit { get; }

    /// <summary>
    /// Gets the data bounds, the min and max values in the axis.
    /// </summary>
    /// <value>
    /// The data bounds.
    /// </value>
    Bounds DataBounds { get; }

    /// <summary>
    /// Gets the data visible bounds, the min and max visible values in the axis that are currently in the UI.
    /// </summary>
    /// <value>
    /// The data bounds.
    /// </value>
    Bounds VisibleDataBounds { get; }

    /// <summary>
    /// Gets or sets the labeler, a function that receives a number and return the label content as string.
    /// </summary>
    /// <value>
    /// The labeler.
    /// </value>
    Func<double, string> Labeler { get; set; }

    /// <summary>
    /// Gets or sets the minimum step, the step defines the interval between every separator in the axis,
    /// LiveCharts will calculate it automatically based on the chart data and the chart size size, if the calculated step is
    /// less than the <see cref="MinStep"/> then <see cref="MinStep"/> will be used as the axis step, default is 0.
    /// </summary>
    /// <value>
    /// The step.
    /// </value>
    double MinStep { get; set; }

    /// <summary>
    /// Gets or sets whether the axis should use the <see cref="MinStep"/> as the actual step of the axis, if true, the library
    /// will not calculate the step of the axis automatically, instead it will use the value in the <see cref="MinStep"/> property.
    /// Be careful when setting this property to true, it might cause performance issues if the step is too small for the current
    /// data in the chart. Default value is false.
    /// </summary>
    bool ForceStepToMin { get; set; }

    /// <summary>
    /// Gets or sets the minimum number of separators (gridlines/labels) the axis should display when the step is calculated
    /// automatically. The library snaps the auto-step up to a nice number (1, 2, 5, 10 × magnitude), and on small charts that
    /// snap can leave too few separators visible; this property forces the auto-step to subdivide further until at least this
    /// many separators fit in the visible range. Set to 0 to disable. Ignored when <see cref="ForceStepToMin"/> is true.
    /// Default is 3.
    /// </summary>
    int MinSeparators { get; set; }

    /// <summary>
    /// Gets or sets the unit with, it means the width of every point (if the series requires it) in the chart values scale, this value
    /// should normally be 1, unless you are plotting in a custom scale, default is 1.
    /// </summary>
    /// <value>
    /// The unit with.
    /// </value>
    double UnitWidth { get; set; }

    /// <summary>
    /// Gets or sets the minimum value visible in the axis, any point less than this value will be hidden, 
    /// set it to null (or double.NaN) to use a value based on the smaller value in the chart.
    /// </summary>
    /// <value>
    /// The minimum value.
    /// </value>
    double? MinLimit { get; set; }

    /// <summary>
    /// Gets or sets the maximum value visible in the axis, any point greater than this value will be hidden, 
    /// set it null (or double.NaN) to use a value based on the greater value in the chart.
    /// </summary>
    /// <value>
    /// The maximum value.
    /// </value>
    double? MaxLimit { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the axis is inverted based on the Cartesian coordinate system.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is inverted; otherwise, <c>false</c>.
    /// </value>
    bool IsInverted { get; set; }

    /// <summary>
    /// Gets or sets the labels rotation in degrees.
    /// </summary>
    /// <value>
    /// The labels rotation.
    /// </value>
    double LabelsRotation { get; set; }

    /// <summary>
    /// Gets or sets the size of the labels.
    /// </summary>
    /// <value>
    /// The size of the text.
    /// </value>
    double TextSize { get; set; }

    /// <summary>
    /// Gets or sets the labels, if labels are not null, then the axis label will be pulled from the labels collection,
    /// the label is mapped to the chart based on the position of the label and the position of the point, both integers,
    /// if the axis requires a label outside the bounds of the labels collection, then the index will be returned as the label.
    /// Default value is null.
    /// </summary>
    /// <value>
    /// The labels.
    /// </value>
    IList<string>? Labels { get; set; }

    /// <summary>
    /// Gets or sets the animations speed, if this property is null, the
    /// <see cref="Chart.ActualAnimationsSpeed"/> property will be used.
    /// </summary>
    /// <value>
    /// The animations speed.
    /// </value>
    TimeSpan? AnimationsSpeed { get; set; }

    /// <summary>
    /// Gets or sets the easing function to animate the series, if this property is null, the
    /// <see cref="Chart.ActualEasingFunction"/> property will be used.
    /// </summary>
    /// <value>
    /// The easing function.
    /// </value>
    Func<float, float>? EasingFunction { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the separator lines are visible.
    /// </summary>
    bool ShowSeparatorLines { get; set; }

    /// <summary>
    /// Gets or sets the custom separators collection, when this property is set (not null)
    /// the axis will use the values in this collection instead of calculating the separators automatically.
    /// Defalut is null.
    /// </summary>
    IEnumerable<double>? CustomSeparators { get; set; }

    /// <summary>
    /// Gets or sets the name paint.
    /// </summary>
    /// <value>
    /// The text paint.
    /// </value>
    Paint? NamePaint { get; set; }

    /// <summary>
    /// Gets or sets the text paint.
    /// </summary>
    /// <value>
    /// The text paint.
    /// </value>
    Paint? LabelsPaint { get; set; }

    /// <summary>
    /// Gets or sets the separators paint.
    /// </summary>
    /// <value>
    /// The separators paint.
    /// </value>
    Paint? SeparatorsPaint { get; set; }

    /// <summary>
    /// Gets the size of the possible.
    /// </summary>
    /// <param name="chart">The chart.</param>
    /// <returns></returns>
    LvcSize GetPossibleSize(Chart chart);

    /// <summary>
    /// Gets the size of the axis name label.
    /// </summary>
    /// <param name="chart">the chart.</param>
    /// <returns></returns>
    LvcSize GetNameLabelSize(Chart chart);
}
