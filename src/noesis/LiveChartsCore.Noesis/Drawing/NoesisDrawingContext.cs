using System.Globalization;
using LiveChartsCore.Drawing;
using LiveChartsCore.Noesis.Painting.Effects;
using LiveChartsCore.Measure;
using LiveChartsCore.Motion;
using LiveChartsCore.Noesis.Painting;
using LiveChartsCore.Painting;
using Noesis;
using Paint = LiveChartsCore.Painting.Paint;

namespace LiveChartsCore.Noesis.Drawing;

/// <summary>Maps LiveCharts drawing operations to Noesis.</summary>
public sealed class NoesisDrawingContext(global::Noesis.DrawingContext canvas) : LiveChartsCore.Drawing.DrawingContext
{
    private readonly Dictionary<LvcColor, SolidColorBrush> _brushes = new();
    private readonly List<BaseComponent> _resources = new();
    private LvcRectangle _paintBounds = new(new LvcPoint(0, 0), new LvcSize(1920, 1080));

    /// <summary>Gets the Noesis drawing target.</summary>
    public global::Noesis.DrawingContext Canvas { get; } = canvas;

    /// <summary>Gets the geometry draw count.</summary>
    public int DrawCount { get; private set; }

    /// <summary>Gets the active fill brush.</summary>
    public Brush? Fill { get; private set; }
    /// <summary>Gets the active stroke pen.</summary>
    public Pen? Stroke { get; private set; }

    /// <inheritdoc />
    public override void LogOnCanvas(string log)
    {
    }

    internal override void OnBeginDraw()
    {
    }

    internal override void OnEndDraw()
    {
    }

    /// <summary>Keeps a drawing resource alive until Noesis consumes the frame.</summary>
    public void Retain(BaseComponent resource)
    {
        _resources.Add(resource);
    }

    internal override void OnBeginZone(CanvasZone zone)
    {
        if (zone.Clip == LvcRectangle.Empty)
        {
            return;
        }

        _paintBounds = zone.Clip;

        var clip = new RectangleGeometry(new Rect(zone.Clip.X, zone.Clip.Y, zone.Clip.Width, zone.Clip.Height));

        _resources.Add(clip);
        Canvas.PushClip(clip);
    }

    internal override void OnEndZone(CanvasZone zone)
    {
        if (zone.Clip != LvcRectangle.Empty)
        {
            Canvas.Pop();
        }
    }

    internal override void SelectPaint(Paint paint)
    {
        ActiveLvcPaint = paint is NoesisPaint ? paint : null;
    }

    internal override void ClearPaintSelection(Paint paint)
    {
        ActiveLvcPaint = null;
    }

    internal override void Draw(IDrawnElement drawable)
    {
        if (drawable is not IDrawnElement<NoesisDrawingContext> element)
        {
            throw new NotSupportedException($"The Noesis renderer cannot draw {drawable.GetType().Name}.");
        }

        var transformed = drawable.HasTransform;

        if (transformed)
        {
            PushTransform(drawable);
        }

        if (ActiveLvcPaint is null)
        {
            DrawWithPaint(element, element.Fill, PaintStyle.Fill);
            DrawWithPaint(element, element.Stroke, PaintStyle.Stroke);

            if (element.Paint is not null && !ReferenceEquals(element.Paint, element.Fill) && !ReferenceEquals(element.Paint, element.Stroke))
            {
                DrawWithPaint(element, element.Paint, PaintStyle.Text);
            }
        }
        else
        {
            var style = ActiveLvcPaint.PaintStyle;

            if ((style & (PaintStyle.Fill | PaintStyle.Text)) != 0)
            {
                DrawWithPaint(element, element.Fill ?? ActiveLvcPaint, PaintStyle.Fill);
            }

            if ((style & PaintStyle.Stroke) != 0)
            {
                DrawWithPaint(element, element.Stroke ?? ActiveLvcPaint, PaintStyle.Stroke);
            }
        }

        if (transformed)
        {
            Canvas.Pop();
        }
    }

    internal SolidColorBrush GetBrush(LvcColor color, float opacity = 1)
    {
        color = LvcColor.FromArgb((byte)Math.Clamp(color.A * opacity, 0, 255), color);

        if (_brushes.TryGetValue(color, out var brush))
        {
            return brush;
        }

        brush = new SolidColorBrush(global::Noesis.Color.FromArgb(color.A, color.R, color.G, color.B));

        _brushes.Add(color, brush);
        _resources.Add(brush);

        return brush;
    }

    private void DrawWithPaint(IDrawnElement<NoesisDrawingContext> element, Paint? paint, PaintStyle style)
    {
        if (ReferenceEquals(paint, MeasureTask.Instance))
        {
            element.Draw(this);

            return;
        }

        if (paint is null)
        {
            return;
        }

        if (paint is not NoesisPaint noesisPaint)
        {
            throw new NotSupportedException($"The Noesis renderer cannot use {paint.GetType().Name}.");
        }

        paint.IsValid = true;

        var brush = element is IColoredGeometry colored && colored.Color != LvcColor.Empty
            ? GetBrush(colored.Color, ActiveOpacity)
            : noesisPaint.CreateBrush(_paintBounds, ActiveOpacity);

        Retain(brush);

        Fill = style == PaintStyle.Stroke ? null : brush;
        Stroke = null;

        if (style == PaintStyle.Stroke && paint.StrokeThickness > 0)
        {
            Stroke = new Pen
            {
                Brush = brush,
                Thickness = paint.StrokeThickness,
                StartLineCap = noesisPaint.StrokeCap,
                EndLineCap = noesisPaint.StrokeCap,
                DashCap = noesisPaint.StrokeCap,
                LineJoin = noesisPaint.StrokeJoin,
                MiterLimit = paint.StrokeMiter > 0 ? paint.StrokeMiter : 10
            };

            if (noesisPaint.PathEffect is DashEffect dash)
            {
                var thickness = paint.StrokeThickness;
                var dashStyle = new DashStyle
                {
                    Dashes = string.Join(",", dash.DashArray.Select(length => (length / thickness).ToString(CultureInfo.InvariantCulture))),
                    Offset = dash.Phase / thickness
                };

                Retain(dashStyle);
                Stroke.DashStyle = dashStyle;
            }

            _resources.Add(Stroke);
        }

        element.Draw(this);
        element.IsValid &= paint.IsValid;
        DrawCount++;
    }

    private void PushTransform(IDrawnElement element)
    {
        var size = element.Measure();
        var origin = element.TransformOrigin;
        var x = element.X + size.Width * origin.X;
        var y = element.Y + size.Height * origin.Y;
        var group = new TransformGroup();

        if (element.HasSkew)
        {
            group.Children.Add(new SkewTransform { AngleX = element.SkewTransform.X, AngleY = element.SkewTransform.Y, CenterX = x, CenterY = y });
        }

        if (element.HasScale)
        {
            group.Children.Add(new ScaleTransform { ScaleX = element.ScaleTransform.X, ScaleY = element.ScaleTransform.Y, CenterX = x, CenterY = y });
        }

        if (element.HasRotation)
        {
            group.Children.Add(new RotateTransform { Angle = element.RotateTransform, CenterX = x, CenterY = y });
        }

        if (element.HasTranslate)
        {
            group.Children.Add(new TranslateTransform(element.TranslateTransform.X, element.TranslateTransform.Y));
        }

        _resources.Add(group);
        Canvas.PushTransform(group);
    }
}
