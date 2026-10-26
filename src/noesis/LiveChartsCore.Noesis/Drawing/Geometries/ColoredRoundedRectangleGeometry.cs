using LiveChartsCore.Drawing;
using LiveChartsCore.Generators;

namespace LiveChartsCore.Noesis.Drawing.Geometries;

/// <inheritdoc />
public partial class ColoredRoundedRectangleGeometry : RoundedRectangleGeometry, IColoredGeometry
{
    /// <summary>Creates a colored geometry.</summary>
    public ColoredRoundedRectangleGeometry()
    {
        Color = LvcColor.Empty;
    }

    /// <inheritdoc />
    [MotionProperty]
    public partial LvcColor Color { get; set; }
}
