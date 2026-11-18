using LiveChartsCore.Drawing;

namespace LiveChartsCore.Noesis.Drawing.Geometries;

/// <inheritdoc />
public class VariableSVGPathGeometry : BaseSVGPathGeometry, IVariableSvgPath
{
    /// <inheritdoc />
    public string? SVGPath
    {
        get => field;
        set
        {
            field = value;
            Path = value is null ? null : global::Noesis.Geometry.Parse(value);
        }
    }
}
