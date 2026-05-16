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
using LiveChartsCore.Measure;
using LiveChartsCore.Painting;

namespace LiveChartsCore.Kernel.Sketches;

/// <summary>
/// Defines an Axis in a Cartesian chart.
/// </summary>
public interface ICartesianAxis : IPlane, INotifyPropertyChanged
{
    /// <summary>
    /// Gets the orientation.
    /// </summary>
    /// <value>
    /// The orientation.
    /// </value>
    AxisOrientation Orientation { get; }

    /// <summary>
    /// Gets or sets the padding around the tick labels along the axis.
    /// </summary>
    /// <value>
    /// The padding in pixels.
    /// </value>
    Padding Padding { get; set; }

    /// <summary>
    /// Gets or sets the xo, a reference used internally to calculate the axis position.
    /// </summary>
    /// <value>
    /// The xo.
    /// </value>
    float Xo { get; set; }

    /// <summary>
    /// Gets or sets the yo, a reference used internally to calculate the axis position.
    /// </summary>
    /// <value>
    /// The yo.
    /// </value>
    float Yo { get; set; }

    /// <summary>
    /// Gets or sets the size of the axis, this value is used internally to calculate the axis position.
    /// </summary>
    /// <value>
    /// The length.
    /// </value>
    LvcSize Size { get; set; }

    /// <summary>
    /// Gets or sets the labels density, it is a factor that determines the distance between labels when calculated
    /// by the library, 0 is the most dense any value greater than 0 will make the labels to be more separated,
    /// values less than 0 will make the labels to overlap (labels rotation could prevent overlapping).
    /// Default value is 0.85.
    /// </summary>
    float LabelsDensity { get; set; }

    /// <summary>
    /// Gets or sets the min zoom delta, the minimum difference between the max and min visible limits of the axis.
    /// default is null and null means that the library will calculate this value based on the current data.
    /// </summary>
    double? MinZoomDelta { get; set; }

    /// <summary>
    /// Gets or sets the distance that the axis can be panned beyond its limits, the unit is percentage of the chart
    /// width or height depending on the axis orientation, default is 0.25 which means that the axis can be panned
    /// 25% of the chart width or height beyond its limits.
    /// </summary>
    double BouncingDistance { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the ticks are centered to the <see cref="IPlane.UnitWidth"/>, default is true.
    /// </summary>
    bool TicksAtCenter { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the separators are centered to the <see cref="IPlane.UnitWidth"/>, default is true.
    /// </summary>
    bool SeparatorsAtCenter { get; set; }

    /// <summary>
    /// Gets or sets the reserved area for the labels.
    /// </summary>
    LvcRectangle LabelsDesiredSize { get; set; }

    /// <summary>
    /// Gets or sets the max label size, this value is used internally to measure the axis.
    /// </summary>
    LvcSize PossibleMaxLabelSize { get; }

    /// <summary>
    /// Gets or sets the reserved area for the name.
    /// </summary>
    LvcRectangle NameDesiredSize { get; set; }

    /// <summary>
    /// Places the title in the same direction as the axis, default is false.
    /// </summary>
    bool InLineNamePlacement { get; set; }

    /// <summary>
    /// Gets or sets the labels alignment, default is null and means that the library will set it based on the
    /// <see cref="Orientation"/> and <see cref="Position"/> properties.
    /// </summary>
    Align? LabelsAlignment { get; set; }

    /// <summary>
    /// Gets or sets the axis position.
    /// </summary>
    /// <value>
    /// The position.
    /// </value>
    AxisPosition Position { get; set; }

    /// <summary>
    /// Gets or sets the shared axes collection, useful to share the zooming an panning between several charts.
    /// </summary>
    IEnumerable<ICartesianAxis>? SharedWith { get; set; }

    /// <summary>
    /// Gets or sets the sub-separators paint.
    /// </summary>
    /// <value>
    /// The separators paint.
    /// </value>
    Paint? SubseparatorsPaint { get; set; }

    /// <summary>
    /// Gets or sets the number of subseparators to draw.
    /// </summary>
    int SubseparatorsCount { get; set; }

    /// <summary>
    /// Gets or sets whether the ticks path should be drawn.
    /// </summary>
    bool DrawTicksPath { get; set; }

    /// <summary>
    /// Gets or sets the separators paint.
    /// </summary>
    /// <value>
    /// The separators paint.
    /// </value>
    Paint? TicksPaint { get; set; }

    /// <summary>
    /// Gets or sets the separators paint.
    /// </summary>
    /// <value>
    /// The separators paint.
    /// </value>
    Paint? SubticksPaint { get; set; }

    /// <summary>
    /// Gets or sets the zero paint.
    /// </summary>
    /// <value>
    /// The separators paint.
    /// </value>
    Paint? ZeroPaint { get; set; }

    /// <summary>
    /// Gets or sets the crosshair paint.
    /// </summary>
    /// <value>
    /// The separators paint.
    /// </value>
    Paint? CrosshairPaint { get; set; }

    /// <summary>
    /// Gets or sets the crosshair labels paint.
    /// </summary>
    /// <value>
    /// The separators paint.
    /// </value>
    Paint? CrosshairLabelsPaint { get; set; }

    /// <summary>
    /// Gets or sets the crosshair background.
    /// </summary>
    /// <value>
    /// The separators paint.
    /// </value>
    LvcColor? CrosshairLabelsBackground { get; set; }

    /// <summary>
    /// Gets or sets the crosshair labels padding.
    /// </summary>
    Padding? CrosshairPadding { get; set; }

    /// <summary>
    /// Gets or sets whether the crosshair snaps to nearest series.
    /// </summary>
    bool CrosshairSnapEnabled { get; set; }

    /// <summary>
    /// Called when the axis measure starts.
    /// </summary>
    /// <param name="chart">The chart.</param>
    /// <param name="orientation">The orientation.</param>
    void OnMeasureStarted(Chart chart, AxisOrientation orientation);

    /// <summary>
    /// Occurs when the axis measure starts.
    /// </summary>
    event Action<Chart, ICartesianAxis>? MeasureStarted;

    /// <summary>
    /// Gets the axis limits considering its own and the <see cref="SharedWith"/> axes.
    /// </summary>
    /// <returns>The limits.</returns>
    AxisLimit GetLimits();

    /// <summary>
    /// Sets the current view limits (own and shared). This is the engine/runtime path used by zoom
    /// and pan; it does <b>not</b> record a user pin. To pin a range that zoom-out and post-zoom fit
    /// must respect as the outer rail, set <see cref="IPlane.MinLimit"/> / <see cref="IPlane.MaxLimit"/>
    /// instead.
    /// </summary>
    /// <param name="min">The min limit.</param>
    /// <param name="max">The max limit.</param>
    /// <param name="step">The separator step.</param>
    /// <param name="notify">if set to <c>true</c> notify the changes.</param>
    /// <param name="propagateShared">if set to <c>true</c> propagate the changes to the shared axes.</param>
    void SetLimits(double min, double max, double step = -1, bool propagateShared = true, bool notify = true);

    /// <summary>
    /// Invalidates the crosshair visual.
    /// </summary>
    /// <param name="chart">The chart.</param>
    /// <param name="pointerPosition">The pointer position</param>
    void InvalidateCrosshair(Chart chart, LvcPoint pointerPosition);

    /// <summary>
    /// Clears the crosshair visual.
    /// </summary>
    /// <param name="chart">The chart.</param>
    void ClearCrosshair(Chart chart);

    /// <summary>
    /// Sets the log base.
    /// </summary>
    /// <param name="newBase">The new base.</param>
    void SetLogBase(double newBase);
}
