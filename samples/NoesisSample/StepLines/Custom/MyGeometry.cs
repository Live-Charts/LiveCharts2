using LiveChartsCore.Noesis;
using LiveChartsCore.Noesis.Drawing.Geometries;
using ViewModelsSamples.StepLines.Custom;

namespace NoesisSample.StepLines.Custom;

// WPF xaml parser does not support generic types
// we instead create non-generic classes that inherit from the generic one.
public class StarStepLineSeries : XamlStepLineSeries<double, StarGeometry> { }
public class SvgStepLineSeries : XamlStepLineSeries<double, VariableSVGPathGeometry> { }
public class MyGeometryStepLineSeries : XamlStepLineSeries<double, MyGeometry> { }
