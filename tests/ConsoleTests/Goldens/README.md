# Goldens

Each `.txt` file is the ANSI-encoded output for one chart configuration in
`Goldens/GoldenChartTests.cs`, produced in `ConsoleRenderMode.Braille` at
160×96 sub-pixels (= 80 cells × 24 cells).

## Regenerating

When you change rendering output intentionally (axis layout, palette, glyph
choice, …), regenerate goldens by running the test project with
`LVC_GOLDEN_REGEN=1`:

```pwsh
$env:LVC_GOLDEN_REGEN = "1"; dotnet test tests/ConsoleTests/; Remove-Item Env:LVC_GOLDEN_REGEN
```

```bash
LVC_GOLDEN_REGEN=1 dotnet test tests/ConsoleTests/
```

The regen pass writes new `.txt` files into the source tree (this folder) and
reports each test as `Inconclusive` so the suite is only "green" against
committed goldens. After regen, review the diff and commit.

## Failures

A normal mismatch writes a `<name>.txt.actual` next to the golden so you can
diff with whatever tool you prefer. Delete the `.actual` file once resolved.
