using System;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Defaults;
using LiveChartsCore.Noesis;
using LiveChartsCore.Noesis.Painting;
using LiveChartsCore.Themes;
using LiveChartsCore.Kernel;
using CommunityToolkit.Mvvm.Input;

namespace ViewModelsSamples.Bars.Race;

// this class is used to store the data for each pilot // mark
// it inherits from ObservableValue, this class implements // mark
// IChartEntity, so it can be used as a data source, it draws the // mark
// Value property as a bar in the chart. // mark
public class PilotInfo : ObservableValue
{
    public PilotInfo(string name, int value, SolidColorPaint paint)
    {
        Name = name;
        Paint = paint;
        Value = value;
    }

    public string Name { get; set; }
    public SolidColorPaint Paint { get; set; }
}

public partial class ViewModel : ObservableObject
{
    private readonly Random _r = new();

    public ViewModel()
    {
        _ = StartRace();
    }

    [ObservableProperty]
    public partial PilotInfo[] Data { get; set; } =
        Fetch();

    public bool IsReading { get; set; } =
        true;

    public Func<ChartPoint, string> LabelsFormatter { get; set; } =
        GetPilotName;

    [RelayCommand]
    public void OnPointMeasured(ChartPoint point)
    {
        // assign the pilot color to the bar

        var pilot = (PilotInfo?)point.Context.DataSource;
        if (point.Context.Visual is null || pilot is null) return;

        point.Context.Visual.Fill = pilot.Paint;
    }

    private static string GetPilotName(ChartPoint point)
    {
        var pilot = (PilotInfo?)point.Context.DataSource;
        return pilot is null ? string.Empty : pilot.Name;
    }

    private async Task StartRace()
    {
        await Task.Delay(1000);

        // to keep this sample simple, we run the next infinite loop
        // in a real application you should stop the loop/task when the view is disposed

        while (IsReading)
        {
            // do a random change to the data
            foreach (var item in Data)
                item.Value += _r.Next(0, 100);

            Data = [.. Data.OrderBy(x => x.Value)];

            await Task.Delay(100);
        }
    }

    private static PilotInfo[] Fetch()
    {
        // generate a different color for each pilot
        var paints = Enumerable.Range(0, 7)
            .Select(i => new SolidColorPaint(ColorPalletes.MaterialDesign500[i]))
            .ToArray();

        return [
            new("Tsunoda",   500,  paints[0]),
            new("Sainz",     450,  paints[1]),
            new("Riccardo",  520,  paints[2]),
            new("Bottas",    550,  paints[3]),
            new("Perez",     660,  paints[4]),
            new("Verstapen", 920,  paints[5]),
            new("Hamilton",  1000, paints[6])
        ];
    }
}
