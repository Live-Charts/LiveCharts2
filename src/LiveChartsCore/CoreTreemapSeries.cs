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
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Drawing;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.Motion;
using LiveChartsCore.Painting;

namespace LiveChartsCore;

/// <summary>
/// Defines a treemap series. Lays out a tree of nodes inside the chart's
/// draw margin using the squarified treemap algorithm (Bruls, Huijing,
/// van Wijk — 2000). Each node's area is proportional to its weight; the
/// algorithm picks per-row layouts that keep the rectangles as close to
/// square as possible.
/// </summary>
/// <typeparam name="TModel">The user's node type.</typeparam>
/// <typeparam name="TVisual">The per-tile visual (any bounded geometry).</typeparam>
/// <typeparam name="TLabel">The per-tile label geometry.</typeparam>
public abstract class CoreTreemapSeries<TModel, TVisual, TLabel>(
    IReadOnlyCollection<TModel>? values)
        : Series<TModel, TVisual, TLabel>(SeriesProperties.Solid, values), ITreemapSeries
            where TModel : class
            where TVisual : BoundedDrawnGeometry, new()
            where TLabel : BaseLabelGeometry, new()
{
    private readonly Dictionary<object, TVisual> _nodeVisuals = new(ReferenceComparer.Instance);
    private readonly Dictionary<object, TLabel> _nodeLabels = new(ReferenceComparer.Instance);
    private readonly Dictionary<object, ChartPoint> _nodePoints = new(ReferenceComparer.Instance);
    private readonly HashSet<object> _seenThisMeasure = new(ReferenceComparer.Instance);
    private readonly HashSet<object> _labeledThisMeasure = new(ReferenceComparer.Instance);
    private readonly Dictionary<object, double> _weightCache = new(ReferenceComparer.Instance);
    private Func<ChartPoint, string>? _tooltipLabelFormatter;

    /// <summary>Gets or sets the fill paint applied to every tile.</summary>
    public Paint? Fill
    {
        get;
        set => SetPaintProperty(ref field, value);
    } = null;

    /// <summary>Gets or sets the stroke paint applied to every tile.</summary>
    public Paint? Stroke
    {
        get;
        set => SetPaintProperty(ref field, value, PaintStyle.Stroke);
    } = null;

    /// <inheritdoc cref="ITreemapSeries.Padding"/>
    public double Padding { get; set => SetProperty(ref field, value); } = 2;

    /// <inheritdoc cref="ITreemapSeries.CornerRadius"/>
    public double CornerRadius { get; set => SetProperty(ref field, value); }

    /// <inheritdoc cref="ITreemapSeries.AssignedRectangle"/>
    public LvcRectangle AssignedRectangle { get; set; }

    /// <inheritdoc cref="ITreemapSeries.GetTotalWeight"/>
    public double GetTotalWeight()
    {
        // No `ValueMapper is null` short-circuit: ResolveValue has its own
        // TreemapNode fallback (covers the XAML wrapper path where the
        // source-generated facade can't auto-wire mappers). Bypassing this
        // method when ValueMapper is null would give the engine a 0 total
        // for every XAML-driven series — they'd all be partitioned to an
        // empty rect, fall back to the full draw margin, and overlap.
        if (Values is null) return 0;
        _weightCache.Clear();
        var total = 0.0;
        foreach (var n in Values)
        {
            if (n is null) continue;
            total += ResolveValue(n);
        }
        return total;
    }

    /// <summary>
    /// Resolves the weight of a node. Leaves return their own value; internal
    /// nodes either return their explicit value or fall back to the sum of
    /// their children's resolved weights. Required.
    /// </summary>
    public Func<TModel, double>? ValueMapper { get; set => SetProperty(ref field, value); }

    /// <summary>
    /// Returns the children of a node, or <c>null</c> for a leaf. <c>null</c>
    /// on the series itself means "every node is a leaf" (flat treemap).
    /// </summary>
    public Func<TModel, IEnumerable<TModel>?>? ChildrenMapper { get; set => SetProperty(ref field, value); }

    /// <summary>
    /// Returns the per-tile label text. Only leaves are labeled — internal
    /// nodes are containers for their children's tiles. Set
    /// <see cref="Series{TModel, TVisual, TLabel}.DataLabelsPaint"/> to opt in.
    /// </summary>
    public Func<TModel, string?>? LabelMapper { get; set => SetProperty(ref field, value); }

    /// <summary>Tooltip label formatter on the typed point — null falls back to "&lt;label&gt;: &lt;value&gt;".</summary>
    public Func<ChartPoint<TModel, TVisual, TLabel>, string>? ToolTipLabelFormatter
    {
        get => _tooltipLabelFormatter;
        set => ((ITreemapSeries)this).TooltipLabelFormatter = value is null ? null : p => value(ConvertToTypedChartPoint(p));
    }

    Func<ChartPoint, string>? ITreemapSeries.TooltipLabelFormatter
    {
        get => _tooltipLabelFormatter;
        set => SetProperty(ref _tooltipLabelFormatter, value);
    }

    // ---- template method ----------------------------------------------------

    /// <summary>
    /// Builds the per-frame measure context from the chart. Captures the
    /// series's assigned sub-rectangle of the draw margin (the engine
    /// partitions the draw margin by series totals before invalidating each
    /// series) plus per-series cosmetic settings. Falls back to the full draw
    /// margin when the engine hasn't assigned a rectangle yet — covers the
    /// single-series case where no partition is needed.
    /// </summary>
    protected virtual TreemapMeasureContext BeginMeasure(TreemapChartEngine chart)
    {
        var isFirstDraw = !((Chart)chart).IsDrawn(((ISeries)this).SeriesId);

        var assigned = AssignedRectangle;
        var hasAssignment = assigned.Size.Width > 0 && assigned.Size.Height > 0;

        return new TreemapMeasureContext(
            chart,
            drawLocation: hasAssignment ? assigned.Location : chart.DrawMarginLocation,
            drawMarginSize: hasAssignment ? assigned.Size : chart.DrawMarginSize,
            padding: (float)Padding,
            cornerRadius: (float)CornerRadius,
            isFirstDraw: isFirstDraw);
    }

    /// <summary>
    /// Ensures a visual exists for the given node. On first creation seeds the
    /// visual at the tile's center with zero size so the squarified position
    /// is reached via animation.
    /// </summary>
    protected virtual TVisual EnsureVisualForNode(object node, LvcRectangle rect, in TreemapMeasureContext ctx)
    {
        if (_nodeVisuals.TryGetValue(node, out var existing))
            return existing;

        var v = new TVisual
        {
            X = rect.Location.X + rect.Size.Width * 0.5f,
            Y = rect.Location.Y + rect.Size.Height * 0.5f,
            Width = 0,
            Height = 0,
        };
        v.Animate(GetAnimation(ctx.Chart));

        _nodeVisuals[node] = v;
        return v;
    }

    /// <summary>
    /// Applies the squarified rectangle (minus padding) to the visual. The
    /// motion layer tweens to the new values automatically.
    /// </summary>
    protected virtual void UpdateVisualGeometry(TVisual visual, LvcRectangle rect, in TreemapMeasureContext ctx)
    {
        visual.X = rect.Location.X;
        visual.Y = rect.Location.Y;
        visual.Width = rect.Size.Width;
        visual.Height = rect.Size.Height;
        visual.RemoveOnCompleted = false;

        if (visual is BaseRoundedRectangleGeometry rr)
            rr.BorderRadius = new LvcPoint(ctx.CornerRadius, ctx.CornerRadius);
    }

    /// <summary>
    /// Collapses a removed tile to zero size at its center for a clean exit.
    /// </summary>
    protected virtual void CollapseRemovedVisual(TVisual visual)
    {
        var cx = visual.X + visual.Width * 0.5f;
        var cy = visual.Y + visual.Height * 0.5f;
        visual.X = cx;
        visual.Y = cy;
        visual.Width = 0;
        visual.Height = 0;
        visual.RemoveOnCompleted = true;
    }

    /// <inheritdoc cref="ChartElement.Invalidate(Chart)"/>
    public sealed override void Invalidate(Chart chart)
    {
        var treemapChart = (TreemapChartEngine)chart;
        _ = GetAnimation(treemapChart);

        var ctx = BeginMeasure(treemapChart);

        InitializePaints(treemapChart);

        _seenThisMeasure.Clear();
        _labeledThisMeasure.Clear();
        _weightCache.Clear();

        if (Values is not null)
        {
            var rect = new LvcRectangle(ctx.DrawLocation, ctx.DrawMarginSize);
            LayoutSiblings(Values, rect, in ctx);
        }

        // Reap visuals for nodes that were not visited this measure.
        if (_nodeVisuals.Count != _seenThisMeasure.Count)
        {
            var toRemove = new List<object>();
            foreach (var kv in _nodeVisuals)
            {
                if (_seenThisMeasure.Contains(kv.Key)) continue;
                CollapseRemovedVisual(kv.Value);
                toRemove.Add(kv.Key);
            }
            foreach (var k in toRemove)
            {
                _ = _nodeVisuals.Remove(k);
                _ = _nodePoints.Remove(k);
            }
        }

        // Reap labels for any key not labeled this pass. Covers: node removed
        // entirely, node became internal (leaf → parent), LabelMapper returned
        // null, ShowDataLabels / DataLabelsPaint / LabelMapper turned off.
        // Note: still-visible-but-too-small tiles ARE added to _labeledThisMeasure
        // (with Opacity=0) so a later resize can bring them back without recreate.
        if (_nodeLabels.Count != _labeledThisMeasure.Count)
        {
            var toDrop = new List<object>();
            foreach (var kv in _nodeLabels)
            {
                if (_labeledThisMeasure.Contains(kv.Key)) continue;
                kv.Value.Opacity = 0;
                kv.Value.RemoveOnCompleted = true;
                toDrop.Add(kv.Key);
            }
            foreach (var k in toDrop) _ = _nodeLabels.Remove(k);
        }
    }

    /// <summary>
    /// Sets per-Z-index ordering on Fill / Stroke / DataLabelsPaint and
    /// registers them as drawable tasks on the canvas. Labels sit above
    /// tiles (PieSeriesDataLabelsBaseZIndex offset) and do not clip to the
    /// draw margin.
    /// </summary>
    private void InitializePaints(TreemapChartEngine chart)
    {
        var actualZIndex = ZIndex == 0 ? ((ISeries)this).SeriesId : ZIndex;

        if (Fill is not null && Fill != Paint.Default)
        {
            Fill.ZIndex = actualZIndex + PaintConstants.SeriesFillZIndexOffset;
            chart.Canvas.AddDrawableTask(Fill, zone: CanvasZone.DrawMargin);
        }
        if (Stroke is not null && Stroke != Paint.Default)
        {
            Stroke.ZIndex = actualZIndex + PaintConstants.SeriesStrokeZIndexOffset;
            chart.Canvas.AddDrawableTask(Stroke, zone: CanvasZone.DrawMargin);
        }
        if (ShowDataLabels && DataLabelsPaint is not null && DataLabelsPaint != Paint.Default)
        {
            DataLabelsPaint.ZIndex =
                PaintConstants.PieSeriesDataLabelsBaseZIndex + actualZIndex + PaintConstants.SeriesGeometryFillZIndexOffset;
            chart.Canvas.AddDrawableTask(DataLabelsPaint);
        }
    }

    private void LayoutSiblings(IEnumerable<TModel> siblings, LvcRectangle rect, in TreemapMeasureContext ctx)
    {
        var items = new List<(double weight, TModel node)>();
        foreach (var node in siblings)
        {
            if (node is null) continue;
            items.Add((ResolveValue(node), node));
        }

        var placements = TreemapLayout.Squarify(items, rect);
        foreach (var (node, childRect) in placements)
            EmitTile(node, childRect, in ctx);
    }

    /// <summary>
    /// Resolves a node's weight. Falls back to the sum of its descendants'
    /// resolved weights when the node has children and no explicit non-zero
    /// value of its own. When mappers are not configured but the node is a
    /// built-in <see cref="TreemapNode"/>, its members are used directly —
    /// covers the XAML path where source-generated wrappers instantiate the
    /// underlying series with an erased TModel that prevents the facade
    /// constructor's auto-wire from running. Memoized per measure pass
    /// (cleared at the top of <see cref="GetTotalWeight"/> and
    /// <see cref="Invalidate"/>) so deep hierarchies don't re-sum the same
    /// subtrees at every depth.
    /// </summary>
    private double ResolveValue(TModel node)
    {
        if (_weightCache.TryGetValue(node!, out var cached)) return cached;

        var v = ValueMapper is not null
            ? ValueMapper(node)
            : node is TreemapNode tn ? tn.Value
            : throw new InvalidOperationException(
                $"{nameof(ValueMapper)} is required on {nameof(CoreTreemapSeries<TModel, TVisual, TLabel>)} when TModel is not {nameof(TreemapNode)}.");

        if (Math.Abs(v) > double.Epsilon)
        {
            _weightCache[node!] = v;
            return v;
        }

        var children = ResolveChildren(node);
        if (children is null)
        {
            _weightCache[node!] = 0;
            return 0;
        }

        var sum = 0.0;
        foreach (var c in children)
        {
            if (c is null) continue;
            sum += ResolveValue(c);
        }
        _weightCache[node!] = sum;
        return sum;
    }

    /// <summary>Resolves a node's children, with the same TreemapNode fallback as <see cref="ResolveValue"/>.</summary>
    private IEnumerable<TModel>? ResolveChildren(TModel node)
    {
        if (ChildrenMapper is not null) return ChildrenMapper(node);
        if (node is TreemapNode tn && tn.Children is { } children)
            return (IEnumerable<TModel>)children;
        return null;
    }

    /// <summary>Resolves a node's label, with the same TreemapNode fallback as <see cref="ResolveValue"/>.</summary>
    private string? ResolveLabel(TModel node)
    {
        if (LabelMapper is not null) return LabelMapper(node);
        if (node is TreemapNode tn) return tn.Name;
        return null;
    }

    private void EmitTile(TModel node, LvcRectangle rect, in TreemapMeasureContext ctx)
    {
        // Inset by padding so siblings get a visible gap. Keep dimensions
        // non-negative — a tile thinner than 2 * padding collapses to a line.
        var pad = ctx.Padding;
        var innerW = rect.Size.Width - pad * 2;
        var innerH = rect.Size.Height - pad * 2;
        if (innerW < 0) innerW = 0;
        if (innerH < 0) innerH = 0;
        var inner = new LvcRectangle(
            new LvcPoint(rect.Location.X + pad, rect.Location.Y + pad),
            new LvcSize(innerW, innerH));

        var visual = EnsureVisualForNode(node!, inner, in ctx);
        if (Fill is not null && Fill != Paint.Default)
            Fill.AddGeometryToPaintTask(ctx.Chart.Canvas, visual);
        if (Stroke is not null && Stroke != Paint.Default)
            Stroke.AddGeometryToPaintTask(ctx.Chart.Canvas, visual);

        UpdateVisualGeometry(visual, inner, in ctx);
        _ = _seenThisMeasure.Add(node!);

        var kids = ResolveChildren(node);
        var isLeaf = kids is null;

        // Leaves get a ChartPoint with a RectangleHoverArea matching the tile
        // so tooltip / DataPointerDown / hover dispatch via the engine just
        // works. Internal nodes don't — a hover would otherwise hit both the
        // child and every ancestor along the path.
        if (isLeaf) EnsureChartPoint(node, inner, in ctx);

        // Labels go on leaves only — internal-node tiles are containers and
        // would just overlap with their children's labels.
        if (isLeaf) MeasureDataLabel(node, inner, in ctx);

        if (!isLeaf)
        {
            // Evict leaf-only state when a node transitions from leaf →
            // internal between measures. The Invalidate-time reaper only
            // fires on disappearance (not in _seenThisMeasure), so without
            // this we'd leave a stale ChartPoint with an active HoverArea
            // (phantom tooltips on internal-node tiles) and a stale label
            // still painted at Opacity=1 over the children's tiles.
            _ = _nodePoints.Remove(node!);
            if (_nodeLabels.TryGetValue(node!, out var staleLabel))
            {
                staleLabel.Opacity = 0;
                staleLabel.RemoveOnCompleted = true;
                _ = _nodeLabels.Remove(node!);
            }
            LayoutSiblings(kids!, inner, in ctx);
        }
    }

    /// <summary>
    /// Creates or refreshes the <see cref="ChartPoint"/> for a leaf — its
    /// HoverArea is set to the tile rectangle, Coordinate.PrimaryValue to
    /// the resolved weight, and DataSource to the user's node model so the
    /// tooltip formatter can access either via context or the typed point.
    /// </summary>
    private void EnsureChartPoint(TModel node, LvcRectangle rect, in TreemapMeasureContext ctx)
    {
        if (!_nodePoints.TryGetValue(node!, out var point))
        {
            var entity = new MappedChartEntity
            {
                MetaData = new ChartEntityMetaData(_ => { }) { EntityIndex = _nodePoints.Count },
            };
            point = new ChartPoint(ctx.Chart.View, this, entity);
            _nodePoints[node!] = point;
        }
        point.Context.DataSource = node;
        point.Context.Visual = _nodeVisuals[node!];

        var ha = point.Context.HoverArea as RectangleHoverArea ?? new RectangleHoverArea();
        _ = ha
            .SetDimensions(rect.Location.X, rect.Location.Y, rect.Size.Width, rect.Size.Height)
            .CenterXToolTip()
            .CenterYToolTip();
        point.Context.HoverArea = ha;

        ((MappedChartEntity)point.Context.Entity).Coordinate =
            new Coordinate(point.Context.Entity.MetaData!.EntityIndex, ResolveValue(node));
    }

    /// <summary>
    /// Ensures a label exists for the given leaf, updates its text + paint,
    /// and centers it inside the tile. No-op when the tile is too small to
    /// fit the label or when label rendering is disabled.
    /// </summary>
    private void MeasureDataLabel(TModel node, LvcRectangle rect, in TreemapMeasureContext ctx)
    {
        if (!ShowDataLabels || DataLabelsPaint is null || DataLabelsPaint == Paint.Default)
            return;

        var text = ResolveLabel(node);
        if (string.IsNullOrEmpty(text)) return;

        var cx = rect.Location.X + rect.Size.Width * 0.5f;
        var cy = rect.Location.Y + rect.Size.Height * 0.5f;

        if (!_nodeLabels.TryGetValue(node!, out var label))
        {
            label = new TLabel
            {
                X = cx,
                Y = cy,
                HorizontalAlign = Align.Middle,
                VerticalAlign = Align.Middle,
                MaxWidth = (float)DataLabelsMaxWidth,
            };
            label.Animate(GetAnimation(ctx.Chart), BaseLabelGeometry.XProperty, BaseLabelGeometry.YProperty);
            _nodeLabels[node!] = label;
        }
        _ = _labeledThisMeasure.Add(node!);

        DataLabelsPaint.AddGeometryToPaintTask(ctx.Chart.Canvas, label);

        label.Text = text!;
        label.TextSize = (float)DataLabelsSize;
        label.Padding = DataLabelsPadding;
        label.Paint = DataLabelsPaint;

        // Cull labels that won't fit. Measure() gives the rendered bounds;
        // a tile narrower than the text just renders a blank rect.
        var measured = label.Measure();
        if (measured.Width > rect.Size.Width || measured.Height > rect.Size.Height)
        {
            label.Opacity = 0;
            return;
        }
        label.Opacity = 1;
        label.X = cx;
        label.Y = cy;
    }

    // ---- Series abstract members (image-gen scope: stubs / no-ops) ----------

    /// <inheritdoc cref="ISeries.GetPrimaryToolTipText(ChartPoint)"/>
    public override string? GetPrimaryToolTipText(ChartPoint point)
    {
        if (_tooltipLabelFormatter is not null) return _tooltipLabelFormatter(point);

        // Default: "<label>: <value>" — or just "<value>" when the label is empty.
        var node = point.Context.DataSource is TModel m ? m : default;
        var label = node is not null ? ResolveLabel(node) : null;
        var value = point.Coordinate.PrimaryValue;
        return string.IsNullOrEmpty(label) ? value.ToString() : $"{label}: {value}";
    }

    /// <inheritdoc cref="ISeries.GetSecondaryToolTipText(ChartPoint)"/>
    public override string? GetSecondaryToolTipText(ChartPoint point) => LiveCharts.IgnoreToolTipLabel;

    /// <inheritdoc cref="Series{TModel, TVisual, TLabel}.FindPointsInPosition"/>
    protected override IEnumerable<ChartPoint> FindPointsInPosition(
        Chart chart, LvcPoint pointerPosition, FindingStrategy strategy, FindPointFor findPointFor) =>
        _nodePoints.Values.Where(p =>
            p.Context.HoverArea is not null &&
            p.Context.HoverArea.IsPointerOver(pointerPosition, strategy));

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
    protected internal override Paint?[] GetPaintTasks() => [Fill, Stroke, DataLabelsPaint];

    /// <inheritdoc cref="Series{TModel, TVisual, TLabel}.SetDefaultPointTransitions(ChartPoint)"/>
    protected override void SetDefaultPointTransitions(ChartPoint chartPoint)
    {
        // No-op: treemap visuals are managed directly via _nodeVisuals and animated
        // in EnsureVisualForNode; no ChartPoint round-trip.
    }

    /// <inheritdoc cref="ISeries.SoftDeleteOrDispose(IChartView)"/>
    public override void SoftDeleteOrDispose(IChartView chart)
    {
        var core = ((ITreemapChartView)chart).Core;

        foreach (var visual in _nodeVisuals.Values)
            CollapseRemovedVisual(visual);
        _nodeVisuals.Clear();
        _nodePoints.Clear();
        foreach (var label in _nodeLabels.Values)
        {
            label.Opacity = 0;
            label.RemoveOnCompleted = true;
        }
        _nodeLabels.Clear();
        _seenThisMeasure.Clear();
        _labeledThisMeasure.Clear();
        _weightCache.Clear();

        foreach (var pt in GetPaintTasks())
            if (pt is not null) core.Canvas.RemovePaintTask(pt);
    }

    private sealed class ReferenceComparer : IEqualityComparer<object>
    {
        public static ReferenceComparer Instance { get; } = new();
        public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);
        public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
    }
}
