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

using System.Collections.Generic;
using LiveChartsCore.Drawing;
using LiveChartsCore.Noesis.Drawing;
using LiveChartsCore.Noesis.Drawing.Geometries;
using LiveChartsCore.Noesis.Drawing.Layouts;
using LiveChartsCore.VisualElements;

namespace LiveChartsCore.Noesis.VisualElements;

/// <summary>
/// Defines the angular ticks visual.
/// </summary>
public class AngularTicksVisual : BaseAngularTicksVisual<ArcGeometry, LineGeometry, LabelGeometry>
{
    private readonly AbsoluteLayout _layout = new();

    /// <inheritdoc cref="Visual.DrawnElement"/>
    // The ticks are many geometries and a Visual draws one element, so they are hosted in a
    // layout. The layout stays at the origin: the ticks are positioned in absolute chart pixels
    // and a layout offsets its children by its own location.
    protected internal override IDrawnElement? DrawnElement => _layout;

    /// <inheritdoc cref="BaseAngularTicksVisual{TArcGeometry, TLineGeometry, TLabelGeometry}.SetChildren(IReadOnlyList{IDrawnElement})"/>
    protected override void SetChildren(IReadOnlyList<IDrawnElement> children)
    {
        _layout.Children.Clear();

        // The base class builds these from the geometry types this class closed over, so they are
        // all SkiaSharp geometries.
        foreach (var child in children)
            _layout.Children.Add((IDrawnElement<NoesisDrawingContext>)child);
    }
}
