using LiveChartsCore.Drawing;

namespace LiveChartsCore.Noesis.Drawing.Geometries;

/// <inheritdoc />
public sealed class HeartGeometry : BaseSVGPathGeometry
{
    /// <summary>Creates the marker.</summary>
    public HeartGeometry() : base(global::Noesis.Geometry.Parse(SVGPoints.Heart))
    {
        FitToSize = true;
    }
}
