using LiveChartsCore.Drawing;
using LiveChartsCore.Generators;
using LiveChartsCore.Painting;

namespace LiveChartsCore.Noesis.Painting;

/// <summary>A solid chart paint with no Skia dependency.</summary>
public partial class SolidColorPaint : NoesisPaint
{
    /// <summary>Creates a transparent paint.</summary>
    public SolidColorPaint() : this(new LvcColor())
    {
    }

    /// <summary>Creates a paint with the given color and stroke width.</summary>
    public SolidColorPaint(LvcColor color, float strokeWidth = 1) : base(strokeWidth)
    {
        Color = color;
    }

    /// <summary>Gets or sets the animated color.</summary>
    [MotionProperty]
    public partial LvcColor Color { get; set; }

    /// <inheritdoc />
    public override Paint CloneTask() => CopyTo(new SolidColorPaint(Color, StrokeThickness));

    internal override global::Noesis.Brush CreateBrush(LvcRectangle bounds, float opacity) =>
        new global::Noesis.SolidColorBrush(ToColor(Color)) { Opacity = opacity };
}
