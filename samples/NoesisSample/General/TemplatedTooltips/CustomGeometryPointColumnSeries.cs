using LiveChartsCore.Noesis;
using ViewModelsSamples.General.TemplatedTooltips;

namespace NoesisSample.General.TemplatedTooltips;

// WPF xaml parser does not support generic types
// we instead create a non-generic class that inherits from the generic one.
public class CustomGeometryPointColumnSeries : XamlColumnSeries<GeometryPoint> { }
