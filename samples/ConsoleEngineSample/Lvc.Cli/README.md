# lvc — LiveCharts in your terminal

Render LiveCharts charts in the terminal from a JSON spec. Designed for
shell pipelines and AI-driven clients (Claude Code, MCP-aware agents,
anything that can shell out) that want animated, interactive-quality
charts without integrating the .NET library directly.

> **Status:** experimental. The console rendering backend is part of the
> LiveCharts2 console-renderer experiment and the JSON schema may shift
> before 1.0.

## Install

```sh
dotnet tool install -g LiveChartsCore.Console.Cli
```

Requires the .NET 8 runtime. After install, `lvc` is on your PATH.

### Optional: install the Claude Code skill

`lvc` ships with a Claude Code skill (`render-chart`) that teaches Claude when
to use inline plain-mode rendering versus spawning an interactive window. Drop
it into your home `~/.claude/skills/` with:

```sh
lvc --install-skill
```

This writes `~/.claude/skills/render-chart/SKILL.md`. Re-run after
`dotnet tool update -g LiveChartsCore.Console.Cli` to pick up any skill
revisions in the new version. Honors `$CLAUDE_HOME` if set.

To remove the skill (e.g., before `dotnet tool uninstall -g`):

```sh
lvc --uninstall-skill
```

No-op if the skill isn't installed; removes the parent directory only if
it's empty so user-authored files in `~/.claude/skills/render-chart/` are
left alone.

For GitHub Copilot users: there's no equivalent skill format; paraphrase
the relevant decision tree from the SKILL.md into your project's
`.github/copilot-instructions.md` by hand.

## Usage

```sh
echo '{"kind":"line","series":[{"values":[1,2,3,4,3,2,1]}]}' | lvc

lvc --json '{"kind":"pie","title":"Slices","series":[{"name":"A","values":[1]},{"name":"B","values":[3]}]}'

lvc --file chart.json --mode sixel
```

Render mode auto-detects: Sixel if the terminal advertises it via DA1
(modern xterm, WezTerm, foot, mintty 3+, mlterm, Black Box, Konsole 25.04+,
Windows Terminal 1.22+), otherwise Braille — which renders on any UTF-8
terminal. Override with `--mode halfblock|braille|sixel|auto`.

Width / height in cells default to the current terminal; override with
`--width N` / `--height N` (or set them in the JSON spec).

### `--no-color` for LLM / agent consumers

Pass `--no-color` (or set the `NO_COLOR` env var) to emit plain Braille /
half-block glyphs with no ANSI escapes. Useful when stdout is read by
something that doesn't paint terminal control sequences — an LLM tool
result, a JSON log line, a code-review snapshot:

```sh
lvc --json '{"kind":"line","series":[{"values":[1,2,3,4,3,2,1]}]}' --no-color
```

`--no-color` implies Braille (or honors `--mode halfblock` if given);
combining it with `--mode sixel` is an error since Sixel encodes pixels
into escape sequences and has no plain form.

## JSON spec

```json
{
  "kind": "line",
  "title": "optional",
  "width": 80,
  "height": 20,
  "series": [
    {
      "name": "Signal",
      "values": [1, 2, 3, 4, 3, 2, 1],
      "points": [[0, 1], [1, 2]]
    }
  ],
  "xAxis": { "name": "X", "labels": ["a", "b", "c"] },
  "yAxis": { "name": "Y" }
}
```

`kind` accepts: `line`, `column`, `row`, `step`, `scatter`,
`stackedcolumn`, `stackedrow`, `stackedarea`, `candlestick`, `box`,
`pie`, `polar`.

`values` is used by line / column / row / step / stacked / pie / polar
series; `points` is used by scatter (`[x, y]`), candlestick
(`[open, high, close, low]`), and box (`[max, q3, q1, median, min]`).

### `--live` for interactive viewing

Pass `--live` to run an interactive chart loop in a real terminal — mouse hover
shows tooltips, click-and-drag pans, mouse wheel zooms, and `q` / `r` / `+` /
`-` keys quit / reset / zoom. Auto-detects Sixel if the terminal supports it
(falls back to Braille otherwise). Ctrl+C also exits.

```sh
lvc --json '{"kind":"line","series":[{"values":[1,2,3,4,3,2,1]}]}' --live
```

`--live` is incompatible with `--no-color` (the loop relies on ANSI escapes to
redraw frames in place) and requires a TTY (won't run with stdout redirected).

To pop a chart into a fresh terminal window from a script or agent, shell out
through your platform's terminal-launcher:

- Windows Terminal: `start wt.exe -- lvc --live --json '...'`
- macOS Terminal:  `osascript -e 'tell app "Terminal" to do script "lvc --live --json ..."'`
- GNOME Terminal:  `gnome-terminal -- lvc --live --json '...'`

### Pie sizing

Pie charts render each series name as a label outside its wedge and hide the
legend, so wedges read as distinct even in `--no-color`. Outer labels need
margin space around the disc — pies need at minimum `--width 80 --height 30`
(or terminal equivalent). Smaller canvases will collide the labels with the
disc and squeeze it to a sliver.

## License

MIT. See the [LiveCharts2 repo](https://github.com/Live-Charts/LiveCharts2).
