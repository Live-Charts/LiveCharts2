using Android.App;
using Android.Content.PM;

using Avalonia.Android;

namespace AvaloniaSample.Android;

[Activity(
    Label = "AvaloniaApplication1.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
}
