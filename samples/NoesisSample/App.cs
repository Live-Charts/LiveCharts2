using System.Diagnostics;
using Noesis;
using NoesisApp;

namespace NoesisSample;

public sealed class App : Application
{
    private static readonly string LogPath = System.IO.Path.Combine(AppContext.BaseDirectory, "noesis-sample.log");
    private readonly bool _smokeTest;
    private readonly bool _smokeAll;
    private readonly bool _stressTest;
    private readonly int _framesPerSample;
    private readonly List<double> _frameTimes = [];
    private long _lastFrame;
    private readonly bool _darkTheme;
    private readonly bool _capture;
    private readonly List<string> _errors = [];
    private string[] _smokeSamples = [];
    private int _sampleIndex;
    private int _frames;

    private App(string[] args)
    {
        _stressTest = args.Contains("--stress-test");
        _framesPerSample = _stressTest ? 600 : 60;
        _smokeAll = args.Contains("--smoke-all");
        _smokeTest = _stressTest || _smokeAll || args.Contains("--smoke-test");
        _darkTheme = args.Contains("--dark");
        _capture = args.Contains("--capture");
    }

    [STAThread]
    public static int Main(string[] args)
    {
        File.WriteAllText(LogPath, $"Started {DateTime.Now:O}{Environment.NewLine}");

        GUI.DisableInspector();
        GUI.DisableHotReload();
        GUI.DisableSocketInit();

        var app = new App(args)
        {
            Uri = "/NoesisSample;component/App.xaml"
        };

        var exitCode = app.Run();

        return app._errors.Count == 0 ? exitCode : 1;
    }

    protected override bool RunInBackground => _smokeTest;

    protected override Display CreateDisplay() => new Win32Display();

    protected override RenderContext CreateRenderContext() => new RenderContextD3D11();

    protected override void OnStartup(StartupEventArgs args)
    {
        base.OnStartup(args);

        var window = (MainWindow)MainWindow;

        window.ApplyTheme(_darkTheme ? "Dark" : "Light");

        if (!_smokeTest)
        {
            return;
        }

        var requestedSample = Environment.GetEnvironmentVariable("LVC_SAMPLE");

        _smokeSamples = !string.IsNullOrWhiteSpace(requestedSample)
            ? [requestedSample]
            : _stressTest
            ? ["General/MultiThreading", "General/MultiThreading2"]
            : _smokeAll
            ? window.SamplePaths
            : ["General/FirstChart", "Lines/Basic", "Lines/AutoUpdate", "Bars/Basic", "Pies/Basic", "Polar/Basic"];

        window.SelectExample(Array.IndexOf(window.SamplePaths, _smokeSamples[0]));

        MainWindow.View.Rendering += OnSmokeFrame;
        MainWindow.Rendering += (_, _) => OnCaptureFrame();
    }

    protected override void OnUnhandledException(Exception exception)
    {
        _errors.Add(exception.ToString());
        File.AppendAllText(LogPath, $"{DateTime.Now:O} {exception}{Environment.NewLine}");

        Console.Error.WriteLine(exception);
    }

    protected override void OnLog(LogLevel level, string channel, string message)
    {
        Console.WriteLine($"[NOESIS/{level}] {message}");
        File.AppendAllText(LogPath, $"{DateTime.Now:O} [{level}] {message}{Environment.NewLine}");

        if (level == LogLevel.Error)
        {
            _errors.Add(message);
        }
    }

    public void ExportChart(FrameworkElement chart, string fileName)
    {
        var remainingFrames = 30;

        MainWindow.Rendering += CaptureChart;

        void CaptureChart(object sender, WindowRenderingEventArgs args)
        {
            remainingFrames--;

            if (remainingFrames > 0)
            {
                return;
            }

            MainWindow.Rendering -= CaptureChart;

            if (!chart.IsLoaded)
            {
                return;
            }

            var transform = chart.TransformToAncestor((Visual)MainWindow.Content);
            var bounds = transform.TransformBounds(new Rect(0, 0, chart.ActualWidth, chart.ActualHeight));

            Directory.CreateDirectory("captures");
            CaptureFrame(System.IO.Path.Combine("captures", fileName), bounds);
        }
    }

    private void CaptureFrame(string path, Rect? bounds = null)
    {
        var context = MainWindow.RenderContext;
        var width = MainWindow.Display.ClientWidth;
        var height = MainWindow.Display.ClientHeight;
        if (width <= 0 || height <= 0)
        {
            Console.WriteLine("Capture skipped: the sample window is minimized.");

            return;
        }

        var target = context.Device.CreateRenderTarget("Noesis chart capture", (uint)width, (uint)height, 1, true);

        try
        {
            var tile = new Tile { X = 0, Y = 0, Width = (uint)width, Height = (uint)height };

            context.Device.SetRenderTarget(target);
            context.Device.BeginTile(target, tile);
            MainWindow.View.Renderer.Render(clear: true);
            context.Device.EndTile(target);
            context.Device.ResolveRenderTarget(target, new[] { tile });

            var capture = context.CaptureRenderTarget(target);

            var left = bounds is null ? 0 : Math.Max(0, (int)bounds.Value.X);
            var top = bounds is null ? 0 : Math.Max(0, (int)bounds.Value.Y);
            var imageWidth = bounds is null ? (int)capture.Width : Math.Min((int)bounds.Value.Width, (int)capture.Width - left);
            var imageHeight = bounds is null ? (int)capture.Height : Math.Min((int)bounds.Value.Height, (int)capture.Height - top);
            var pixels = new byte[imageWidth * imageHeight * 4];

            for (var row = 0; row < imageHeight; row++)
            {
                Buffer.BlockCopy(capture.Pixels, ((top + row) * (int)capture.Width + left) * 4, pixels, row * imageWidth * 4, imageWidth * 4);
            }

            using var stream = File.Create(path);
            using var writer = new BinaryWriter(stream);

            writer.Write((ushort)0x4D42);
            writer.Write(54 + pixels.Length);
            writer.Write(0);
            writer.Write(54);
            writer.Write(40);
            writer.Write(imageWidth);
            writer.Write(-imageHeight);
            writer.Write((ushort)1);
            writer.Write((ushort)32);
            writer.Write(0);
            writer.Write(pixels.Length);
            writer.Write(0);
            writer.Write(0);
            writer.Write(0);
            writer.Write(0);
            writer.Write(pixels);
        }
        finally
        {
            context.SetDefaultRenderTarget(width, height, false);
            target.Dispose();
        }
    }

    private void OnCaptureFrame()
    {
        if (!_capture || _frames % _framesPerSample != _framesPerSample - 1)
        {
            return;
        }

        var sample = _smokeSamples[_sampleIndex].Replace('/', '-');

        Directory.CreateDirectory("captures");
        CaptureFrame(System.IO.Path.Combine("captures", $"{sample}-{(_darkTheme ? "dark" : "light")}.bmp"));
    }

    private void OnSmokeFrame(object sender, Noesis.EventArgs args)
    {
        var now = Stopwatch.GetTimestamp();

        if (_stressTest && _lastFrame != 0 && _frames % _framesPerSample > 120)
        {
            _frameTimes.Add((now - _lastFrame) * 1000d / Stopwatch.Frequency);
        }

        _lastFrame = now;
        _frames++;

        if (_frames % _framesPerSample != 0)
        {
            return;
        }

        Dispatcher.BeginInvoke(AdvanceSample);
    }

    private void AdvanceSample()
    {
        var window = (MainWindow)MainWindow;

        try
        {
            window.VerifyCurrentExample();
        }
        catch (Exception exception)
        {
            OnUnhandledException(exception);
        }

        if (_stressTest && _frameTimes.Count > 0)
        {
            _frameTimes.Sort();

            var percentile = _frameTimes[(int)(_frameTimes.Count * 0.95)];

            Console.WriteLine($"Frame timing {window.CurrentSample}: mean {_frameTimes.Average():F2} ms, p95 {percentile:F2} ms, {_frameTimes.Count} frames.");
            _frameTimes.Clear();
        }

        _sampleIndex++;

        if (_sampleIndex < _smokeSamples.Length)
        {
            window.SelectExample(Array.IndexOf(window.SamplePaths, _smokeSamples[_sampleIndex]));

            return;
        }

        MainWindow.View.Rendering -= OnSmokeFrame;

        Console.WriteLine($"Noesis gallery smoke test: {_smokeSamples.Length} samples, {_errors.Count} errors.");

        MainWindow.Close();
    }
}
