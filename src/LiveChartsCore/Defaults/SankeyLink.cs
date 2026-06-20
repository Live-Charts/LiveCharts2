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

namespace LiveChartsCore.Defaults;

/// <summary>
/// A directed weighted edge in a sankey diagram. <typeparamref name="TNode"/>
/// is the user's node type — <see cref="Source"/> and <see cref="Target"/> are
/// references to actual node instances in the series's <c>Values</c>
/// collection (link-by-reference, matching the d3-sankey mental model).
/// </summary>
/// <typeparam name="TNode">The user's node type.</typeparam>
public class SankeyLink<TNode>
{
    /// <summary>Initializes a new instance of the <see cref="SankeyLink{TNode}"/> class.</summary>
    public SankeyLink() { Source = default!; Target = default!; }

    /// <summary>Initializes a new instance of the <see cref="SankeyLink{TNode}"/> class.</summary>
    public SankeyLink(TNode source, TNode target, double weight)
    {
        Source = source;
        Target = target;
        Weight = weight;
    }

    /// <summary>The originating node.</summary>
    public TNode Source { get; set; }

    /// <summary>The destination node.</summary>
    public TNode Target { get; set; }

    /// <summary>Flow magnitude; sets the ribbon's vertical thickness at both ends.</summary>
    public double Weight { get; set; }
}
