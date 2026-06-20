// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Console.Drawing;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.Motion;

namespace LiveChartsCore.Console;

/// <summary>
/// Off-screen chart base that owns a <see cref="CoreMotionCanvas"/> and produces an ANSI string
/// (or writes directly to <see cref="System.Console"/>) on demand. Mirrors the role of
/// <c>InMemorySkiaSharpChart</c> in the SkiaSharp engine, but adds a live-loop helper since
/// terminals can be redrawn in place via cursor-home escapes.
/// </summary>
public abstract class InMemoryConsoleChart
{
    static InMemoryConsoleChart()
    {
        _ = LiveChartsConsole.EnsureInitialized();
    }

    private ConsoleSurface? _surface;
    private ConsoleRenderMode _mode = ConsoleRenderMode.HalfBlock;
    private LvcColor _background = default; // A=0 → "transparent / not set" sentinel.
    private bool _backgroundExplicit;
    private bool _backgroundDetected;
    private bool _sixelCellSizeExplicit;
    private bool _sixelCellSizeDetected;
    private int _sixelCellWidth = 10;
    private int _sixelCellHeight = 22;

    public CoreMotionCanvas CoreCanvas { get; } = new();

    /// <summary>
    /// Width in sub-pixels (= cells × <see cref="ConsoleSurface.CellWidth"/> for the current mode).
    /// </summary>
    public int Width { get; set; } = 120;

    /// <summary>
    /// Height in sub-pixels (= cells × <see cref="ConsoleSurface.CellHeight"/> for the current mode).
    /// </summary>
    public int Height { get; set; } = 40;

    /// <summary>
    /// Background color. Defaults to "transparent" (A = 0) so the terminal background shows
    /// through. The chart will try to detect the actual terminal background once on first
    /// render via OSC 11 and replace this with a solid color when detection succeeds. Set this
    /// explicitly to opt out of detection.
    /// </summary>
    public LvcColor Background
    {
        get => _background;
        set { _background = value; _backgroundExplicit = true; }
    }

    /// <summary>
    /// Lock object that <see cref="Render"/> and <see cref="RenderFrame"/> hold for the duration
    /// of measurement + draw. Callers mutating chart data from a thread other than the render
    /// thread (e.g. an <c>ObservableCollection&lt;T&gt;</c> driven by a background task) MUST
    /// take this lock around their mutations to avoid mid-enumeration exceptions in the chart's
    /// data factory.
    /// </summary>
    public object SyncRoot { get; } = new();

    /// <summary>
    /// Cell encoding for the next render. Set this BEFORE first render. Switching mode in-flight
    /// is allowed but recreates the surface and resets the cached cell-size that LabelGeometry
    /// uses for layout — call <see cref="ConfigureFromTerminalCells"/> afterwards if you sized
    /// the chart by cell count.
    /// </summary>
    public ConsoleRenderMode RenderMode
    {
        get => _mode;
        set
        {
            if (_mode == value) return;
            _mode = value;
            _surface = null;
            UpdateGlobalCellSize();
        }
    }

    /// <summary>
    /// Pixels per terminal cell when in <see cref="ConsoleRenderMode.Sixel"/>. If unset, the
    /// chart queries the terminal at first render via <c>\x1b[16t</c> and uses its reported
    /// cell pixel size — this aligns the image to cell boundaries and prevents the empty
    /// strip that otherwise appears below the chart when the assumed and actual cell heights
    /// differ. If detection fails (e.g., the terminal doesn't support the report), the
    /// defaults below are used. Setting either value explicitly opts out of detection.
    /// </summary>
    public int SixelCellWidth
    {
        get => _sixelCellWidth;
        set { _sixelCellWidth = value; _sixelCellSizeExplicit = true; }
    }

    public int SixelCellHeight
    {
        get => _sixelCellHeight;
        set { _sixelCellHeight = value; _sixelCellSizeExplicit = true; }
    }

    /// <summary>
    /// Sizes <see cref="Width"/> and <see cref="Height"/> so the chart fills the given number of
    /// terminal cells under the current <see cref="RenderMode"/>.
    /// </summary>
    public void ConfigureFromTerminalCells(int cellCols, int cellRows)
    {
        // Resolve cell size BEFORE deriving Width/Height — otherwise we'd size the surface
        // using the assumed defaults, then detect a different real cell size on first
        // render, and end up with a misaligned image until the next configure.
        EnsureSixelCellSizeResolved();

        var (cw, ch) = CellPixelSize();
        Width = Math.Max(cw, cellCols * cw);
        Height = Math.Max(ch, cellRows * ch);
        UpdateGlobalCellSize();
    }

    private (int cw, int ch) CellPixelSize() => _mode switch
    {
        ConsoleRenderMode.Braille => (2, 4),
        ConsoleRenderMode.Sixel => (SixelCellWidth, SixelCellHeight),
        _ => (1, 2),
    };

    private void UpdateGlobalCellSize()
    {
        // Read by LabelGeometry.Measure so axis layout reserves the right number of pixels per
        // character. For cell-grid modes this is the cell pixel size (1 char = 1 cell). For
        // Sixel, labels are bitmap-rendered, so this is the bitmap glyph cell instead — it
        // diverges from the terminal cell pixel size used to size the Sixel image overall.
        var (w, h) = _mode switch
        {
            ConsoleRenderMode.Braille => (2, 4),
            ConsoleRenderMode.Sixel => (BitmapFont.CellWidth(), BitmapFont.CellHeight()),
            _ => (1, 2),
        };
        Drawing.Geometries.LabelGeometry.GlyphPixelsW = w;
        Drawing.Geometries.LabelGeometry.GlyphPixelsH = h;
    }

    protected abstract Chart GetCoreChart();

    /// <summary>
    /// One-shot render. Disables animations so the result is the fully-settled state, then
    /// unloads the chart. Use for "save image" / snapshot workflows.
    /// </summary>
    /// <param name="home">Prepend cursor-home so a follow-up frame overwrites in place.</param>
    /// <param name="color">
    /// When false, return a plain-text rendering with no ANSI escapes — readable in any tool
    /// result that doesn't interpret terminal control sequences (e.g. an LLM Bash tool).
    /// Requires <see cref="RenderMode"/> to be HalfBlock or Braille; Sixel cannot emit plain.
    /// </param>
    public string Render(bool home = false, bool color = true)
    {
        var coreChart = GetCoreChart()
            ?? throw new InvalidOperationException("CoreChart is not available.");

        lock (SyncRoot)
        {
            EnsureBackgroundResolved();
            EnsureSixelCellSizeResolved();
            var surface = AcquireSurface();

            coreChart.Canvas.DisableAnimations = true;
            coreChart.IsLoaded = true;

            coreChart.Measure();
            coreChart.Canvas.DrawFrame(new ConsoleDrawingContext(CoreCanvas, surface, Background));
            RenderTooltipOverlay(surface);

            coreChart.Unload();
            _surface = null; // unload disposes paint state, force a fresh surface next call.

            return surface.ToAnsi(home, color);
        }
    }

    /// <summary>
    /// Draws a single live frame. Does NOT disable animations or unload the chart, so successive
    /// calls drive the motion system and produce in-flight tween states. Cheap to call in a loop.
    /// </summary>
    public string RenderFrame(bool home = true)
    {
        var coreChart = GetCoreChart()
            ?? throw new InvalidOperationException("CoreChart is not available.");

        lock (SyncRoot)
        {
            EnsureBackgroundResolved();
            EnsureSixelCellSizeResolved();
            var surface = AcquireSurface();

            // Idempotent — Load is invoked once in the constructor; this just guards re-entry.
            coreChart.IsLoaded = true;

            // DON'T call coreChart.Measure() here — the engine's own update throttler runs
            // Measure when data changes (INPC propagates through DataFactory → chart.Update).
            // Calling it every render frame meant CorePieSeries reset
            // dougnutGeometry.PushOut = (float)Pushout = 0 each frame, which clobbered the
            // Hover state's PushOut animation toward HoverPushout. The visible symptom was
            // "slice tries to push out, snaps back, repeats." Animations are driven by
            // motion-property tweens during DrawFrame; no Measure needed for them.
            coreChart.Canvas.DrawFrame(new ConsoleDrawingContext(CoreCanvas, surface, Background));
            RenderTooltipOverlay(surface);

            return surface.ToAnsi(home);
        }
    }

    /// <summary>
    /// Pumps <see cref="RenderFrame"/> at the requested cadence to <paramref name="output"/>
    /// (defaulting to <see cref="System.Console.Out"/>) until <paramref name="ct"/> fires.
    /// Hides the cursor while running and resets terminal state on exit.
    /// </summary>
    public async Task RenderLoopAsync(
        int fps = 30,
        TextWriter? output = null,
        CancellationToken ct = default)
    {
        output ??= System.Console.Out;
        var period = TimeSpan.FromMilliseconds(1000.0 / Math.Max(1, fps));

        // \x1b[0m up-front: \x1b[2J erases the screen using the *currently active* SGR
        // background — if PowerShell's prompt or any prior output left a non-default bg
        // selected, the cleared screen would inherit it. Reset SGR first so the clear
        // uses the terminal's actual default background.
        // Run the terminal-query auto-detections up-front, while stdin is still in cooked
        // mode and nobody's reading raw bytes. ConsoleMouse below will switch stdin into
        // a continuously-read raw-byte loop that would swallow OSC 11 / CSI 16t responses
        // before our helpers can pick them up.
        EnsureTerminalDetections();

        // Linked CTS so q / Q can quit alongside the caller's ct.
        using var loopCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        // Mouse capture: hover drives tooltips, click-and-drag drives pan, wheel zooms in/out
        // around the cursor. The engine handles pan internally — once a Press fires it sets
        // _isPanning, then subsequent Move events go to the panning throttler instead of the
        // tooltip throttler. Release clears the flag and Move resumes feeding hover.
        //
        // Keyboard: q/Q quits, R resets zoom, +/- zoom at chart center, arrow keys pan by
        // 1/8 of the draw margin in that direction.
        var mouse = new ConsoleMouse(
            onEvent: (col, row, action) =>
            {
                NoteMouseInput();
                switch (action)
                {
                    case MouseAction.Move:      SimulatePointerMoveOrLeaveAtCell(col, row); break;
                    case MouseAction.Press:     SimulatePointerDownAtCell(col, row); break;
                    case MouseAction.Release:   SimulatePointerUpAtCell(col, row); break;
                    case MouseAction.WheelUp:   SimulateZoomAtCell(col, row, zoomIn: true); break;
                    case MouseAction.WheelDown: SimulateZoomAtCell(col, row, zoomIn: false); break;
                }
            },
            onKey: action =>
            {
                switch (action)
                {
                    case ConsoleKeyAction.Quit:      loopCts.Cancel(); break;
                    case ConsoleKeyAction.ResetZoom: ResetZoom(); break;
                    case ConsoleKeyAction.ZoomIn:    SimulateZoomAtCenter(true); break;
                    case ConsoleKeyAction.ZoomOut:   SimulateZoomAtCenter(false); break;
                    case ConsoleKeyAction.PanUp:     SimulatePan(0, +PanStepPx); break;
                    case ConsoleKeyAction.PanDown:   SimulatePan(0, -PanStepPx); break;
                    case ConsoleKeyAction.PanLeft:   SimulatePan(+PanStepPx, 0); break;
                    case ConsoleKeyAction.PanRight:  SimulatePan(-PanStepPx, 0); break;
                }
            });
        mouse.Start(output);

        await output.WriteAsync("\x1b[0m\x1b[?25l\x1b[2J");

        try
        {
            while (!loopCts.IsCancellationRequested)
            {
                AdaptToTerminalSize();

                var frame = RenderFrame(home: true);
                await output.WriteAsync(frame);
                await output.FlushAsync(loopCts.Token);

                try { await Task.Delay(period, loopCts.Token); }
                catch (OperationCanceledException) { break; }
            }
        }
        finally
        {
            mouse.Stop();
            await output.WriteAsync("\x1b[0m\x1b[?25h\n"); // reset color, show cursor.
            await output.FlushAsync(CancellationToken.None);
        }
    }

    /// <summary>
    /// Pan step in surface pixels per arrow-key tap. Sized so a single tap is visible but
    /// holding the key (auto-repeat) still gives reasonable cursor-keys-feel scrolling.
    /// </summary>
    private const int PanStepPx = 30;

    /// <summary>
    /// Convenience: one-shot render + write to stdout.
    /// </summary>
    public void Print(bool home = false) =>
        System.Console.Out.Write(Render(home));

    /// <summary>
    /// Simulates a pointer-move event at the given pixel coordinates on the surface — drives
    /// the chart engine's hover detection, which in turn calls <c>Tooltip.Show</c> with the
    /// nearest data points. Public wrapper around <c>Chart.InvokePointerMove</c>, which is
    /// protected internal in LiveChartsCore. Use this to script tooltip behavior (e.g., scan
    /// across the chart over time) before real keyboard input is wired up.
    /// </summary>
    public void SimulatePointerMove(int xPixels, int yPixels)
    {
        var core = GetCoreChart();
        if (core is null) return;
        core.InvokePointerMove(new LvcPoint(xPixels, yPixels));
    }

    /// <summary>
    /// Convenience wrapper around <see cref="SimulatePointerMove"/> that takes terminal
    /// cell coordinates (0-based) and translates them into surface pixel coordinates using
    /// the current render mode's per-cell pixel size. ConsoleMouse emits cell coords from
    /// xterm SGR sequences, so this is the natural target for mouse callbacks.
    /// </summary>
    public void SimulatePointerMoveAtCell(int cellCol, int cellRow)
    {
        var (cw, ch) = CellPixelSize();
        SimulatePointerMove(cellCol * cw + cw / 2, cellRow * ch + ch / 2);
    }

    /// <summary>
    /// Like <see cref="SimulatePointerMoveAtCell"/>, but if the resulting pixel position
    /// falls outside the chart's draw margin we fire <see cref="SimulatePointerLeft"/>
    /// instead. Engines don't fire Tooltip.Hide on their own when the pointer drifts past
    /// the plot area (they just early-return from DrawToolTip), so without this the
    /// tooltip would sit there forever once the user mouses out of the chart.
    /// </summary>
    public void SimulatePointerMoveOrLeaveAtCell(int cellCol, int cellRow)
    {
        var core = GetCoreChart();
        if (core is null) return;

        var (cw, ch) = CellPixelSize();
        var xPix = cellCol * cw + cw / 2;
        var yPix = cellRow * ch + ch / 2;

        var loc = core.DrawMarginLocation;
        var sz = core.DrawMarginSize;
        if (xPix < loc.X || xPix > loc.X + sz.Width ||
            yPix < loc.Y || yPix > loc.Y + sz.Height)
        {
            core.InvokePointerLeft();
        }
        else
        {
            core.InvokePointerMove(new LvcPoint(xPix, yPix));
        }
    }

    /// <summary>
    /// Fires a pointer-down event. The chart engine sets _isPanning so that subsequent
    /// PointerMove events drive its panning throttler — meaning a click-and-drag becomes
    /// a chart pan automatically (subject to the chart's <c>ZoomMode</c> flags).
    /// </summary>
    public void SimulatePointerDownAtCell(int cellCol, int cellRow)
    {
        var core = GetCoreChart();
        if (core is null) return;
        var (cw, ch) = CellPixelSize();
        core.InvokePointerDown(new LvcPoint(cellCol * cw + cw / 2, cellRow * ch + ch / 2), false);
    }

    /// <summary>
    /// Fires a pointer-up event. Clears the engine's panning flag, so subsequent
    /// PointerMove events return to driving hover/tooltip rather than pan.
    /// </summary>
    public void SimulatePointerUpAtCell(int cellCol, int cellRow)
    {
        var core = GetCoreChart();
        if (core is null) return;
        var (cw, ch) = CellPixelSize();
        core.InvokePointerUp(new LvcPoint(cellCol * cw + cw / 2, cellRow * ch + ch / 2), false);
    }

    /// <summary>
    /// Zooms the chart at the given cell (the cell becomes the zoom pivot, so the data
    /// point under the cursor stays fixed while everything else scales around it). No-op
    /// for non-cartesian charts since pie / polar don't have a cartesian zoom concept.
    /// Honors the chart's current <c>ZoomMode</c> flags.
    /// </summary>
    public void SimulateZoomAtCell(int cellCol, int cellRow, bool zoomIn)
    {
        if (GetCoreChart() is not CartesianChartEngine cartesian) return;

        var (cw, ch) = CellPixelSize();
        var pivot = new LvcPoint(cellCol * cw + cw / 2, cellRow * ch + ch / 2);
        var direction = zoomIn ? ZoomDirection.ZoomIn : ZoomDirection.ZoomOut;

        // ZoomMode lives on the view (the chart class), accessed via ICartesianChartView.
        var mode = ((ICartesianChartView)cartesian.View).ZoomMode;
        cartesian.Zoom(mode, pivot, direction);
    }

    /// <summary>
    /// Keyboard-shortcut zoom: pivot at the chart's draw-margin center. Used by + / -.
    /// </summary>
    public void SimulateZoomAtCenter(bool zoomIn)
    {
        if (GetCoreChart() is not CartesianChartEngine cartesian) return;
        var pivot = new LvcPoint(
            cartesian.DrawMarginLocation.X + cartesian.DrawMarginSize.Width / 2,
            cartesian.DrawMarginLocation.Y + cartesian.DrawMarginSize.Height / 2);
        var direction = zoomIn ? ZoomDirection.ZoomIn : ZoomDirection.ZoomOut;
        var mode = ((ICartesianChartView)cartesian.View).ZoomMode;
        cartesian.Zoom(mode, pivot, direction);
    }

    /// <summary>
    /// Keyboard-shortcut pan: shifts the visible range by (dxPix, dyPix) in surface pixel
    /// coords. Engine treats positive dx as "drag right" (so panning right shows data
    /// further to the right). Used by arrow keys.
    /// </summary>
    public void SimulatePan(int dxPix, int dyPix)
    {
        if (GetCoreChart() is not CartesianChartEngine cartesian) return;
        var mode = ((ICartesianChartView)cartesian.View).ZoomMode;
        cartesian.Pan(mode, new LvcPoint(dxPix, dyPix));
    }

    /// <summary>
    /// Clears any user-applied axis limits so the chart auto-fits its data again. Used by
    /// the R keyboard shortcut. No-op for non-cartesian charts (their zoom is handled
    /// differently or doesn't apply).
    /// </summary>
    public void ResetZoom()
    {
        if (GetCoreChart() is not CartesianChartEngine cartesian) return;
        foreach (var axis in cartesian.XAxes) { axis.MinLimit = null; axis.MaxLimit = null; }
        foreach (var axis in cartesian.YAxes) { axis.MinLimit = null; axis.MaxLimit = null; }
    }

    /// <summary>
    /// UTC timestamp of the most recent mouse-driven pointer event (set by the live loop's
    /// ConsoleMouse callback). Used by the sample's --tooltip auto-scan to back off when
    /// real mouse input is available, so the two don't race for the tooltip cursor.
    /// </summary>
    public DateTime LastMouseInputTimeUtc { get; private set; } = DateTime.MinValue;

    /// <summary>
    /// Optional chart title rendered as a centered strip at the top of the surface. Console-
    /// specific shortcut — sidesteps the engine's Visual-element title machinery (which
    /// requires a per-platform LabelVisual implementation) by rendering directly in our
    /// post-DrawFrame overlay pass. Doesn't reserve space, so on tightly-sized charts the
    /// title may overlap the top of the chart's content; pad your terminal accordingly or
    /// leave it null. Distinct from the IChartView.Title property on derived chart types,
    /// which expects an engine-managed Visual / VisualElement instead of a plain string.
    /// </summary>
    public string? TitleText { get; set; }

    internal void NoteMouseInput() => LastMouseInputTimeUtc = DateTime.UtcNow;

    /// <summary>
    /// Simulates the pointer leaving the chart — fires <c>Tooltip.Hide</c> via the engine.
    /// </summary>
    public void SimulatePointerLeft()
    {
        var core = GetCoreChart();
        if (core is null) return;
        core.InvokePointerLeft();
    }

    private void RenderTooltipOverlay(ConsoleSurface surface)
    {
        // GetCoreChart returns the chart engine, whose Tooltip / Legend are set from the
        // theme factories or whatever the view assigned. We only know how to render the
        // console flavors; anything else is a noop.
        var core = GetCoreChart();
        if (core?.Legend is ConsoleLegend legend) legend.Render(surface);
        if (core?.Tooltip is ConsoleTooltip tooltip) tooltip.Render(surface);

        RenderSelectedPointsMarker(surface);
        RenderTitle(surface);
    }

    private void RenderTitle(ConsoleSurface surface)
    {
        var title = TitleText;
        if (string.IsNullOrEmpty(title)) return;

        var (charW, charH) = surface.Mode == ConsoleRenderMode.Sixel
            ? (Drawing.BitmapFont.CellWidth(), Drawing.BitmapFont.CellHeight())
            : (surface.CellWidth, surface.CellHeight);

        var x = (surface.Width - title.Length * charW) / 2;
        if (x < 0) x = 0;

        var fg = surface.StrongForeground;
        if (surface.Mode == ConsoleRenderMode.Sixel)
            Drawing.BitmapFont.DrawText(surface, x, 0, title, fg);
        else
            surface.DrawText(x, 0, title, fg);
    }

    /// <summary>
    /// Selected points captured from the chart's DataPointerDown event (wired in the
    /// SourceGenChart bridge). Rendered as a small marker on top of the chart so the user
    /// can see which point they clicked even after the pointer moves elsewhere.
    /// </summary>
    private List<Kernel.ChartPoint>? _selectedPoints;

    internal void SetSelectedPoints(IEnumerable<Kernel.ChartPoint>? points)
    {
        _selectedPoints = points is null ? null : [.. points];
    }

    private void RenderSelectedPointsMarker(ConsoleSurface surface)
    {
        var pts = _selectedPoints;
        if (pts is null || pts.Count == 0) return;

        // Bright accent so the marker reads against any background. The cross shape is
        // cheap (8 SetPixel calls) and stays visible at every render mode without depending
        // on the bitmap font.
        var accent = new LvcColor(255, 200, 50);
        foreach (var point in pts)
        {
            if (point.Context.Visual is not BoundedDrawnGeometry visual) continue;
            var cx = (int)(visual.X + visual.Width / 2);
            var cy = (int)(visual.Y + visual.Height / 2);
            for (var d = -3; d <= 3; d++)
            {
                surface.SetPixel(cx + d, cy, accent);
                surface.SetPixel(cx, cy + d, accent);
            }
        }
    }

    /// <summary>
    /// Runs the OSC 11 background and CSI 16t cell-pixel-size queries up-front, while stdin
    /// is still in cooked mode and not being read by anyone else. Idempotent — both helpers
    /// short-circuit if already resolved or if the user set a value explicitly.
    ///
    /// Important to call this BEFORE starting <see cref="ConsoleMouse"/> (or any other raw
    /// stdin consumer): the mouse reader runs continuously and would swallow the terminal's
    /// query responses before our detection helpers could read them, leaving the chart
    /// stuck on a default (often dark) background that doesn't match the terminal.
    /// </summary>
    public void EnsureTerminalDetections()
    {
        EnsureBackgroundResolved();
        EnsureSixelCellSizeResolved();
    }

    private void EnsureBackgroundResolved()
    {
        // If the user set Background explicitly, honor it. Otherwise try ONCE to detect the
        // terminal's real background via OSC 11. If detection fails, leave it transparent so
        // the cell encoder emits default-attr escapes and Sixel uses Pb=1 — either way, the
        // terminal's own background shows through.
        if (_backgroundExplicit || _backgroundDetected) return;
        _backgroundDetected = true;

        var detected = ConsoleTerminal.TryDetectBackground();
        if (detected.HasValue)
        {
            var c = detected.Value;
            _background = new LvcColor(c.R, c.G, c.B); // force opaque so the encoder paints it.
        }
    }

    private void EnsureSixelCellSizeResolved()
    {
        // Only meaningful in Sixel mode; cell-grid modes use fixed sub-pixel ratios.
        if (_mode != ConsoleRenderMode.Sixel) return;
        if (_sixelCellSizeExplicit || _sixelCellSizeDetected) return;
        _sixelCellSizeDetected = true;

        var detected = ConsoleTerminal.TryDetectCellPixelSize();
        if (!detected.HasValue) return;

        // Set fields directly — the property setters would mark the values as "explicit"
        // and disable redetection if the user later switches modes.
        _sixelCellWidth = detected.Value.width;
        _sixelCellHeight = detected.Value.height;
    }

    private ConsoleSurface AcquireSurface()
    {
        if (_surface is null
            || _surface.Mode != _mode
            || _surface.Width != Width
            || _surface.Height != Height
            || (_mode == ConsoleRenderMode.Sixel && (_surface.CellWidth != SixelCellWidth || _surface.CellHeight != SixelCellHeight)))
        {
            _surface = new ConsoleSurface(Width, Height, _mode, SixelCellWidth, SixelCellHeight);
            UpdateGlobalCellSize();
        }
        _surface.Background = Background;
        _surface.Clear();
        return _surface;
    }

    private void AdaptToTerminalSize()
    {
        if (System.Console.IsOutputRedirected) return;

        int cols, rows;
        try
        {
            cols = Math.Max(20, System.Console.WindowWidth - 1);
            rows = Math.Max(6, System.Console.WindowHeight - 2);
        }
        catch (IOException)
        {
            return;
        }

        ConfigureFromTerminalCells(cols, rows);
    }
}
