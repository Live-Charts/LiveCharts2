---
name: render-chart
description: Use when the user asks to render, visualize, plot, or "show me a chart of" any kind of series data (line, column, row, scatter, stacked, candlestick, box, pie, polar, heat). Renders via the `lvc` CLI from a JSON spec — inline in your reply for simple charts you need to read, or in a new terminal window for interactive / color-dependent ones (heat maps, pies with many slices, anything with > 3 series the user wants to explore).
---

# render-chart

The user wants to *see* data as a chart. You have access to a renderer (`lvc`)
that writes LiveCharts to a terminal. Pick the right mode for the situation,
install the tool if it's missing, and either inline the result in your reply
or hand the user a live window.

## Decision: inline vs live window

| Situation | Mode | Why |
| --- | --- | --- |
| ≤ 3 series, line / column / row / stacked-bar / scatter, you need to *verify* the shape | `--no-color` inline | Plain Braille glyphs render in a fenced code block. You can read it; the user sees it. |
| User said "open this in a new window" / "let me play with it" / "zoom into this" | `--live` in a new terminal | Interactive Sixel + mouse hover + wheel zoom + click-drag pan. The user explores; you can't read the result, so don't pretend to. |
| Heat map | `--live` in a new terminal | Heat reads the gradient by color. Strip color and it's an undifferentiated blob. |
| Pie with > 4 slices | `--live` in a new terminal | Outer labels collide and Pushout gaps stop reading at small canvas sizes. |
| Anything where the *meaning* depends on color (gradient encodes magnitude, similar curves only separable by hue) | `--live` in a new terminal | Plain mode can't carry color information — see [[reference_claude_code_cli_ansi_passthrough]]. |
| User asks to "compare these two/three series" with simple shapes | `--no-color` inline | Single bar series stays Braille; multi-bar uses textures; line series get end-of-line labels. Distinguishable without color. |

**Default to `--no-color` inline** when you can read the chart — it keeps the
conversation flowing and you can describe what's in it. Reserve `--live` for
when there's genuinely no way to convey the information in monochrome glyphs.

## Step 1 — make sure dotnet is installed

```sh
dotnet --version
```

If that errors, the user needs the .NET 8 SDK or runtime. Point them at
<https://dotnet.microsoft.com/download> and stop — don't try to install it
for them (admin install, varies per OS).

## Step 2 — install `lvc` as a global tool (once)

```sh
dotnet tool install -g LiveChartsCore.Console.Cli
```

If they already have it, this errors with "already installed" — that's fine.
Update with `dotnet tool update -g LiveChartsCore.Console.Cli`.

After install, `lvc` is on the user's PATH. Verify with:

```sh
lvc --json '{"kind":"line","series":[{"values":[1,2,3,2,1]}]}' --no-color
```

You should see a small Braille line chart in stdout.

## Step 3 — write the JSON spec

Pass via `--json '<spec>'`, `--file path.json`, or pipe stdin.

```json
{
  "kind": "line",
  "title": "optional",
  "width": 70,
  "height": 20,
  "series": [
    {
      "name": "Signal",
      "values": [1, 2, 3, 4],
      "strokeThickness": 2
    }
  ],
  "xAxis": { "name": "X", "labels": ["a", "b", "c", "d"] },
  "yAxis": { "name": "Y" }
}
```

`kind` accepts: `line`, `column`, `row`, `step`, `scatter`, `stackedcolumn`,
`stackedrow`, `stackedarea`, `candlestick`, `box`, `pie`, `polar`.

Series shape:
- `values: number[]` — line / column / row / step / stacked / pie / polar
- `points: number[][]` — scatter (`[x, y]`) / candlestick (`[open, high, close, low]`) / box (`[max, q3, q1, median, min]`)
- `strokeThickness: number` — line / step (optional, default 1)

For pie: needs `--width ≥ 80 --height ≥ 30` (outer labels collide otherwise).
For stacked column / row with 2+ series: per-series textures auto-applied in
plain mode; legend shows the texture next to each series name.

## Step 4a — inline rendering (the common path)

```sh
lvc --json '<spec>' --no-color --width 70 --height 20
```

Capture the output and paste it back inside a fenced code block in your reply.
CommonMark renders it monospace, the user sees the same glyphs you see.

Sizing: ~70×20 is a sweet spot. Smaller charts get cramped; larger ones might
wrap weirdly in the user's terminal. Default to 70×20 unless the user asked
otherwise.

Read what you rendered. Describe what's in it — "the blue series climbs from
~5 to ~12, the red oscillates around 8" — so the user can sanity-check it
matches what they wanted. Don't claim "the chart looks correct" without
naming the actual shapes you see.

## Step 4b — live window (interactive / color path)

Spawn a new terminal so it doesn't block your conversation:

**Windows (Windows Terminal):**
```pwsh
Start-Process wt.exe -ArgumentList '-d', $pwd.Path, 'pwsh', '-NoExit', '-NoLogo', '-Command', 'lvc --live --file demo.json'
```

**macOS (Terminal.app):**
```sh
osascript -e "tell app \"Terminal\" to do script \"lvc --live --file $PWD/demo.json\""
```

**Linux (GNOME Terminal):**
```sh
gnome-terminal --working-directory="$PWD" -- lvc --live --file demo.json
```

For multi-line JSON, write a `demo.json` file first via the Write tool and
pass `--file` — avoids cross-shell quoting hell.

**Critical:** you can't read the live window's contents. After launching,
tell the user "I opened a chart in a new window — what do you see?" and
iterate from their feedback. Don't fabricate a description of what's
"probably there."

In the live window the user gets: mouse hover (tooltips), click-and-drag (pan),
mouse wheel (zoom), `q` (quit), `r` (reset zoom), `+` / `-` (zoom at center).

## Common pitfalls

- **`--live --no-color`** is an error: live mode redraws frames in place via
  ANSI escapes, which is exactly what `--no-color` strips. Pick one.
- **`--live` with stdout redirected** errors out — it needs a real TTY. Always
  spawn a new terminal window, never pipe `--live` output.
- **`--mode sixel --no-color`** errors out — Sixel is a binary escape sequence
  with no plain form. Drop `--mode sixel` or drop `--no-color`.
- **Pie charts in plain mode at small canvases** collapse the disc to a sliver
  because outer labels reserve margin space. Use `--width 80 --height 30`
  minimum, or accept a smaller render in `--live` mode where the disc has
  full canvas.
- **Stacked bar plain mode with 1 series** keeps Braille `⣿` fills (no
  textures kick in until 2+ bar series exist). This is intentional.
- **You cannot see colors in the user's terminal from your own output.** Inline
  ANSI escapes you write get collapsed to one inline-code style by the
  CommonMark renderer — see [[reference_claude_code_cli_ansi_passthrough]].
  This is why `--no-color` is the inline path: plain glyphs don't pretend to
  carry color information.

## When NOT to use this skill

- The user asks to *edit chart configuration* in a LiveCharts app (XAML /
  source-code view) — that's a code-edit task, not a rendering task.
- The user wants to *understand* a chart they already have a screenshot of —
  use vision on the image, not lvc.
- The data is structural / hierarchical (trees, graphs, org charts) — lvc
  only handles statistical chart kinds.
