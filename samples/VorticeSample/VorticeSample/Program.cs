using System.Collections.ObjectModel;
using LiveChartsCore;
using LiveChartsCore.Vortice;
using VorticeSample;

// ------------------------------------------------------------------------------
// THIS IS A PROOF OF CONCEPT DEMO
// IT DRAWS A BASIC CARTESIAN CHART USING VORTICE DIRECT2D AS RENDERER
// NO DEPENDENCY ON SKIASHARP AT ALL.
// ------------------------------------------------------------------------------

LiveCharts.Configure(config => config
    .AddDefaultMappers()
    .AddVortice()
    .AddVorticeDefaultTheme()
    .HasRenderingSettings(renderSettings =>
    {
        renderSettings.ShowFPS = true;
    }));

using TestApplication app = new()
{
    PresentOptions = Vortice.Direct2D1.PresentOptions.Immediately
    //PresentOptions = Vortice.Direct2D1.PresentOptions.None
};

var data = new ObservableCollection<double> { 3, 2, 5, 3 };

var chart = new CartesianChart
{
    Series = [new ColumnSeries<double>(data)]
};

async Task randomize()
{
    var r = new Random();

    while (true)
    {
        await Task.Delay(2000);
        for (var i = 0; i < data.Count; i++)
            data[i] = r.NextDouble() * 10;
    }
}

_ = randomize();

app.AddControl(chart);

app.Run();
