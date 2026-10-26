using LiveChartsCore.Noesis.Drawing.Geometries;

namespace ViewModelsSamples.Pies.AngularGauge;

public class SmallNeedle : NeedleGeometry
{
    public SmallNeedle()
    {
        ScaleTransform = new(0.6f, 0.6f);
    }
}
