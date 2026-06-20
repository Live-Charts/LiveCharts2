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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;

namespace LiveChartsCore;

/// <summary>
/// d3-sankey layout (Bostock 2012 / Riehmann 2005). Assigns each node a
/// column based on longest-path depth, distributes nodes vertically within
/// each column with iterative sweep-relaxation to minimize ribbon crossings,
/// then computes per-link Y bands at each end.
///
/// Output is consumed by <see cref="CoreSankeySeries{TNode, TVisual, TLabel}"/>;
/// the algorithm itself is pure-functional (no chart references) so it can be
/// snapshot-tested independently and reused if a second sankey-shaped series
/// family ever appears.
///
/// Assumes a DAG. Self-loops are silently dropped. Cycles throw —
/// detecting + breaking them automatically is out of scope for v1 (a real
/// sankey diagram with cycles is almost always a data bug).
/// </summary>
internal static class SankeyLayout
{
    public sealed class Result<TNode> where TNode : notnull
    {
        public Result(
            Dictionary<TNode, NodeBox> nodes,
            List<RibbonBox<TNode>> links)
        {
            Nodes = nodes;
            Links = links;
        }
        public IReadOnlyDictionary<TNode, NodeBox> Nodes { get; }
        public IReadOnlyList<RibbonBox<TNode>> Links { get; }
    }

    public readonly struct NodeBox
    {
        public NodeBox(float x, float y, float width, float height, double value)
        { X = x; Y = y; Width = width; Height = height; Value = value; }
        public float X { get; }
        public float Y { get; }
        public float Width { get; }
        public float Height { get; }
        /// <summary>Layout-computed node value (max of incoming + outgoing weight sums).</summary>
        public double Value { get; }
    }

    public readonly struct RibbonBox<TNode> where TNode : notnull
    {
        public RibbonBox(
            SankeyLink<TNode> link,
            float sourceX, float sourceY0, float sourceY1,
            float targetX, float targetY0, float targetY1)
        {
            Link = link;
            SourceX = sourceX; SourceY0 = sourceY0; SourceY1 = sourceY1;
            TargetX = targetX; TargetY0 = targetY0; TargetY1 = targetY1;
        }
        public SankeyLink<TNode> Link { get; }
        public float SourceX { get; }
        public float SourceY0 { get; }
        public float SourceY1 { get; }
        public float TargetX { get; }
        public float TargetY0 { get; }
        public float TargetY1 { get; }
    }

    /// <summary>Result of <see cref="ComputeBipartiteArc"/> — same shape as
    /// <see cref="Result{TNode}"/> but with arc-segment node boxes and chord
    /// ribbon endpoints.</summary>
    public sealed class ArcResult<TNode> where TNode : notnull
    {
        public ArcResult(
            Dictionary<TNode, ArcNodeBox> nodes,
            List<ChordRibbonBox<TNode>> links)
        {
            Nodes = nodes;
            Links = links;
        }
        public IReadOnlyDictionary<TNode, ArcNodeBox> Nodes { get; }
        public IReadOnlyList<ChordRibbonBox<TNode>> Links { get; }
    }

    /// <summary>Per-node arc placement: annular sector centered at (CenterX,
    /// CenterY), from InnerRadius to OuterRadius, drawn clockwise from
    /// StartAngle for SweepAngle degrees (Skia convention).</summary>
    public readonly struct ArcNodeBox
    {
        public ArcNodeBox(float cx, float cy, float innerR, float outerR, float startAngle, float sweepAngle, double value)
        {
            CenterX = cx; CenterY = cy;
            InnerRadius = innerR; OuterRadius = outerR;
            StartAngle = startAngle; SweepAngle = sweepAngle;
            Value = value;
        }
        public float CenterX { get; }
        public float CenterY { get; }
        public float InnerRadius { get; }
        public float OuterRadius { get; }
        public float StartAngle { get; }
        public float SweepAngle { get; }
        /// <summary>Layout-computed node value (max of incoming + outgoing weight sums).</summary>
        public double Value { get; }
    }

    /// <summary>Per-link chord placement: 4 cartesian endpoints on inner-arc
    /// chords (two per node, in band order) plus the chart center used as
    /// the cubic-Bezier control attractor.</summary>
    public readonly struct ChordRibbonBox<TNode> where TNode : notnull
    {
        public ChordRibbonBox(
            SankeyLink<TNode> link,
            float centerX, float centerY,
            float sp0X, float sp0Y, float sp1X, float sp1Y,
            float tp0X, float tp0Y, float tp1X, float tp1Y)
        {
            Link = link;
            CenterX = centerX; CenterY = centerY;
            SourceP0X = sp0X; SourceP0Y = sp0Y;
            SourceP1X = sp1X; SourceP1Y = sp1Y;
            TargetP0X = tp0X; TargetP0Y = tp0Y;
            TargetP1X = tp1X; TargetP1Y = tp1Y;
        }
        public SankeyLink<TNode> Link { get; }
        public float CenterX { get; }
        public float CenterY { get; }
        public float SourceP0X { get; }
        public float SourceP0Y { get; }
        public float SourceP1X { get; }
        public float SourceP1Y { get; }
        public float TargetP0X { get; }
        public float TargetP0Y { get; }
        public float TargetP1X { get; }
        public float TargetP1Y { get; }
    }

    public static Result<TNode> Compute<TNode>(
        IEnumerable<TNode> nodes,
        IEnumerable<SankeyLink<TNode>> links,
        LvcRectangle rect,
        float nodeWidth,
        float nodePadding,
        int iterations) where TNode : notnull
    {
        var nodeList = new List<TNode>();
        var byNode = new Dictionary<TNode, _State<TNode>>(ReferenceComparer<TNode>.Instance);

        foreach (var n in nodes)
        {
            if (n is null) continue;
            if (byNode.ContainsKey(n)) continue;
            var s = new _State<TNode>(n);
            byNode[n] = s;
            nodeList.Add(n);
        }

        var keepLinks = new List<SankeyLink<TNode>>();
        foreach (var l in links)
        {
            if (l is null) continue;
            if (l.Source is null || l.Target is null) continue;
            if (ReferenceComparer<TNode>.Instance.Equals(l.Source, l.Target)) continue; // self-loop
            if (!byNode.TryGetValue(l.Source, out var src)) continue;
            if (!byNode.TryGetValue(l.Target, out var tgt)) continue;
            if (l.Weight <= 0 || double.IsNaN(l.Weight) || double.IsInfinity(l.Weight)) continue;
            src.Outgoing.Add(l);
            tgt.Incoming.Add(l);
            keepLinks.Add(l);
        }

        if (nodeList.Count == 0 || rect.Size.Width <= 0 || rect.Size.Height <= 0)
            return new Result<TNode>(
                new Dictionary<TNode, NodeBox>(ReferenceComparer<TNode>.Instance),
                []);

        // --- depth assignment (longest path from sources) ----------------
        // Kahn-style BFS: start from nodes with no incoming, propagate.
        // We detect a cycle by tracking how many nodes never reach the queue.
        var inDegree = new Dictionary<TNode, int>(ReferenceComparer<TNode>.Instance);
        foreach (var n in nodeList) inDegree[n] = byNode[n].Incoming.Count;

        var queue = new Queue<TNode>();
        foreach (var n in nodeList)
            if (inDegree[n] == 0)
            {
                byNode[n].Depth = 0;
                queue.Enqueue(n);
            }

        var processed = 0;
        while (queue.Count > 0)
        {
            var n = queue.Dequeue();
            processed++;
            var s = byNode[n];
            foreach (var link in s.Outgoing)
            {
                var tgt = byNode[link.Target];
                if (s.Depth + 1 > tgt.Depth) tgt.Depth = s.Depth + 1;
                inDegree[link.Target]--;
                if (inDegree[link.Target] == 0) queue.Enqueue(link.Target);
            }
        }
        if (processed < nodeList.Count)
            throw new InvalidOperationException(
                "SankeyLayout: graph contains a cycle. v1 only supports acyclic graphs.");

        var maxDepth = 0;
        foreach (var n in nodeList)
            if (byNode[n].Depth > maxDepth) maxDepth = byNode[n].Depth;

        // --- node values (max of in/out weight sums) ---------------------
        foreach (var n in nodeList)
        {
            var s = byNode[n];
            var inSum = 0.0;
            foreach (var l in s.Incoming) inSum += l.Weight;
            var outSum = 0.0;
            foreach (var l in s.Outgoing) outSum += l.Weight;
            s.Value = inSum > outSum ? inSum : outSum;
            if (s.Value <= 0) s.Value = 1; // pure-sink/pure-source isolated nodes still need a slot
        }

        // --- per-column buckets ------------------------------------------
        var columns = new List<List<TNode>>(maxDepth + 1);
        for (var i = 0; i <= maxDepth; i++) columns.Add([]);
        foreach (var n in nodeList) columns[byNode[n].Depth].Add(n);

        // --- column X positions ------------------------------------------
        // Clamp dx to 0 when nodeWidth >= availW — otherwise dx goes negative and
        // later columns shift left of earlier ones, also breaking the SourceX <=
        // TargetX invariant the ribbon geometry's Measure() relies on.
        var availW = rect.Size.Width;
        var dx = maxDepth > 0 ? (availW - nodeWidth) / maxDepth : 0;
        if (dx < 0) dx = 0;
        for (var d = 0; d <= maxDepth; d++)
            foreach (var n in columns[d]) byNode[n].X = rect.Location.X + d * dx;

        // --- vertical scale ----------------------------------------------
        // Each column's required height = sum(node values) * scale + padding
        // between siblings. Pick scale so the densest column fits.
        var maxColumnSum = 0.0;
        var maxColumnNodes = 0;
        foreach (var col in columns)
        {
            var sum = 0.0;
            foreach (var n in col) sum += byNode[n].Value;
            if (sum > maxColumnSum) maxColumnSum = sum;
            if (col.Count > maxColumnNodes) maxColumnNodes = col.Count;
        }
        var availH = rect.Size.Height - nodePadding * Math.Max(0, maxColumnNodes - 1);
        if (availH < 1) availH = 1;
        var yScale = (float)(availH / maxColumnSum);

        foreach (var n in nodeList)
            byNode[n].Height = (float)(byNode[n].Value * yScale);

        // --- initial Y stacking ------------------------------------------
        foreach (var col in columns)
        {
            var y = rect.Location.Y;
            foreach (var n in col)
            {
                var s = byNode[n];
                s.Y = y;
                y += s.Height + nodePadding;
            }
        }

        // --- sweep relaxation --------------------------------------------
        // Alternating left-to-right and right-to-left sweeps. Each sweep
        // shifts each node toward the weighted Y center of its neighbors
        // (weights = link weights), then enforces no-overlap via stack-style
        // push apart. Alpha decays so later iterations converge.
        var alpha = 1f;
        for (var iter = 0; iter < iterations; iter++)
        {
            alpha *= 0.99f;
            _relaxToTargets(byNode, columns, alpha);
            _resolveCollisions(byNode, columns, rect, nodePadding);
            _relaxToSources(byNode, columns, alpha);
            _resolveCollisions(byNode, columns, rect, nodePadding);
        }

        // --- per-link Y offsets ------------------------------------------
        // Sort each node's outgoing links by target.Y so ribbons don't cross
        // at the source side. Same for incoming sorted by source.Y at the
        // target side. Then stack the link bands within the node's height.
        foreach (var n in nodeList)
        {
            var s = byNode[n];
            s.Outgoing.Sort((a, b) => byNode[a.Target].Y.CompareTo(byNode[b.Target].Y));
            s.Incoming.Sort((a, b) => byNode[a.Source].Y.CompareTo(byNode[b.Source].Y));
        }

        var nodeBoxes = new Dictionary<TNode, NodeBox>(ReferenceComparer<TNode>.Instance);
        foreach (var n in nodeList)
        {
            var s = byNode[n];
            nodeBoxes[n] = new NodeBox(s.X, s.Y, nodeWidth, s.Height, s.Value);
        }

        var ribbons = new List<RibbonBox<TNode>>(keepLinks.Count);
        var outRunning = new Dictionary<TNode, float>(ReferenceComparer<TNode>.Instance);
        var inRunning = new Dictionary<TNode, float>(ReferenceComparer<TNode>.Instance);
        foreach (var n in nodeList)
        {
            var s = byNode[n];
            outRunning[n] = s.Y;
            inRunning[n] = s.Y;
        }
        foreach (var n in nodeList)
        {
            var s = byNode[n];
            foreach (var link in s.Outgoing)
            {
                var tgt = byNode[link.Target];
                var thickness = (float)(link.Weight * yScale);
                var sx = s.X + nodeWidth;
                var sy0 = outRunning[n];
                var sy1 = sy0 + thickness;
                outRunning[n] = sy1;

                var tx = tgt.X;
                var ty0 = inRunning[link.Target];
                var ty1 = ty0 + thickness;
                inRunning[link.Target] = ty1;

                ribbons.Add(new RibbonBox<TNode>(link, sx, sy0, sy1, tx, ty0, ty1));
            }
        }

        return new Result<TNode>(nodeBoxes, ribbons);
    }

    /// <summary>
    /// BipartiteArc layout: every node lies on one of two arcs (source nodes
    /// on a left arc centered at 180°, target nodes on a right arc centered
    /// at 0°), with ribbons curving through the chart center as cubic Beziers
    /// with control point at center. Returns angular node spans + the four
    /// chord endpoints per ribbon, all in cartesian coordinates derived from
    /// the inner radius of each node's arc segment.
    ///
    /// Throws <see cref="InvalidOperationException"/> if the graph has more
    /// than two depth columns — caller is responsible for catching upstream
    /// and surfacing a clear API error.
    /// </summary>
    public static ArcResult<TNode> ComputeBipartiteArc<TNode>(
        IEnumerable<TNode> nodes,
        IEnumerable<SankeyLink<TNode>> links,
        LvcRectangle rect,
        float nodeWidth,
        float nodePadding,
        float arcSpanDegrees,
        float labelMargin) where TNode : notnull
    {
        var nodeList = new List<TNode>();
        var byNode = new Dictionary<TNode, _State<TNode>>(ReferenceComparer<TNode>.Instance);

        foreach (var n in nodes)
        {
            if (n is null) continue;
            if (byNode.ContainsKey(n)) continue;
            var s = new _State<TNode>(n);
            byNode[n] = s;
            nodeList.Add(n);
        }

        var keepLinks = new List<SankeyLink<TNode>>();
        foreach (var l in links)
        {
            if (l is null) continue;
            if (l.Source is null || l.Target is null) continue;
            if (ReferenceComparer<TNode>.Instance.Equals(l.Source, l.Target)) continue;
            if (!byNode.TryGetValue(l.Source, out var src)) continue;
            if (!byNode.TryGetValue(l.Target, out var tgt)) continue;
            if (l.Weight <= 0 || double.IsNaN(l.Weight) || double.IsInfinity(l.Weight)) continue;
            src.Outgoing.Add(l);
            tgt.Incoming.Add(l);
            keepLinks.Add(l);
        }

        if (nodeList.Count == 0 || rect.Size.Width <= 0 || rect.Size.Height <= 0)
            return new ArcResult<TNode>(
                new Dictionary<TNode, ArcNodeBox>(ReferenceComparer<TNode>.Instance),
                []);

        // Depth assignment — same Kahn BFS as Compute, then assert exactly 2
        // columns. Pass-through nodes (any node that is both target of some
        // link and source of another) push maxDepth > 1 and are rejected with
        // a clear message; users wanting multi-column flows must stay on
        // Vertical layout.
        var inDegree = new Dictionary<TNode, int>(ReferenceComparer<TNode>.Instance);
        foreach (var n in nodeList) inDegree[n] = byNode[n].Incoming.Count;

        var queue = new Queue<TNode>();
        foreach (var n in nodeList)
            if (inDegree[n] == 0)
            {
                byNode[n].Depth = 0;
                queue.Enqueue(n);
            }

        var processed = 0;
        while (queue.Count > 0)
        {
            var n = queue.Dequeue();
            processed++;
            var s = byNode[n];
            foreach (var link in s.Outgoing)
            {
                var tgt = byNode[link.Target];
                if (s.Depth + 1 > tgt.Depth) tgt.Depth = s.Depth + 1;
                inDegree[link.Target]--;
                if (inDegree[link.Target] == 0) queue.Enqueue(link.Target);
            }
        }
        if (processed < nodeList.Count)
            throw new InvalidOperationException(
                "SankeyLayout: graph contains a cycle. v1 only supports acyclic graphs.");

        var maxDepth = 0;
        foreach (var n in nodeList)
            if (byNode[n].Depth > maxDepth) maxDepth = byNode[n].Depth;

        if (maxDepth > 1)
            throw new InvalidOperationException(
                "SankeyLayout.BipartiteArc: graph has more than two depth columns. " +
                "BipartiteArc requires strictly bipartite data — every node must be " +
                "either pure source (no incoming) or pure sink (no outgoing). " +
                "Use SankeyLayoutKind.Vertical for multi-column flows.");

        foreach (var n in nodeList)
        {
            var s = byNode[n];
            var inSum = 0.0;
            foreach (var l in s.Incoming) inSum += l.Weight;
            var outSum = 0.0;
            foreach (var l in s.Outgoing) outSum += l.Weight;
            s.Value = inSum > outSum ? inSum : outSum;
            if (s.Value <= 0) s.Value = 1;
        }

        var leftCol = new List<TNode>();
        var rightCol = new List<TNode>();
        foreach (var n in nodeList)
        {
            if (byNode[n].Depth == 0) leftCol.Add(n);
            else rightCol.Add(n);
        }

        // Geometry of the chart-center ellipse: pick a circle whose radius
        // fits both axes minus the label margin (reserved by the caller via
        // labelMargin). NodeWidth is the radial thickness, so innerR =
        // outerR - nodeWidth. Ribbons attach to innerR.
        var cx = rect.Location.X + rect.Size.Width * 0.5f;
        var cy = rect.Location.Y + rect.Size.Height * 0.5f;
        var avail = (rect.Size.Width < rect.Size.Height ? rect.Size.Width : rect.Size.Height) * 0.5f;
        var outerR = avail - labelMargin;
        if (outerR < nodeWidth + 1) outerR = nodeWidth + 1;
        var innerR = outerR - nodeWidth;
        if (innerR < 1) innerR = 1;

        // Convert nodePadding (px) to an angular padding on the inner radius;
        // matches the "gap reads as N pixels regardless of arc size" intent
        // of the existing NodePadding property.
        const float toDeg = (float)(180.0 / System.Math.PI);
        var angularPadDeg = (nodePadding / innerR) * toDeg;

        var nodeBoxes = new Dictionary<TNode, ArcNodeBox>(ReferenceComparer<TNode>.Instance);
        var nodeAngular = new Dictionary<TNode, _ArcSpan>(ReferenceComparer<TNode>.Instance);

        // Left arc: visual top = 180+ArcSpan/2 (Skia angle), going down =
        // counterclockwise = angle decreases. Right arc: visual top =
        // -ArcSpan/2, going down = clockwise = angle increases.
        _LayoutColumnOnArc(leftCol, byNode, cx, cy, innerR, outerR,
            visualTopAngleDeg: 180f + arcSpanDegrees * 0.5f, direction: -1,
            arcSpanDegrees, angularPadDeg, nodeBoxes, nodeAngular);
        _LayoutColumnOnArc(rightCol, byNode, cx, cy, innerR, outerR,
            visualTopAngleDeg: -arcSpanDegrees * 0.5f, direction: +1,
            arcSpanDegrees, angularPadDeg, nodeBoxes, nodeAngular);

        // Sort outgoing/incoming so chord bands stack in target's visual
        // order at the source, and source's visual order at the target —
        // direct port of the vertical sweep's "sort by other-end Y" trick,
        // adapted to "sort by other-end cartesian Y on the inner arc."
        foreach (var n in nodeList)
        {
            var s = byNode[n];
            s.Outgoing.Sort((a, b) => _VisualTopY(nodeAngular[a.Target], cy, innerR)
                .CompareTo(_VisualTopY(nodeAngular[b.Target], cy, innerR)));
            s.Incoming.Sort((a, b) => _VisualTopY(nodeAngular[a.Source], cy, innerR)
                .CompareTo(_VisualTopY(nodeAngular[b.Source], cy, innerR)));
        }

        var ribbons = new List<ChordRibbonBox<TNode>>(keepLinks.Count);
        var outAcc = new Dictionary<TNode, double>(ReferenceComparer<TNode>.Instance);
        var inAcc = new Dictionary<TNode, double>(ReferenceComparer<TNode>.Instance);
        foreach (var n in nodeList) { outAcc[n] = 0; inAcc[n] = 0; }

        foreach (var n in nodeList)
        {
            var s = byNode[n];
            var spanN = nodeAngular[n];
            foreach (var link in s.Outgoing)
            {
                var tgt = byNode[link.Target];
                var spanT = nodeAngular[link.Target];

                // Source-side band: a fractional slice of n's angular extent,
                // proportional to link.Weight / n.Value. Direction = n's
                // arc direction.
                var bandFracS = (float)(link.Weight / s.Value);
                var bandSweepS = spanN.SweepMag * bandFracS;
                var sBandStart = spanN.VisualTopAngleDeg + spanN.Direction * (float)outAcc[n];
                var sBandEnd = sBandStart + spanN.Direction * bandSweepS;
                outAcc[n] += bandSweepS;

                var bandFracT = (float)(link.Weight / tgt.Value);
                var bandSweepT = spanT.SweepMag * bandFracT;
                var tBandStart = spanT.VisualTopAngleDeg + spanT.Direction * (float)inAcc[link.Target];
                var tBandEnd = tBandStart + spanT.Direction * bandSweepT;
                inAcc[link.Target] += bandSweepT;

                // Chord endpoints on inner arc, in Skia cartesian.
                const float toRad = (float)(System.Math.PI / 180);
                var sp0x = cx + (float)System.Math.Cos(sBandStart * toRad) * innerR;
                var sp0y = cy + (float)System.Math.Sin(sBandStart * toRad) * innerR;
                var sp1x = cx + (float)System.Math.Cos(sBandEnd * toRad) * innerR;
                var sp1y = cy + (float)System.Math.Sin(sBandEnd * toRad) * innerR;
                var tp0x = cx + (float)System.Math.Cos(tBandStart * toRad) * innerR;
                var tp0y = cy + (float)System.Math.Sin(tBandStart * toRad) * innerR;
                var tp1x = cx + (float)System.Math.Cos(tBandEnd * toRad) * innerR;
                var tp1y = cy + (float)System.Math.Sin(tBandEnd * toRad) * innerR;

                ribbons.Add(new ChordRibbonBox<TNode>(
                    link, cx, cy,
                    sp0x, sp0y, sp1x, sp1y,
                    tp0x, tp0y, tp1x, tp1y));
            }
        }

        return new ArcResult<TNode>(nodeBoxes, ribbons);
    }

    /// <summary>Lays out one column's nodes on its arc — proportional sweeps
    /// from the column's value sum, separated by angularPadDeg.</summary>
    private static void _LayoutColumnOnArc<TNode>(
        List<TNode> col,
        Dictionary<TNode, _State<TNode>> byNode,
        float cx, float cy, float innerR, float outerR,
        float visualTopAngleDeg, int direction,
        float arcSpanDegrees, float angularPadDeg,
        Dictionary<TNode, ArcNodeBox> nodeBoxes,
        Dictionary<TNode, _ArcSpan> nodeAngular) where TNode : notnull
    {
        if (col.Count == 0) return;

        var total = 0.0;
        foreach (var n in col) total += byNode[n].Value;
        if (total <= 0) total = 1;

        var totalPad = angularPadDeg * (col.Count - 1);
        var usableSweep = arcSpanDegrees - totalPad;
        if (usableSweep < 1) usableSweep = 1;

        var acc = 0f;
        foreach (var n in col)
        {
            var s = byNode[n];
            var sweep = (float)(s.Value / total) * usableSweep;
            var nodeVisualTop = visualTopAngleDeg + direction * acc;
            // Skia start = smaller angle; for direction=+1 that's the visual
            // top, for direction=-1 it's the visual bottom (visualTop - sweep).
            var skiaStart = direction > 0 ? nodeVisualTop : nodeVisualTop - sweep;

            nodeBoxes[n] = new ArcNodeBox(cx, cy, innerR, outerR, skiaStart, sweep, s.Value);
            nodeAngular[n] = new _ArcSpan(nodeVisualTop, sweep, direction);

            acc += sweep + angularPadDeg;
        }
    }

    private static float _VisualTopY(_ArcSpan span, float cy, float innerR)
    {
        const float toRad = (float)(System.Math.PI / 180);
        return cy + (float)System.Math.Sin(span.VisualTopAngleDeg * toRad) * innerR;
    }

    private readonly struct _ArcSpan
    {
        public _ArcSpan(float visualTopAngleDeg, float sweepMag, int direction)
        {
            VisualTopAngleDeg = visualTopAngleDeg;
            SweepMag = sweepMag;
            Direction = direction;
        }
        public float VisualTopAngleDeg { get; }
        public float SweepMag { get; }
        public int Direction { get; }
    }

    private static void _relaxToTargets<TNode>(
        Dictionary<TNode, _State<TNode>> byNode, List<List<TNode>> columns, float alpha)
        where TNode : notnull
    {
        // Forward sweep: shift each node toward the weighted Y of its
        // outgoing neighbors (i.e., pull source toward its targets).
        for (var d = columns.Count - 2; d >= 0; d--)
        {
            foreach (var n in columns[d])
            {
                var s = byNode[n];
                if (s.Outgoing.Count == 0) continue;
                var weightSum = 0.0;
                var weightedY = 0.0;
                foreach (var link in s.Outgoing)
                {
                    var tgt = byNode[link.Target];
                    weightSum += link.Weight;
                    weightedY += (tgt.Y + tgt.Height * 0.5f) * link.Weight;
                }
                if (weightSum <= 0) continue;
                var center = (float)(weightedY / weightSum);
                s.Y += (center - (s.Y + s.Height * 0.5f)) * alpha;
            }
        }
    }

    private static void _relaxToSources<TNode>(
        Dictionary<TNode, _State<TNode>> byNode, List<List<TNode>> columns, float alpha)
        where TNode : notnull
    {
        // Backward sweep: shift each node toward the weighted Y of its
        // incoming neighbors.
        for (var d = 1; d < columns.Count; d++)
        {
            foreach (var n in columns[d])
            {
                var s = byNode[n];
                if (s.Incoming.Count == 0) continue;
                var weightSum = 0.0;
                var weightedY = 0.0;
                foreach (var link in s.Incoming)
                {
                    var src = byNode[link.Source];
                    weightSum += link.Weight;
                    weightedY += (src.Y + src.Height * 0.5f) * link.Weight;
                }
                if (weightSum <= 0) continue;
                var center = (float)(weightedY / weightSum);
                s.Y += (center - (s.Y + s.Height * 0.5f)) * alpha;
            }
        }
    }

    private static void _resolveCollisions<TNode>(
        Dictionary<TNode, _State<TNode>> byNode,
        List<List<TNode>> columns,
        LvcRectangle rect,
        float padding) where TNode : notnull
    {
        // Per-column: sort by Y, then top-down push apart enforcing padding,
        // then bottom-up clamp inside the draw rect. The two-pass dance is
        // the standard d3-sankey collision step.
        var top = rect.Location.Y;
        var bottom = rect.Location.Y + rect.Size.Height;

        foreach (var col in columns)
        {
            if (col.Count == 0) continue;
            col.Sort((a, b) => byNode[a].Y.CompareTo(byNode[b].Y));

            // Top-down
            var minY = top;
            for (var i = 0; i < col.Count; i++)
            {
                var s = byNode[col[i]];
                if (s.Y < minY) s.Y = minY;
                minY = s.Y + s.Height + padding;
            }

            // Bottom-up: if the bottom node overflowed, push the column up.
            var maxY = bottom;
            for (var i = col.Count - 1; i >= 0; i--)
            {
                var s = byNode[col[i]];
                if (s.Y + s.Height > maxY) s.Y = maxY - s.Height;
                maxY = s.Y - padding;
            }
        }
    }

    private sealed class _State<TNode> where TNode : notnull
    {
        public _State(TNode node) { Node = node; }
        public TNode Node { get; }
        public int Depth { get; set; }
        public double Value { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Height { get; set; }
        public List<SankeyLink<TNode>> Incoming { get; } = [];
        public List<SankeyLink<TNode>> Outgoing { get; } = [];
    }

    private sealed class ReferenceComparer<T> : IEqualityComparer<T> where T : notnull
    {
        public static ReferenceComparer<T> Instance { get; } = new();
        public bool Equals(T? x, T? y) => ReferenceEquals(x, y);
        public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
    }
}
