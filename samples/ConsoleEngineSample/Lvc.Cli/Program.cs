// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using LiveChartsCore;
using LiveChartsCore.Console;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Sketches;

// ----------------------------------------------------------------------------
// `lvc` — render LiveCharts to the terminal from a JSON chart spec. The intent
// is for AI-driven clients (Claude Code, MCP-aware agents, anything that can
// shell out) to ship terminal charts without integrating the .NET library
// directly. Read JSON on stdin or via --json, write ANSI to stdout.
//
//   echo '{"kind":"line","series":[{"values":[1,2,3,4,3,2,1]}]}' | lvc
//   lvc --json '{"kind":"pie","title":"Slices","series":[{"name":"A","values":[1]},{"name":"B","values":[3]}]}'
//   lvc --file chart.json --mode sixel
//
// Default mode is auto-detect (Sixel if the terminal advertises it via DA1,
// otherwise Braille). Override with --mode halfblock|braille|sixel.
// ----------------------------------------------------------------------------

if (args.Contains("--help") || args.Contains("-h"))
{
    PrintUsage();
    return 0;
}

if (args.Contains("--install-skill"))
{
    // Drop the embedded render-chart SKILL.md into the user's home so Claude Code (and any
    // other tool that reads ~/.claude/skills/) picks it up. One-time post-install step;
    // users on tool update -g re-run it to refresh.
    return InstallSkill();
}

if (args.Contains("--uninstall-skill"))
{
    // Inverse of --install-skill — remove the SKILL.md and the now-empty parent dir.
    // No-op (with a friendly note) if the skill wasn't installed.
    return UninstallSkill();
}

System.Console.OutputEncoding = Encoding.UTF8;

string? jsonInput = null;
ConsoleRenderMode? modeOverride = null;
int? widthOverride = null;
int? heightOverride = null;
var noColor = false;
var live = false;

for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--json" when i + 1 < args.Length:
            jsonInput = args[++i];
            break;
        case "--file" when i + 1 < args.Length:
            jsonInput = File.ReadAllText(args[++i]);
            break;
        case "--mode" when i + 1 < args.Length:
            modeOverride = args[++i].ToLowerInvariant() switch
            {
                "sixel" => ConsoleRenderMode.Sixel,
                "braille" => ConsoleRenderMode.Braille,
                "halfblock" => ConsoleRenderMode.HalfBlock,
                "auto" => null,
                _ => throw new ArgumentException($"Unknown --mode: {args[i]}"),
            };
            break;
        case "--width" when i + 1 < args.Length:
            widthOverride = int.Parse(args[++i]);
            break;
        case "--height" when i + 1 < args.Length:
            heightOverride = int.Parse(args[++i]);
            break;
        // Emit plain glyphs only — no SGR escapes, no cursor-home. Intended for AI tool
        // consumers (Claude Code, MCP agents) whose stdout is read as text, not painted to
        // a terminal. Honored by the standard NO_COLOR env var too.
        case "--no-color":
        case "--plain":
            noColor = true;
            break;
        // Run a live interactive chart loop instead of one-shot rendering. Drives the
        // engine's animations, accepts mouse hover/pan/wheel + q/r/+/- keys, exits on
        // Ctrl+C or q. Requires a real TTY (Sixel + cursor-home tricks); --no-color is
        // rejected because the loop relies on ANSI escapes to redraw frames in place.
        case "--live":
            live = true;
            break;
        default:
            // Unknown flag — silently ignore so additions don't break older callers.
            break;
    }
}

if (!noColor && Environment.GetEnvironmentVariable("NO_COLOR") is { Length: > 0 })
    noColor = true;

// stdin fallback for the JSON spec — natural for shell pipelines.
if (jsonInput is null && System.Console.IsInputRedirected)
    jsonInput = System.Console.In.ReadToEnd();

if (string.IsNullOrWhiteSpace(jsonInput))
{
    System.Console.Error.WriteLine("error: no chart spec provided. Pipe JSON on stdin, or use --json / --file.");
    PrintUsage();
    return 2;
}

ChartSpec spec;
try
{
    spec = JsonSerializer.Deserialize(jsonInput, ChartJsonContext.Default.ChartSpec)
        ?? throw new InvalidOperationException("spec deserialized to null");
}
catch (JsonException ex)
{
    System.Console.Error.WriteLine($"error: invalid JSON — {ex.Message}");
    return 2;
}

LiveCharts.Configure(c => c
    .AddConsole()
    .AddConsoleDefaultTheme()
    .AddDefaultMappers());

// Resolve render mode. Explicit > spec > auto-detect (DA1 → Sixel, else Braille).
// Auto-detect requires a real TTY; with output redirected we fall through to Braille
// (valid on any UTF-8 terminal that later cats the file). --no-color forces Braille
// regardless of detection — Sixel is inherently a binary escape sequence, no plain
// equivalent, and the AI-consumer scenario this flag exists for never wants the auto
// upgrade to Sixel even on a capable terminal.
var mode = modeOverride
    ?? (noColor
        ? ConsoleRenderMode.Braille
        : System.Console.IsOutputRedirected
            ? ConsoleRenderMode.Braille
            : ConsoleTerminal.TryDetectSixelSupport() ? ConsoleRenderMode.Sixel : ConsoleRenderMode.Braille);

if (noColor && mode == ConsoleRenderMode.Sixel)
{
    System.Console.Error.WriteLine("error: --no-color is incompatible with --mode sixel; Sixel has no plain-text form.");
    return 2;
}

if (live && noColor)
{
    // Live loop redraws in place with cursor-home + frame-by-frame ANSI; without color
    // there's nothing to keep the screen coherent. Either run one-shot --no-color, or
    // drop --no-color and use --live in a real terminal.
    System.Console.Error.WriteLine("error: --live is incompatible with --no-color; live mode needs ANSI escapes to redraw frames.");
    return 2;
}

if (live && System.Console.IsOutputRedirected)
{
    System.Console.Error.WriteLine("error: --live requires a TTY; stdout is redirected. Drop --live for one-shot rendering.");
    return 2;
}

// Cell-based size — fall back to terminal size, then 120x30 if the terminal isn't a TTY
// (output redirected, etc.). Spec width/height are in cells.
int cols, rows;
try
{
    cols = widthOverride ?? spec.Width ?? Math.Max(40, System.Console.WindowWidth - 1);
    rows = heightOverride ?? spec.Height ?? Math.Max(10, System.Console.WindowHeight - 2);
}
catch (System.IO.IOException)
{
    cols = widthOverride ?? spec.Width ?? 120;
    rows = heightOverride ?? spec.Height ?? 30;
}

InMemoryConsoleChart chart;
try { chart = BuildChart(spec, mode); }
catch (Exception ex)
{
    System.Console.Error.WriteLine($"error: {ex.Message}");
    return 2;
}

if (!string.IsNullOrEmpty(spec.Title)) chart.TitleText = spec.Title;
chart.ConfigureFromTerminalCells(cols, rows);

if (live)
{
    // Cartesian charts get pan/zoom enabled in live mode — RenderLoopAsync wires the
    // mouse-wheel + arrow keys into the engine's Pan/Zoom calls, but only if the view
    // declares it supports both axes. Pie/Polar don't get this since their zoom semantics
    // aren't meaningful.
    if (chart is CartesianChart cartesian)
        cartesian.ZoomMode = LiveChartsCore.Measure.ZoomAndPanMode.X | LiveChartsCore.Measure.ZoomAndPanMode.Y;

    using var cts = new CancellationTokenSource();
    System.Console.CancelKeyPress += (_, e) =>
    {
        if (cts.IsCancellationRequested) return;
        e.Cancel = true;
        cts.Cancel();
    };

    await chart.RenderLoopAsync(fps: 30, ct: cts.Token);
    return 0;
}

System.Console.Out.Write(chart.Render(home: false, color: !noColor));
return 0;

// ----------------------------------------------------------------------------

static InMemoryConsoleChart BuildChart(ChartSpec spec, ConsoleRenderMode mode) => spec.Kind?.ToLowerInvariant() switch
{
    "line"    => Cartesian(mode, spec, s =>
    {
        // Drop the series name at the end of each line so multi-line charts read in plain
        // mode without a separate legend lookup. Labels only fire for the last data point
        // (formatter returns "" everywhere else, which the engine skips); position Right so
        // the name sits past the rightmost point instead of overlapping it. Single-line
        // charts get a redundant label, but it's at the end so it doesn't clutter the curve.
        var lastIndex = (s.Values?.Length ?? 0) - 1;
        var line = new LineSeries<double>(s.Values ?? [])
        {
            Name = s.Name,
            GeometrySize = 0,
            LineSmoothness = 0.6,
            DataLabelsPaint = new LiveChartsCore.Console.Painting.SolidColorPaint(new LvcColor(255, 255, 255)),
            DataLabelsFormatter = p => p.Index == lastIndex ? p.Context.Series.Name ?? "" : "",
            DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Right,
        };
        // Pre-set Stroke with the user-requested thickness; theme rule preserves
        // StrokeThickness and overrides the color, so we use a placeholder LvcColor here.
        if (s.StrokeThickness is { } t)
            line.Stroke = new LiveChartsCore.Console.Painting.SolidColorPaint(default, (float)t);
        return line;
    }, legendOverride: LiveChartsCore.Measure.LegendPosition.Hidden),
    "column"  => Cartesian(mode, spec, s => new ColumnSeries<double>(s.Values ?? []) { Name = s.Name }),
    "row"     => Cartesian(mode, spec, s => new RowSeries<double>(s.Values ?? []) { Name = s.Name }),
    "step"    => Cartesian(mode, spec, s => new StepLineSeries<double>(s.Values ?? []) { Name = s.Name, GeometrySize = 0 }),
    "stackedcolumn" => Cartesian(mode, spec, s => new StackedColumnSeries<double>(s.Values ?? []) { Name = s.Name }),
    "stackedrow"    => Cartesian(mode, spec, s => new StackedRowSeries<double>(s.Values ?? []) { Name = s.Name }),
    "stackedarea"   => Cartesian(mode, spec, s => new StackedAreaSeries<double>(s.Values ?? []) { Name = s.Name }),
    "scatter" => Cartesian(mode, spec, s => new ScatterSeries<ObservablePoint>(
        (s.Points ?? []).Select(p => new ObservablePoint(p[0], p[1])).ToArray()) { Name = s.Name }),
    "candlestick" => Cartesian(mode, spec, s => new CandlestickSeries<FinancialPointI>(
        (s.Points ?? []).Select(p => new FinancialPointI(p[1], p[0], p[3], p[2])).ToArray()) { Name = s.Name }),
    "box" => Cartesian(mode, spec, s => new BoxSeries<BoxValue>(
        (s.Points ?? []).Select(p => new BoxValue(max: p[0], thirdQuartile: p[1], firstQuartile: p[2], min: p[4], median: p[3])).ToArray()) { Name = s.Name }),
    "pie" => new PieChart
    {
        RenderMode = mode,
        // Pushout creates visible gaps between wedges so they read as distinct shapes
        // even in plain mode (no color contrast to separate them). Outer-position
        // data labels then name each wedge from outside the disc, which makes the
        // legend redundant — hide it.
        Series = spec.Series.Select(s => (ISeries)new PieSeries<double>(s.Values?[0] ?? 0)
        {
            Name = s.Name,
            Pushout = 3,
            DataLabelsPaint = new LiveChartsCore.Console.Painting.SolidColorPaint(new LvcColor(255, 255, 255)),
            DataLabelsFormatter = p => p.Context.Series.Name ?? "",
            DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Outer,
        }).ToArray(),
        LegendPosition = LiveChartsCore.Measure.LegendPosition.Hidden,
    },
    "polar" => new PolarChart
    {
        RenderMode = mode,
        Series = spec.Series.Select(s => (ISeries)new PolarLineSeries<double>(s.Values ?? []) { Name = s.Name, GeometrySize = 0, LineSmoothness = 0.6 }).ToArray(),
        LegendPosition = LiveChartsCore.Measure.LegendPosition.Bottom,
    },
    null  => throw new ArgumentException("missing 'kind'"),
    var k => throw new ArgumentException($"unknown 'kind': {k}"),
};

static InMemoryConsoleChart Cartesian(ConsoleRenderMode mode, ChartSpec spec, Func<SeriesSpec, ISeries> factory,
    LiveChartsCore.Measure.LegendPosition? legendOverride = null) =>
    new CartesianChart
    {
        RenderMode = mode,
        Series = spec.Series.Select(factory).ToArray(),
        XAxes = spec.XAxis is { } xa ? [new Axis { Name = xa.Name, Labels = xa.Labels }] : null!,
        YAxes = spec.YAxis is { } ya ? [new Axis { Name = ya.Name, Labels = ya.Labels }] : null!,
        // Line series self-label at the end of each curve so the legend is redundant;
        // pass Hidden as override there. Default: legend at bottom when there are 2+
        // series to map back to.
        LegendPosition = legendOverride
            ?? (spec.Series.Length > 1 ? LiveChartsCore.Measure.LegendPosition.Bottom : LiveChartsCore.Measure.LegendPosition.Hidden),
    };

static int InstallSkill()
{
    const string resourceName = "lvc.render-chart.SKILL.md";

    var asm = System.Reflection.Assembly.GetExecutingAssembly();
    using var stream = asm.GetManifestResourceStream(resourceName);
    if (stream is null)
    {
        System.Console.Error.WriteLine($"error: embedded skill resource '{resourceName}' missing from this build.");
        return 2;
    }

    var (targetDir, targetPath) = SkillInstallPath();

    try
    {
        Directory.CreateDirectory(targetDir);
        using var fs = File.Create(targetPath);
        stream.CopyTo(fs);
    }
    catch (Exception ex)
    {
        System.Console.Error.WriteLine($"error: failed to write skill to {targetPath} -- {ex.Message}");
        return 2;
    }

    System.Console.Out.WriteLine($"Installed render-chart skill -> {targetPath}");
    return 0;
}

static int UninstallSkill()
{
    var (targetDir, targetPath) = SkillInstallPath();

    if (!File.Exists(targetPath))
    {
        System.Console.Out.WriteLine($"No render-chart skill at {targetPath} -- nothing to remove.");
        return 0;
    }

    try
    {
        File.Delete(targetPath);
        // Best-effort dir cleanup. Only remove if empty so we don't wipe user-authored
        // files that happen to share the directory.
        if (Directory.Exists(targetDir) && Directory.GetFileSystemEntries(targetDir).Length == 0)
            Directory.Delete(targetDir);
    }
    catch (Exception ex)
    {
        System.Console.Error.WriteLine($"error: failed to remove skill at {targetPath} -- {ex.Message}");
        return 2;
    }

    System.Console.Out.WriteLine($"Removed render-chart skill <- {targetPath}");
    return 0;
}

static (string targetDir, string targetPath) SkillInstallPath()
{
    // Honor $CLAUDE_HOME if set, otherwise default to ~/.claude. Lets a user with a custom
    // Claude config root point at it without symlinking.
    var claudeHome = Environment.GetEnvironmentVariable("CLAUDE_HOME")
        ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".claude");
    var dir = Path.Combine(claudeHome, "skills", "render-chart");
    return (dir, Path.Combine(dir, "SKILL.md"));
}

static void PrintUsage() => System.Console.Error.WriteLine("""
    usage: lvc [--mode auto|sixel|braille|halfblock] [--width N] [--height N]
               [--no-color] [--live] [--json '<spec>' | --file path]
           lvc --install-skill | --uninstall-skill

    JSON spec:
      {
        "kind": "line",                       // line | column | row | step | scatter | stackedcolumn | stackedrow | stackedarea | candlestick | box | pie | polar
        "title": "optional",
        "width": 80, "height": 20,            // cells; defaults to terminal size
        "series": [
          {
            "name": "Signal",
            "values": [1, 2, 3, ...],         // line/column/row/step/stacked/pie/polar
            "points": [[x, y], ...]           // scatter / candlestick (open,high,close,low) / box (max,q3,q1,median,min)
          }
        ],
        "xAxis": { "name": "X", "labels": ["a","b","c"] },
        "yAxis": { "name": "Y" }
      }

    Reads from stdin if --json/--file not given:
      echo '{"kind":"line","series":[{"values":[1,2,3,4,3,2,1]}]}' | lvc
    """);

// ----------------------------------------------------------------------------
// JSON model. Records keep the schema legible; STJ source-gen avoids reflection
// at startup so the CLI launches faster.
// ----------------------------------------------------------------------------

internal record ChartSpec(
    string? Kind,
    string? Title,
    int? Width,
    int? Height,
    SeriesSpec[] Series,
    AxisSpec? XAxis,
    AxisSpec? YAxis);

internal record SeriesSpec(
    string? Name,
    double[]? Values,
    double[][]? Points,
    double? StrokeThickness);

internal record AxisSpec(
    string? Name,
    string[]? Labels);

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(ChartSpec))]
internal partial class ChartJsonContext : JsonSerializerContext { }
