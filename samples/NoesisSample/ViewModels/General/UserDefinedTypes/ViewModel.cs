using System;
using LiveChartsCore;
using LiveChartsCore.Kernel;

namespace ViewModelsSamples.General.UserDefinedTypes;

public class ViewModel
{
    public ViewModel()
    {
        LiveCharts.Configure(config =>
        {
            // register the custom type
            // this is a global registration, so all charts will use this mapping
            // in the mapper we use the Population property as the Y coordinate
            // and the index of the city in our collection as the X coordinate
            config.HasMap<City>((city, index) =>
                new(index, city.Population));
        });
    }

    public City[] Values { get; set; } = [
        new City { Name = "Tokyo", Population = 4 },
        new City { Name = "New York", Population = 6 },
        new City { Name = "Seoul", Population = 2 },
        new City { Name = "Moscow", Population = 8 },
        new City { Name = "Shanghai", Population = 3 },
        new City { Name = "Guadalajara", Population = 4 }
    ];

    public Func<ChartPoint, string> TooltipFormatter { get; set; } = point =>
    {
        var city = (City)point.Context.DataSource!;

        return $"{city.Population}M people in {city.Name}";
    };
}
