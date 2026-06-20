using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LiveChartsCore; // mark
using ViewModelsSamples;

#if UI_TESTING
using Factos.Avalonia;
#endif

namespace AvaloniaSample;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        // LiveCharts configuration section: // mark
        LiveCharts.Configure(c => c // mark
            .AddLiveChartsAppSettings()); // mark
    }

    public override void OnFrameworkInitializationCompleted()
    {
#if UI_TESTING
        this.UseFactosApp();
#else
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView();
        }

        base.OnFrameworkInitializationCompleted();
#endif
    }
}
