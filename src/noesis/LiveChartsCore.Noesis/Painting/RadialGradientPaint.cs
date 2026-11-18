using LiveChartsCore.Drawing;
using LiveChartsCore.Painting;
using Noesis;

namespace LiveChartsCore.Noesis.Painting;

/// <summary>A radial chart gradient.</summary>
public class RadialGradientPaint : NoesisPaint
{
    /// <summary>Creates a two-color gradient.</summary>
    public RadialGradientPaint(LvcColor center, LvcColor outer) : this([center, outer])
    {
    }

    /// <summary>Creates a radial gradient.</summary>
    public RadialGradientPaint(LvcColor[] gradientStops, LvcPoint? center = null, float radius = 0.5f, float[]? colorPos = null, GradientSpreadMethod tileMode = GradientSpreadMethod.Pad)
    {
        GradientStops = gradientStops;
        Center = center ?? new LvcPoint(0.5f, 0.5f);
        Radius = radius;
        ColorPos = colorPos;
        TileMode = tileMode;
    }

    /// <summary>Gets or sets the gradient colors.</summary>
    public LvcColor[] GradientStops { get; set; }

    /// <summary>Gets or sets the center.</summary>
    public LvcPoint Center { get; set; }

    /// <summary>Gets or sets the radius.</summary>
    public float Radius { get; set; }

    /// <summary>Gets or sets color offsets.</summary>
    public float[]? ColorPos { get; set; }

    /// <summary>Gets or sets the spread method.</summary>
    public GradientSpreadMethod TileMode { get; set; }

    /// <inheritdoc />
    public override Paint CloneTask() => CopyTo(new RadialGradientPaint(GradientStops, Center, Radius, ColorPos, TileMode));

    internal override Brush CreateBrush(LvcRectangle bounds, float opacity)
    {
        var center = new Point(bounds.X + bounds.Width * Center.X, bounds.Y + bounds.Height * Center.Y);
        var radius = Math.Min(bounds.Width, bounds.Height) * Radius;
        var brush = new RadialGradientBrush
        {
            Center = center,
            GradientOrigin = center,
            RadiusX = radius,
            RadiusY = radius,
            MappingMode = BrushMappingMode.Absolute,
            SpreadMethod = TileMode,
            Opacity = opacity
        };

        for (var index = 0; index < GradientStops.Length; index++)
        {
            brush.GradientStops.Add(new GradientStop(ToColor(GradientStops[index]), ColorPos?[index] ?? (float)index / Math.Max(1, GradientStops.Length - 1)));
        }

        return brush;
    }
}
