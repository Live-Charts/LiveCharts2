using LiveChartsCore.Kernel;
using LiveChartsCore.Noesis;

namespace ViewModelsSamples;

public static partial class CustomLiveChartsExtensions
{
    public static LiveChartsSettings AddLiveChartsAppSettings(this LiveChartsSettings settings) =>
        settings.UseNoesis().HasMap<City>((city, index) => new(index, city.Population));
}
