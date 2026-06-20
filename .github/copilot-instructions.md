# LiveCharts2 Copilot Instructions

This document helps coding agents work efficiently with the LiveCharts2 repository.

## Repository Overview

LiveCharts2 is a flexible, cross-platform charting library for .NET. It follows a layered architecture where:
- **Core library** (`LiveChartsCore`) is platform-agnostic and handles all chart mathematics
- **SkiaSharp backend** renders the charts using SkiaSharp
- **Platform-specific views** provide UI controls for various frameworks (WPF, Avalonia, MAUI, Blazor, etc.)

## Repository Structure

```
LiveCharts2/
├── src/
│   ├── LiveChartsCore/                    # Platform-agnostic core library
│   │   ├── Kernel/                        # Core charting engine
│   │   ├── Drawing/                       # Drawing abstractions
│   │   ├── Motion/                        # Animation system
│   │   ├── Measure/                       # Chart measurement logic
│   │   └── [Series types]/                # Line, Bar, Pie, Scatter, etc.
│   ├── skiasharp/                         # SkiaSharp rendering implementations
│   │   ├── LiveChartsCore.SkiaSharp/      # Core SkiaSharp provider
│   │   ├── LiveChartsCore.SkiaSharp.WPF/
│   │   ├── LiveChartsCore.SkiaSharp.Avalonia/
│   │   ├── LiveChartsCore.SkiaSharpView.Maui/
│   │   ├── LiveChartsCore.SkiaSharpView.Blazor/
│   │   └── [other platforms]/
│   └── _Shared.Native/                    # Native platform interop
├── samples/                               # Sample applications
│   ├── ViewModelsSamples/                 # Shared ViewModels for all samples
│   │   └── Index.cs                       # List of all sample paths
│   ├── WPFSample/
│   ├── AvaloniaSample/
│   ├── MauiSample/
│   ├── VorticeSample/                     # DirectX sample (core without SkiaSharp)
│   └── [other platforms]/
├── tests/
│   ├── CoreTests/                         # Core unit tests using MSTest
│   │   ├── ChartTests/                    # High-level chart tests
│   │   ├── SeriesTests/                   # Series-specific tests
│   │   ├── LayoutTests/                   # Layout tests
│   │   ├── CoreObjectsTests/              # Core objects tests
│   │   └── OtherTests/                    # Axes, events, etc.
│   ├── SnapshotTests/                     # Snapshot/image comparison tests (net10.0)
│   ├── UITests/                           # UI testing orchestrator
│   │   └── Program.cs                     # Factos-based multi-platform test runner
│   └── SharedUITests/                     # Shared UI tests (referenced by sample apps)
│       ├── CartesianChartTests.cs
│       ├── PieChartTests.cs
│       ├── PolarChartTests.cs
│       └── MapChartTests.cs
├── docs/                                  # Documentation (Scriban templates)
│   ├── samples/                           # Sample documentation templates
│   ├── shared/                            # Reusable template fragments
│   ├── cartesianChart/                    # Cartesian chart docs
│   ├── piechart/                          # Pie chart docs
│   └── polarchart/                        # Polar chart docs
└── generators/                            # Code generators
```

## Key Architecture Concepts

### 1. Layered Design
- **LiveChartsCore**: Pure .NET, no UI dependencies, handles all calculations
- **SkiaSharp Provider**: Implements `IDrawingProvider` to render using SkiaSharp
- **Platform Views**: WPF/Avalonia/MAUI/etc. specific controls that host the renderer

### 2. Multi-Platform Targeting

Core projects (`LiveChartsCore`, `LiveChartsCore.SkiaSharp`) target:
- `net462`, `netstandard2.0`, `net8.0`, `net8.0-windows`
- **No mobile workloads required** to build the core library

**Platform-specific view projects** (WPF, Avalonia, MAUI, etc.) have their own target framework requirements based on the platform.

### 3. Sample Structure
- **ViewModelsSamples**: Contains shared ViewModels used across all UI frameworks
- **Index.cs**: Defines available samples as string paths (e.g., "Lines/Basic", "Pies/Doughnut")
- Each platform sample project (WPF, Avalonia, etc.) references ViewModelsSamples and creates platform-specific views

### 4. VorticeSample
A special sample demonstrating how to use LiveChartsCore without SkiaSharp, using DirectX instead. This shows the core library is truly rendering-agnostic.

## Building the Repository

### Prerequisites
- .NET SDK (see `global.json` for minimum version)
- No workloads required for core projects
- Platform-specific projects (MAUI, UNO, Avalonia Browser) require relevant workloads:
  ```bash
  dotnet workload install maui
  dotnet workload install wasm-tools
  ```

### Build Methods

#### Quick Build - Core Projects
```bash
dotnet build src/LiveChartsCore/LiveChartsCore.csproj
dotnet build src/skiasharp/LiveChartsCore.SkiaSharp/LiveChartsCore.SkiaSharpView.csproj
```

#### Quick Build - Platform Views
```bash
# Build specific platform views (recommended for development)
dotnet build src/skiasharp/LiveChartsCore.SkiaSharp.WPF/LiveChartsCore.SkiaSharpView.Wpf.csproj
dotnet build src/skiasharp/LiveChartsCore.SkiaSharp.Avalonia/LiveChartsCore.SkiaSharpView.Avalonia.csproj
```

#### Full Build (Windows)
```bash
# Build platform-specific projects individually
dotnet build src/skiasharp/LiveChartsCore.SkiaSharp.WPF/LiveChartsCore.SkiaSharpView.Wpf.csproj
dotnet build src/skiasharp/LiveChartsCore.SkiaSharp.WinForms/LiveChartsCore.SkiaSharpView.WinForms.csproj
# Or use platform-specific solution files (see below)
```

#### Build with Solution Files
```bash
# Use platform-specific solution files
dotnet build LiveCharts.WPF.slnx
dotnet build LiveCharts.Avalonia.slnx
dotnet build LiveCharts.Maui.slnx
```

### Common Build Issues and Workarounds

#### Issue: Missing workload errors (NETSDK1147)
```
error NETSDK1147: To build this project, the following workloads must be installed: maui
```

**Context**: This error occurs when building platform-specific view projects (MAUI, UNO, Avalonia Browser) that require specific workloads. Core projects (`LiveChartsCore`, `LiveChartsCore.SkiaSharp`) do NOT require workloads.

**Workaround Options:**
1. Install the required workload: `dotnet workload install maui`
2. Build only the platform you need (e.g., WPF or Avalonia desktop on Windows)
3. Use platform-specific solution files that don't include all targets

#### Issue: SkiaSharp version conflicts
The project supports multiple SkiaSharp versions:
- `MinSkiaSharpVersion`: 2.88.9 (minimum supported)
- `LatestSkiaSharpVersion`: 3.119.0 (default for GPU support)

Defined in `Directory.Build.props`.

#### Issue: Multi-targeting complexity
When building fails for specific targets, you can:
1. Use `-f` to target specific framework: `dotnet build -f net8.0`
2. Edit `TargetFrameworks` in .csproj to focus on needed platforms

## Testing

### Unit Tests (Core Library)

**Location**: `tests/CoreTests/`

**Framework**: MSTest with coverlet for code coverage

**Run Tests:**
```bash
dotnet test tests/CoreTests/

# Run for specific framework
dotnet test tests/CoreTests/ --framework net8.0

# Run with coverage
dotnet test tests/CoreTests/ --collect:"XPlat Code Coverage"
```

**Test Structure:**
- `ChartTests/`: High-level chart functionality
- `SeriesTests/`: Tests for Line, Bar, Pie, Scatter, Heat, etc.
- `LayoutTests/`: Stack and table layouts
- `CoreObjectsTests/`: Transitions, colors, labels
- `OtherTests/`: Axes, events, data providers, visual elements
- `MockedObjects/`: Test helpers and mocks
- `TestsInitializer.cs`: MSTest assembly initialization

**Important**: Tests use `CoreMotionCanvas.IsTesting = true` to disable animations during testing.

### Snapshot Tests

**Location**: `tests/SnapshotTests/`

**Framework**: MSTest, targets `net10.0`

Snapshot tests render charts to images and compare them against stored reference snapshots. They are run in CI on Windows.

**Run Tests:**
```bash
dotnet test tests/SnapshotTests/
```

### UI Testing

**Location**: `tests/UITests/` (orchestrator) and `tests/SharedUITests/` (shared tests)

**Framework**: [Factos](https://github.com/beto-rodriguez/Factos) - A multi-platform UI testing framework

**How it works:**
1. Shared UI tests are defined in `tests/SharedUITests/` (shared project)
2. Each sample application references `SharedUITests` 
3. The `tests/UITests/Program.cs` orchestrator:
   - Starts various sample applications (Avalonia, WPF, MAUI, Blazor, etc.)
   - Connects to them via Factos
   - Runs the shared UI tests against each platform
4. Tests ensure charts render correctly across all supported UI frameworks

**Test Coverage:**
- `CartesianChartTests.cs`: Cartesian chart rendering and behavior
- `PieChartTests.cs`: Pie/Doughnut chart tests
- `PolarChartTests.cs`: Polar chart tests  
- `MapChartTests.cs`: Map chart tests
- `AvaloniaTests.cs`: Avalonia-specific tests

**Running UI Tests:**
```bash
# Run UI tests (requires sample apps to be built)
dotnet run --project tests/UITests/

# Run against specific platform (see Program.cs for options)
dotnet run --project tests/UITests/ -- --select wpf
dotnet run --project tests/UITests/ -- --select avalonia-desktop
dotnet run --project tests/UITests/ -- --select maui --test-env "tf=net10.0-windows10.0.19041.0"
```

**Important Notes:**
- UI testing requires the Factos package
- Each platform may need specific prerequisites (emulators for mobile, browsers for web)
- In Debug mode, tests use project references; in Release mode, they use NuGet packages
- The orchestrator supports testing against multiple target frameworks
- Mobile platforms (Android, iOS) require running emulators

**Build Configuration for UI Tests:**
UI test configuration is managed through MSBuild properties. When `UITesting=true` is set, samples include the shared UI test project.

## Running Samples

### Sample Applications
Each platform has its own sample application that references `ViewModelsSamples`:

```bash
# Run WPF sample
dotnet run --project samples/WPFSample/WPFSample.csproj

# Run Avalonia sample
dotnet run --project samples/AvaloniaSample/AvaloniaSample.csproj

# Run Console sample (no UI)
dotnet run --project samples/ConsoleSample/ConsoleSample.csproj
```

### Adding New Samples
1. Add ViewModel class in `samples/ViewModelsSamples/[Category]/[Name].cs`
2. Add path to `samples/ViewModelsSamples/Index.cs`
3. Create platform-specific view files in each sample project (WPF, Avalonia, etc.)

### Sample Platforms Reference

The following platforms each need a view for every sample. **ConsoleSample** and **VorticeSample** are excluded — they don't follow this pattern.

| Platform | Root path | View file(s) | Base class | LVC namespace (xmlns:lvc) |
|---|---|---|---|---|
| **Avalonia** | `samples/AvaloniaSample/[Category]/[Name]/` | `View.axaml` + `View.axaml.cs` | `UserControl` | `using:LiveChartsCore.SkiaSharpView.Avalonia` |
| **WPF** | `samples/WPFSample/[Category]/[Name]/` | `View.xaml` + `View.xaml.cs` | `UserControl` | `clr-namespace:LiveChartsCore.SkiaSharpView.WPF;assembly=LiveChartsCore.SkiaSharpView.WPF` |
| **MAUI** | `samples/MauiSample/[Category]/[Name]/` | `View.xaml` + `View.xaml.cs` | `ContentPage` | `clr-namespace:LiveChartsCore.SkiaSharpView.Maui;assembly=LiveChartsCore.SkiaSharpView.Maui` |
| **WinUI** | `samples/WinUISample/WinUISample/Samples/[Category]/[Name]/` | `View.xaml` + `View.xaml.cs` | `UserControl` (`sealed partial`) | `using:LiveChartsCore.SkiaSharpView.WinUI` |
| **WinForms** | `samples/WinFormsSample/[Category]/[Name]/` | `View.cs` + `View.Designer.cs` + `View.resx` | `UserControl` (`partial`) | N/A — code-only |
| **Blazor** | `samples/BlazorSample/Pages/[Category]/[Name]/` | `View.razor` | N/A — Razor component | `@using LiveChartsCore.SkiaSharpView.Blazor` |
| **EtoForms** | `samples/EtoFormsSample/[Category]/[Name]/` | `View.cs` | `Panel` (non-partial) | N/A — code-only |
| **UnoPlatform** | *(no separate files)* | Reuses `WinUISample` views via reflection | — | — |

**Key facts for each platform:**

- **All XAML platforms (Avalonia, WPF, MAUI, WinUI)** use `Activator.CreateInstance` with the pattern `{Platform}.{Category}.{Name}.View` to load views — so the **C# namespace must exactly match** `{PlatformPrefix}.{Category}.{Name}` and the class must be named `View`.
- **WinForms** and **EtoForms** use the same reflection pattern. The view class must be `partial class View : UserControl` (WinForms) or `class View : Panel` (EtoForms).
- **Blazor** uses Razor's `@page "/{Category}/{Name}"` directive for routing. The nav menu reads `ViewModelsSamples.Index.Samples` automatically.
- **UnoPlatform** (`samples/UnoPlatformSample/`) loads views from the `WinUISample` assembly — no separate files are needed.

**XAML DataContext / BindingContext patterns:**

```xml
<!-- Avalonia (View.axaml) -->
<UserControl xmlns:lvc="using:LiveChartsCore.SkiaSharpView.Avalonia"
             xmlns:vms="using:ViewModelsSamples.[Category].[Name]"
             x:DataType="vms:ViewModel">
    <UserControl.DataContext><vms:ViewModel/></UserControl.DataContext>
</UserControl>

<!-- WPF (View.xaml) -->
<UserControl xmlns:lvc="clr-namespace:LiveChartsCore.SkiaSharpView.WPF;assembly=LiveChartsCore.SkiaSharpView.WPF"
             xmlns:vms="clr-namespace:ViewModelsSamples.[Category].[Name];assembly=ViewModelsSamples">
    <UserControl.DataContext><vms:ViewModel/></UserControl.DataContext>
</UserControl>

<!-- MAUI (View.xaml) — ContentPage + XamlCompilation attribute on code-behind -->
<ContentPage xmlns:lvc="clr-namespace:LiveChartsCore.SkiaSharpView.Maui;assembly=LiveChartsCore.SkiaSharpView.Maui"
             xmlns:vms="clr-namespace:ViewModelsSamples.[Category].[Name];assembly=ViewModelsSamples"
             x:DataType="vms:ViewModel">
    <ContentPage.BindingContext><vms:ViewModel/></ContentPage.BindingContext>
</ContentPage>

<!-- WinUI (View.xaml) — sealed partial class -->
<UserControl xmlns:lvc="using:LiveChartsCore.SkiaSharpView.WinUI"
             xmlns:vms="using:ViewModelsSamples.[Category].[Name]">
    <UserControl.DataContext><vms:ViewModel/></UserControl.DataContext>
</UserControl>
```

**Code-only platforms (WinForms / EtoForms)** typically instantiate the ViewModel directly or inline the data:

```csharp
// WinForms — partial class View : UserControl
var vm = new ViewModel();
var chart = new GeoMap { Series = vm.Series, ... };
chart.Location = new System.Drawing.Point(0, 0);
chart.Size = new System.Drawing.Size(50, 50);
chart.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
Controls.Add(chart);

// EtoForms — class View : Panel (non-partial, no Designer file)
var vm = new ViewModel();
var chart = new GeoMap { Series = vm.Series, ... };
Content = new DynamicLayout(chart);
```

**WinForms `View.Designer.cs`** is always a minimal boilerplate — copy from any existing sample, just update the namespace.

**UI-testing accessor** — most XAML views expose a `Chart` property under `#if UI_TESTING` for the Factos test runner:

```csharp
// XAML platforms (WPF, Avalonia, MAUI, WinUI)
#if UI_TESTING
    public SomeChartType Chart => chartNamedInXaml;
#endif

// EtoForms / WinForms
public SomeChartType Chart;  // public field, always present
```

**Blazor** exposes the chart via `@ref`:
```razor
<CartesianChart @ref="Chart" .../>
@code { public CartesianChart Chart; }
```

## Code Style and Conventions

### Editor Config
The repository uses `.editorconfig` based on [.NET Runtime coding style](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md) with exceptions.

**Key Style Rules:**
- **Indentation**: 4 spaces
- **Line endings**: LF, insert final newline
- **Braces**: New line before open brace (Allman style)
- **var usage**: Use `var` freely (explicitly allowed)
- **Single-line if**: Allowed and preferred when line is short
- **Naming**:
  - Private/internal fields: `_camelCase`
  - Static private fields: `s_camelCase`
  - Constants: `PascalCase`
- **Using directives**: Outside namespace

### File Naming
**Critical for auto-generated documentation:**
- File names MUST match the class name exactly
- `public class Hello` → `Hello.cs`
- `public class Hello<T>` → `Hello.cs` (ignore generics)
- Generic and non-generic with same name → same file (only if inheritance relationship)

### Important Constants
Defined in `Directory.Build.props`:
- `LiveChartsVersion`: Current version (2.0.0-rc6.1)
- `MinSkiaSharpVersion`: 2.88.9
- `LatestSkiaSharpVersion`: 3.119.0
- `LangVersion`: 14.0 (C# 14)

## Build Configuration Properties

### Rendering Settings
Rendering settings are configured via MSBuild properties:
- `GPU`: Enable/disable GPU acceleration
- `VSYNC`: Enable/disable vertical sync
- `FPS`: Frame rate (10, 20, 30, 45, 60, 75, 90, 120)
- `Diagnose`: Enable diagnostic mode

These create conditional compilation symbols for testing different rendering modes.

### Development Flags
In `Directory.Build.props`:
- `UseNuGetForSamples`: Use NuGet packages vs project references (default: false)
- `UseNuGetForGenerator`: Use NuGet generator package (default: true)

## CI/CD

### GitHub Actions Workflows

#### 1. Main CI (`livecharts.yml`)
- Triggers: Pull requests
- Runs on: `windows-2025` (pack/test), `ubuntu-24.04` (Linux/browser), `macos-26` (Mac/iOS)
- Steps:
  1. **Pack**: Builds NuGet packages for all platform libraries (core, skiasharp, WPF, Avalonia, MAUI, Blazor, WinUI, UNO, WinForms, Eto)
  2. **test-core**: Runs `CoreTests` on `net8.0` and `net462`
  3. **test-snapshot**: Runs `SnapshotTests`
  4. **test-windows/linux/mac/browser/android/ios**: Runs Factos UI tests for each platform
- On tag pushes (after tests pass): publishes packages to NuGet.org

#### 2. Publish (`publish.yml`)
- Handles NuGet package publishing

**Note**: The CI uses NuGet packages (not project references) when running UI tests in Release mode.

## Common Development Workflows

### Adding a New Series Type
1. Create series class in `src/LiveChartsCore/[SeriesType]/`
2. Implement series interfaces (`ISeries`, etc.)
3. Create SkiaSharp drawable in `src/skiasharp/LiveChartsCore.SkiaSharp/Drawing/Geometries/`
4. Add tests in `tests/CoreTests/SeriesTests/`
5. Create sample ViewModel in `samples/ViewModelsSamples/`
6. Update `samples/ViewModelsSamples/Index.cs`

### Adding Platform Support
1. Create new project in `src/skiasharp/LiveChartsCore.SkiaSharpView.[Platform]/`
2. Reference `LiveChartsCore.SkiaSharp` project
3. Create platform-specific control classes
4. Add shared code to `_Shared/` if applicable
5. Create sample application in `samples/[Platform]Sample/`
6. Add platform-specific solution file

### Updating Documentation

**Important**: Documentation files in the `docs/` folder are **Scriban templates**, not final markdown files.

**How it works:**
1. Template files are compiled by an external (non-open-source) repository
2. Templates use [Scriban](https://github.com/scriban/scriban) - a fast, powerful, and lightweight text templating language
3. Scriban supports custom functions and expressions embedded in the markdown

**Common Scriban expressions you'll find:**

**File inclusion** - Renders content from source files:
```
{{~ render "~/../samples/ViewModelsSamples/Events/Cartesian/ViewModel.cs" ~}}
{{~ render "~/../samples/MauiSample/MauiProgram.cs" ~}}
{{~ render "~/../samples/{samples_folder}/Events/Cartesian{view_extension}" ~}}
```

**Conditionals** - Platform-specific content:
```
{{~ if xaml ~}}
  Content for XAML platforms (WPF, Avalonia, UNO, WinUI, MAUI)
{{~ end ~}}

{{~ if winforms ~}}
  Content specific to WinForms
{{~ end ~}}
```

**Variables** - Dynamic content:
```
{{ website_url }}/docs/{{ platform }}/{{ version }}/About
{{ assets_url }}/docs/{{ unique_name }}/result.gif
{{ name | to_title_case }}
{{ edit_source | replace_local_to_server }}
```

**Loops** - Iterate over collections:
```
{{~ for r in related_to ~}}
  <a href="{{ compile this r.url }}">{{ r.name }}</a>
{{~ end ~}}
```

**Template structure:**
- `docs/samples/[category]/[name]/template.md` - Sample documentation templates
- `docs/shared/*.md` - Reusable template fragments included via `{{ render "~/shared/..." }}`
- `docs/piechart/`, `docs/cartesianChart/`, etc. - Feature documentation with templates

**When editing docs:**
- Always edit the `.md` files as Scriban templates
- Test template syntax (though final compilation happens externally)
- Use `{{~ ~}}` syntax to strip whitespace around expressions
- File paths in `render` are relative to the template location (use `~/../` for repo root)

## Important Notes for Coding Agents

### Do's
- ✅ Use project references during development (not NuGet packages)
- ✅ Follow the exact file naming convention (critical for docs)
- ✅ Run tests after changes to core or series logic
- ✅ Use platform-specific solution files for focused development
- ✅ Consult `CONTRIBUTING.md` for detailed style guide
- ✅ Use shared code in `_Shared/` folders when adding cross-platform features

### Don'ts
- ❌ Don't break multi-platform support when modifying core projects
- ❌ Don't add platform-specific code to `LiveChartsCore` (keep it agnostic)
- ❌ Don't ignore `.editorconfig` warnings
- ❌ Don't remove or modify working tests without good reason
- ❌ Don't add new dependencies without checking compatibility across all target frameworks

### Special Considerations
- The library supports .NET Framework 4.6.2 - maintain compatibility
- SkiaSharp is abstracted - core library should work with other rendering engines
- Animation system (`Motion/`) is critical - changes require extensive testing
- Multi-threading: Chart updates can come from any thread; proper synchronization is essential

## Quick Reference Commands

```bash
# === Building ===
# Build core library (no workloads needed)
dotnet build src/LiveChartsCore/LiveChartsCore.csproj
dotnet build src/skiasharp/LiveChartsCore.SkiaSharp/LiveChartsCore.SkiaSharpView.csproj

# Build platform-specific projects
dotnet build src/skiasharp/LiveChartsCore.SkiaSharp.WPF/LiveChartsCore.SkiaSharpView.Wpf.csproj
dotnet build src/skiasharp/LiveChartsCore.SkiaSharp.Avalonia/LiveChartsCore.SkiaSharpView.Avalonia.csproj

# === Install Workloads (for platform-specific projects) ===
dotnet workload install maui --skip-sign-check
dotnet workload install wasm-tools --skip-sign-check

# Check installed workloads
dotnet workload list

# === Testing ===
# Run core unit tests
dotnet test tests/CoreTests/ --framework net8.0

# Run snapshot tests
dotnet test tests/SnapshotTests/

# Run UI tests (requires sample apps to build)
dotnet run --project tests/UITests/

# Run UI tests for specific platform
dotnet run --project tests/UITests/ -- --select wpf
dotnet run --project tests/UITests/ -- --select avalonia-desktop

# === Running Samples ===
# Run WPF sample
dotnet run --project samples/WPFSample/WPFSample.csproj

# Run Avalonia sample
dotnet run --project samples/AvaloniaSample/AvaloniaSample.csproj

# Run Console sample (no UI)
dotnet run --project samples/ConsoleSample/ConsoleSample.csproj

# === Troubleshooting ===
# Clean build artifacts
dotnet clean
find . -type d -name "bin" -o -name "obj" | xargs rm -rf

# Restore packages
dotnet restore

# Check for workload issues
dotnet workload restore --skip-sign-check
```

## Documented Errors and Workarounds

This section documents actual errors encountered when working with this repository and their solutions.

### Error 1: NETSDK1147 - Missing Workloads

**Error Message:**
```
error NETSDK1147: To build this project, the following workloads must be installed: maui
To install these workloads, run the following command: dotnet workload restore
```

**Context**: This occurs when building platform-specific view projects (MAUI, UNO, Avalonia Browser) that require specific workloads. Core projects (`LiveChartsCore`, `LiveChartsCore.SkiaSharp`) do NOT require any workloads.

**Workarounds:**

**Option 1: Install Required Workloads**
```bash
dotnet workload install maui --skip-sign-check
dotnet workload install wasm-tools --skip-sign-check
```

**Option 2: Build Platform-Specific Projects that don't need workloads**
```bash
# Build only WPF (Windows only)
dotnet build src/skiasharp/LiveChartsCore.SkiaSharp.WPF/LiveChartsCore.SkiaSharpView.Wpf.csproj

# Build only Avalonia desktop (cross-platform)
dotnet build src/skiasharp/LiveChartsCore.SkiaSharp.Avalonia/LiveChartsCore.SkiaSharpView.Avalonia.csproj

# Use platform-specific solution files
dotnet build LiveCharts.WPF.slnx
```

### Error 2: Visual Studio Component Required

**Error Message:**
```
Unhandled exception: The imported file "$(MSBuildExtensionsPath32)/Microsoft/VisualStudio/v$(VisualStudioVersion)/CodeSharing/Microsoft.CodeSharing.Common.Default.props" does not exist and appears to be part of a Visual Studio component.
```

**Context**: Appears when running `dotnet workload restore` on non-Windows systems or when Visual Studio is not installed.

**Why it happens**: The `src/skiasharp/_Shared.WinUI/_Shared.WinUI.shproj` shared project requires Visual Studio components that are Windows-specific.

**Workaround**: This error can be ignored if you're not building WinUI projects. The workload installation succeeds despite this error. If you need to build WinUI:
- Use Windows with Visual Studio 2022 installed
- Use `msbuild` instead of `dotnet build` for WinUI projects

### Error 3: Ambiguous Argument with Git

**Error Message:**
```
fatal: ambiguous argument 'origin/branch-name': unknown revision or path not in the working tree.
```

**Context**: After fetching a branch with `git fetch origin branch-name`, trying to reference it as `origin/branch-name`.

**Why it happens**: Git fetch stores the ref as `FETCH_HEAD`, not as a trackable remote branch.

**Solution**: Use `FETCH_HEAD` or create a local tracking branch:
```bash
# Option 1: Use FETCH_HEAD directly
git log FETCH_HEAD

# Option 2: Create tracking branch
git fetch origin main
git checkout -b main --track origin/main

# Option 3: Fetch with branch creation
git fetch origin main:main
```

### Error 4: Package Not Found During Build

**Context**: Sample applications may fail to build if NuGet packages are not found.

**Why it happens**: `UseNuGetForSamples` flag in `Directory.Build.props` controls whether samples use project references or NuGet packages.

**Solution**: Ensure you're using project references during development:
```xml
<!-- In Directory.Build.props -->
<UseNuGetForSamples>false</UseNuGetForSamples>
```

Or restore NuGet packages if building from packages:
```bash
dotnet restore
```

### Error 5: Strong Name Assembly Conflicts

**Context**: When building for .NET Framework 4.6.2, you may encounter assembly version conflicts.

**Why it happens**: .NET Framework uses strong-named assemblies, and SkiaSharp has different versioning.

**Referenced Issue**: https://github.com/mono/SkiaSharp/issues/3153

**Solution**: The project is configured to handle this, but if you encounter issues:
1. Clean the solution: `dotnet clean`
2. Delete `bin` and `obj` folders
3. Restore and rebuild: `dotnet restore && dotnet build`

### Error 6: Test Build Failures on CI

**Context**: UI tests may fail with target framework mismatches in CI.

**Solution**: The UI test infrastructure uses special MSBuild properties:
- `TestBuildTargetFramework`: Override target framework for test builds
- `IsTestBuild`: Flag to indicate test build
- `UITesting`: Flag to include shared UI tests

Example from `tests/UITests/Program.cs`:
```csharp
MSBuildArg tf_n10w = new("TestBuildTargetFramework", "net10.0-windows");
MSBuildArg isTest = new("IsTestBuild", "true");
```

## Troubleshooting

### Problem: Can't build platform-specific projects - workload errors
**Solution**: Install the required workload for the platform you're targeting (e.g., `dotnet workload install maui`). Core projects (`LiveChartsCore`, `LiveChartsCore.SkiaSharp`) build without any workloads.

### Problem: SkiaSharp errors
**Solution**: Check SkiaSharp version in `Directory.Build.props`, ensure NuGet restore succeeded

### Problem: Tests fail with animation issues
**Solution**: Verify `CoreMotionCanvas.IsTesting = true` in test initialization

### Problem: Sample won't run
**Solution**: Ensure platform-specific dependencies are installed (e.g., .NET Desktop Runtime for WPF)

### Problem: Generator errors
**Solution**: Check `UseNuGetForGenerator` setting and ensure LiveChartsGenerators package/project is available

## Issue Reproduction & Fix Workflow

When picking up a GitHub issue (reproduce → diagnose → fix → regression test → PR),
follow the canonical workflow at [`.claude/skills/repro-and-fix/SKILL.md`](../.claude/skills/repro-and-fix/SKILL.md).
That doc is the single source of truth for both Claude Code and Copilot Coding Agent —
keep edits there, not duplicated here.

Quick reference for the dev-loop hooks the workflow relies on:

- **`LVC_SAMPLE=<sample-path>`** — XAML samples (Avalonia / WPF / WinUI / MAUI / Uno) auto-navigate to the named sample on launch, e.g. `LVC_SAMPLE=VisualTest/Issue1986Repro`. Skips manual UI navigation for repros.
- **`LVC_SCREENSHOT=<png-path>`** — Avalonia / WPF / WinUI samples render the main window to PNG via `RenderTargetBitmap` shortly after activation and exit. Captures scale to physical pixels on HiDPI.
- **`LVC_SCREENSHOT_DELAY_MS=<ms>`** — overrides the 3 s default settle delay before the in-app screenshot is taken (use on slower CI hosts).
- **`.claude/scripts/capture-window.{ps1,-macos.sh,-linux.sh}`** — per-OS PrintWindow / `screencapture` / `grim` fallbacks for platforms without an in-app capture path (MAUI, Uno).

Repro views live under `samples/AvaloniaSample/VisualTest/Issue<N>Repro/` (or
the equivalent under whichever platform sample fits the bug). Their code-behind
exposes helpers (e.g. `FindTemplatedGaugeSeries()`) that Factos UI tests call
directly. Factos and `LVC_SAMPLE` navigate by path and don't need the repro
registered in `samples/ViewModelsSamples/Index.cs` — **do not commit changes
to that file for a repro view.** It's shared across every platform sample,
so a single-platform repro entry will crash the load on the other platforms.

## Resources

- **Main Documentation**: https://livecharts.dev
- **Contributing Guide**: `CONTRIBUTING.md`
- **Repository**: https://github.com/Live-Charts/LiveCharts2
- **Code of Conduct**: `CODE_OF_CONDUCT.md`
- **License**: MIT (see `LICENSE`)

## Version Information

- **Current Version**: 2.0.0-rc6.1 (Release Candidate)
- **C# Language Version**: 14.0
- **SkiaSharp**: 2.88.9 (min) to 3.119.0 (latest)
- **Target Frameworks**: `net462`, `netstandard2.0`, `net8.0`, `net8.0-windows` (core); plus platform-specific targets for view projects
