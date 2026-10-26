using LiveChartsCore.Drawing;

namespace LiveChartsCore.Noesis.Drawing.Geometries;

/// <inheritdoc />
public sealed class DiamondGeometry : BaseSVGPathGeometry
{
    /// <summary>Creates the marker.</summary>
    public DiamondGeometry() : base(global::Noesis.Geometry.Parse(SVGPoints.Diamond))
    {
        FitToSize = true;
    }
}
