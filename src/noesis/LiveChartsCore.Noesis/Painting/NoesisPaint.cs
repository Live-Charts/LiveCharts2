using LiveChartsCore.Drawing;
using LiveChartsCore.Noesis.Painting.Effects;
using LiveChartsCore.Noesis.Painting.ImageFilters;
using LiveChartsCore.Painting;
using Noesis;
using DrawingContext = LiveChartsCore.Drawing.DrawingContext;

namespace LiveChartsCore.Noesis.Painting;

/// <summary>Chart paint rendered by Noesis.</summary>
public abstract class NoesisPaint(float strokeWidth = 1) : Paint(strokeWidth)
{
    /// <summary>Gets or sets the font family.</summary>
    public string FontFamily { get; set; } = "Arial";

    /// <summary>Gets or sets the font weight.</summary>
    public FontWeight FontWeight { get; set; } = FontWeight.Normal;

    /// <summary>Gets or sets the font stretch.</summary>
    public FontStretch FontWidth { get; set; } = FontStretch.Normal;

    /// <summary>Gets or sets the font style.</summary>
    public FontStyle FontSlant { get; set; } = FontStyle.Normal;

    /// <summary>Gets or sets stroke caps.</summary>
    public PenLineCap StrokeCap { get; set; }

    /// <summary>Gets or sets stroke joins.</summary>
    public PenLineJoin StrokeJoin { get; set; }

    /// <summary>Gets or sets the stroke effect.</summary>
    public PathEffect? PathEffect { get; set; }

    /// <summary>Gets or sets the image filter.</summary>
    public ImageFilter? ImageFilter { get; set; }

    /// <summary>Copies the paint settings to a clone.</summary>
    protected T CopyTo<T>(T clone) where T : NoesisPaint
    {
        clone.ZIndex = ZIndex;
        clone.IsAntialias = IsAntialias;
        clone.PaintStyle = PaintStyle;
        clone.StrokeThickness = StrokeThickness;
        clone.StrokeMiter = StrokeMiter;
        clone.FontFamily = FontFamily;
        clone.FontWeight = FontWeight;
        clone.FontWidth = FontWidth;
        clone.FontSlant = FontSlant;
        clone.StrokeCap = StrokeCap;
        clone.StrokeJoin = StrokeJoin;
        clone.PathEffect = PathEffect;
        clone.ImageFilter = ImageFilter;

        return clone;
    }

    internal abstract Brush CreateBrush(LvcRectangle bounds, float opacity);

    internal override void OnPaintStarted(DrawingContext drawingContext, IDrawnElement? drawnElement)
    {
    }

    internal override void OnPaintFinished(DrawingContext drawingContext, IDrawnElement? drawnElement)
    {
    }

    internal override void ApplyOpacityMask(DrawingContext context, float opacity, IDrawnElement? drawnElement)
    {
    }

    internal override void RestoreOpacityMask(DrawingContext context, float opacity, IDrawnElement? drawnElement)
    {
    }

    internal override void DisposeTask()
    {
    }

    internal static global::Noesis.Color ToColor(LvcColor color) =>
        global::Noesis.Color.FromArgb(color.A, color.R, color.G, color.B);
}
