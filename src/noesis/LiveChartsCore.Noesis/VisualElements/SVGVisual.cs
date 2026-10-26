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
using LiveChartsCore.Noesis.Drawing.Geometries;
using LiveChartsCore.VisualElements;
using LiveChartsCore.Drawing;

namespace LiveChartsCore.Noesis.VisualElements;

/// <summary>
/// Defines a visual element in a chart that draws a svg geometry in the user interface.
/// </summary>
[Obsolete($"Replaced by {nameof(Visual)}.")]
public class SVGVisual : GeometryVisual<VariableSVGPathGeometry>
{
    /// <summary>
    /// Gets or sets the SVG path.
    /// </summary>
    public global::Noesis.Geometry? Path { get; set => SetProperty(ref field, value); }

    /// <inheritdoc cref="VisualElement.OnInvalidated(Chart)"/>
    protected internal override void OnInvalidated(Chart chart)
    {
        base.OnInvalidated(chart);
        if (_geometry is not null) _geometry.Path = Path;
    }
}
