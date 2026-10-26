using LiveChartsCore.Noesis;
using ViewModelsSamples.General.UserDefinedTypes;

namespace NoesisSample.General.UserDefinedTypes;

// WPF xaml parser does not support generic types
// we instead create a non-generic class that inherits from the generic one.
public class CustomCityLineSeries : XamlLineSeries<City> { }
