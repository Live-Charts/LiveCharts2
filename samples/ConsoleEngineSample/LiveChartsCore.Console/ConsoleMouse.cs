// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using System.Runtime.InteropServices;

namespace LiveChartsCore.Console;

/// <summary>
/// Discriminates the mouse-event categories we report from the parsed SGR sequences.
/// Press / Release fire on left-button down / up only — middle and right buttons are
/// silently dropped (we don't have handlers for them yet). Move covers both no-button
/// hover (when ?1003 any-event tracking is on) and button-held drag. WheelUp / WheelDown
/// are the scroll-wheel events the terminal reports as button codes 64 / 65.
/// </summary>
public enum MouseAction { Move, Press, Release, WheelUp, WheelDown }

/// <summary>
/// Keyboard actions surfaced by <see cref="ConsoleMouse"/>'s reader. Only a small set of
/// known shortcuts — anything else is ignored. Quit fires on q / Q (Ctrl+C is handled
/// separately via Console.CancelKeyPress so it works even before the reader starts).
/// </summary>
public enum ConsoleKeyAction { Quit, ResetZoom, ZoomIn, ZoomOut, PanUp, PanDown, PanLeft, PanRight }

/// <summary>
/// Captures terminal mouse events via xterm SGR mouse mode (CSI ?1003h + CSI ?1006h).
/// Modern terminals (Windows Terminal, iTerm2, WezTerm, ghostty, mintty) all support it;
/// the terminal reports each mouse interaction by injecting an escape sequence onto stdin
/// shaped like <c>ESC [ &lt; button;col;row M</c> (press/move) or <c>... m</c> (release).
/// We parse those sequences in a background reader and dispatch to the user-supplied
/// callback as 0-based (col, row) cell coordinates plus a <see cref="MouseAction"/>.
///
/// On Windows we also flip the console input mode to disable line buffering / echo and
/// enable VT input so the terminal's escape emissions reach our reader as raw bytes (the
/// legacy console layer would otherwise process some of them). ENABLE_PROCESSED_INPUT
/// stays on so Ctrl+C still fires CancelKeyPress instead of arriving as a 0x03 byte.
/// On non-Windows platforms we rely on the host already presenting stdin in raw VT mode
/// (typical for any modern terminal-attached process) — there's no portable .NET API to
/// switch tty modes mid-program, and most users running this in a terminal won't need it.
/// </summary>
public sealed class ConsoleMouse : IDisposable
{
    private readonly Action<int, int, MouseAction> _onEvent;
    private readonly Action<ConsoleKeyAction>? _onKey;
    private TextWriter? _output;
    private CancellationTokenSource? _cts;
    private uint _originalConsoleMode;
    private bool _modeChanged;

    public ConsoleMouse(Action<int, int, MouseAction> onEvent, Action<ConsoleKeyAction>? onKey = null)
    {
        _onEvent = onEvent;
        _onKey = onKey;
    }

    /// <summary>
    /// Enables mouse capture and starts the background reader. Bails silently if stdin or
    /// stdout is redirected (no TTY → no mouse).
    /// </summary>
    public void Start(TextWriter output)
    {
        if (_cts is not null) return;
        if (System.Console.IsInputRedirected || System.Console.IsOutputRedirected) return;

        _output = output;
        TrySetRawConsoleMode();

        _cts = new CancellationTokenSource();
        _ = Task.Run(() => ReadLoop(_cts.Token));

        // ?1003 = any-event tracking (every mouse move, not just button-down/drag).
        // ?1006 = SGR encoding (decimal text coords; old encodings cap col/row at 223).
        output.Write("\x1b[?1003h\x1b[?1006h");
        output.Flush();
    }

    /// <summary>
    /// Disables mouse capture and restores the prior console mode. Safe to call multiple
    /// times. The background reader task is left to die with the process — Stream.Read on
    /// stdin doesn't honor cancellation cleanly across platforms, and abandoning a blocked
    /// read is harmless once we've already disabled mouse-mode escape generation.
    /// </summary>
    public void Stop()
    {
        if (_cts is null) return;

        _cts.Cancel();
        try
        {
            _output?.Write("\x1b[?1006l\x1b[?1003l");
            _output?.Flush();
        }
        catch { /* output may be closed during shutdown */ }
        TryRestoreConsoleMode();

        _cts = null;
        _output = null;
    }

    public void Dispose() => Stop();

    private void ReadLoop(CancellationToken ct)
    {
        try
        {
            using var stream = System.Console.OpenStandardInput();
            var buffer = new byte[256];

            // Tiny state machine: drives parsing across multiple Read calls so a sequence
            // split mid-stream (rare but possible) parses cleanly. Any unexpected byte
            // resets to INITIAL — this drops malformed sequences without corrupting the
            // rest of the input stream.
            var state = 0;
            int btn = 0, col = 0, row = 0;

            while (!ct.IsCancellationRequested)
            {
                var n = stream.Read(buffer, 0, buffer.Length);
                if (n <= 0) break;

                for (var i = 0; i < n; i++)
                {
                    var b = buffer[i];
                    switch (state)
                    {
                        case 0:
                            if (b == 0x1b) state = 1;
                            else DispatchKeyByte(b);
                            break;
                        case 1:
                            // After ESC: '[' starts a CSI sequence (mouse SGR or arrow
                            // keys), anything else means ESC was followed by something we
                            // don't recognize — drop both and reset.
                            state = b == '[' ? 2 : 0;
                            break;
                        case 2:
                            if (b == '<') { state = 3; btn = col = row = 0; }
                            else
                            {
                                // CSI A/B/C/D = arrow keys. Anything else: drop.
                                DispatchArrowByte(b);
                                state = 0;
                            }
                            break;
                        case 3:
                            if (b >= '0' && b <= '9') btn = btn * 10 + (b - '0');
                            else if (b == ';') state = 4;
                            else state = 0;
                            break;
                        case 4:
                            if (b >= '0' && b <= '9') col = col * 10 + (b - '0');
                            else if (b == ';') state = 5;
                            else state = 0;
                            break;
                        case 5:
                            if (b >= '0' && b <= '9') row = row * 10 + (b - '0');
                            else if (b == 'M' || b == 'm')
                            {
                                // Button-code bit layout (xterm SGR):
                                //   bit 6 (0x40) = wheel event (low bit picks up vs down)
                                //   bit 5 (0x20) = motion (drag if buttons held, hover otherwise)
                                //   bits 0-1    = button index (0 left, 1 middle, 2 right)
                                // 'M' terminator = press or motion; 'm' = release.
                                MouseAction? action = null;
                                if ((btn & 0x40) != 0)
                                    action = (btn & 1) == 0 ? MouseAction.WheelUp : MouseAction.WheelDown;
                                else if ((btn & 0x20) != 0)
                                    action = MouseAction.Move;
                                else if ((btn & 0x03) == 0) // left button only
                                    action = b == 'M' ? MouseAction.Press : MouseAction.Release;

                                if (action.HasValue)
                                {
                                    try { _onEvent(col - 1, row - 1, action.Value); }
                                    catch { /* swallow — don't take down the reader for callback bugs */ }
                                }
                                state = 0;
                            }
                            else state = 0;
                            break;
                    }
                }
            }
        }
        catch { /* read failure or process tearing down */ }
    }

    private void DispatchKeyByte(byte b)
    {
        if (_onKey is null) return;
        ConsoleKeyAction? key = b switch
        {
            (byte)'q' or (byte)'Q' => ConsoleKeyAction.Quit,
            (byte)'r' or (byte)'R' => ConsoleKeyAction.ResetZoom,
            (byte)'+' or (byte)'=' => ConsoleKeyAction.ZoomIn,
            (byte)'-' or (byte)'_' => ConsoleKeyAction.ZoomOut,
            _ => null,
        };
        if (key.HasValue)
        {
            try { _onKey(key.Value); }
            catch { /* swallow — don't take down the reader for callback bugs */ }
        }
    }

    private void DispatchArrowByte(byte b)
    {
        if (_onKey is null) return;
        ConsoleKeyAction? key = b switch
        {
            (byte)'A' => ConsoleKeyAction.PanUp,
            (byte)'B' => ConsoleKeyAction.PanDown,
            (byte)'C' => ConsoleKeyAction.PanRight,
            (byte)'D' => ConsoleKeyAction.PanLeft,
            _ => null,
        };
        if (key.HasValue)
        {
            try { _onKey(key.Value); }
            catch { }
        }
    }

    // -------- Windows console mode --------

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetConsoleMode(IntPtr handle, out uint mode);
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetConsoleMode(IntPtr handle, uint mode);

    private const int STD_INPUT_HANDLE = -10;
    private const uint ENABLE_LINE_INPUT = 0x0002;
    private const uint ENABLE_ECHO_INPUT = 0x0004;
    private const uint ENABLE_WINDOW_INPUT = 0x0008;
    private const uint ENABLE_MOUSE_INPUT = 0x0010;
    private const uint ENABLE_VIRTUAL_TERMINAL_INPUT = 0x0200;

    private void TrySetRawConsoleMode()
    {
        if (OperatingSystem.IsWindows())
        {
            try
            {
                var handle = GetStdHandle(STD_INPUT_HANDLE);
                if (handle == IntPtr.Zero) return;
                if (!GetConsoleMode(handle, out var mode)) return;

                _originalConsoleMode = mode;

                // Strip line buffering, echo, and legacy-event sources (window resize /
                // Win32 mouse-event records) — we want xterm SGR sequences only. Keep
                // ENABLE_PROCESSED_INPUT so Ctrl+C still routes through CancelKeyPress
                // instead of arriving as a 0x03 byte. Enable VT input so the terminal's
                // xterm-mode escape emissions reach us as raw bytes.
                var newMode = (mode & ~(ENABLE_LINE_INPUT | ENABLE_ECHO_INPUT |
                                        ENABLE_WINDOW_INPUT | ENABLE_MOUSE_INPUT))
                            | ENABLE_VIRTUAL_TERMINAL_INPUT;

                if (SetConsoleMode(handle, newMode))
                    _modeChanged = true;
            }
            catch { /* mouse capture is best-effort */ }
        }
        else
        {
            // POSIX: shell out to stty to put the terminal in raw mode (no canonical /
            // line buffering, no echo). Without this, Console.OpenStandardInput().Read
            // would block until a newline arrives — the terminal's xterm SGR escape
            // sequences would never reach our reader because they don't include \n.
            // `isig` keeps Ctrl+C firing SIGINT (which CancelKeyPress hooks). On exit we
            // run `stty sane` to restore reasonable defaults — not necessarily the user's
            // exact prior state, but "sane" is close enough for a terminal that was just
            // a normal interactive shell.
            _modeChanged = TryRunStty("raw -echo isig");
        }
    }

    private void TryRestoreConsoleMode()
    {
        if (!_modeChanged) return;
        if (OperatingSystem.IsWindows())
        {
            try
            {
                var handle = GetStdHandle(STD_INPUT_HANDLE);
                if (handle != IntPtr.Zero) SetConsoleMode(handle, _originalConsoleMode);
            }
            catch { }
        }
        else
        {
            _ = TryRunStty("sane");
        }
        _modeChanged = false;
    }

    private static bool TryRunStty(string args)
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "stty",
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            using var p = System.Diagnostics.Process.Start(psi);
            if (p is null) return false;
            return p.WaitForExit(1000) && p.ExitCode == 0;
        }
        catch { return false; }
    }
}
