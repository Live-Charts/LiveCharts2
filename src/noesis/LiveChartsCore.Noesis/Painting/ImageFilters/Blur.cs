namespace LiveChartsCore.Noesis.Painting.ImageFilters;

/// <summary>Describes a Gaussian blur.</summary>
public sealed class Blur(float sigmaX, float sigmaY) : ImageFilter
{
    /// <summary>Gets the horizontal blur.</summary>
    public float SigmaX { get; } = sigmaX;

    /// <summary>Gets the vertical blur.</summary>
    public float SigmaY { get; } = sigmaY;
}
