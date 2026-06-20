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
using LiveChartsCore.Drawing.Segments;

namespace LiveChartsCore;

/// <summary>
/// Defines the row series. All per-frame measure logic lives on the
/// <see cref="HorizontalBarSeries{TModel, TVisual, TLabel, TErrorGeometry}"/>
/// parent; this class exists for ABI continuity and so user-facing
/// <see cref="SkiaSharpView.RowSeries{TModel}"/> retains the expected generic
/// type hierarchy.
/// </summary>
/// <typeparam name="TModel">The type of the model.</typeparam>
/// <typeparam name="TVisual">The type of the visual.</typeparam>
/// <typeparam name="TLabel">The type of the label.</typeparam>
/// <typeparam name="TErrorGeometry">The type of the error geometry.</typeparam>
public abstract class CoreRowSeries<TModel, TVisual, TLabel, TErrorGeometry>
    : HorizontalBarSeries<TModel, TVisual, TLabel, TErrorGeometry>
        where TVisual : BoundedDrawnGeometry, new()
        where TLabel : BaseLabelGeometry, new()
        where TErrorGeometry : BaseLineGeometry, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CoreRowSeries{TModel, TVisual, TLabel, TErrorGeometry}"/> class.
    /// </summary>
    /// <param name="values">The values.</param>
    /// <param name="isStacked">if set to <c>true</c> [is stacked].</param>
    public CoreRowSeries(IReadOnlyCollection<TModel>? values, bool isStacked = false)
        : base(values, isStacked)
    {
    }
}
