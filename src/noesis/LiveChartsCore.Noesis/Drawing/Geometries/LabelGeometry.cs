using LiveChartsCore.Drawing;
using LiveChartsCore.Measure;
using LiveChartsCore.Noesis.Painting;
using Noesis;

namespace LiveChartsCore.Noesis.Drawing.Geometries;

/// <inheritdoc />
public class LabelGeometry : BaseLabelGeometry, IDrawnElement<NoesisDrawingContext>
{
    private FontFamily _font = new("Arial");
    private string _fontName = "Arial";
    private (FontWeight Weight, FontStretch Stretch, FontStyle Style, bool RightToLeft) _textStyle;
    private FormattedText? _text;
    private string? _previousText;
    private float _previousSize;
    private float _previousWidth;

    /// <inheritdoc />
    public override LvcSize Measure() => GetSize().GetRotatedSize(RotateTransform);

    /// <inheritdoc />
    public virtual void Draw(NoesisDrawingContext context)
    {
        var paint = Paint as NoesisPaint ?? context.ActiveLvcPaint as NoesisPaint;
        var size = GetSize(paint);
        var x = X - AlignmentOffset(HorizontalAlign, size.Width);
        var y = Y - AlignmentOffset(VerticalAlign, size.Height);

        if (Background != LvcColor.Empty)
        {
            context.Canvas.DrawRectangle(context.GetBrush(Background, context.ActiveOpacity), null, new Rect(x, y, size.Width, size.Height));
        }

        var brush = context.Fill ?? context.Stroke?.Brush;

        if (brush is null)
        {
            return;
        }

        var text = CreateText(paint, brush);

        context.Retain(text);
        context.Canvas.DrawText(text, new Point(x + Padding.Left, y + Padding.Top));
    }

    private FormattedText CreateText(NoesisPaint? paint, Brush brush)
    {
        var fontName = paint?.FontFamily ?? "Arial";

        if (_fontName != fontName)
        {
            _font = new FontFamily(fontName);
            _fontName = fontName;
        }

        var direction = LiveChartsNoesis.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        return new FormattedText(
            Text,
            _font,
            paint?.FontWeight ?? FontWeight.Normal,
            paint?.FontWidth ?? FontStretch.Normal,
            paint?.FontSlant ?? FontStyle.Normal,
            Math.Max(1, TextSize),
            brush,
            direction,
            Math.Clamp(MaxWidth, 1, 16384));
    }

    private LvcSize GetSize(NoesisPaint? paint = null)
    {
        paint ??= Paint as NoesisPaint;

        var style = (paint?.FontWeight ?? FontWeight.Normal, paint?.FontWidth ?? FontStretch.Normal, paint?.FontSlant ?? FontStyle.Normal, LiveChartsNoesis.IsRightToLeft);
        if (_text is null || _previousText != Text || _previousSize != TextSize || _previousWidth != MaxWidth || _fontName != (paint?.FontFamily ?? "Arial") || _textStyle != style)
        {
            _text?.Dispose();

            _text = CreateText(paint, Brushes.Black);
            _textStyle = style;
            _previousText = Text;
            _previousSize = TextSize;
            _previousWidth = MaxWidth;
        }

        var bounds = _text.Bounds;

        return new LvcSize(bounds.Width + Padding.Left + Padding.Right, bounds.Height + Padding.Top + Padding.Bottom);
    }

    internal override void OnDisposed()
    {
        _text?.Dispose();
        _text = null;

        base.OnDisposed();
    }

    private static float AlignmentOffset(Align alignment, float size) => alignment switch
    {
        Align.Middle => size / 2,
        Align.End => size,
        _ => 0
    };
}
