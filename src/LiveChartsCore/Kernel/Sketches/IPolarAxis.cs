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
using LiveChartsCore.Measure;

namespace LiveChartsCore.Kernel.Sketches;

/// <summary>
/// Defines a polar axis.
/// </summary>
public interface IPolarAxis : IPlane
{
    /// <summary>
    /// Gets the orientation.
    /// </summary>
    /// <value>
    /// The orientation.
    /// </value>
    PolarAxisOrientation Orientation { get; }

    /// <summary>
    /// Gets or sets the ro, a reference used internally to calculate the axis position.
    /// </summary>
    /// <value>
    /// The ro.
    /// </value>
    float Ro { get; set; }

    /// <summary>
    /// Gets the angle in degrees of the line the labels are drawn along, 0 points right and the angle grows clockwise.
    /// The default, <see cref="double.NaN"/>, places them halfway between the first two spokes of the angle axis, so
    /// they never sit on a spoke, under the points of the series or on an angle label. This property has no effect
    /// when the axis has <see cref="PolarAxisOrientation.Angle"/> orientation.
    /// </summary>
    double LabelsAngle { get; set; }

    /// <summary>
    /// Gets or sets the labels vertical alignment.
    /// </summary>
    Align LabelsVerticalAlignment { get; set; }

    /// <summary>
    /// Gets or sets the labels horizontal alignment.
    /// </summary>
    Align LabelsHorizontalAlignment { get; set; }

    /// <summary>
    /// Gets or sets the labels padding.
    /// </summary>
    Padding LabelsPadding { get; set; }

    /// <summary>
    /// Gets or sets the labels background.
    /// </summary>
    LvcColor LabelsBackground { get; set; }

    /// <summary>
    /// Initializes the axis for the specified orientation.
    /// </summary>
    /// <param name="orientation">The orientation.</param>
    void Initialize(PolarAxisOrientation orientation);

    /// <summary>
    /// Occurs when the axis is initialized.
    /// </summary>
    event Action<IPolarAxis>? Initialized;
}
