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
using LiveChartsCore.Painting;
using LiveChartsCore.Noesis.Drawing.Geometries;
using LiveChartsCore.Themes;
using LiveChartsCore.VisualElements;

namespace LiveChartsCore.Noesis.VisualElements;

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
    private Paint? _paint;

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

        // A paint that came in on the geometry is an explicit choice, but it was not made
        // through the Paint property, so route it through now: otherwise the theme reads it
        // as unset and Measure overwrites it with the themed paint.
        if (labelGeometry.Paint is not null) Paint = labelGeometry.Paint;
    }

    /// <summary>
    /// Gets the underlying <see cref="LabelGeometry"/>. Use this to read or
    /// mutate label properties such as
    /// <see cref="BaseLabelGeometry.Text"/>,
    /// <see cref="BaseLabelGeometry.TextSize"/> or
    /// <see cref="BaseLabelGeometry.Padding"/> after construction.
    /// </summary>
    /// <remarks>
    /// Prefer <see cref="Paint"/> over <see cref="BaseLabelGeometry.Paint"/>: a paint set here
    /// is invisible to the theme, which then can not tell it apart from an unset paint and
    /// overwrites it.
    /// </remarks>
    public LabelGeometry Label => _drawnElement;

    /// <summary>
    /// Gets or sets the paint used to draw the label, when null the theme sets it.
    /// </summary>
    public Paint? Paint
    {
        get => _paint;
        set
        {
            SetPaintProperty(ref _paint, value, PaintStyle.Text);

            // Forward right away instead of on measure: the geometry paint is what the label is
            // drawn and measured with, and the chart measures the title before it invalidates it.
            _drawnElement.Paint = _paint;
        }
    }

    /// <inheritdoc cref="Visual.DrawnElement"/>
    protected internal override IDrawnElement? DrawnElement => _drawnElement;

    /// <inheritdoc cref="Visual.ApplyStyle(Theme)"/>
    protected override void ApplyStyle(Theme theme) =>
        theme.ApplyStyleTo<DrawnLabelVisual>(this);

    /// <inheritdoc cref="Visual.Measure(Chart)"/>
    protected override void Measure(Chart chart)
    {

    }
}
