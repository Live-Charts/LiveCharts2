# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

LiveCharts2 is a cross-platform .NET charting library with a layered architecture:
- **`src/LiveChartsCore/`** — Platform-agnostic core (math, series, axes, animation). No UI dependencies. Targets `net462`, `netstandard2.0`, `net8.0`, `net8.0-windows`.
- **`src/skiasharp/LiveChartsCore.SkiaSharp/`** — SkiaSharp rendering backend implementing the core drawing abstractions.
- **`src/skiasharp/LiveChartsCore.SkiaSharp.{Platform}/`** — Platform-specific view controls (WPF, Avalonia, MAUI, Blazor, WinForms, WinUI, Eto, UNO).
- **`generators/LiveChartsGenerators/`** — Roslyn source generator for boilerplate reduction.

## Build Commands

```bash
# Core library (no workloads needed)
dotnet build src/LiveChartsCore/LiveChartsCore.csproj
dotnet build src/skiasharp/LiveChartsCore.SkiaSharp/LiveChartsCore.SkiaSharpView.csproj

# Platform-specific (examples)
dotnet build src/skiasharp/LiveChartsCore.SkiaSharp.WPF/LiveChartsCore.SkiaSharpView.Wpf.csproj
dotnet build src/skiasharp/LiveChartsCore.SkiaSharp.Avalonia/LiveChartsCore.SkiaSharpView.Avalonia.csproj

# Platform-specific solution files
dotnet build LiveCharts.WPF.slnx
dotnet build LiveCharts.Avalonia.slnx
dotnet build LiveCharts.Maui.slnx

# Target a specific framework when multi-targeting causes issues
dotnet build -f net8.0
```

MAUI/WASM projects require workloads: `dotnet workload install maui --skip-sign-check` / `dotnet workload install wasm-tools --skip-sign-check`. Core and desktop projects (WPF, Avalonia, WinForms) do not.

## Testing

```bash
# Unit tests (MSTest) — primary test suite
dotnet test tests/CoreTests/ --framework net8.0

# Snapshot tests (image comparison, net10.0 only)
dotnet test tests/SnapshotTests/

# UI tests via Factos (requires sample apps built)
dotnet run --project tests/UITests/
dotnet run --project tests/UITests/ -- --select wpf
dotnet run --project tests/UITests/ -- --select avalonia-desktop
```

Tests use `CoreMotionCanvas.IsTesting = true` to disable animations. UI tests are defined in `tests/SharedUITests/` (shared project) and run against each platform via the Factos orchestrator in `tests/UITests/`.

## Running Samples

```bash
dotnet run --project samples/WPFSample/WPFSample.csproj
dotnet run --project samples/AvaloniaSample/AvaloniaSample.csproj
dotnet run --project samples/ConsoleSample/ConsoleSample.csproj
```

Sample ViewModels live in `samples/ViewModelsSamples/` and are shared across all platform samples. Each platform sample creates its own views. `samples/ViewModelsSamples/Index.cs` lists all available sample paths.

## Architecture Details

**Rendering pipeline**: Data → Core series engine (measurement, layout) → SkiaSharp drawables → Platform-native surface. The core is rendering-agnostic — `samples/VorticeSample/` demonstrates using DirectX instead of SkiaSharp.

**Shared projects**: `src/skiasharp/_Shared/`, `_Shared.Xaml/`, `_Shared.WinUI/` contain code shared across platform views via MSBuild linked files (configured in `build/*.Build.props`).

**Code generation**: `LiveChartsGenerators` is a Roslyn analyzer/generator. Controlled by `UseNuGetForGenerator` in `Directory.Build.props` (default: `true` = NuGet package).

**Key build properties** (`Directory.Build.props`):
- `UseNuGetForSamples`: `false` during development (project references), `true` for CI/release
- `UITesting`: set to `true` to include shared UI tests in sample projects
- `GPU`, `VSYNC`, `Diagnose`: rendering mode overrides for testing

## Code Style

Based on [.NET Runtime coding style](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md) with these exceptions (enforced via `.editorconfig`):
- `var` is freely used everywhere (not restricted to explicit right-hand types)
- Single-line `if` without braces is preferred when the line is short; break long lines rather than adding braces
- Private fields: `_camelCase`, static private: `s_camelCase`, constants: `PascalCase`
- 4-space indentation, Allman braces, LF line endings

**File naming is critical**: file names must match the class name exactly (`Hello<T>` → `Hello.cs`). Generic and non-generic with the same name go in the same file (only when related by inheritance). This is required for automatic documentation generation.

## Key Constraints

- **Never add platform-specific code to `LiveChartsCore`** — it must remain platform-agnostic
- **.NET Framework 4.6.2 compatibility must be maintained** (strong-named assemblies, `LiveCharts.snk`)
- **SkiaSharp version range**: min 2.88.9, latest 3.119.0 — changes must respect both
- **C# 14.0** language version
- Animation system (`Motion/`) is core infrastructure — changes require extensive testing
- Chart updates can arrive from any thread; synchronization is essential

## Additional Documentation

See `.github/copilot-instructions.md` for extended guidance including: sample platform view patterns (XAML/code-only/Blazor), adding new series types, CI/CD workflow details, and documented build errors with workarounds.
