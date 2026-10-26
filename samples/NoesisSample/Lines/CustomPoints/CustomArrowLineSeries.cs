using LiveChartsCore.Noesis;
using ViewModelsSamples.Lines.CustomPoints;

namespace NoesisSample.Lines.CustomPoints;

// WPF xaml parser does not support generic types
// we instead create a non-generic class that inherits from the generic one.
public class CustomArrowLineSeries : XamlLineSeries<DataPoint, ArrowGeometry> { }
