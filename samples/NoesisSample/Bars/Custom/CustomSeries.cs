using LiveChartsCore.Noesis;
using LiveChartsCore.Noesis.Drawing.Geometries;
using ViewModelsSamples.Bars.Custom;

namespace NoesisSample.Bars.Custom;

// WPF xaml parser does not support generic types
// we instead create a non-generic class that inherits from the generic one.
public class DiamondColumnSeries : XamlColumnSeries<double, DiamondGeometry> { }
public class VariableSVGPathColumnSeries : XamlColumnSeries<double, VariableSVGPathGeometry> { }
public class MyGeometryColumnSeries : XamlColumnSeries<double, MyGeometry> { }
