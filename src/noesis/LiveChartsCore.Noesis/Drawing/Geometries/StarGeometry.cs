using LiveChartsCore.Drawing;

namespace LiveChartsCore.Noesis.Drawing.Geometries;

/// <inheritdoc />
public sealed class StarGeometry : BaseSVGPathGeometry
{
    /// <summary>Creates the marker.</summary>
    public StarGeometry() : base(global::Noesis.Geometry.Parse(SVGPoints.Star))
    {
        FitToSize = true;
    }
}
