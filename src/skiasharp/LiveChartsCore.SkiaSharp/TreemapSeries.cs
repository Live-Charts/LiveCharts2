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
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;

namespace LiveChartsCore.SkiaSharpView;

/// <summary>
/// Defines a treemap series in the user interface.
/// </summary>
/// <typeparam name="TModel">The user's node type.</typeparam>
public class TreemapSeries<TModel>
    : TreemapSeries<TModel, RoundedRectangleGeometry, LabelGeometry>
        where TModel : class
{
    /// <summary>Initializes a new instance of the <see cref="TreemapSeries{TModel}"/> class.</summary>
    public TreemapSeries() : base() { }

    /// <summary>Initializes a new instance of the <see cref="TreemapSeries{TModel}"/> class with values.</summary>
    public TreemapSeries(IReadOnlyCollection<TModel>? values) : base(values) { }
}

/// <summary>
/// Defines a treemap series in the user interface, with a custom tile visual.
/// </summary>
public class TreemapSeries<TModel, TVisual>
    : TreemapSeries<TModel, TVisual, LabelGeometry>
        where TModel : class
        where TVisual : BoundedDrawnGeometry, new()
{
    /// <summary>Initializes a new instance of the <see cref="TreemapSeries{TModel, TVisual}"/> class.</summary>
    public TreemapSeries() : base() { }

    /// <summary>Initializes a new instance of the <see cref="TreemapSeries{TModel, TVisual}"/> class with values.</summary>
    public TreemapSeries(IReadOnlyCollection<TModel>? values) : base(values) { }
}

/// <summary>
/// Defines a treemap series in the user interface, with a custom tile visual
/// and label.
/// </summary>
public class TreemapSeries<TModel, TVisual, TLabel>
    : CoreTreemapSeries<TModel, TVisual, TLabel>
        where TModel : class
        where TVisual : BoundedDrawnGeometry, new()
        where TLabel : BaseLabelGeometry, new()
{
    static TreemapSeries()
    {
        LiveChartsSkiaSharp.EnsureInitialized();
    }

    /// <summary>Initializes a new instance of the <see cref="TreemapSeries{TModel, TVisual, TLabel}"/> class.</summary>
    public TreemapSeries() : base(null)
    {
        WireTreemapNodeDefaults();
    }

    /// <summary>Initializes a new instance of the <see cref="TreemapSeries{TModel, TVisual, TLabel}"/> class with values.</summary>
    public TreemapSeries(IReadOnlyCollection<TModel>? values) : base(values)
    {
        WireTreemapNodeDefaults();
    }

    private void WireTreemapNodeDefaults()
    {
        // First-use ergonomics for the built-in TreemapNode model: a fresh
        // TreemapSeries<TreemapNode> draws without the user wiring mappers
        // themselves. Series<>'s typed properties on a custom TModel are not
        // touched.
        if (typeof(TModel) != typeof(TreemapNode)) return;

        ValueMapper ??= m => ((TreemapNode)(object)m!).Value;
        ChildrenMapper ??= m =>
        {
            var c = ((TreemapNode)(object)m!).Children;
            if (c is null) return null;
            // Re-cast via IEnumerable<TModel> since TModel is TreemapNode here.
            var typed = (IEnumerable<TModel>)c;
            return typed;
        };
        LabelMapper ??= m => ((TreemapNode)(object)m!).Name;
    }
}
