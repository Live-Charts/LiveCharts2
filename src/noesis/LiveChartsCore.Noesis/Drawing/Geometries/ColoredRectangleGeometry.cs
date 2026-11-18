using LiveChartsCore.Drawing;
using LiveChartsCore.Generators;

namespace LiveChartsCore.Noesis.Drawing.Geometries;

/// <inheritdoc />
public partial class ColoredRectangleGeometry : RectangleGeometry, IColoredGeometry
{
    /// <summary>Creates a colored geometry.</summary>
    public ColoredRectangleGeometry()
    {
        Color = LvcColor.Empty;
    }

    /// <inheritdoc />
    [MotionProperty]
    public partial LvcColor Color { get; set; }
}
