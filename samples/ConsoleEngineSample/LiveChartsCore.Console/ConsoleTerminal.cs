// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using System.Globalization;
using System.Text.RegularExpressions;
using LiveChartsCore.Drawing;

namespace LiveChartsCore.Console;

/// <summary>
/// Helpers that talk to the host terminal via interactive control sequences. These all assume
/// stdout/stdin are real TTYs; pipe redirection makes them silently fail.
/// </summary>
public static class ConsoleTerminal
{
    /// <summary>
    /// Queries the terminal for its current background color via the OSC 11 sequence
    /// (<c>\x1b]11;?\x07</c>). Returns null if the terminal doesn't respond within
    /// <paramref name="timeoutMs"/>, doesn't support OSC 11 at all, or stdin is redirected.
    /// Modern terminals (Windows Terminal, iTerm2, WezTerm, ghostty, recent VSCode, xterm)
    /// implement this; older or stripped-down terminals don't.
    /// </summary>
    public static LvcColor? TryDetectBackground(int timeoutMs = 100)
    {
        if (System.Console.IsInputRedirected || System.Console.IsOutputRedirected)
            return null;

        try
        {
            // Send the query. BEL terminator (\x07) is more widely understood than ST.
            System.Console.Out.Write("\x1b]11;?\x07");
            System.Console.Out.Flush();

            var collected = new System.Text.StringBuilder();
            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

            while (DateTime.UtcNow < deadline)
            {
                if (System.Console.KeyAvailable)
                {
                    var key = System.Console.ReadKey(intercept: true);
                    var ch = key.KeyChar;
                    _ = collected.Append(ch);
                    if (ch == '\x07') break;
                    var s = collected.ToString();
                    if (s.EndsWith("\x1b\\")) break;
                }
                else
                {
                    Thread.Sleep(5);
                }
            }

            // Response shape: ESC ] 11 ; rgb : RRRR / GGGG / BBBB ( BEL | ESC \ ).
            // Components are 1–4 hex digits each; iTerm2 sends 4, others vary.
            var match = Regex.Match(
                collected.ToString(),
                @"rgb:([0-9a-fA-F]+)/([0-9a-fA-F]+)/([0-9a-fA-F]+)");
            if (!match.Success) return null;

            return new LvcColor(
                ScaleHexComponent(match.Groups[1].Value),
                ScaleHexComponent(match.Groups[2].Value),
                ScaleHexComponent(match.Groups[3].Value));
        }
        catch
        {
            // Any I/O hiccup (no console attached, raw mode failure on weird hosts) → bail.
            return null;
        }
    }

    /// <summary>
    /// Queries the terminal for its character cell size in pixels via the DECRQSS-style
    /// <c>\x1b[16t</c> sequence. Response shape: <c>\x1b[6;height;widtht</c>. Returns null
    /// if the terminal doesn't respond within <paramref name="timeoutMs"/>, doesn't support
    /// the report, or stdin is redirected. Knowing this lets the Sixel renderer size the
    /// image so its height is an exact multiple of the cell pixel height — which means the
    /// image lands on cell boundaries and there's no partial-cell strip below it.
    /// </summary>
    public static (int width, int height)? TryDetectCellPixelSize(int timeoutMs = 100)
    {
        if (System.Console.IsInputRedirected || System.Console.IsOutputRedirected)
            return null;

        try
        {
            System.Console.Out.Write("\x1b[16t");
            System.Console.Out.Flush();

            var collected = new System.Text.StringBuilder();
            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

            while (DateTime.UtcNow < deadline)
            {
                if (System.Console.KeyAvailable)
                {
                    var key = System.Console.ReadKey(intercept: true);
                    var ch = key.KeyChar;
                    _ = collected.Append(ch);
                    if (ch == 't') break;
                }
                else
                {
                    Thread.Sleep(5);
                }
            }

            // Response: ESC [ 6 ; <h> ; <w> t. Some terminals omit the 6; prefix or use
            // different leading codes — match permissively on the trailing two-number tuple.
            var match = Regex.Match(collected.ToString(), @"\[6;(\d+);(\d+)t");
            if (!match.Success) return null;

            var h = int.Parse(match.Groups[1].Value);
            var w = int.Parse(match.Groups[2].Value);
            if (h <= 0 || w <= 0 || h > 200 || w > 200) return null; // sanity clamp.

            return (w, h);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Queries the terminal for its Primary Device Attributes via the DA1 sequence
    /// (<c>\x1b[c</c>) and inspects the capability list for code 4 (Sixel). Modern
    /// terminals that ship with Sixel support — Windows Terminal (recent), iTerm2,
    /// WezTerm, ghostty, mlterm, xterm with --enable-sixel-graphics — advertise it here.
    /// Returns false if the terminal doesn't respond, doesn't include 4, or stdin/stdout
    /// is redirected.
    /// </summary>
    public static bool TryDetectSixelSupport(int timeoutMs = 100)
    {
        if (System.Console.IsInputRedirected || System.Console.IsOutputRedirected)
            return false;

        try
        {
            System.Console.Out.Write("\x1b[c");
            System.Console.Out.Flush();

            var collected = new System.Text.StringBuilder();
            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

            while (DateTime.UtcNow < deadline)
            {
                if (System.Console.KeyAvailable)
                {
                    var key = System.Console.ReadKey(intercept: true);
                    var ch = key.KeyChar;
                    _ = collected.Append(ch);
                    if (ch == 'c') break;
                }
                else
                {
                    Thread.Sleep(5);
                }
            }

            // Response: ESC [ ? <terminal-class> ; <cap1> ; <cap2> ; ... c
            // Sixel capability is code 4. Match the parameter run between ? and c, then
            // split on ';' so we don't accidentally match "14" or "40" as "4".
            var match = Regex.Match(collected.ToString(), @"\[\?([\d;]+)c");
            if (!match.Success) return false;

            foreach (var cap in match.Groups[1].Value.Split(';'))
                if (cap == "4") return true;
            return false;
        }
        catch
        {
            return false;
        }
    }

    private static byte ScaleHexComponent(string hex)
    {
        if (string.IsNullOrEmpty(hex)) return 0;
        var v = int.Parse(hex, NumberStyles.HexNumber);
        var max = (1 << (hex.Length * 4)) - 1;
        return max == 0 ? (byte)0 : (byte)(v * 255 / max);
    }
}
