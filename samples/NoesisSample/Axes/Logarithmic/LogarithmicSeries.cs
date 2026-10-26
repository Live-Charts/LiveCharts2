using LiveChartsCore.Noesis;
using ViewModelsSamples.Axes.Logarithmic;

namespace NoesisSample.Axes.Logarithmic;

// WPF xaml parser does not support generic types
// we instead create a non-generic class that inherits from the generic one.
public class LogarithmicSeries : XamlLineSeries<LogarithmicPoint> { }
