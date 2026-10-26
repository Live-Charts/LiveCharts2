namespace LiveChartsCore.Noesis.Drawing.Geometries;

/// <summary>Holds the shared map viewport.</summary>
public sealed class MapViewportTransform
{
    /// <summary>Gets or sets zoom.</summary>
    public float Zoom { get; set; } = 1;

    /// <summary>Gets or sets horizontal pan.</summary>
    public float PanX { get; set; }

    /// <summary>Gets or sets vertical pan.</summary>
    public float PanY { get; set; }

    /// <summary>Gets or sets the horizontal center.</summary>
    public float CenterX { get; set; }

    /// <summary>Gets or sets the vertical center.</summary>
    public float CenterY { get; set; }

    /// <summary>Gets whether the viewport is transformed.</summary>
    public bool IsActive => Zoom != 1 || PanX != 0 || PanY != 0;

    /// <summary>Maps a pointer into map coordinates.</summary>
    public (float X, float Y) InverseTransform(float x, float y) =>
        ((x - PanX - CenterX) / Zoom + CenterX, (y - PanY - CenterY) / Zoom + CenterY);
}
