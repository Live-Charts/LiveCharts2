namespace LiveChartsCore.Noesis.Painting.Effects;

/// <summary>Describes a dashed stroke.</summary>
public sealed class DashEffect(float[] dashArray, float phase = 0) : PathEffect
{
    /// <summary>Gets the dash lengths.</summary>
    public float[] DashArray { get; } = dashArray;

    /// <summary>Gets the dash offset.</summary>
    public float Phase { get; } = phase;
}
