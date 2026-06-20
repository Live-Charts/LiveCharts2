// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using LiveChartsCore;
using LiveChartsCore.Console;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;

// ----------------------------------------------------------------------------
// Live system-metrics dashboard. Renders a 2x2 grid showing CPU usage, total
// memory footprint, top-N processes by memory, and live process count — all
// driven by Process.GetProcesses() sampled once per second. Cross-platform
// (Process API works on Windows / Linux / macOS); no PerformanceCounter or
// /proc parsing required.
//
// Pick render mode with --halfblock / --braille / --sixel — default is auto-
// detect (Sixel if the terminal advertises it via DA1, otherwise Braille).
// Mouse hover shows tooltips per point. Pan / zoom are intentionally disabled
// here since the data continuously scrolls.
// ----------------------------------------------------------------------------

System.Console.OutputEncoding = Encoding.UTF8;

LiveCharts.Configure(c => c
    .AddConsole()
    .AddConsoleDefaultTheme()
    .AddDefaultMappers());

var mode = args.Contains("--sixel") ? ConsoleRenderMode.Sixel
         : args.Contains("--braille") ? ConsoleRenderMode.Braille
         : args.Contains("--halfblock") ? ConsoleRenderMode.HalfBlock
         : ConsoleTerminal.TryDetectSixelSupport() ? ConsoleRenderMode.Sixel
         : ConsoleRenderMode.Braille;

int? sixelCw = ParseIntFlag(args, "--sixel-cw");
int? sixelCh = ParseIntFlag(args, "--sixel-ch");

int cols, rows;
try
{
    cols = Math.Max(40, System.Console.WindowWidth - 1);
    rows = Math.Max(10, System.Console.WindowHeight - 2);
}
catch (System.IO.IOException) { cols = 120; rows = 30; }

const int HistoryLen = 60;   // 1 sample/sec → 1 minute of history
const int TopN = 10;
const int SampleIntervalMs = 1000;

// Time-series collections start empty and fill as samples roll in. Each tick we Add a new
// point at the end and RemoveAt(0) once we hit HistoryLen — the chart engine sees the
// surviving points' X indices decrement and animates them sliding left (true scroll), and
// the new point fades in at the right. Shifting in place (mutating .Value at fixed indices)
// would keep X stable but bounce Y between neighbors, which read as a vertical jitter.
//
// The top-N chart is different: the columns are categorical (top by mem) and the entity at
// each rank changes on every sample, so we keep that one as fixed-size with in-place
// mutation. Slot 0 always represents "current #1 by memory", regardless of which process.
var cpuData = new ObservableCollection<ObservableValue>();
var memData = new ObservableCollection<ObservableValue>();
var procCountData = new ObservableCollection<ObservableValue>();
var topProcMemData = new ObservableCollection<ObservableValue>(
    Enumerable.Range(0, TopN).Select(_ => new ObservableValue(0)));
var topProcNames = new List<string>(Enumerable.Repeat("—", TopN));

var halfCols = Math.Max(20, (cols - 2) / 2);
var halfRows = Math.Max(6, (rows - 2) / 2);

var cpuChart = MakeLineChart("CPU %", cpuData);
var memChart = MakeLineChart("Memory MB", memData);
var procChart = MakeLineChart("Processes", procCountData);
var topChart = (CartesianChart)Configure(new CartesianChart
{
    RenderMode = mode,
    Series = [
        new ColumnSeries<ObservableValue>(topProcMemData) { Name = "Top by mem (MB)" }
    ],
    XAxes = [new Axis { Labels = topProcNames }],
});

// Titles render as a centered strip at the top of each surface — handy on a 2x2 grid
// where the legend strip alone (one short series name per panel) doesn't immediately tell
// you which chart you're looking at without reading values off the axes.
cpuChart.TitleText = "CPU %";
memChart.TitleText = "Memory MB";
procChart.TitleText = "Processes";
topChart.TitleText = "Top by memory (MB)";

var charts = new[] { cpuChart, memChart, procChart, topChart };

CartesianChart MakeLineChart(string name, ObservableCollection<ObservableValue> data) =>
    (CartesianChart)Configure(new CartesianChart
    {
        RenderMode = mode,
        Series = [
            new LineSeries<ObservableValue>(data) { Name = name, GeometrySize = 0, LineSmoothness = 0.3 }
        ],
    });

InMemoryConsoleChart Configure(InMemoryConsoleChart c)
{
    if (sixelCw.HasValue) c.SixelCellWidth = sixelCw.Value;
    if (sixelCh.HasValue) c.SixelCellHeight = sixelCh.Value;
    c.ConfigureFromTerminalCells(halfCols, halfRows);
    return c;
}

using var cts = new CancellationTokenSource();
System.Console.CancelKeyPress += (_, e) =>
{
    if (cts.IsCancellationRequested) return;
    e.Cancel = true;
    cts.Cancel();
};

// Sampler — every SampleIntervalMs, walk all processes, aggregate CPU time / RSS / count
// and update the four chart series in place. CPU% is computed from the delta of summed
// TotalProcessorTime against wall-clock time, divided by core count to land in 0..100%.
var prevCpuTime = TimeSpan.Zero;
var prevTimestamp = DateTime.MinValue;

_ = Task.Run(async () =>
{
    while (!cts.IsCancellationRequested)
    {
        try { await Task.Delay(SampleIntervalMs, cts.Token); }
        catch (OperationCanceledException) { return; }

        // Lock all chart sync roots before mutating shared data — the per-chart Measure
        // reads these collections from the render thread, and we don't want a partial
        // write to surface as a NaN or shifted-but-not-replaced sample.
        foreach (var c in charts) Monitor.Enter(c.SyncRoot);
        try { SampleMetrics(); }
        finally { foreach (var c in charts) Monitor.Exit(c.SyncRoot); }
    }
});

// Run terminal detections before mouse capture (mouse reader steals raw stdin and would
// swallow OSC 11 / CSI 16t responses). One pass per chart — they cache independently.
foreach (var c in charts) c.EnsureTerminalDetections();

// Mouse routing — pick the quadrant the cursor is in and dispatch the local-coords event
// to that chart. Pan / zoom are no-ops here since none of the charts opted into ZoomMode.
var mouse = new ConsoleMouse((mcol, mrow, action) =>
{
    var col1 = mcol + 1;
    var row1 = mrow + 1;
    var (chart, qRow, qCol) = row1 <= halfRows
        ? (col1 <= halfCols ? (cpuChart, 1, 1) : ((InMemoryConsoleChart)memChart, 1, halfCols + 2))
        : (col1 <= halfCols ? ((InMemoryConsoleChart)procChart, halfRows + 2, 1)
                            : ((InMemoryConsoleChart)topChart, halfRows + 2, halfCols + 2));

    var localCol = col1 - qCol;
    var localRow = row1 - qRow;
    switch (action)
    {
        case MouseAction.Move:    chart.SimulatePointerMoveOrLeaveAtCell(localCol, localRow); break;
        case MouseAction.Press:   chart.SimulatePointerDownAtCell(localCol, localRow); break;
        case MouseAction.Release: chart.SimulatePointerUpAtCell(localCol, localRow); break;
    }
});
mouse.Start(System.Console.Out);

await System.Console.Out.WriteAsync("\x1b[0m\x1b[?25l\x1b[2J");
await System.Console.Out.FlushAsync();

var period = TimeSpan.FromMilliseconds(1000.0 / 30);
var lastCols = cols;
var lastRows = rows;

try
{
    var sb = new StringBuilder();
    while (!cts.IsCancellationRequested)
    {
        // Resize handling: re-read window, reconfigure each chart, clear screen so the old
        // (larger) frame's pixels don't bleed through.
        var resized = false;
        if (!System.Console.IsOutputRedirected)
        {
            int newCols, newRows;
            try
            {
                newCols = Math.Max(40, System.Console.WindowWidth - 1);
                newRows = Math.Max(10, System.Console.WindowHeight - 2);
            }
            catch (System.IO.IOException) { newCols = lastCols; newRows = lastRows; }

            if (newCols != lastCols || newRows != lastRows)
            {
                lastCols = newCols;
                lastRows = newRows;
                halfCols = Math.Max(20, (newCols - 2) / 2);
                halfRows = Math.Max(6, (newRows - 2) / 2);
                foreach (var c in charts) c.ConfigureFromTerminalCells(halfCols, halfRows);
                resized = true;
            }
        }

        sb.Clear();
        if (resized) _ = sb.Append("\x1b[0m\x1b[2J");

        EmitChartAt(sb, cpuChart,  1,             1);
        EmitChartAt(sb, memChart,  1,             halfCols + 2);
        EmitChartAt(sb, procChart, halfRows + 2,  1);
        EmitChartAt(sb, topChart,  halfRows + 2,  halfCols + 2);

        await System.Console.Out.WriteAsync(sb);
        await System.Console.Out.FlushAsync(cts.Token);

        try { await Task.Delay(period, cts.Token); }
        catch (OperationCanceledException) { break; }
    }
}
finally
{
    mouse.Stop();
    await System.Console.Out.WriteAsync("\x1b[0m\x1b[?25h\n");
    await System.Console.Out.FlushAsync(CancellationToken.None);
}
return;

static void EmitChartAt(StringBuilder sb, InMemoryConsoleChart chart, int row, int col)
{
    var rendered = chart.RenderFrame(home: false);
    if (chart.RenderMode == ConsoleRenderMode.Sixel)
    {
        _ = sb.Append("\x1b[").Append(row).Append(';').Append(col).Append('H').Append(rendered);
    }
    else
    {
        var lines = rendered.Split('\n');
        for (var i = 0; i < lines.Length; i++)
            _ = sb.Append("\x1b[").Append(row + i).Append(';').Append(col).Append('H').Append(lines[i]);
    }
}

void SampleMetrics()
{
    var procs = Process.GetProcesses();
    var totalCpu = TimeSpan.Zero;
    var totalMemBytes = 0L;
    var byMem = new List<(string Name, long Mem)>(procs.Length);

    foreach (var p in procs)
    {
        long mem = 0;
        TimeSpan cpu = TimeSpan.Zero;
        var name = "?";
        // Each property can throw on processes the user can't open (system / elevated
        // procs from a non-elevated session). Best-effort; missing samples just contribute
        // zero to the aggregate.
        try { mem = p.WorkingSet64; } catch { }
        try { cpu = p.TotalProcessorTime; } catch { }
        try { name = p.ProcessName; } catch { }

        totalCpu += cpu;
        totalMemBytes += mem;
        if (mem > 0) byMem.Add((name, mem));

        try { p.Dispose(); } catch { }
    }

    var now = DateTime.UtcNow;
    double cpuPct = 0;
    if (prevTimestamp != DateTime.MinValue)
    {
        var wallDeltaSec = (now - prevTimestamp).TotalSeconds;
        var cpuDeltaSec = (totalCpu - prevCpuTime).TotalSeconds;
        if (wallDeltaSec > 0)
            cpuPct = Math.Min(100, cpuDeltaSec / wallDeltaSec * 100.0 / Environment.ProcessorCount);
    }
    prevCpuTime = totalCpu;
    prevTimestamp = now;

    Shift(cpuData, cpuPct);
    Shift(memData, totalMemBytes / 1024.0 / 1024.0);
    Shift(procCountData, procs.Length);

    // Top-N by working set.
    byMem.Sort((a, b) => b.Mem.CompareTo(a.Mem));
    for (var i = 0; i < TopN; i++)
    {
        if (i < byMem.Count)
        {
            topProcMemData[i].Value = byMem[i].Mem / 1024.0 / 1024.0;
            // Truncate to 8 chars so the X-axis labels don't overlap each other.
            topProcNames[i] = byMem[i].Name.Length > 8 ? byMem[i].Name[..8] : byMem[i].Name;
        }
        else
        {
            topProcMemData[i].Value = 0;
            topProcNames[i] = "—";
        }
    }
}

static void Shift(ObservableCollection<ObservableValue> data, double newValue)
{
    // Add new point at the end; if we've reached the history cap, drop the oldest. The
    // chart engine sees existing entities' X indices decrement and animates them sliding
    // left, which reads as a smooth horizontal scroll. The new point fades in at the
    // right edge.
    if (data.Count >= HistoryLen) data.RemoveAt(0);
    data.Add(new ObservableValue(newValue));
}

static int? ParseIntFlag(string[] argv, string flag)
{
    for (var i = 0; i < argv.Length - 1; i++)
        if (argv[i] == flag && int.TryParse(argv[i + 1], out var v)) return v;
    return null;
}
