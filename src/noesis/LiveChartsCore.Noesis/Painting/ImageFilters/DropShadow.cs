using LiveChartsCore.Drawing;

namespace LiveChartsCore.Noesis.Painting.ImageFilters;

/// <summary>Describes a drop shadow.</summary>
public sealed class DropShadow(float dx, float dy, float sigmaX, float sigmaY, LvcColor color) : ImageFilter
{
    /// <summary>Gets the horizontal offset.</summary>
    public float Dx { get; } = dx;

    /// <summary>Gets the vertical offset.</summary>
    public float Dy { get; } = dy;

    /// <summary>Gets the horizontal blur.</summary>
    public float SigmaX { get; } = sigmaX;

    /// <summary>Gets the vertical blur.</summary>
    public float SigmaY { get; } = sigmaY;

    /// <summary>Gets the shadow color.</summary>
    public LvcColor Color { get; } = color;
}
