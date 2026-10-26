using LiveChartsCore.Noesis;
using LiveChartsCore.Noesis.Drawing.Geometries;
using LiveChartsCore.Defaults;
using ViewModelsSamples.Scatter.Custom;

namespace NoesisSample.Scatter.Custom;

// WPF xaml parser does not support generic types
// we instead create non-generic classes that inherit from the generic one.
public class HeartScatterSeries : XamlScatterSeries<ObservablePoint, HeartGeometry> { }
public class SvgScatterSeries : XamlScatterSeries<ObservablePoint, VariableSVGPathGeometry> { }
public class MyGeometryScatterSeries : XamlScatterSeries<ObservablePoint, MyGeometry> { }
