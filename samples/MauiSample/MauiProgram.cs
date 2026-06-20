using LiveChartsCore.SkiaSharpView.Maui;
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls.Hosting;
using LiveChartsCore;
using ViewModelsSamples;

#if UI_TESTING
using Factos.MAUI;
#endif

namespace MauiSample;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        _ = builder
            .UseSkiaSharp() // mark
            .UseLiveCharts() // mark
         // .UseLiveCharts(config => config  // LiveCharts configuration section // mark
         //     .AddLiveChartsAppSettings()) // if required, configure LiveCharts settings here // mark
            .UseMauiApp()
            .ConfigureFonts(fonts =>
            {
                _ = fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                _ = fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        return builder.Build();
    }

    public static MauiAppBuilder UseMauiApp(this MauiAppBuilder builder)
    {
        // Note: this sample is pulled from the main LiveCharts repository.
        // in the repo this sample is used to build UI tests. 

#if UI_TESTING
        builder.UseFactosApp();
#else
        builder.UseMauiApp<App>();
#endif

        return builder;
    }
}
