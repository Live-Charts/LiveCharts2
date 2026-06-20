using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace WinUISample;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Samples = ViewModelsSamples.Index.Samples;
        grid.DataContext = this;

        // Dev-loop hook: LVC_SAMPLE selects an initial sample by path
        // (e.g. LVC_SAMPLE=Bars/Basic). The view is resolved by type name, so
        // this works for any sample — including repro views that are
        // intentionally not listed in the shared index.
        var initial = Environment.GetEnvironmentVariable("LVC_SAMPLE");
        if (!string.IsNullOrWhiteSpace(initial)
            && Type.GetType($"WinUISample.{initial.Replace('/', '.')}.View") is { } viewType)
            content.Content = Activator.CreateInstance(viewType);

        // Dev-loop hook: when LVC_SCREENSHOT is set, render the window to PNG
        // shortly after activation and exit. Lets agents/scripts capture the
        // rendered sample without external screenshot tooling.
        var screenshotPath = Environment.GetEnvironmentVariable("LVC_SCREENSHOT");
        if (!string.IsNullOrWhiteSpace(screenshotPath))
            Activated += async (_, _) => await CaptureAndExitAsync(screenshotPath);
    }

    public string[] Samples { get; set; }

    private void Border_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if ((sender as FrameworkElement).DataContext is not string ctx) throw new Exception("Sample not found");

        content.Content = Activator.CreateInstance(null, $"WinUISample.{ctx.Replace('/', '.')}.View").Unwrap();
    }

    private bool _captured;
    private async Task CaptureAndExitAsync(string path)
    {
        // Activated fires multiple times; only run once.
        if (_captured) return;
        _captured = true;

        // Wait for the chart's measure + initial animation frames to settle.
        // See AvaloniaSample/MainWindow.axaml.cs for rationale (default 3s,
        // override via LVC_SCREENSHOT_DELAY_MS for slower hosts).
        await Task.Delay(GetDelayMs());

        try
        {
            var rtb = new RenderTargetBitmap();
            await rtb.RenderAsync(grid);
            var pixels = await rtb.GetPixelsAsync();

            var fullPath = Path.GetFullPath(path);
            var dir = Path.GetDirectoryName(fullPath)
                ?? throw new ArgumentException($"LVC_SCREENSHOT path has no parent directory: {path}", nameof(path));
            Directory.CreateDirectory(dir);

            var folder = await StorageFolder.GetFolderFromPathAsync(dir);
            var file = await folder.CreateFileAsync(Path.GetFileName(fullPath), CreationCollisionOption.ReplaceExisting);
            using var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Premultiplied,
                (uint)rtb.PixelWidth, (uint)rtb.PixelHeight,
                96, 96,
                pixels.ToArray());
            await encoder.FlushAsync();
            Console.WriteLine($"[LVC_SCREENSHOT] saved {rtb.PixelWidth}x{rtb.PixelHeight} to {fullPath}");
        }
        // Catch only the *expected* capture failure modes — file/path I/O,
        // permission, invalid arguments, encoder state, WinRT-side errors
        // — and let truly unexpected exceptions (OOM, chart-engine bugs,
        // etc.) propagate so CI sees a real stack trace.
        catch (Exception ex) when (
            ex is IOException                  // also covers DirectoryNotFound, PathTooLong, FileNotFound
            or UnauthorizedAccessException
            or ArgumentException               // also covers ArgumentNullException
            or NotSupportedException
            or InvalidOperationException
            or System.Runtime.InteropServices.COMException)
        {
            Console.Error.WriteLine($"[LVC_SCREENSHOT] capture failed: {ex.Message}");
            Environment.ExitCode = 1;
        }

        Close();
    }

    private static int GetDelayMs()
    {
        var raw = Environment.GetEnvironmentVariable("LVC_SCREENSHOT_DELAY_MS");
        return int.TryParse(raw, out var ms) && ms > 0 ? ms : 3000;
    }
}
