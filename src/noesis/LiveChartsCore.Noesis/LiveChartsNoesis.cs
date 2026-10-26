using LiveChartsCore.Kernel;

namespace LiveChartsCore.Noesis;

/// <summary>Configures the Noesis chart backend.</summary>
public static class LiveChartsNoesis
{
    private static bool _initialized;

    /// <summary>Gets or sets right-to-left chart text layout.</summary>
    public static bool IsRightToLeft { get; set; }

    internal static void EnsureInitialized()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        LiveCharts.Configure(settings => settings.UseNoesis());
    }

    /// <summary>Registers native geometry, paints, and chart defaults.</summary>
    public static LiveChartsSettings UseNoesis(this LiveChartsSettings settings)
    {
        settings.HasProvider(new NoesisProvider());
        settings.AddDefaultMappers();
        settings.AddDefaultTheme();

        return settings;
    }
}
