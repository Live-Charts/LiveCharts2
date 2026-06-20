using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;

namespace CoreTests.MockedObjects;

public class TestLabel : LabelGeometry
{
    public TestLabel()
    {
        Background = new LvcColor(200, 200, 200, 50);
    }
}
