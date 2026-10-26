using LiveChartsCore.Noesis;
using LiveChartsCore.Noesis.Drawing.Geometries;

namespace NoesisSample.Lines.Basic;

// WPF xaml parser does not support generic types
// we instead create a non-generic class that inherits from the generic one.
public class CustomStarLineSeries : XamlLineSeries<int, StarGeometry> { }
