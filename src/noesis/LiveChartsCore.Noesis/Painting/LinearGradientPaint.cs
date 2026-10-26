using LiveChartsCore.Drawing;
using LiveChartsCore.Painting;
using Noesis;

namespace LiveChartsCore.Noesis.Painting;

/// <summary>A linear chart gradient.</summary>
public class LinearGradientPaint : NoesisPaint
{
    /// <summary>Gets the default start point.</summary>
    public static readonly LvcPoint DefaultStartPoint = new(0, 0.5f);

    /// <summary>Gets the default end point.</summary>
    public static readonly LvcPoint DefaultEndPoint = new(1, 0.5f);

    /// <summary>Creates a two-color gradient.</summary>
    public LinearGradientPaint(LvcColor start, LvcColor end) : this([start, end])
    {
    }

    /// <summary>Creates a gradient.</summary>
    public LinearGradientPaint(LvcColor[] gradientStops, LvcPoint? startPoint = null, LvcPoint? endPoint = null, float[]? colorPos = null, GradientSpreadMethod tileMode = GradientSpreadMethod.Pad)
    {
        GradientStops = gradientStops;
        StartPoint = startPoint ?? DefaultStartPoint;
        EndPoint = endPoint ?? DefaultEndPoint;
        ColorPos = colorPos;
        TileMode = tileMode;
    }

    /// <summary>Gets or sets the gradient colors.</summary>
    public LvcColor[] GradientStops { get; set; }

    /// <summary>Gets or sets the start point.</summary>
    public LvcPoint StartPoint { get; set; }

    /// <summary>Gets or sets the end point.</summary>
    public LvcPoint EndPoint { get; set; }

    /// <summary>Gets or sets color offsets.</summary>
    public float[]? ColorPos { get; set; }

    /// <summary>Gets or sets the spread method.</summary>
    public GradientSpreadMethod TileMode { get; set; }

    /// <inheritdoc />
    public override Paint CloneTask() => CopyTo(new LinearGradientPaint(GradientStops, StartPoint, EndPoint, ColorPos, TileMode));

    internal override Brush CreateBrush(LvcRectangle bounds, float opacity)
    {
        var brush = new LinearGradientBrush
        {
            StartPoint = new Point(bounds.X + bounds.Width * StartPoint.X, bounds.Y + bounds.Height * StartPoint.Y),
            EndPoint = new Point(bounds.X + bounds.Width * EndPoint.X, bounds.Y + bounds.Height * EndPoint.Y),
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
