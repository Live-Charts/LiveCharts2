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
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;

namespace LiveChartsCore.SkiaSharpView;

/// <summary>Defines a sankey series in the user interface.</summary>
public class SankeySeries<TModel>
    : SankeySeries<TModel, ColoredRoundedRectangleGeometry, LabelGeometry>
    where TModel : class
{
    /// <summary>Initializes a new instance of the <see cref="SankeySeries{TModel}"/> class.</summary>
    public SankeySeries() : base() { }

    /// <summary>Initializes a new instance with the given nodes.</summary>
    public SankeySeries(IReadOnlyCollection<TModel>? nodes) : base(nodes) { }
}

/// <summary>Defines a sankey series in the user interface with a custom node visual.</summary>
public class SankeySeries<TModel, TVisual>
    : SankeySeries<TModel, TVisual, LabelGeometry>
    where TModel : class
    where TVisual : BoundedDrawnGeometry, new()
{
    /// <summary>Initializes a new instance of the <see cref="SankeySeries{TModel, TVisual}"/> class.</summary>
    public SankeySeries() : base() { }

    /// <summary>Initializes a new instance with the given nodes.</summary>
    public SankeySeries(IReadOnlyCollection<TModel>? nodes) : base(nodes) { }
}

/// <summary>Defines a sankey series in the user interface with custom node visual and label.</summary>
public class SankeySeries<TModel, TVisual, TLabel>
    : CoreSankeySeries<TModel, TVisual, TLabel>
    where TModel : class
    where TVisual : BoundedDrawnGeometry, new()
    where TLabel : BaseLabelGeometry, new()
{
    static SankeySeries() => LiveChartsSkiaSharp.EnsureInitialized();

    /// <summary>Initializes a new instance of the <see cref="SankeySeries{TModel, TVisual, TLabel}"/> class.</summary>
    public SankeySeries() : base(null) { }

    /// <summary>Initializes a new instance with the given nodes.</summary>
    public SankeySeries(IReadOnlyCollection<TModel>? nodes) : base(nodes) { }

    /// <inheritdoc cref="CoreSankeySeries{TModel, TVisual, TLabel}.CreateRibbonVisual"/>
    protected override BaseSankeyRibbonGeometry CreateRibbonVisual() => new SankeyRibbonGeometry();

    /// <inheritdoc cref="CoreSankeySeries{TModel, TVisual, TLabel}.CreateArcNodeVisual"/>
    protected override BaseSankeyArcSegmentGeometry CreateArcNodeVisual() => new SankeyArcSegmentGeometry();

    /// <inheritdoc cref="CoreSankeySeries{TModel, TVisual, TLabel}.CreateChordRibbonVisual"/>
    protected override BaseSankeyChordRibbonGeometry CreateChordRibbonVisual() => new SankeyChordRibbonGeometry();
}
