using System;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace UnoPlatformSample;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
        Samples = ViewModelsSamples.Index.Samples;
        grid.DataContext = this;

        // Show a chart on first load instead of a blank pane.
        if (Samples.Length > 0)
            content.Content = LoadSample(Samples[0]);
    }

    public string[] Samples { get; }

    private void Border_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is string ctx)
            content.Content = LoadSample(ctx);
    }

    // Sample views are linked in from WinUISample (namespace "WinUISample.<group>.<name>.View").
    // Resolve the type explicitly in THIS assembly: Activator.CreateInstance(null, ...) relies on
    // "calling assembly" resolution that is unreliable under WASM AOT (loads fine interpreted, but
    // returns nothing AOT-compiled, leaving a blank pane).
    private static object? LoadSample(string ctx)
    {
        var typeName = $"WinUISample.{ctx.Replace('/', '.')}.View";
        try
        {
            var type = typeof(MainPage).Assembly.GetType(typeName, throwOnError: false);
            return type is null ? null : Activator.CreateInstance(type);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[gallery] failed to load {typeName}: {ex.Message}");
            return null;
        }
    }
}
