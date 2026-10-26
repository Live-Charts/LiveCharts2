using LiveChartsCore.Noesis.Drawing;
using LiveChartsCore.Noesis.Drawing.Geometries;
using Noesis;

namespace NoesisSample.Pies.Icons;

public class SvgLabel : LabelGeometry
{
    private Geometry? _path;
    private readonly SolidColorBrush _brush = new(Color.FromRgb(245, 245, 245));
    private readonly FontFamily _font = new("Arial");

    public string Name { get; set; } = string.Empty;

    public float Size { get; set; }

    public string SvgString
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            _path = string.IsNullOrWhiteSpace(value) ? null : Geometry.Parse(value);
        }
    } = string.Empty;

    public override void Draw(NoesisDrawingContext context)
    {
        if (_path is null)
        {
            return;
        }

        var bounds = _path.Bounds;
        var extent = Math.Max(bounds.Width, bounds.Height);

        if (extent <= 0 || Size <= 0)
        {
            return;
        }

        var scale = Size / extent;
        var transforms = new TransformGroup();

        transforms.Children.Add(new TranslateTransform(-bounds.X, -bounds.Y));
        transforms.Children.Add(new ScaleTransform(scale, scale));
        transforms.Children.Add(new TranslateTransform(X, Y));

        context.Retain(transforms);
        context.Canvas.PushTransform(transforms);
        context.Canvas.DrawGeometry(_brush, null, _path);
        context.Canvas.Pop();

        var text = new FormattedText(Name, _font, 16, _brush);

        context.Retain(text);
        context.Canvas.DrawText(text, new Point(X, Y - text.Bounds.Height - 10));
    }
}
