// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using System.Collections.ObjectModel;
using System.Text;
using LiveChartsCore;
using LiveChartsCore.Console;
using LiveChartsCore.Console.Painting;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;

// ----------------------------------------------------------------------------
// Renders a chart in the terminal. Pick which series kind to render with
// --line / --column / --row / --scatter / --step / --stackedcolumn /
// --stackedrow / --stackedarea / --stackedsteparea / --candlestick / --box /
// --heat / --pie / --polar — or pass --grid for a live 2x2 layout showing
// four charts side-by-side. Pick render mode with --halfblock / --braille /
// --sixel — default is auto-detect (Sixel if the terminal advertises it via
// DA1, otherwise Braille; HalfBlock stays opt-in). Cartesian kinds use a
// CartesianChart; --pie uses a PieChart; --polar uses a PolarChart.
//
// Data is sampled from EasingFunctions.BounceInOut shifted by a shared offset
// that advances every tick — produces a clean wave that scrolls across the
// chart instead of jittering randomly.
//
//   * If stdout is a terminal       → live animated chart (Ctrl+C to exit;
//                                      second Ctrl+C force-kills).
//   * If stdout is redirected/pipe  → one-shot snapshot to chart.ansi.
// ----------------------------------------------------------------------------

System.Console.OutputEncoding = Encoding.UTF8;

LiveCharts.Configure(c => c
    .AddConsole()
    .AddConsoleDefaultTheme()
    .AddDefaultMappers());

var mode = args.Contains("--sixel") ? ConsoleRenderMode.Sixel
         : args.Contains("--braille") ? ConsoleRenderMode.Braille
         : args.Contains("--halfblock") ? ConsoleRenderMode.HalfBlock
         // Default: ask the terminal what it supports. DA1 + cap 4 = Sixel; otherwise we
         // fall through to Braille (safe everywhere a UTF-8 monospace font is). Detection
         // is a sub-100ms terminal round-trip, only happens once, and silently degrades
         // to Braille if stdout is redirected.
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

const int Points = 32;          // line/step
const int Bars = 16;            // column/row/stacked
const int ScatterPoints = 24;
const int HeatCols = 7;         // heat — X grid size (one per day-of-week label)
const int HeatRows = 12;        // heat — Y grid size (one per month label)
string[] HeatXLabels = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];
string[] HeatYLabels = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
const int PieSlices = 6;        // pie — number of wedges
const double PhaseLine = 0;
const double PhaseBars = 0.15;
const double PhaseStackedA = 0;
const double PhaseStackedB = 0.5;

var sharedOffset = 0.0;

var lineData = new ObservableCollection<double>(Bounce(Points, PhaseLine, sharedOffset, signed: true));
var barData = new ObservableCollection<double>(Bounce(Bars, PhaseBars, sharedOffset, signed: true));
var scatterData = new ObservableCollection<ObservablePoint>(Scatter(ScatterPoints, sharedOffset));
var stack1Data = new ObservableCollection<double>(Bounce(Bars, PhaseStackedA, sharedOffset, signed: false));
var stack2Data = new ObservableCollection<double>(Bounce(Bars, PhaseStackedB, sharedOffset, signed: false));
var candleData = new ObservableCollection<FinancialPointI>(Candles(Bars, sharedOffset));
var boxData = new ObservableCollection<BoxValue>(Boxes(Bars, sharedOffset));
var heatData = new ObservableCollection<WeightedPoint>(Heat(HeatCols, HeatRows, sharedOffset));
var pieData = new ObservableValue[PieSlices];
for (var i = 0; i < PieSlices; i++) pieData[i] = new ObservableValue(SamplePieValue(i));

var kind = SelectedKind(args);

if (kind == "grid")
{
    await RunGridAsync();
    return;
}

// PieChart, PolarChart, and CartesianChart all share the same InMemoryConsoleChart base —
// they expose the same RenderMode / SixelCellWidth / Render / RenderLoop surface — so we
// keep the rest of the program polymorphic against that base after this branch.
// All single-chart kinds get a bottom legend by default — handy for multi-series cases
// (stacked, candlestick, box) and harmless for single-series (it's a one-entry strip).
// Heat skips it since one categorical-data series adds no signal.
InMemoryConsoleChart chart = kind switch
{
    "pie" => new PieChart
    {
        RenderMode = mode,
        Series = pieData
            .Select((pv, i) => (ISeries)new PieSeries<ObservableValue>(pv) { Name = $"S{i}" })
            .ToArray(),
        LegendPosition = LegendPosition.Bottom,
    },
    "polar" => new PolarChart
    {
        RenderMode = mode,
        Series = [
            new PolarLineSeries<double>(lineData) { Name = "Signal", GeometrySize = 0, LineSmoothness = 0.65 }
        ],
        LegendPosition = LegendPosition.Bottom,
    },
    "heat" => new CartesianChart
    {
        // Categorical labels demonstrate the BitmapFont glyph coverage in Sixel mode —
        // months / days exercise both upper- and lower-case letters.
        RenderMode = mode,
        Series = SelectSeries(args),
        XAxes = [new Axis { Labels = HeatXLabels }],
        YAxes = [new Axis { Labels = HeatYLabels }],
        // Heat is categorical on both axes — pan/zoom would slide whole cells in and out
        // of view, which works but isn't very meaningful. Leave it disabled for heat.
    },
    _ => new CartesianChart
    {
        RenderMode = mode,
        Series = SelectSeries(args),
        LegendPosition = LegendPosition.Bottom,
        // Default ZoomMode = None means mouse wheel and click-drag are no-ops. Enable
        // both axes here so cartesian samples are pannable / zoomable out of the box.
        ZoomMode = ZoomAndPanMode.X | ZoomAndPanMode.Y,
    },
};

// Only set SixelCellWidth/Height when the user provided a CLI flag — otherwise leave them
// alone so the chart can auto-detect the terminal's real cell pixel size at first render.
// Setting either property (even to the same default) opts out of detection.
if (sixelCw.HasValue) chart.SixelCellWidth = sixelCw.Value;
if (sixelCh.HasValue) chart.SixelCellHeight = sixelCh.Value;

chart.ConfigureFromTerminalCells(cols, rows);

var forceLive = args.Contains("--live");
var forceSnapshot = args.Contains("--snapshot");

if (forceSnapshot || (!forceLive && System.Console.IsOutputRedirected))
{
    File.WriteAllText("chart.ansi", chart.Render(home: false));
    System.Console.WriteLine(
        $"Rendered {chart.Width}x{chart.Height} sub-pixels ({cols}x{rows} cells, mode={mode}, kind={kind}) to chart.ansi.");
    return;
}

using var cts = new CancellationTokenSource();
System.Console.CancelKeyPress += (_, e) =>
{
    if (cts.IsCancellationRequested) return;
    e.Cancel = true;
    cts.Cancel();
};

// --tooltip simulates a pointer scanning left-to-right across the chart so tooltips fire
// without real input. Real keyboard-driven hover comes later (interactivity step); this is
// just the demo trigger. Sweep period is 4s so each point gets a noticeable highlight.
if (args.Contains("--tooltip"))
{
    var tooltipStart = DateTime.UtcNow;
    _ = Task.Run(async () =>
    {
        while (!cts.IsCancellationRequested)
        {
            try { await Task.Delay(50, cts.Token); }
            catch (OperationCanceledException) { return; }

            // Back off when real mouse input is recent — otherwise the auto-scan and the
            // mouse-driven SimulatePointerMove race for the tooltip cursor and it ends up
            // jumping between "wherever the scan is" and "where the mouse pointer is".
            // The scan is meant for headless / piped output, not for live + TTY.
            if ((DateTime.UtcNow - chart.LastMouseInputTimeUtc).TotalSeconds < 2) continue;

            var elapsed = (DateTime.UtcNow - tooltipStart).TotalSeconds;
            var phase = elapsed / 4 % 1.0;
            chart.SimulatePointerMove((int)(phase * chart.Width), chart.Height / 2);
        }
    });
}

// Data driver — bumps the shared offset by a small step every 250ms. The chart's animation
// system tweens between snapshots so the visual is smooth at the render FPS.
_ = Task.Run(async () =>
{
    while (!cts.IsCancellationRequested)
    {
        try { await Task.Delay(200, cts.Token); }
        catch (OperationCanceledException) { return; }

        sharedOffset += 0.025;

        lock (chart.SyncRoot)
        {
            ApplyBounce(lineData, PhaseLine, sharedOffset, signed: true);
            ApplyBounce(barData, PhaseBars, sharedOffset, signed: true);
            ApplyScatter(scatterData, sharedOffset);
            ApplyBounce(stack1Data, PhaseStackedA, sharedOffset, signed: false);
            ApplyBounce(stack2Data, PhaseStackedB, sharedOffset, signed: false);
            ApplyCandles(candleData, sharedOffset);
            ApplyBoxes(boxData, sharedOffset);
            ApplyHeat(heatData, HeatCols, HeatRows, sharedOffset);
            // Pie data is intentionally static — slice values shouldn't change while you're
            // hovering over them, otherwise the PushOut hover effect competes with constant
            // wedge re-layout for visual attention.
        }
    }
});

await chart.RenderLoopAsync(fps: 30, ct: cts.Token);

ISeries[] SelectSeries(string[] argv) => SelectedKind(argv) switch
{
    "column" => [
        new ColumnSeries<double>(barData) { Name = "Bars" }
    ],
    "row" => [
        new RowSeries<double>(barData) { Name = "Bars" }
    ],
    "scatter" => [
        new ScatterSeries<ObservablePoint>(scatterData) { Name = "Scatter", GeometrySize = 8 }
    ],
    "step" => [
        new StepLineSeries<double>(lineData) { Name = "Signal", GeometrySize = 0 }
    ],
    "stackedcolumn" => [
        new StackedColumnSeries<double>(stack1Data) { Name = "A" },
        new StackedColumnSeries<double>(stack2Data) { Name = "B" }
    ],
    "stackedrow" => [
        new StackedRowSeries<double>(stack1Data) { Name = "A" },
        new StackedRowSeries<double>(stack2Data) { Name = "B" }
    ],
    "stackedarea" => [
        new StackedAreaSeries<double>(stack1Data) { Name = "A" },
        new StackedAreaSeries<double>(stack2Data) { Name = "B" }
    ],
    "stackedsteparea" => [
        new StackedStepAreaSeries<double>(stack1Data) { Name = "A" },
        new StackedStepAreaSeries<double>(stack2Data) { Name = "B" }
    ],
    "candlestick" => [
        new CandlestickSeries<FinancialPointI>(candleData) { Name = "OHLC" }
    ],
    "box" => [
        new BoxSeries<BoxValue>(boxData) { Name = "Distribution" }
    ],
    "heat" => [
        new HeatSeries<WeightedPoint>(heatData) { Name = "Heat" }
    ],
    _ => [
        new LineSeries<double>(lineData) { Name = "Signal", GeometrySize = 0, LineSmoothness = 0.65 }
    ]
};

static string SelectedKind(string[] argv)
{
    if (argv.Contains("--column")) return "column";
    if (argv.Contains("--row")) return "row";
    if (argv.Contains("--scatter")) return "scatter";
    if (argv.Contains("--step")) return "step";
    if (argv.Contains("--stackedcolumn")) return "stackedcolumn";
    if (argv.Contains("--stackedrow")) return "stackedrow";
    if (argv.Contains("--stackedarea")) return "stackedarea";
    if (argv.Contains("--stackedsteparea")) return "stackedsteparea";
    if (argv.Contains("--candlestick")) return "candlestick";
    if (argv.Contains("--box")) return "box";
    if (argv.Contains("--heat")) return "heat";
    if (argv.Contains("--pie")) return "pie";
    if (argv.Contains("--polar")) return "polar";
    if (argv.Contains("--grid")) return "grid";
    return "line";
}

// ----------------------------------------------------------------------------
// Data shapers — sum of three phase-shifted sines at golden-ratio harmonics.
// The traveling-wave term (-t * speed) makes each frequency component scroll
// across the chart at a slightly different rate, so the curve never quite
// loops onto itself and the eye sees an organic, evolving pattern.
// "signed" leaves the result in roughly [-1, 1]; otherwise it's mapped to [0, 1].
// ----------------------------------------------------------------------------

static double[] Bounce(int n, double phase, double offset, bool signed)
{
    var data = new double[n];
    for (var i = 0; i < n; i++) data[i] = SampleBounce(i, n, phase, offset, signed);
    return data;
}

static void ApplyBounce(IList<double> dst, double phase, double offset, bool signed)
{
    for (var i = 0; i < dst.Count; i++) dst[i] = SampleBounce(i, dst.Count, phase, offset, signed);
}

static double SampleBounce(double i, int n, double phase, double offset, bool signed)
{
    var x = i / (double)(n - 1);
    var t = offset * 2 * Math.PI;
    var p = phase * 2 * Math.PI;

    var w = 0.65 * Math.Sin(2 * Math.PI * 1.000 * x - t * 1.0 + p)
          + 0.30 * Math.Sin(2 * Math.PI * 1.618 * x - t * 0.7 + p * 1.3)
          + 0.10 * Math.Sin(2 * Math.PI * 2.618 * x - t * 1.3 + p * 0.5);

    return signed ? w : (w + 1) * 0.5; // roughly [0, 1] for unsigned (stacked).
}

// Scatter — X bounces along one phase, Y along another, so points trace a
// Lissajous-ish path that breathes with the offset.
static ObservablePoint[] Scatter(int n, double offset)
{
    var data = new ObservablePoint[n];
    for (var i = 0; i < n; i++) data[i] = ScatterPoint(i, n, offset);
    return data;
}

static void ApplyScatter(IList<ObservablePoint> dst, double offset)
{
    for (var i = 0; i < dst.Count; i++)
    {
        var p = ScatterPoint(i, dst.Count, offset);
        dst[i].X = p.X;
        dst[i].Y = p.Y;
    }
}

static ObservablePoint ScatterPoint(int i, int n, double offset)
{
    // Spread X across the line range so it overlays cleanly on the same axis.
    var x = i / (double)(n - 1) * (Points - 1);
    var y = SampleBounce(i, n, 0.10 + (i & 1) * 0.20, offset, signed: true);
    return new ObservablePoint(x, y);
}

// Candlesticks — bounce drives the close price; high/low expand around it.
static FinancialPointI[] Candles(int n, double offset)
{
    var data = new FinancialPointI[n];
    for (var i = 0; i < n; i++) data[i] = CandlePoint(i, n, offset);
    return data;
}

static void ApplyCandles(IList<FinancialPointI> dst, double offset)
{
    // Mutate in-place rather than replacing the FinancialPointI instance: replacement
    // fires a Replace notification that the chart treats as "remove + add a different
    // entity", which restarts the candle's tween from zero every tick. Property mutation
    // triggers INotifyPropertyChanged on the same entity, so the existing tween redirects
    // smoothly to the new target without losing its mid-flight position.
    for (var i = 0; i < dst.Count; i++)
    {
        var existing = dst[i];
        var next = CandlePoint(i, dst.Count, offset);
        existing.High = next.High;
        existing.Open = next.Open;
        existing.Close = next.Close;
        existing.Low = next.Low;
    }
}

static FinancialPointI CandlePoint(int i, int n, double offset)
{
    // Open and close are samples from the *same* smooth wave taken half a step apart —
    // each candle's up/down direction is just the local slope of one continuous curve,
    // so adjacent candles correlate and the body color changes gradually as the wave
    // scrolls. The original formulation (open from i-1, close from i) read two
    // independent wave samples; correct in the limit but the slope at each i changed
    // faster than a viewer's "candle expectation", which made bodies flip erratically.
    var open = 100 + SampleBounce(i, n, 0.05, offset, signed: true) * 5;
    var close = 100 + SampleBounce(i + 0.5, n, 0.05, offset, signed: true) * 5;
    var high = Math.Max(open, close) + 1.5 + SampleBounce(i, n, 0.30, offset, signed: false) * 1.5;
    var low = Math.Min(open, close) - 1.5 - SampleBounce(i, n, 0.40, offset, signed: false) * 1.5;
    return new FinancialPointI(high, open, close, low);
}

// Boxes — bounced median + bounce-modulated spread so the boxes breathe in/out.
static BoxValue[] Boxes(int n, double offset)
{
    var data = new BoxValue[n];
    for (var i = 0; i < n; i++) data[i] = BoxPoint(i, n, offset);
    return data;
}

static void ApplyBoxes(IList<BoxValue> dst, double offset)
{
    // Same in-place mutation as ApplyCandles — see comment there for the reasoning.
    for (var i = 0; i < dst.Count; i++)
    {
        var existing = dst[i];
        var next = BoxPoint(i, dst.Count, offset);
        existing.Max = next.Max;
        existing.ThirdQuartile = next.ThirdQuartile;
        existing.Median = next.Median;
        existing.FirtQuartile = next.FirtQuartile;
        existing.Min = next.Min;
    }
}

static BoxValue BoxPoint(int i, int n, double offset)
{
    var center = 70 + SampleBounce(i, n, 0.05, offset, signed: true) * 10;
    var spread = 4 + SampleBounce(i, n, 0.30, offset, signed: false) * 6;
    // BoxValue ctor is (max, thirdQuartile, firstQuartile, min, median) — not the order
    // most box-plot libraries use. Use named args so the next reader doesn't get tripped.
    return new BoxValue(
        max: center + spread + 2,
        thirdQuartile: center + spread / 2,
        firstQuartile: center - spread / 2,
        min: center - spread - 2,
        median: center);
}

// Pie — slice values are static so the hover PushOut effect doesn't compete with constant
// wedge re-layout for visual attention. The shaper still varies values across slices to
// produce a recognizable spread.
static double SamplePieValue(int i)
{
    // Floor of 1 keeps every wedge visible; the per-slice phase via i*0.8 gives every
    // slice a different starting size in the [1, 5] range.
    return 1 + (Math.Sin(i * 0.8) + 1) * 2;
}

// Heat — 2D grid of WeightedPoint(x, y, weight). Weight is driven by a 2D scrolling wave so
// the heatmap "breathes" with the offset. X and Y are fixed grid coords (set once); only the
// Weight property mutates each tick.
static WeightedPoint[] Heat(int cols, int rows, double offset)
{
    var data = new WeightedPoint[cols * rows];
    for (var x = 0; x < cols; x++)
        for (var y = 0; y < rows; y++)
            data[x * rows + y] = new WeightedPoint(x, y, HeatWeight(x, y, cols, rows, offset));
    return data;
}

static void ApplyHeat(IList<WeightedPoint> dst, int cols, int rows, double offset)
{
    for (var x = 0; x < cols; x++)
        for (var y = 0; y < rows; y++)
            dst[x * rows + y].Weight = HeatWeight(x, y, cols, rows, offset);
}

static double HeatWeight(int xi, int yi, int cols, int rows, double offset)
{
    var x = xi / (double)(cols - 1);
    var y = yi / (double)(rows - 1);
    var t = offset * 2 * Math.PI;
    // Two phase-shifted 2D sines — each travels in its own direction so the gradient
    // doesn't repeat as a simple horizontal slide.
    var w = 0.6 * Math.Sin(2 * Math.PI * x - t) * Math.Cos(2 * Math.PI * y - t * 0.5)
          + 0.4 * Math.Sin(2 * Math.PI * 1.5 * x + 2 * Math.PI * y - t * 0.7);
    return (w + 1) * 0.5; // map to roughly [0, 1]
}

async Task RunGridAsync()
{
    // Quadrant size: half the terminal in each axis, with a 1-cell gutter between charts so
    // their borders don't run together. Floor at minimums so a tiny window doesn't produce
    // a degenerate Sixel image.
    var halfCols = Math.Max(20, (cols - 2) / 2);
    var halfRows = Math.Max(6, (rows - 2) / 2);

    InMemoryConsoleChart Configure(InMemoryConsoleChart c)
    {
        if (sixelCw.HasValue) c.SixelCellWidth = sixelCw.Value;
        if (sixelCh.HasValue) c.SixelCellHeight = sixelCh.Value;
        c.ConfigureFromTerminalCells(halfCols, halfRows);
        return c;
    }

    // ZoomMode = X|Y enables mouse-wheel zoom and click-drag pan on each cartesian chart.
    // Heat is left at None (default) since its categorical axes don't pan/zoom meaningfully.
    var both = ZoomAndPanMode.X | ZoomAndPanMode.Y;
    var lineChart = (CartesianChart)Configure(new CartesianChart
    {
        RenderMode = mode,
        Series = [new LineSeries<double>(lineData) { Name = "Line", GeometrySize = 0, LineSmoothness = 0.65 }],
        ZoomMode = both,
    });
    var columnChart = (CartesianChart)Configure(new CartesianChart
    {
        RenderMode = mode,
        Series = [new ColumnSeries<double>(barData) { Name = "Bars" }],
        ZoomMode = both,
    });
    var scatterChart = (CartesianChart)Configure(new CartesianChart
    {
        RenderMode = mode,
        Series = [new ScatterSeries<ObservablePoint>(scatterData) { Name = "Scatter", GeometrySize = 8 }],
        ZoomMode = both,
    });
    var heatChart = (CartesianChart)Configure(new CartesianChart
    {
        RenderMode = mode,
        Series = [new HeatSeries<WeightedPoint>(heatData) { Name = "Heat" }],
        XAxes = [new Axis { Labels = HeatXLabels }],
        YAxes = [new Axis { Labels = HeatYLabels }],
    });

    var charts = new[] { lineChart, columnChart, scatterChart, heatChart };

    using var gridCts = new CancellationTokenSource();
    System.Console.CancelKeyPress += (_, e) =>
    {
        if (gridCts.IsCancellationRequested) return;
        e.Cancel = true;
        gridCts.Cancel();
    };

    // Run terminal detections before starting the mouse reader (which steals raw stdin).
    // Each chart needs its own detection pass since the OSC 11 / CSI 16t responses are
    // cached per-instance. Loop them serially so responses don't interleave.
    foreach (var c in charts) c.EnsureTerminalDetections();

    // Mouse routing: figure out which quadrant the cell falls in (using the CURRENT
    // halfCols/halfRows, not capture — they change on resize), translate to that chart's
    // local cell coords, dispatch the event type to the per-chart helper.
    //
    // Keyboard: q quits, R resets zoom on every quadrant. Pan/zoom keys are intentionally
    // not routed here — pan/zoom on a 2x2 grid is ambiguous (which chart?) and the mouse
    // handles per-quadrant interaction cleanly anyway.
    var mouse = new ConsoleMouse(onEvent: (col, row, action) =>
    {
        // Quadrants are positioned at (1,1), (1, halfCols+2), (halfRows+2, 1),
        // (halfRows+2, halfCols+2). Mouse coords are 0-based, our positions are 1-based.
        // Convert mouse to 1-based for the comparison, then pick the chart by which half.
        var mouse1Col = col + 1;
        var mouse1Row = row + 1;
        var (chart, qRow, qCol) = mouse1Row <= halfRows
            ? (mouse1Col <= halfCols
                ? (lineChart, 1, 1)
                : ((InMemoryConsoleChart)columnChart, 1, halfCols + 2))
            : (mouse1Col <= halfCols
                ? ((InMemoryConsoleChart)scatterChart, halfRows + 2, 1)
                : ((InMemoryConsoleChart)heatChart, halfRows + 2, halfCols + 2));

        var localCol = mouse1Col - qCol;
        var localRow = mouse1Row - qRow;
        // NoteMouseInput omitted: the --tooltip auto-scan is in the single-chart path
        // (we returned early to RunGridAsync before it runs), so there's nothing to gate.
        switch (action)
        {
            case MouseAction.Move:      chart.SimulatePointerMoveOrLeaveAtCell(localCol, localRow); break;
            case MouseAction.Press:     chart.SimulatePointerDownAtCell(localCol, localRow); break;
            case MouseAction.Release:   chart.SimulatePointerUpAtCell(localCol, localRow); break;
            case MouseAction.WheelUp:   chart.SimulateZoomAtCell(localCol, localRow, zoomIn: true); break;
            case MouseAction.WheelDown: chart.SimulateZoomAtCell(localCol, localRow, zoomIn: false); break;
        }
    },
    onKey: action =>
    {
        switch (action)
        {
            case ConsoleKeyAction.Quit:
                gridCts.Cancel();
                break;
            case ConsoleKeyAction.ResetZoom:
                foreach (var c in charts) c.ResetZoom();
                break;
        }
    });
    mouse.Start(System.Console.Out);

    await System.Console.Out.WriteAsync("\x1b[0m\x1b[?25l\x1b[2J");
    await System.Console.Out.FlushAsync();

    var period = TimeSpan.FromMilliseconds(1000.0 / 30);
    var dataInterval = TimeSpan.FromMilliseconds(200);
    var nextDataTick = DateTime.UtcNow + dataInterval;

    var lastCols = cols;
    var lastRows = rows;

    try
    {
        var sb = new StringBuilder();
        while (!gridCts.IsCancellationRequested)
        {
            // Resize detection: re-read terminal size each frame and reconfigure the four
            // charts when it changes. The screen-clear is necessary because shrinking the
            // window leaves the previous frame's larger Sixel/cell content in cells that
            // the new (smaller) quadrants no longer touch.
            var resized = false;
            if (!System.Console.IsOutputRedirected)
            {
                int newCols, newRows;
                try
                {
                    newCols = Math.Max(40, System.Console.WindowWidth - 1);
                    newRows = Math.Max(10, System.Console.WindowHeight - 2);
                }
                catch (System.IO.IOException)
                {
                    newCols = lastCols;
                    newRows = lastRows;
                }

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

            // Tick data inline (single-threaded loop — no SyncRoot dance needed since the
            // mutation and the per-chart Measure happen sequentially on the same thread).
            if (DateTime.UtcNow >= nextDataTick)
            {
                sharedOffset += 0.025;
                ApplyBounce(lineData, PhaseLine, sharedOffset, signed: true);
                ApplyBounce(barData, PhaseBars, sharedOffset, signed: true);
                ApplyScatter(scatterData, sharedOffset);
                ApplyHeat(heatData, HeatCols, HeatRows, sharedOffset);
                nextDataTick = DateTime.UtcNow + dataInterval;
            }

            sb.Clear();
            // Reset SGR before clearing so the cleared cells inherit the terminal default,
            // not a stray fg/bg from earlier output (PSReadLine prompts, our own SGR state).
            if (resized) _ = sb.Append("\x1b[0m\x1b[2J");

            // Quadrant positions are recomputed each frame from the current halfCols /
            // halfRows so a resize takes effect immediately.
            EmitChartAt(sb, lineChart,    1,             1);
            EmitChartAt(sb, columnChart,  1,             halfCols + 2);
            EmitChartAt(sb, scatterChart, halfRows + 2,  1);
            EmitChartAt(sb, heatChart,    halfRows + 2,  halfCols + 2);

            await System.Console.Out.WriteAsync(sb);
            await System.Console.Out.FlushAsync(gridCts.Token);

            try { await Task.Delay(period, gridCts.Token); }
            catch (OperationCanceledException) { break; }
        }
    }
    finally
    {
        mouse.Stop();
        await System.Console.Out.WriteAsync("\x1b[0m\x1b[?25h\n");
        await System.Console.Out.FlushAsync(CancellationToken.None);
    }
}

static void EmitChartAt(StringBuilder sb, InMemoryConsoleChart chart, int row, int col)
{
    var rendered = chart.RenderFrame(home: false);

    if (chart.RenderMode == ConsoleRenderMode.Sixel)
    {
        // Sixel renders at the current cursor position — one absolute move and we're done.
        _ = sb.Append("\x1b[").Append(row).Append(';').Append(col).Append('H').Append(rendered);
    }
    else
    {
        // Cell-grid output uses '\n' between rows, which moves to column 0 of the next line.
        // For a quadrant-positioned chart we need each row to land at the chart's left edge,
        // not the terminal's, so reposition before each line.
        var lines = rendered.Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            _ = sb.Append("\x1b[").Append(row + i).Append(';').Append(col).Append('H').Append(lines[i]);
        }
    }
}

static int? ParseIntFlag(string[] argv, string flag)
{
    for (var i = 0; i < argv.Length - 1; i++)
        if (argv[i] == flag && int.TryParse(argv[i + 1], out var v)) return v;
    return null;
}
