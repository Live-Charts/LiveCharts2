using LiveChartsCore.Drawing;

namespace LiveChartsCore.Noesis.Drawing.Geometries;

/// <inheritdoc />
public sealed class CrossGeometry : BaseSVGPathGeometry
{
    /// <summary>Creates the marker.</summary>
    public CrossGeometry() : base(global::Noesis.Geometry.Parse(SVGPoints.Cross))
    {
        FitToSize = true;
    }
}
