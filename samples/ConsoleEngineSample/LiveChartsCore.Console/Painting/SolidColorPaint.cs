// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Console.Drawing;
using LiveChartsCore.Drawing;
using LiveChartsCore.Painting;

namespace LiveChartsCore.Console.Painting;

public class SolidColorPaint : ConsolePaint
{
    public SolidColorPaint() { }

    public SolidColorPaint(LvcColor color) { Color = color; }

    public SolidColorPaint(LvcColor color, float strokeWidth) : base(strokeWidth) { Color = color; }

    public LvcColor Color { get; set; } = new(255, 255, 255);

    public override Paint CloneTask() => new SolidColorPaint { Color = Color };

    internal override void OnPaintStarted(DrawingContext drawingContext, IDrawnElement? drawnElement) =>
        ((ConsoleDrawingContext)drawingContext).ActiveColor = Color;

    internal override Paint Transitionate(float progress, Paint target)
    {
        if (target is not SolidColorPaint to) return target;
        Color = new LvcColor(
            (byte)(Color.R + progress * (to.Color.R - Color.R)),
            (byte)(Color.G + progress * (to.Color.G - Color.G)),
            (byte)(Color.B + progress * (to.Color.B - Color.B)),
            (byte)(Color.A + progress * (to.Color.A - Color.A)));
        return this;
    }

    internal override void ApplyOpacityMask(DrawingContext context, float opacity, IDrawnElement? drawnElement)
    {
        // skipped — single-cell color cannot meaningfully alpha-blend without a per-pixel
        // composite step we don't have.
    }

    internal override void RestoreOpacityMask(DrawingContext context, float opacity, IDrawnElement? drawnElement) { }
}
