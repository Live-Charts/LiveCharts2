using LiveChartsCore.Drawing;

namespace LiveChartsCore.Noesis;

/// <summary>Named colors for chart examples and styles.</summary>
public static class DrawingColors
{
    /// <summary>Returns the color with the given alpha.</summary>
    public static LvcColor WithAlpha(this LvcColor color, byte alpha) => LvcColor.FromArgb(alpha, color);

    /// <summary>Gets the Black color.</summary>
    public static LvcColor Black => new(0, 0, 0, 255);

    /// <summary>Gets the Blue color.</summary>
    public static LvcColor Blue => new(0, 0, 255, 255);

    /// <summary>Gets the CadetBlue color.</summary>
    public static LvcColor CadetBlue => new(95, 158, 160, 255);

    /// <summary>Gets the DeepSkyBlue color.</summary>
    public static LvcColor DeepSkyBlue => new(0, 191, 255, 255);

    /// <summary>Gets the Gray color.</summary>
    public static LvcColor Gray => new(128, 128, 128, 255);

    /// <summary>Gets the Green color.</summary>
    public static LvcColor Green => new(0, 128, 0, 255);

    /// <summary>Gets the LightGray color.</summary>
    public static LvcColor LightGray => new(211, 211, 211, 255);

    /// <summary>Gets the LightSteelBlue color.</summary>
    public static LvcColor LightSteelBlue => new(176, 196, 222, 255);

    /// <summary>Gets the Red color.</summary>
    public static LvcColor Red => new(255, 0, 0, 255);

    /// <summary>Gets the Transparent color.</summary>
    public static LvcColor Transparent => new(0, 0, 0, 0);

    /// <summary>Gets the White color.</summary>
    public static LvcColor White => new(255, 255, 255, 255);

    /// <summary>Gets the Yellow color.</summary>
    public static LvcColor Yellow => new(255, 255, 0, 255);

}
