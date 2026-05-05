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

using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.VisualElements;

namespace LiveChartsCore.SkiaSharpView.VisualElements;

/// <summary>
/// Defines a label visual element.
/// </summary>
public class DrawnLabelVisual : Visual
{
    private readonly LabelGeometry _drawnElement = new()
    {
        HorizontalAlign = Align.Start,
        VerticalAlign = Align.Start
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="DrawnLabelVisual"/> class.
    /// </summary>
    public DrawnLabelVisual()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DrawnLabelVisual"/> class.
    /// </summary>
    /// <param name="labelGeometry">The label.</param>
    public DrawnLabelVisual(LabelGeometry labelGeometry)
    {
        // Chart.AddTitleToChart positions the title at (X, 0) and expects the
        // bbox top-left at that point. BaseLabelGeometry defaults to Align.Middle,
        // which would center the bbox on Y=0 and clip the top half.
        labelGeometry.HorizontalAlign = Align.Start;
        labelGeometry.VerticalAlign = Align.Start;
        _drawnElement = labelGeometry;
    }

    /// <inheritdoc cref="Visual.DrawnElement"/>
    protected internal override IDrawnElement? DrawnElement => _drawnElement;

    /// <inheritdoc cref="Visual.Measure(Chart)"/>
    protected override void Measure(Chart chart)
    {

    }
}
