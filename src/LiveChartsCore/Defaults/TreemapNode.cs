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

namespace LiveChartsCore.Defaults;

/// <summary>
/// A built-in hierarchical model for treemap series. Leaves carry the
/// numeric weight via <see cref="Value"/>; internal nodes either carry an
/// explicit aggregate value or leave it as <c>0</c>, in which case the
/// series rolls children up automatically.
/// </summary>
public class TreemapNode
{
    /// <summary>Initializes a new instance of the <see cref="TreemapNode"/> class.</summary>
    public TreemapNode() { }

    /// <summary>
    /// Initializes a new leaf <see cref="TreemapNode"/> with the given value
    /// and optional name.
    /// </summary>
    public TreemapNode(double value, string? name = null)
    {
        Value = value;
        Name = name;
    }

    /// <summary>
    /// Initializes a new internal <see cref="TreemapNode"/> with children and
    /// an optional name. The aggregate value is rolled up from the children.
    /// </summary>
    public TreemapNode(string? name, IEnumerable<TreemapNode> children)
    {
        Name = name;
        Children = children;
    }

    /// <summary>The weight of the node (typically only set on leaves).</summary>
    public double Value { get; set; }

    /// <summary>An optional human-readable name; surfaced via the label mapper.</summary>
    public string? Name { get; set; }

    /// <summary>The children of this node, or <c>null</c> for a leaf.</summary>
    public IEnumerable<TreemapNode>? Children { get; set; }
}
