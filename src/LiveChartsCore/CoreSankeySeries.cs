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
using System.Linq;
using System.Runtime.CompilerServices;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Drawing;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.Motion;
using LiveChartsCore.Painting;

namespace LiveChartsCore;

/// <summary>
/// Defines a sankey series. The graph topology (Values = nodes,
/// <see cref="Links"/> = edges) is laid out via the d3-sankey algorithm
/// (<see cref="SankeyLayout"/>) on every measure pass.
/// </summary>
/// <typeparam name="TModel">The user's node type.</typeparam>
/// <typeparam name="TVisual">The per-node visual (any bounded geometry).</typeparam>
/// <typeparam name="TLabel">The per-node label geometry.</typeparam>
public abstract class CoreSankeySeries<TModel, TVisual, TLabel>(
    IReadOnlyCollection<TModel>? values)
        : Series<TModel, TVisual, TLabel>(SeriesProperties.Solid | SeriesProperties.Sankey, values), ISankeySeries
            where TModel : class
            where TVisual : BoundedDrawnGeometry, new()
            where TLabel : BaseLabelGeometry, new()
{
    private readonly Dictionary<TModel, TVisual> _nodeVisuals = new(ReferenceComparer<TModel>.Instance);
    private readonly Dictionary<SankeyLink<TModel>, BaseSankeyRibbonGeometry> _linkVisuals =
        new(ReferenceComparer<SankeyLink<TModel>>.Instance);
    private readonly Dictionary<TModel, BaseSankeyArcSegmentGeometry> _arcNodeVisuals =
        new(ReferenceComparer<TModel>.Instance);
    private readonly Dictionary<SankeyLink<TModel>, BaseSankeyChordRibbonGeometry> _chordLinkVisuals =
        new(ReferenceComparer<SankeyLink<TModel>>.Instance);
    private readonly Dictionary<TModel, TLabel> _nodeLabels = new(ReferenceComparer<TModel>.Instance);
    private readonly Dictionary<TModel, ChartPoint> _nodePoints = new(ReferenceComparer<TModel>.Instance);
    private Func<ChartPoint, string>? _tooltipLabelFormatter;
    // Implicit ribbon paint: a CloneTask() of Fill, allocated lazily when the
    // user hasn't set LinkFill. See InitializePaints for the rationale —
    // ribbons and nodes need DIFFERENT paint tasks so ZIndex can enforce
    // "ribbons under nodes"; sharing a single paint puts them in the same
    // HashSet<IDrawnElement> on Paint and draw order becomes undefined.
    // Source-pinned so we re-clone when the user swaps Fill at runtime.
    private Paint? _implicitLinkPaint;
    private Paint? _implicitLinkPaintSource;

    /// <summary>Gets or sets the fill paint applied to every node rectangle.</summary>
    public Paint? Fill
    {
        get;
        set => SetPaintProperty(ref field, value);
    } = null;

    /// <summary>Gets or sets the stroke paint applied to every node rectangle.</summary>
    public Paint? Stroke
    {
        get;
        set => SetPaintProperty(ref field, value, PaintStyle.Stroke);
    } = null;

    /// <summary>
    /// Gets or sets the fill paint applied to every flow ribbon. Typically a
    /// tinted/translucent version of <see cref="Fill"/>; when null, the
    /// ribbons inherit <see cref="Fill"/> directly.
    /// </summary>
    public Paint? LinkFill
    {
        get;
        set => SetPaintProperty(ref field, value);
    } = null;

    /// <inheritdoc cref="ISankeySeries.NodeWidth"/>
    public double NodeWidth { get; set => SetProperty(ref field, value); } = 12;

    /// <inheritdoc cref="ISankeySeries.NodePadding"/>
    public double NodePadding { get; set => SetProperty(ref field, value); } = 8;

    /// <inheritdoc cref="ISankeySeries.NodeCornerRadius"/>
    public double NodeCornerRadius { get; set => SetProperty(ref field, value); }

    /// <inheritdoc cref="ISankeySeries.LayoutIterations"/>
    public int LayoutIterations { get; set => SetProperty(ref field, value); } = 32;

    /// <inheritdoc cref="ISankeySeries.NodeLabelPosition"/>
    public SankeyLabelPosition NodeLabelPosition { get; set => SetProperty(ref field, value); }

    /// <inheritdoc cref="ISankeySeries.Layout"/>
    public SankeyLayoutKind Layout { get; set => SetProperty(ref field, value); }

    /// <inheritdoc cref="ISankeySeries.ArcSpanDegrees"/>
    public double ArcSpanDegrees { get; set => SetProperty(ref field, value); } = 150;

    /// <summary>
    /// Per-node color override. When non-null, the returned <see cref="LvcColor"/>
    /// is applied to the node's visual — the visual must implement
    /// <see cref="IColoredGeometry"/> (the default
    /// <c>ColoredRoundedRectangleGeometry</c> does). When null the series's
    /// <see cref="Fill"/> paint color flows through unchanged.
    /// </summary>
    public Func<TModel, LvcColor>? NodeColorMapper { get; set => SetProperty(ref field, value); }

    /// <summary>
    /// Per-link color override. When null, ribbons inherit the source node's
    /// color (via <see cref="NodeColorMapper"/>) tinted to half opacity, or
    /// fall back to <see cref="LinkFill"/> / <see cref="Fill"/>. Set
    /// explicitly to take full control of per-ribbon color including alpha.
    /// </summary>
    public Func<SankeyLink<TModel>, LvcColor>? LinkColorMapper { get; set => SetProperty(ref field, value); }

    /// <summary>
    /// The graph's edges. Each <see cref="SankeyLink{TModel}"/>.Source and .Target
    /// must reference instances present in <see cref="Series{TModel, TVisual, TLabel}.Values"/>;
    /// links pointing at unknown nodes are silently dropped by the layout.
    /// </summary>
    public IEnumerable<SankeyLink<TModel>>? Links { get; set => SetProperty(ref field, value); }

    /// <summary>
    /// Returns the per-node label text. Set
    /// <see cref="Series{TModel, TVisual, TLabel}.DataLabelsPaint"/> to opt
    /// in. When null and TModel is the built-in <see cref="SankeyNode"/>,
    /// <see cref="SankeyNode.Name"/> is used automatically.
    /// </summary>
    public Func<TModel, string?>? LabelMapper { get; set => SetProperty(ref field, value); }

    /// <summary>Tooltip label formatter on the typed point — null falls back to "&lt;label&gt;: &lt;value&gt;".</summary>
    public Func<ChartPoint<TModel, TVisual, TLabel>, string>? ToolTipLabelFormatter
    {
        get => _tooltipLabelFormatter;
        set => ((ISankeySeries)this).TooltipLabelFormatter = value is null ? null : p => value(ConvertToTypedChartPoint(p));
    }

    Func<ChartPoint, string>? ISankeySeries.TooltipLabelFormatter
    {
        get => _tooltipLabelFormatter;
        set => SetProperty(ref _tooltipLabelFormatter, value);
    }

    // ---- template method ----------------------------------------------------

    /// <summary>Builds the per-frame measure context.</summary>
    protected virtual SankeyMeasureContext BeginMeasure(SankeyChartEngine chart)
    {
        var isFirstDraw = !((Chart)chart).IsDrawn(((ISeries)this).SeriesId);

        return new SankeyMeasureContext(
            chart,
            drawLocation: chart.DrawMarginLocation,
            drawMarginSize: chart.DrawMarginSize,
            nodeWidth: (float)NodeWidth,
            nodePadding: (float)NodePadding,
            layoutIterations: LayoutIterations,
            isFirstDraw: isFirstDraw);
    }

    /// <summary>Ensures a visual exists for a node — seeds at its target rect center with zero size for entrance animation.</summary>
    private TVisual EnsureVisualForNode(TModel node, SankeyLayout.NodeBox box)
    {
        if (_nodeVisuals.TryGetValue(node, out var existing)) return existing;
        var v = new TVisual
        {
            X = box.X + box.Width * 0.5f,
            Y = box.Y + box.Height * 0.5f,
            Width = 0,
            Height = 0,
        };

        // Seed Color BEFORE Animate (in the Invalidate loop below) so the
        // motion property's start value is the right color — assigning after
        // Animate makes the property animate from Empty/Black to the target,
        // which under IsTesting reads as the start (= invisible/wrong color).
        // Same pattern CoreHeatSeries.EnsureVisualForPoint uses.
        if (NodeColorMapper is not null && v is IColoredGeometry coloredInit)
            coloredInit.Color = NodeColorMapper(node);

        _nodeVisuals[node] = v;
        return v;
    }

    /// <summary>Ensures a ribbon geometry exists for a link — seeded collapsed at the source midpoint.</summary>
    private BaseSankeyRibbonGeometry EnsureVisualForLink(SankeyLink<TModel> link, SankeyLayout.RibbonBox<TModel> box)
    {
        if (_linkVisuals.TryGetValue(link, out var existing)) return existing;

        var midY = (box.SourceY0 + box.SourceY1) * 0.5f;
        var v = CreateRibbonVisual();
        v.SourceX = box.SourceX;
        v.SourceY0 = midY;
        v.SourceY1 = midY;
        v.TargetX = box.TargetX;
        v.TargetY0 = midY;
        v.TargetY1 = midY;

        // Seed Color before Animate — see EnsureVisualForNode rationale.
        if (LinkColorMapper is not null)
        {
            v.Color = LinkColorMapper(link);
        }
        else if (NodeColorMapper is not null)
        {
            var srcColor = NodeColorMapper(link.Source);
            v.Color = new LvcColor(srcColor.R, srcColor.G, srcColor.B, (byte)(srcColor.A / 2));
        }

        _linkVisuals[link] = v;
        return v;
    }

    /// <summary>Factory hook — subclasses override to pick the ribbon visual concrete type (default RoundedRectangleGeometry-shaped sibling per platform).</summary>
    protected abstract BaseSankeyRibbonGeometry CreateRibbonVisual();

    /// <summary>Factory hook for the arc-segment node visual used in
    /// <see cref="SankeyLayoutKind.BipartiteArc"/>. The vertical-mode TVisual
    /// generic doesn't apply here (arc segments aren't BoundedDrawnGeometry);
    /// platform facades pick the concrete type.</summary>
    protected abstract BaseSankeyArcSegmentGeometry CreateArcNodeVisual();

    /// <summary>Factory hook for the chord-ribbon visual used in
    /// <see cref="SankeyLayoutKind.BipartiteArc"/>.</summary>
    protected abstract BaseSankeyChordRibbonGeometry CreateChordRibbonVisual();

    /// <inheritdoc cref="ChartElement.Invalidate(Chart)"/>
    public sealed override void Invalidate(Chart chart)
    {
        var sankeyChart = (SankeyChartEngine)chart;
        var anim = GetAnimation(sankeyChart);
        var ctx = BeginMeasure(sankeyChart);

        InitializePaints(sankeyChart);

        if (Values is null || Values.Count == 0)
        {
            ReapAll();
            return;
        }

        // Reap whichever-layout's stale visuals exist when the user toggles
        // Layout at runtime. ReapVerticalVisuals / ReapArcVisuals only act on
        // their own dicts, so this is cheap when no switch occurred.
        if (Layout == SankeyLayoutKind.BipartiteArc)
        {
            ReapVerticalVisuals();
            MeasureBipartiteArc(sankeyChart, ctx, anim);
            return;
        }

        ReapArcVisuals();
        MeasureVertical(sankeyChart, ctx, anim);
    }

    private void MeasureVertical(SankeyChartEngine sankeyChart, SankeyMeasureContext ctx, Animation anim)
    {
        // For Auto label placement we need per-node in/out degree (source =
        // no incoming -> outside-left; sink = no outgoing -> outside-right;
        // pass-through -> inside). Compute once per measure.
        var incoming = new Dictionary<TModel, int>(ReferenceComparer<TModel>.Instance);
        var outgoing = new Dictionary<TModel, int>(ReferenceComparer<TModel>.Instance);
        foreach (var n in Values) { incoming[n] = 0; outgoing[n] = 0; }
        if (Links is not null)
            foreach (var l in Links)
            {
                if (l is null) continue;
                // SankeyLink<TNode>'s parameterless ctor sets Source/Target to default!,
                // and the layout pass null-filters; mirror that filtering here so the
                // degree dictionaries (which hash by reference) don't throw on null keys.
                if (l.Source is not null && outgoing.TryGetValue(l.Source, out var outDeg)) outgoing[l.Source] = outDeg + 1;
                if (l.Target is not null && incoming.TryGetValue(l.Target, out var inDeg)) incoming[l.Target] = inDeg + 1;
            }

        // Reserve canvas margin for outside-placed labels so they don't clip
        // off the chart edges. Skip for Inside placement and when labels are
        // disabled. Measured up-front so the layout columns get the right
        // horizontal budget on the first try.
        var (leftReserve, rightReserve) = MeasureOutsideLabelReserve(incoming, outgoing);
        var rect = new LvcRectangle(
            new LvcPoint(ctx.DrawLocation.X + leftReserve, ctx.DrawLocation.Y),
            new LvcSize(ctx.DrawMarginSize.Width - leftReserve - rightReserve, ctx.DrawMarginSize.Height));

        var layout = SankeyLayout.Compute(
            Values,
            Links ?? [],
            rect,
            ctx.NodeWidth,
            ctx.NodePadding,
            ctx.LayoutIterations);

        // Nodes
        var seenNodes = new HashSet<TModel>(ReferenceComparer<TModel>.Instance);
        var chartCenterX = rect.Location.X + rect.Size.Width * 0.5f;
        foreach (var kv in layout.Nodes)
        {
            var node = kv.Key;
            var box = kv.Value;
            var visual = EnsureVisualForNode(node, box);
            if (Fill is not null && Fill != Paint.Default)
                Fill.AddGeometryToPaintTask(sankeyChart.Canvas, visual);
            if (Stroke is not null && Stroke != Paint.Default)
                Stroke.AddGeometryToPaintTask(sankeyChart.Canvas, visual);

            EnsureNodeAnimation(visual, anim);
            visual.X = box.X;
            visual.Y = box.Y;
            visual.Width = box.Width;
            visual.Height = box.Height;
            visual.RemoveOnCompleted = false;

            if (visual is BaseRoundedRectangleGeometry rr)
            {
                var r = (float)NodeCornerRadius;
                rr.BorderRadius = new LvcPoint(r, r);
            }

            // Per-node color (requires an IColoredGeometry visual — true for
            // the default ColoredRoundedRectangleGeometry).
            if (NodeColorMapper is not null && visual is IColoredGeometry coloredNode)
                coloredNode.Color = NodeColorMapper(node);

            EnsureChartPointForRect(node, box, box.Value, sankeyChart);
            MeasureNodeLabel(node, box, incoming[node], outgoing[node], chartCenterX, anim, sankeyChart);
            _ = seenNodes.Add(node);
        }

        // Links — use LinkFill if user-set, else the implicit Fill clone
        // (separate paint task; see InitializePaints).
        var ribbonPaint = GetEffectiveRibbonPaint();
        var seenLinks = new HashSet<SankeyLink<TModel>>(ReferenceComparer<SankeyLink<TModel>>.Instance);
        foreach (var rb in layout.Links)
        {
            var visual = EnsureVisualForLink(rb.Link, rb);
            if (ribbonPaint is not null && ribbonPaint != Paint.Default)
                ribbonPaint.AddGeometryToPaintTask(sankeyChart.Canvas, visual);

            EnsureRibbonAnimation(visual, anim);
            visual.SourceX = rb.SourceX;
            visual.SourceY0 = rb.SourceY0;
            visual.SourceY1 = rb.SourceY1;
            visual.TargetX = rb.TargetX;
            visual.TargetY0 = rb.TargetY0;
            visual.TargetY1 = rb.TargetY1;

            // Per-link color: explicit LinkColorMapper wins; otherwise tint
            // the source-node color at half alpha so ribbons read as "from
            // node X" without overpowering the destination.
            if (LinkColorMapper is not null)
            {
                visual.Color = LinkColorMapper(rb.Link);
            }
            else if (NodeColorMapper is not null)
            {
                var srcColor = NodeColorMapper(rb.Link.Source);
                visual.Color = new LvcColor(srcColor.R, srcColor.G, srcColor.B, (byte)(srcColor.A / 2));
            }
            visual.RemoveOnCompleted = false;

            _ = seenLinks.Add(rb.Link);
        }

        // Reap orphans
        if (_nodeVisuals.Count != seenNodes.Count)
        {
            var toRemove = new List<TModel>();
            foreach (var kv in _nodeVisuals)
                if (!seenNodes.Contains(kv.Key))
                {
                    CollapseRemovedNode(kv.Value);
                    toRemove.Add(kv.Key);
                }
            foreach (var k in toRemove)
            {
                _ = _nodeVisuals.Remove(k);
                _ = _nodePoints.Remove(k);
            }
        }

        if (_linkVisuals.Count != seenLinks.Count)
        {
            var toRemove = new List<SankeyLink<TModel>>();
            foreach (var kv in _linkVisuals)
                if (!seenLinks.Contains(kv.Key))
                {
                    CollapseRemovedRibbon(kv.Value);
                    toRemove.Add(kv.Key);
                }
            foreach (var k in toRemove) _ = _linkVisuals.Remove(k);
        }

        if (_nodeLabels.Count > 0)
        {
            var toRemove = new List<TModel>();
            foreach (var kv in _nodeLabels)
                if (!seenNodes.Contains(kv.Key))
                {
                    kv.Value.Opacity = 0;
                    kv.Value.RemoveOnCompleted = true;
                    toRemove.Add(kv.Key);
                }
            foreach (var k in toRemove) _ = _nodeLabels.Remove(k);
        }
    }

    // ---- BipartiteArc layout ------------------------------------------------

    private void MeasureBipartiteArc(SankeyChartEngine sankeyChart, SankeyMeasureContext ctx, Animation anim)
    {
        // Arc mode reserves a uniform radial margin around the ellipse for
        // labels — measured up-front so the inner/outer radii are stable on
        // the first paint. Then SankeyLayout.ComputeBipartiteArc validates
        // bipartite-ness (throws on multi-column) and produces the angular
        // node spans + chord endpoints.
        var labelMargin = MeasureArcLabelMargin();
        var rect = new LvcRectangle(ctx.DrawLocation, ctx.DrawMarginSize);

        var layout = SankeyLayout.ComputeBipartiteArc(
            Values!,
            Links ?? [],
            rect,
            ctx.NodeWidth,
            ctx.NodePadding,
            (float)ArcSpanDegrees,
            labelMargin);

        var seenNodes = new HashSet<TModel>(ReferenceComparer<TModel>.Instance);
        foreach (var kv in layout.Nodes)
        {
            var node = kv.Key;
            var box = kv.Value;
            var visual = EnsureArcVisualForNode(node, box);

            if (Fill is not null && Fill != Paint.Default)
                Fill.AddGeometryToPaintTask(sankeyChart.Canvas, visual);
            if (Stroke is not null && Stroke != Paint.Default)
                Stroke.AddGeometryToPaintTask(sankeyChart.Canvas, visual);

            visual.Animate(anim);
            visual.CenterX = box.CenterX;
            visual.CenterY = box.CenterY;
            visual.InnerRadius = box.InnerRadius;
            visual.OuterRadius = box.OuterRadius;
            visual.StartAngle = box.StartAngle;
            visual.SweepAngle = box.SweepAngle;
            visual.CornerRadius = (float)NodeCornerRadius;
            visual.RemoveOnCompleted = false;

            if (NodeColorMapper is not null)
                visual.Color = NodeColorMapper(node);

            EnsureChartPointForArc(node, box, box.Value, sankeyChart);
            MeasureArcNodeLabel(node, box, anim, sankeyChart);
            _ = seenNodes.Add(node);
        }

        var ribbonPaint = GetEffectiveRibbonPaint();
        var seenLinks = new HashSet<SankeyLink<TModel>>(ReferenceComparer<SankeyLink<TModel>>.Instance);
        foreach (var rb in layout.Links)
        {
            var visual = EnsureChordVisualForLink(rb.Link, rb);
            if (ribbonPaint is not null && ribbonPaint != Paint.Default)
                ribbonPaint.AddGeometryToPaintTask(sankeyChart.Canvas, visual);

            visual.Animate(anim);
            visual.CenterX = rb.CenterX;
            visual.CenterY = rb.CenterY;
            visual.SourceP0X = rb.SourceP0X; visual.SourceP0Y = rb.SourceP0Y;
            visual.SourceP1X = rb.SourceP1X; visual.SourceP1Y = rb.SourceP1Y;
            visual.TargetP0X = rb.TargetP0X; visual.TargetP0Y = rb.TargetP0Y;
            visual.TargetP1X = rb.TargetP1X; visual.TargetP1Y = rb.TargetP1Y;

            if (LinkColorMapper is not null)
            {
                visual.Color = LinkColorMapper(rb.Link);
            }
            else if (NodeColorMapper is not null)
            {
                var srcColor = NodeColorMapper(rb.Link.Source);
                visual.Color = new LvcColor(srcColor.R, srcColor.G, srcColor.B, (byte)(srcColor.A / 2));
            }
            visual.RemoveOnCompleted = false;

            _ = seenLinks.Add(rb.Link);
        }

        // Reap orphans (arc dicts)
        if (_arcNodeVisuals.Count != seenNodes.Count)
        {
            var toRemove = new List<TModel>();
            foreach (var kv in _arcNodeVisuals)
                if (!seenNodes.Contains(kv.Key))
                {
                    CollapseRemovedArcNode(kv.Value);
                    toRemove.Add(kv.Key);
                }
            foreach (var k in toRemove)
            {
                _ = _arcNodeVisuals.Remove(k);
                _ = _nodePoints.Remove(k);
            }
        }
        if (_chordLinkVisuals.Count != seenLinks.Count)
        {
            var toRemove = new List<SankeyLink<TModel>>();
            foreach (var kv in _chordLinkVisuals)
                if (!seenLinks.Contains(kv.Key))
                {
                    CollapseRemovedChordRibbon(kv.Value);
                    toRemove.Add(kv.Key);
                }
            foreach (var k in toRemove) _ = _chordLinkVisuals.Remove(k);
        }
        if (_nodeLabels.Count > 0)
        {
            var toRemove = new List<TModel>();
            foreach (var kv in _nodeLabels)
                if (!seenNodes.Contains(kv.Key))
                {
                    kv.Value.Opacity = 0;
                    kv.Value.RemoveOnCompleted = true;
                    toRemove.Add(kv.Key);
                }
            foreach (var k in toRemove) _ = _nodeLabels.Remove(k);
        }
    }

    /// <summary>Seeds a new arc visual collapsed at its center angle (zero
    /// sweep), so its entrance animates "wedge opens up."</summary>
    private BaseSankeyArcSegmentGeometry EnsureArcVisualForNode(TModel node, SankeyLayout.ArcNodeBox box)
    {
        if (_arcNodeVisuals.TryGetValue(node, out var existing)) return existing;
        var v = CreateArcNodeVisual();
        v.CenterX = box.CenterX;
        v.CenterY = box.CenterY;
        v.InnerRadius = box.InnerRadius;
        v.OuterRadius = box.OuterRadius;
        v.StartAngle = box.StartAngle + box.SweepAngle * 0.5f;
        v.SweepAngle = 0;
        v.CornerRadius = (float)NodeCornerRadius;
        if (NodeColorMapper is not null)
            v.Color = NodeColorMapper(node);
        _arcNodeVisuals[node] = v;
        return v;
    }

    /// <summary>Seeds a new chord visual collapsed at the chart center, so
    /// its entrance animates "ribbon expands out from center."</summary>
    private BaseSankeyChordRibbonGeometry EnsureChordVisualForLink(SankeyLink<TModel> link, SankeyLayout.ChordRibbonBox<TModel> box)
    {
        if (_chordLinkVisuals.TryGetValue(link, out var existing)) return existing;
        var v = CreateChordRibbonVisual();
        v.CenterX = box.CenterX;
        v.CenterY = box.CenterY;
        v.SourceP0X = box.CenterX; v.SourceP0Y = box.CenterY;
        v.SourceP1X = box.CenterX; v.SourceP1Y = box.CenterY;
        v.TargetP0X = box.CenterX; v.TargetP0Y = box.CenterY;
        v.TargetP1X = box.CenterX; v.TargetP1Y = box.CenterY;
        if (LinkColorMapper is not null)
        {
            v.Color = LinkColorMapper(link);
        }
        else if (NodeColorMapper is not null)
        {
            var srcColor = NodeColorMapper(link.Source);
            v.Color = new LvcColor(srcColor.R, srcColor.G, srcColor.B, (byte)(srcColor.A / 2));
        }
        _chordLinkVisuals[link] = v;
        return v;
    }

    /// <summary>
    /// Places a node's label radially outside its arc segment. Anchor sits
    /// just past the outer radius along the node's mid-angle; text is rotated
    /// so it reads outward from the chart center on whichever half of the
    /// circle the node lives on (right half = rotation matches angle; left
    /// half = +180° flip + HorizontalAlign.End so the text grows away from
    /// the anchor toward the chart edge instead of toward the center).
    /// </summary>
    private void MeasureArcNodeLabel(TModel node, SankeyLayout.ArcNodeBox box, Animation anim, SankeyChartEngine chart)
    {
        if (!ShowDataLabels || DataLabelsPaint is null || DataLabelsPaint == Paint.Default)
            return;
        var text = ResolveLabel(node);
        if (string.IsNullOrEmpty(text)) return;

        var midAngle = box.StartAngle + box.SweepAngle * 0.5f;
        var θ = ((midAngle % 360f) + 360f) % 360f;
        const float toRad = (float)(System.Math.PI / 180);
        const float gap = 4f;

        var anchorR = box.OuterRadius + gap;
        var labelX = box.CenterX + (float)System.Math.Cos(θ * toRad) * anchorR;
        var labelY = box.CenterY + (float)System.Math.Sin(θ * toRad) * anchorR;

        // Right half: text reads outward, anchored at its near end (Start).
        // Left half: flip rotation 180° + anchor the far end (End) so the
        // visible text still grows away from chart center.
        var flipped = θ > 90f && θ < 270f;
        var rotation = flipped ? θ - 180f : θ;
        var hAlign = flipped ? Align.End : Align.Start;

        if (!_nodeLabels.TryGetValue(node, out var label))
        {
            label = new TLabel
            {
                X = labelX,
                Y = labelY,
                HorizontalAlign = hAlign,
                VerticalAlign = Align.Middle,
                MaxWidth = (float)DataLabelsMaxWidth,
            };
            label.Animate(anim, BaseLabelGeometry.XProperty, BaseLabelGeometry.YProperty);
            _nodeLabels[node] = label;
        }

        DataLabelsPaint.AddGeometryToPaintTask(chart.Canvas, label);

        label.Text = text!;
        label.TextSize = (float)DataLabelsSize;
        label.Padding = DataLabelsPadding;
        label.Paint = DataLabelsPaint;
        label.HorizontalAlign = hAlign;
        label.VerticalAlign = Align.Middle;
        label.X = labelX;
        label.Y = labelY;
        label.RotateTransform = rotation;
        label.Opacity = 1;
        label.RemoveOnCompleted = false;
    }

    /// <summary>Measures the widest label so the layout can shrink the outer
    /// radius by that amount + a small gap; otherwise long labels would clip
    /// the canvas edge. Returns 0 when labels are off.</summary>
    private float MeasureArcLabelMargin()
    {
        if (!ShowDataLabels || DataLabelsPaint is null || DataLabelsPaint == Paint.Default)
            return 0f;
        if (Values is null) return 0f;

        const float gap = 4f;
        var probe = new TLabel
        {
            TextSize = (float)DataLabelsSize,
            Padding = DataLabelsPadding,
            Paint = DataLabelsPaint,
            MaxWidth = (float)DataLabelsMaxWidth,
        };

        float maxW = 0f;
        foreach (var node in Values)
        {
            var text = ResolveLabel(node);
            if (string.IsNullOrEmpty(text)) continue;
            probe.Text = text!;
            var size = probe.Measure();
            if (size.Width > maxW) maxW = size.Width;
        }
        return maxW > 0 ? maxW + gap : 0f;
    }

    /// <summary>
    /// Resolves a node's label, with a fallback to <see cref="SankeyNode.Name"/>
    /// when no <see cref="LabelMapper"/> is set and the node is the built-in type.
    /// </summary>
    private string? ResolveLabel(TModel node)
    {
        if (LabelMapper is not null) return LabelMapper(node);
        if (node is SankeyNode sn) return sn.Name;
        return null;
    }

    /// <summary>
    /// Places a node label per <see cref="NodeLabelPosition"/>:
    /// <list type="bullet">
    ///   <item><c>Auto</c> — pure source (no incoming) goes outside-left,
    ///     pure sink (no outgoing) goes outside-right, pass-through goes
    ///     inside-centered. Matches the d3-sankey convention.</item>
    ///   <item><c>Outside</c> — flips per-node based on chart center
    ///     (label on the side closer to the chart edge).</item>
    ///   <item><c>Inside</c> — always centered on the node.</item>
    /// </list>
    /// </summary>
    private void MeasureNodeLabel(
        TModel node, SankeyLayout.NodeBox box, int inDeg, int outDeg,
        float chartCenterX, Animation anim, SankeyChartEngine chart)
    {
        if (!ShowDataLabels || DataLabelsPaint is null || DataLabelsPaint == Paint.Default)
            return;

        var text = ResolveLabel(node);
        if (string.IsNullOrEmpty(text)) return;

        // Resolve effective placement.
        var placement = NodeLabelPosition;
        var nodeCenterX = box.X + box.Width * 0.5f;
        bool isInside;
        bool labelOnRight;
        if (placement == SankeyLabelPosition.Inside)
        {
            isInside = true;
            labelOnRight = false; // unused for inside
        }
        else if (placement == SankeyLabelPosition.Outside)
        {
            // The original heuristic: label opposite chart center to avoid the
            // next column / chart edge. Stays meaningful when the user
            // explicitly opts out of Auto.
            isInside = false;
            labelOnRight = nodeCenterX > chartCenterX;
        }
        else
        {
            // Auto: in-deg=0 -> source -> outside-left; out-deg=0 -> sink
            // -> outside-right; both nonzero -> inside.
            if (inDeg == 0 && outDeg > 0) { isInside = false; labelOnRight = false; }
            else if (outDeg == 0 && inDeg > 0) { isInside = false; labelOnRight = true; }
            else { isInside = true; labelOnRight = false; }
        }

        const float gap = 4f;
        float labelX;
        float labelY = box.Y + box.Height * 0.5f;
        Align hAlign;
        if (isInside)
        {
            labelX = box.X + box.Width * 0.5f;
            hAlign = Align.Middle;
        }
        else if (labelOnRight)
        {
            labelX = box.X + box.Width + gap;
            hAlign = Align.Start;
        }
        else
        {
            labelX = box.X - gap;
            hAlign = Align.End;
        }

        if (!_nodeLabels.TryGetValue(node, out var label))
        {
            label = new TLabel
            {
                X = labelX,
                Y = labelY,
                HorizontalAlign = hAlign,
                VerticalAlign = Align.Middle,
                MaxWidth = (float)DataLabelsMaxWidth,
            };
            label.Animate(anim, BaseLabelGeometry.XProperty, BaseLabelGeometry.YProperty);
            _nodeLabels[node] = label;
        }

        DataLabelsPaint.AddGeometryToPaintTask(chart.Canvas, label);

        label.Text = text!;
        label.TextSize = (float)DataLabelsSize;
        label.Padding = DataLabelsPadding;
        label.Paint = DataLabelsPaint;
        label.HorizontalAlign = hAlign;
        label.VerticalAlign = Align.Middle;
        label.X = labelX;
        label.Y = labelY;
        label.Opacity = 1;
        label.RemoveOnCompleted = false;
    }

    /// <summary>
    /// Returns (leftReserve, rightReserve) — pixel budgets to subtract from
    /// the chart's left/right edges so outside-placed labels fit. Empty when
    /// placement is <see cref="SankeyLabelPosition.Inside"/> or labels are off.
    /// Auto reserves left for pure-source nodes (inDeg=0) and right for
    /// pure-sink nodes (outDeg=0); Outside reserves both sides for all nodes.
    /// </summary>
    private (float Left, float Right) MeasureOutsideLabelReserve(
        Dictionary<TModel, int> incoming, Dictionary<TModel, int> outgoing)
    {
        if (!ShowDataLabels || DataLabelsPaint is null || DataLabelsPaint == Paint.Default)
            return (0f, 0f);
        if (NodeLabelPosition == SankeyLabelPosition.Inside)
            return (0f, 0f);
        if (Values is null) return (0f, 0f);

        const float gap = 4f;

        // A single throwaway label measures successive node texts — Measure()
        // re-renders with the current Text/TextSize/Padding/Paint, so the
        // instance is reusable across iterations.
        var probe = new TLabel
        {
            TextSize = (float)DataLabelsSize,
            Padding = DataLabelsPadding,
            Paint = DataLabelsPaint,
            MaxWidth = (float)DataLabelsMaxWidth,
        };

        float leftMax = 0f, rightMax = 0f;
        foreach (var node in Values)
        {
            var text = ResolveLabel(node);
            if (string.IsNullOrEmpty(text)) continue;

            probe.Text = text!;
            var size = probe.Measure();

            // Outside places every node's label outside (per the heuristic);
            // Auto only places pure-source/pure-sink outside.
            var inDeg = incoming.TryGetValue(node, out var i) ? i : 0;
            var outDeg = outgoing.TryGetValue(node, out var o) ? o : 0;

            if (NodeLabelPosition == SankeyLabelPosition.Outside)
            {
                // Outside flips per side; reserve symmetrically since either
                // edge may host any node's label.
                if (size.Width > leftMax) leftMax = size.Width;
                if (size.Width > rightMax) rightMax = size.Width;
            }
            else // Auto
            {
                if (inDeg == 0 && outDeg > 0 && size.Width > leftMax) leftMax = size.Width;
                else if (outDeg == 0 && inDeg > 0 && size.Width > rightMax) rightMax = size.Width;
            }
        }

        return (leftMax > 0 ? leftMax + gap : 0f, rightMax > 0 ? rightMax + gap : 0f);
    }

    private static void EnsureNodeAnimation(TVisual visual, Animation anim)
    {
        // Idempotent: re-animating with the same animation is a no-op.
        visual.Animate(anim);
    }

    private static void EnsureRibbonAnimation(BaseSankeyRibbonGeometry visual, Animation anim)
    {
        visual.Animate(anim);
    }

    private static void CollapseRemovedNode(TVisual visual)
    {
        var cx = visual.X + visual.Width * 0.5f;
        var cy = visual.Y + visual.Height * 0.5f;
        visual.X = cx;
        visual.Y = cy;
        visual.Width = 0;
        visual.Height = 0;
        visual.RemoveOnCompleted = true;
    }

    private static void CollapseRemovedRibbon(BaseSankeyRibbonGeometry visual)
    {
        var midY = (visual.SourceY0 + visual.SourceY1) * 0.5f;
        visual.SourceY0 = midY;
        visual.SourceY1 = midY;
        var midTy = (visual.TargetY0 + visual.TargetY1) * 0.5f;
        visual.TargetY0 = midTy;
        visual.TargetY1 = midTy;
        visual.RemoveOnCompleted = true;
    }

    private static void CollapseRemovedArcNode(BaseSankeyArcSegmentGeometry visual)
    {
        // Shrink to a zero-sweep wedge at its current start angle. The
        // animator interpolates the sweep down which reads as "wedge fades
        // closed," parallel to the rect collapse for vertical mode.
        visual.SweepAngle = 0;
        visual.RemoveOnCompleted = true;
    }

    private static void CollapseRemovedChordRibbon(BaseSankeyChordRibbonGeometry visual)
    {
        // Collapse all four anchors to the chord-attractor center — the band
        // animates into a single point at chart center, which reads as "ribbon
        // pulls into center then disappears."
        visual.SourceP0X = visual.CenterX; visual.SourceP0Y = visual.CenterY;
        visual.SourceP1X = visual.CenterX; visual.SourceP1Y = visual.CenterY;
        visual.TargetP0X = visual.CenterX; visual.TargetP0Y = visual.CenterY;
        visual.TargetP1X = visual.CenterX; visual.TargetP1Y = visual.CenterY;
        visual.RemoveOnCompleted = true;
    }

    private void ReapAll()
    {
        ReapVerticalVisuals();
        ReapArcVisuals();
        foreach (var l in _nodeLabels.Values)
        {
            l.Opacity = 0;
            l.RemoveOnCompleted = true;
        }
        _nodeLabels.Clear();
        _nodePoints.Clear();
    }

    private void ReapVerticalVisuals()
    {
        if (_nodeVisuals.Count == 0 && _linkVisuals.Count == 0) return;
        foreach (var v in _nodeVisuals.Values) CollapseRemovedNode(v);
        _nodeVisuals.Clear();
        foreach (var v in _linkVisuals.Values) CollapseRemovedRibbon(v);
        _linkVisuals.Clear();
    }

    private void ReapArcVisuals()
    {
        if (_arcNodeVisuals.Count == 0 && _chordLinkVisuals.Count == 0) return;
        foreach (var v in _arcNodeVisuals.Values) CollapseRemovedArcNode(v);
        _arcNodeVisuals.Clear();
        foreach (var v in _chordLinkVisuals.Values) CollapseRemovedChordRibbon(v);
        _chordLinkVisuals.Clear();
    }

    /// <summary>
    /// Returns the paint ribbons should attach to. Resolved per measure so
    /// runtime LinkFill changes / clone invalidations propagate immediately.
    /// </summary>
    private Paint? GetEffectiveRibbonPaint() =>
        LinkFill is not null && LinkFill != Paint.Default ? LinkFill : _implicitLinkPaint;

    private void InitializePaints(SankeyChartEngine chart)
    {
        var actualZIndex = ZIndex == 0 ? ((ISeries)this).SeriesId : ZIndex;

        // Ribbons MUST live in a different paint task from nodes so ZIndex
        // can enforce "ribbons under nodes" deterministically — Paint stores
        // geometries in a HashSet<IDrawnElement>, so two visual types in
        // the same task draw in undefined order. When the user provides
        // LinkFill we already get two tasks; when they don't, we clone Fill
        // into _implicitLinkPaint and use that for ribbons. Re-clone if
        // Fill identity changes (runtime swap).
        Paint? ribbonPaint;
        if (LinkFill is not null && LinkFill != Paint.Default)
        {
            ribbonPaint = LinkFill;
            DisposeImplicitLinkPaint(chart);
        }
        else if (Fill is not null && Fill != Paint.Default)
        {
            if (_implicitLinkPaint is null || !ReferenceEquals(_implicitLinkPaintSource, Fill))
            {
                DisposeImplicitLinkPaint(chart);
                _implicitLinkPaint = Fill.CloneTask();
                _implicitLinkPaintSource = Fill;
            }
            ribbonPaint = _implicitLinkPaint;
        }
        else
        {
            ribbonPaint = null;
            DisposeImplicitLinkPaint(chart);
        }

        if (ribbonPaint is not null && ribbonPaint != Paint.Default)
        {
            ribbonPaint.ZIndex = actualZIndex + PaintConstants.SeriesFillZIndexOffset;
            chart.Canvas.AddDrawableTask(ribbonPaint, zone: CanvasZone.DrawMargin);
        }
        if (Fill is not null && Fill != Paint.Default)
        {
            Fill.ZIndex = actualZIndex + PaintConstants.SeriesFillZIndexOffset + 1;
            chart.Canvas.AddDrawableTask(Fill, zone: CanvasZone.DrawMargin);
        }
        if (Stroke is not null && Stroke != Paint.Default)
        {
            Stroke.ZIndex = actualZIndex + PaintConstants.SeriesStrokeZIndexOffset;
            chart.Canvas.AddDrawableTask(Stroke, zone: CanvasZone.DrawMargin);
        }
        if (ShowDataLabels && DataLabelsPaint is not null && DataLabelsPaint != Paint.Default)
        {
            // Labels sit above tiles + ribbons; PieSeriesDataLabelsBaseZIndex
            // is the established "always-on-top, no clipping" base offset.
            DataLabelsPaint.ZIndex =
                PaintConstants.PieSeriesDataLabelsBaseZIndex + actualZIndex + PaintConstants.SeriesGeometryFillZIndexOffset;
            chart.Canvas.AddDrawableTask(DataLabelsPaint);
        }
    }

    private void DisposeImplicitLinkPaint(SankeyChartEngine chart)
    {
        if (_implicitLinkPaint is null) return;
        chart.Canvas.RemovePaintTask(_implicitLinkPaint);
        _implicitLinkPaint = null;
        _implicitLinkPaintSource = null;
    }

    // ---- Hit-test + tooltip surface -----------------------------------------

    /// <summary>
    /// Creates or refreshes the <see cref="ChartPoint"/> for a node — its
    /// HoverArea is set to the node tile (rectangle for Vertical, annular
    /// sector for BipartiteArc), Coordinate.PrimaryValue to the node's
    /// resolved value (max of in/out weight sum), DataSource to the user's
    /// node model. Same shape treemap PR #2295 uses for leaf tiles.
    /// </summary>
    private void EnsureChartPointForRect(TModel node, SankeyLayout.NodeBox box, double value, SankeyChartEngine chart)
    {
        if (!_nodePoints.TryGetValue(node, out var point))
        {
            var entity = new MappedChartEntity
            {
                MetaData = new ChartEntityMetaData(_ => { }) { EntityIndex = _nodePoints.Count },
            };
            point = new ChartPoint(chart.View, this, entity);
            _nodePoints[node] = point;
        }
        point.Context.DataSource = node;
        if (_nodeVisuals.TryGetValue(node, out var visual)) point.Context.Visual = visual;

        var ha = point.Context.HoverArea as RectangleHoverArea ?? new RectangleHoverArea();
        _ = ha
            .SetDimensions(box.X, box.Y, box.Width, box.Height)
            .CenterXToolTip()
            .CenterYToolTip();
        point.Context.HoverArea = ha;

        ((MappedChartEntity)point.Context.Entity).Coordinate =
            new Coordinate(point.Context.Entity.MetaData!.EntityIndex, value);
    }

    private void EnsureChartPointForArc(TModel node, SankeyLayout.ArcNodeBox box, double value, SankeyChartEngine chart)
    {
        if (!_nodePoints.TryGetValue(node, out var point))
        {
            var entity = new MappedChartEntity
            {
                MetaData = new ChartEntityMetaData(_ => { }) { EntityIndex = _nodePoints.Count },
            };
            point = new ChartPoint(chart.View, this, entity);
            _nodePoints[node] = point;
        }
        point.Context.DataSource = node;
        if (_arcNodeVisuals.TryGetValue(node, out var visual)) point.Context.Visual = visual;

        var ha = point.Context.HoverArea as AnnularSectorHoverArea ?? new AnnularSectorHoverArea();
        _ = ha
            .SetDimensions(box.CenterX, box.CenterY, box.InnerRadius, box.OuterRadius, box.StartAngle, box.SweepAngle)
            .AnchorAtOuterMidpoint();
        point.Context.HoverArea = ha;

        ((MappedChartEntity)point.Context.Entity).Coordinate =
            new Coordinate(point.Context.Entity.MetaData!.EntityIndex, value);
    }

    // ---- Series abstract members --------------------------------------------

    /// <inheritdoc cref="ISeries.GetPrimaryToolTipText(ChartPoint)"/>
    public override string? GetPrimaryToolTipText(ChartPoint point)
    {
        if (_tooltipLabelFormatter is not null) return _tooltipLabelFormatter(point);

        // Default: "<label>: <value>" — or just "<value>" when no label.
        var node = point.Context.DataSource is TModel m ? m : default;
        var label = node is not null ? ResolveLabel(node) : null;
        var value = point.Coordinate.PrimaryValue;
        return string.IsNullOrEmpty(label) ? value.ToString() : $"{label}: {value}";
    }

    /// <inheritdoc cref="ISeries.GetSecondaryToolTipText(ChartPoint)"/>
    public override string? GetSecondaryToolTipText(ChartPoint point) => LiveCharts.IgnoreToolTipLabel;

    /// <inheritdoc cref="Series{TModel, TVisual, TLabel}.FindPointsInPosition"/>
    protected override IEnumerable<ChartPoint> FindPointsInPosition(
        Chart chart, LvcPoint pointerPosition, FindingStrategy strategy, FindPointFor findPointFor)
    {
        var hits = _nodePoints.Values.Where(p =>
            p.Context.HoverArea is not null &&
            p.Context.HoverArea.IsPointerOver(pointerPosition, strategy));

        // Mirror Series.FindPointsInPosition: the *TakeClosest strategies
        // (CompareAllTakeClosest=4, CompareOnlyXTakeClosest=5,
        // CompareOnlyYTakeClosest=6, ExactMatchTakeClosest=8) collapse
        // overlapping hits to a single closest point. Without this filter
        // hovering between two adjacent node arcs would return both, which
        // breaks DataPointerDown / tooltip behavior for those strategies.
        var s = (int)strategy;
        if (s is (>= 4 and <= 6) or 8)
            hits = hits
                .Select(x => new { distance = x.DistanceTo(pointerPosition, strategy), point = x })
                .OrderBy(x => x.distance)
                .SelectFirst(x => x.point);

        return hits;
    }

    /// <inheritdoc cref="Series{TModel, TVisual, TLabel}.GetMiniatureGeometry(ChartPoint?)"/>
    public override IDrawnElement GetMiniatureGeometry(ChartPoint? point) =>
        new TVisual
        {
            Fill = Fill,
            Stroke = Stroke,
            StrokeThickness = (float)MiniatureStrokeThickness,
            ClippingBounds = LvcRectangle.Empty,
            Width = (float)MiniatureShapeSize,
            Height = (float)MiniatureShapeSize,
        };

    /// <inheritdoc cref="ChartElement.GetPaintTasks"/>
    protected internal override Paint?[] GetPaintTasks() => [Fill, Stroke, LinkFill, DataLabelsPaint];

    /// <inheritdoc cref="Series{TModel, TVisual, TLabel}.SetDefaultPointTransitions(ChartPoint)"/>
    protected override void SetDefaultPointTransitions(ChartPoint chartPoint)
    {
        // No-op: sankey visuals are managed directly via _nodeVisuals / _linkVisuals;
        // no ChartPoint round-trip.
    }

    /// <inheritdoc cref="ISeries.SoftDeleteOrDispose(IChartView)"/>
    public override void SoftDeleteOrDispose(IChartView chart)
    {
        var core = ((ISankeyChartView)chart).Core;
        ReapAll();
        // The implicit Fill clone isn't in GetPaintTasks() (it's a derived
        // resource the series owns internally) so it needs explicit cleanup.
        DisposeImplicitLinkPaint(core);
        foreach (var pt in GetPaintTasks())
            if (pt is not null) core.Canvas.RemovePaintTask(pt);
    }

    private sealed class ReferenceComparer<T> : IEqualityComparer<T> where T : notnull
    {
        public static ReferenceComparer<T> Instance { get; } = new();
        public bool Equals(T? x, T? y) => ReferenceEquals(x, y);
        public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
    }
}
