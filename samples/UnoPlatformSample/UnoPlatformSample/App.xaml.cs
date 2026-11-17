using LiveChartsCore;
using Microsoft.UI.Xaml;
using ViewModelsSamples;
#if UI_TESTING
using Factos.Uno;
using Microsoft.Extensions.Hosting;
#endif

namespace UnoPlatformSample;

public partial class App : Application
{
    public App()
    {
        this.InitializeComponent();
    }

    protected Window? MainWindow { get; private set; }

#if UI_TESTING
    protected IHost? Host { get; private set; }

    // Factos.Uno drives the UI tests through the Uno.Extensions hosting + navigation builder:
    // UseFactosApp() registers its FactosShell at the root route, which navigation then mounts
    // into this app's Shell. This host is built ONLY for UI-test builds (UITesting=true); the
    // lean, AOT-publishable WASM showcase uses the minimal MainPage path below and never pulls
    // in Hosting/Navigation. See UnoPlatformSample.csproj.
    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Recommended place to configure LiveCharts.
        LiveCharts.Configure(c => c.AddLiveChartsAppSettings());

        var builder = this.CreateBuilder(args)
            .Configure(host => host.UseFactosApp());

        MainWindow = builder.Window;

        Host = await builder.NavigateAsync<Shell>();
    }
#else
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Recommended place to configure LiveCharts.
        LiveCharts.Configure(c => c.AddLiveChartsAppSettings());

        MainWindow = new Window();
        if (MainWindow.Content is not MainPage)
            MainWindow.Content = new MainPage();

        MainWindow.Activate();
    }
#endif
}
