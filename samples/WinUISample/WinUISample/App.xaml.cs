using System;
using LiveChartsCore; // mark
using ViewModelsSamples;
using Microsoft.UI.Xaml;

namespace WinUISample;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // LiveCharts configuration section: // mark
        LiveCharts.Configure(c => c // mark
            .AddLiveChartsAppSettings()); // mark

        m_window = new MainWindow();
        m_window.Activate();

#if UI_TESTING
        Factos.WinUI.SetupExtensions.UseFactosApp(this);
#endif
    }

    private Window m_window;
}
