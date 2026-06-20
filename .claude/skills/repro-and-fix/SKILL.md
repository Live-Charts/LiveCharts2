---
name: repro-and-fix
description: Use when the user asks to investigate, reproduce, or fix a GitHub issue (e.g. "let's work on issue #1234", a github.com/Live-Charts/LiveCharts2/issues/N URL, or "reproduce and propose a fix"). Implements the LiveCharts workflow end-to-end — worktree, in-sample repro view, auto-launched sample, screenshot/log verification, diagnosis, fix, separate regression-test commit, and PR — with minimal user interaction.
---

# repro-and-fix

A user pointed you at a GitHub issue and asked you to reproduce + fix it. Run
the workflow below. The whole point is that the dev loop is *self-contained*:
you build the repro, launch the sample at that repro automatically, verify the
bug yourself (screenshot or logs), fix it, verify the fix, and commit. Don't
ask the user for visual confirmations you can get yourself.

> **Shared with Copilot Coding Agent.** `.github/copilot-instructions.md` links
> here as the canonical issue → reproduce → fix → PR workflow. Edit this file
> when changing the workflow; keep the Copilot doc as a thin pointer to avoid
> drift.

## When this skill applies

- "Let's work on https://github.com/Live-Charts/LiveCharts2/issues/N"
- "Reproduce issue #N and propose a fix"
- "Investigate this bug report: <URL>"
- A user paste of an issue URL with no other instruction

If the user is asking about *theory* ("why does the gauge use a doughnut
geometry?") or a *non-issue* task (refactor, doc update, perf), this skill is
not the right tool — fall back to general workflow.

## Pick the platform that matches the issue

Read the issue's **labels** and **repro snippet** to choose. The workflow is
the same on every XAML platform — only paths and launch commands change.

| Platform | Sample dir                    | Launch project (Debug build)                                                | In-app screenshot |
| -------- | ----------------------------- | --------------------------------------------------------------------------- | ----------------- |
| Avalonia | `samples/AvaloniaSample/`     | `samples/AvaloniaSample/Platforms/AvaloniaSample.Desktop/.../*.Desktop.exe` | ✅ supported       |
| WPF      | `samples/WPFSample/`          | `samples/WPFSample/.../WPFSample.exe`                                       | ✅ supported       |
| WinUI    | `samples/WinUISample/WinUISample/` | `samples/WinUISample/WinUISample/.../WinUISample.exe`                  | ✅ supported       |
| MAUI     | `samples/MauiSample/`         | per-platform; `dotnet build -t:Run -f net10.0-windows10.0.19041.0`          | ❌ use OS tool     |
| Uno      | `samples/UnoPlatformSample/UnoPlatformSample/` | per-platform                                              | ❌ use OS tool     |

If the issue is platform-agnostic (a core engine bug), prefer Avalonia for the
fastest dev loop on Windows (no WindowsAppSDK / packaged-build overhead).

## Project conventions you must follow

These are durable rules from `~/.claude/projects/.../memory/MEMORY.md`. Re-read
the relevant memory file if uncertain — these are not optional:

- **Worktree per issue.** Create `../LiveCharts2-fix-<issue>-<slug>` first
  (`feedback_worktree_per_issue`). Don't work in the primary tree.
- **One concept per commit.** Fix and regression test go in *separate* commits.
  The regression-test commit must fail when the fix is reverted — verify this
  before pushing (`feedback_commit_separation`).
- **Snapshot tests for render-path changes.** If your fix touches paint, layout,
  z-index, or draw order, run `tests/SnapshotTests/` before claiming done — unit
  tests have missed catastrophic regressions
  (`feedback_run_snapshots_for_render_changes`).
- **Update docs when public API changes** in the same change set as a separate
  commit (`feedback_update_docs_for_api`).
- Check `Directory.Build.props` for `<LiveChartsVersion>` and use that value in
  user-facing text (`project_current_version`).

## Workflow

### 1. Read the issue and decide if it still reproduces

```bash
gh issue view <N> --repo Live-Charts/LiveCharts2 --json title,body,labels,state,comments
```

Pay attention to **comments**, not just the body. Issues commonly evolve:

- The original symptom may already be fixed in main (commenter says "newer
  releases fixed this") and the *current* bug is a downstream observation.
  This is exactly what happened with #2008 — the `GaugeValue` TemplateBinding
  bug in the title was already resolved; the live bug was a `CornerRadius=0`
  override in the comments.
- Multiple users may report different things in the same thread. Identify the
  most recent reporter's actual symptom.

If you suspect the title bug is already fixed: build the original repro
verbatim and verify *first*, before doing diagnosis. Cite the commenter who
said it's fixed in your PR description.

### 2. Set up the worktree

```bash
git worktree add ../LiveCharts2-fix-<issue>-<short-slug> -b fix/issue-<N>-<short-slug>
```

All edits, builds, and runs happen in the worktree. The primary tree stays
clean for other work.

### 3. Build the repro view (XAML platforms)

The repro convention is a **sample view inside the platform sample app**:

```
samples/<Platform>Sample/VisualTest/Issue<N>Repro/View.<axaml|xaml>
samples/<Platform>Sample/VisualTest/Issue<N>Repro/View.<axaml|xaml>.cs
```

Match the issue's repro setup as closely as you can — same `ControlTemplate`
shape, same property values, same paints. *Don't simplify* until you've
confirmed the bug fires; over-minimization can accidentally avoid the trigger.

Existing repros to mirror:

- `samples/AvaloniaSample/VisualTest/Issue1986Repro/` — TabControl + ScrollViewer
- `samples/AvaloniaSample/VisualTest/Issue1417Repro/` — GeoMap reattach

The View's code-behind should expose helpers that a Factos test can call
(e.g. `FindTemplatedGaugeSeries()`). This is how the regression test hooks in
without going through the visual tree.

**Do not commit changes to `samples/ViewModelsSamples/Index.cs` for repro
views.** Factos navigates to a sample by path via `LVC_SAMPLE` regardless of
whether the path is listed in the index, and the index is shared across every
platform sample. If you add `VisualTest/Issue<N>Repro` there and the view only
exists in `samples/AvaloniaSample/`, every other platform sample (WPF, MAUI,
WinUI, Uno) will throw when it loads the index. Leave the file untouched on
your branch.

If it's genuinely easier for your *local* dev loop to pick the repro from the
sample selector dropdown rather than via `LVC_SAMPLE`, edit `Index.cs`
temporarily — but revert it before committing. `git restore
samples/ViewModelsSamples/Index.cs` before each commit, or just don't stage
the file.

### 4. Auto-launch the sample at your repro

All XAML platform samples read `LVC_SAMPLE` at startup and load that sample
instead of the default landing screen. Set it before launching:

```bash
# bash / WSL
LVC_SAMPLE=VisualTest/Issue<N>Repro \
  ./samples/AvaloniaSample/Platforms/AvaloniaSample.Desktop/bin/Debug/net10.0/AvaloniaSample.Desktop.exe
```

```powershell
# PowerShell
$env:LVC_SAMPLE = "VisualTest/Issue<N>Repro"
.\samples\WPFSample\bin\Debug\net10.0-windows\WPFSample.exe
```

Build first if needed: `dotnet build samples/<Platform>Sample/... -c Debug`.

### 5. Verify the bug yourself

Two paths — pick based on the bug type:

**A. In-app screenshot for visual bugs** (color, shape, position, rendering
artifacts). Avalonia/WPF/WinUI samples support `LVC_SCREENSHOT`: when set,
the sample renders its main window via the framework's native
`RenderTargetBitmap` after a short delay (default 3 s, override with
`LVC_SCREENSHOT_DELAY_MS`) and exits. No external tooling, no race against
a screenshot tool launching too early. The capture is scaled to the
window's physical pixels (`RenderScaling` on Avalonia,
`VisualTreeHelper.GetDpi` on WPF) so HiDPI displays produce a sharp PNG
that matches what's on screen:

```bash
LVC_SAMPLE=VisualTest/Issue<N>Repro \
LVC_SCREENSHOT=$(pwd)/.claude/screenshots/<N>-before.png \
  ./samples/AvaloniaSample/Platforms/AvaloniaSample.Desktop/bin/Debug/net10.0/AvaloniaSample.Desktop.exe
```

```powershell
# PowerShell
$env:LVC_SAMPLE = "VisualTest/Issue<N>Repro"
$env:LVC_SCREENSHOT = "$pwd\.claude\screenshots\<N>-before.png"
.\samples\WPFSample\bin\Debug\net10.0-windows\WPFSample.exe
```

After it exits, `Read` the PNG. You're multimodal — describe what you see,
compare to a control sample (often `Pies/Gauge1` or `General/FirstChart` for
"what should this look like"). If the bug is subtle (a few pixels), make the
repro larger (`Width=380`, `Height=380`) so it's clearly visible.

**Fallback for MAUI/Uno or other-OS edge cases:** use the OS-appropriate
helper script. They all take `<ProcessName> <OutPath> [WaitSeconds]` and
poll for the target window before capturing:

| OS      | Script                                       | Underlying tool                            |
| ------- | -------------------------------------------- | ------------------------------------------ |
| Windows | `.claude/scripts/capture-window.ps1`         | `PrintWindow` via Win32 (works occluded)   |
| macOS   | `.claude/scripts/capture-window-macos.sh`    | `osascript` + `screencapture -l <id>`      |
| Linux   | `.claude/scripts/capture-window-linux.sh`    | `xdotool`+`import` / `swaymsg`+`grim` / `gnome-screenshot` |

The Linux script tries X11 → sway → GNOME in priority order; install the
matching package if none are present (the script prints the apt/dnf hint).
Use these only when `LVC_SCREENSHOT` isn't supported on the target platform
(MAUI, Uno) — for Avalonia/WPF/WinUI prefer in-app capture.

**B. Console logs for state/dataflow bugs** (binding doesn't fire, value
doesn't propagate, event order is wrong):

Add temporary `Console.WriteLine` calls along the suspected path:

```csharp
Console.WriteLine($"[issue<N>] OnPropertyChanged: {change.Property.Name} old={change.OldValue} new={change.NewValue}");
```

The platform sample writes to stdout when launched from the terminal. Run via
the bash tool with `run_in_background=true` (no `LVC_SCREENSHOT` so the app
stays open), wait a few seconds, kill the app, then `grep` the captured output
file. **Remove the diagnostics before committing.**

If neither approach decisively confirms the bug, escalate to the user with
**one** well-framed question, not a chain of polling questions. Example:

> "The before-fix capture shows the gauge endcaps as flat, contrary to the
> issue's screenshot. Did you toggle CornerRadius after the screenshot was
> taken in the original report?"

### 6. Diagnose the root cause

Trace from symptom backward through the rendering/data pipeline. Cross-check
against `MEMORY.md` — many bugs in this repo have related ancestors
(z-index issues, theme overrides, motion-property defaults, source-gen
behavior). If you find a matching memory note, use its diagnosis as a starting
point but verify against current code; memories go stale.

Common gotchas in this codebase:

- **Theme overrides user-set values** when the user's value equals the DP
  default — Avalonia skips `OnPropertyChanged` and `_userSets` never gets
  populated. `XamlGaugeSeries.EndInit` syncs IsSet DPs as a remediation; if
  you see this pattern in another wrapper, the same fix shape applies.
- **Series controls aren't in the visual tree.** `OnInitialized` and
  `TemplatedParent` may not fire/be set the way you'd expect. Use `EndInit`
  instead.
- **Theme rules run from the chart engine measure pass**, not at construction.
  `_isInternalSet` + `_userSets` is the existing precedence mechanism — don't
  reinvent it.
- **Motion properties animate from a stale "from" value.** A value flip from
  N→0 may render N for one frame.

### 7. Write the regression test first

Test-first: by the end of step 6 you understand the root cause and the
observable that would catch a regression. Write the test now, *before*
applying the fix, and run it — it must fail for the right reason. A test
written after the fix risks passing against both versions of the code; a
test that fails first proves it's gated on the broken behavior, and you
skip the awkward "comment out the fix, rebuild, re-run" verification dance.

Test choice depends on the bug nature:

- **Programmatic state assertion** (the bug is in state propagation, not
  pixels): Factos test in `tests/SharedUITests/`. Navigate to the repro,
  query a helper method on the View that exposes the state under test,
  assert. Gate by `#if <PLATFORM>_UI_TESTING` if platform-specific (most
  XAML-platform-quirk bugs are).
- **Pixel-perfect rendering**: snapshot test in `tests/SnapshotTests/`.
- **Animation trajectory** (the bug is in per-frame interpolation, not the
  final value — a bar pops to target instead of growing, a slice doesn't
  sweep, a transition runs at the wrong speed, a refactor risks changing the
  motion shape of an existing series): JSON animation baseline under
  `tests/CoreTests/AnimationBaselines/` driven by
  `tests/CoreTests/Helpers/SeriesAnimationCapture.cs`. The helper steps
  `CoreMotionCanvas.DebugElapsedMilliseconds`, captures the visual's geometry
  at each sample timestamp, and `AssertTrajectoryMatches` compares the
  trajectory to a committed `.json`. First run writes the new trajectory to
  `AnimationBaselinesNew/` and fails with a "review and commit" message —
  inspect the file, then move it into `AnimationBaselines/`. Re-baseline the
  same way: delete the JSON, re-run, move from `New/` back. On mismatch the
  helper also drops `[EXPECTED]`/`[RESULT]` files into
  `AnimationBaselinesDiff/` so you can diff them directly. Caller must use
  `EasingFunctions.Lineal` + fixed `MinLimit`/`MaxLimit` so interpolation is
  deterministic; the existing `tests/CoreTests/SeriesTests/*AnimationTests.cs`
  files are the templates. Prefer this over a plain MSTest whenever the
  observable is "how the value changes between two states" — a single
  before/after assertion can't catch a regression that only mis-paints the
  intermediate frames.
- **Pure C# logic** (chart engine, motion, math): MSTest in
  `tests/CoreTests/`.

Run the test and confirm the failure message matches the symptom you saw in
step 5 — not, for example, a setup error or NullRef from missing test
infrastructure. If it fails for an unrelated reason, the test isn't yet
catching the bug; fix the test before moving on.

To run a single Factos UI test:

```bash
# In tests/UITests/Program.cs, set: var appToRun = "<platform>-desktop"
# (avalonia-desktop / wpf-net10 / winui / maui / uno).
dotnet build tests/UITests/UITests.csproj -c Debug
cd tests/UITests/bin/Debug/net10.0 && dotnet UITests.dll
# Revert the appToRun change before committing — it's a local dev-loop tweak.
```

### 8. Apply the fix

Smallest possible change. Don't refactor surrounding code. Don't add
comments unless the WHY is non-obvious — but if the bug is a subtle
interaction (Avalonia behavior, theme precedence, source-gen quirk), a comment
explaining *why this fix is necessary* is warranted because future readers
won't reconstruct it from memory.

### 9. Verify the fix

Re-run the failing test from step 7 — it must now pass. Then re-run step 5
with the fix in place to capture an "after" screenshot at a sibling path and
compare visually:

```bash
LVC_SAMPLE=VisualTest/Issue<N>Repro \
LVC_SCREENSHOT=$(pwd)/.claude/screenshots/<N>-after.png \
  ./samples/<Platform>Sample/.../<Platform>Sample.exe
```

`Read` both images. Don't claim the fix works without comparing. The test
passing is necessary but not sufficient for visual bugs — pixel-equivalent
output can still look subtly wrong if the assertion was too narrow.

### 10. Commit and PR

You developed test-first, but commit fix-first. Two commits, in this order:

1. `fix(<scope>): <one-line summary> (#<N>)` — only the fix.
2. `test(<scope>): regression for <symptom> (#<N>)` — only the test +
   repro view. Do not stage `samples/ViewModelsSamples/Index.cs`; see step 3
   for why.

Why fix-first commit order despite test-first development: every commit on
the branch should be individually green so `git bisect` lands on a usable
state. If the test commit landed first it would be a failing-test commit
that bisect would (correctly) flag as broken. Fix-then-test keeps both
commits green while still satisfying the project rule that reverting the
fix must surface the regression — revert commit 1 alone and the test from
commit 2 fails.

Commit message style follows recent history (`git log --oneline -10`).
Body: imperative, mention the root-cause mechanism, why the test catches it.
End with the standard `Co-Authored-By` line.

Push and open the PR with `gh pr create`. The PR body should include:

- **Summary** bullet list (what changed, what it closes)
- **Why** section (root cause walkthrough — the most valuable part)
- **Test plan** checklist with what you actually verified locally

### 11. Update memory

After the PR is open, write a `project_issue_<N>.md` memory and add a one-line
entry to `MEMORY.md`. Capture: **what was non-obvious** about the bug, what
diagnosis steps led to the fix, and what conventions/files the next person
needs. Don't re-document the code — just the insights.

## Anti-patterns to avoid

- **Asking the user "is the bug visible now?" repeatedly.** Capture a
  screenshot or read logs. If you can't decide from one capture + one
  question, you haven't framed the question precisely enough.
- **Simplifying the repro before confirming.** A "minimal" repro that doesn't
  trigger the bug is worse than a verbose one that does.
- **Writing the test after the fix.** Develop test-first (step 7 before
  step 8) so the failure is real and you don't have to comment out the fix
  to verify the test catches anything. A test authored against
  already-fixed code can pass for the wrong reason.
- **Mixing fix + test + cleanup in one commit.** Project policy is strict
  here — every fix needs a separately revertible test commit.
- **Force-pushing for "cleanliness".** Never. Stack new commits.
- **Trusting memory over code.** Memory files name specific functions and
  files; verify they still exist before recommending — projects refactor.

## Tooling reference

- Issue triage: `gh issue view <N> --repo Live-Charts/LiveCharts2`
- **Auto-launch sample**: `LVC_SAMPLE=<path>` env var (Avalonia, WPF, WinUI,
  MAUI, Uno).
- **In-app screenshot**: `LVC_SCREENSHOT=<path>` env var (Avalonia, WPF,
  WinUI). Sample renders to PNG after `LVC_SCREENSHOT_DELAY_MS` (default
  3000) and exits.
- **External screenshot fallback** — one script per OS, all take the same
  `<ProcessName> <OutPath> [WaitSeconds]` shape:
  - Windows: `.claude/scripts/capture-window.ps1` (also accepts
    `-WindowTitle <substring>` instead of `-ProcessName`)
  - macOS:   `.claude/scripts/capture-window-macos.sh`
  - Linux:   `.claude/scripts/capture-window-linux.sh` (X11 / sway / GNOME)
- Factos UI tests: `tests/UITests/`, set `appToRun` in `Program.cs` for
  local runs.
- Build per-platform: `LiveCharts.<Platform>.slnx` solutions.
