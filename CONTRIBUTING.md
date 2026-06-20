# Testing

LiveCharts2 has three categories of tests. Please make sure relevant tests pass before submitting a pull request.

## Core tests

Located in `tests/CoreTests/`. These are MSTest unit tests that validate the core charting engine logic without any UI. They cover chart behavior, series calculations, layout, axes, events and more.

**Structure:**

- `ChartTests/` – high-level chart functionality (null handling, visual elements).
- `SeriesTests/` – tests for every series type (Column, Line, Pie, Scatter, Heat, Financial, Stacked, etc.).
- `LayoutTests/` – stack and table layout tests.
- `CoreObjectsTests/` – transitions, colors, labels.
- `OtherTests/` – axes, events, data providers, visual elements.
- `MockedObjects/` – test helpers and mocks.
- `TestsInitializer.cs` – assembly-level setup; sets `CoreMotionCanvas.IsTesting = true` to disable animations during testing.

**Run:**

```bash
# run on .NET 8
dotnet test tests/CoreTests/ --framework net8.0

# run on .NET Framework 4.6.2 (Windows only)
dotnet test tests/CoreTests/ --framework net462
```

## Snapshot tests

Located in `tests/SnapshotTests/`. These tests render charts to images using the in-memory SkiaSharp backend (`SKCartesianChart`, `SKPieChart`, etc.) and compare the result against reference PNG files stored in `tests/SnapshotTests/Snapshots/`.

If a reference snapshot does not exist yet, the test fails and generates a new image for you to review. If pixels differ beyond the allowed tolerance the test fails and produces diff images so you can inspect the changes.

**Run:**

```bash
dotnet test tests/SnapshotTests/
```

When you add a new snapshot test, run it once to generate the initial image, review it, then copy it into the `Snapshots` folder and commit it.

## UI tests

Located in `tests/UITests/` (orchestrator) and `tests/SharedUITests/` (shared test definitions). These tests use [Factos](https://github.com/beto-rodriguez/Factos) to run the same set of UI tests against every supported platform (Avalonia, WPF, MAUI, WinUI, Blazor, WinForms, Eto, Uno, Android, iOS).

**How it works:**

1. Shared tests are defined in `tests/SharedUITests/` (e.g. `CartesianChartTests.cs`, `PieChartTests.cs`, `PolarChartTests.cs`, `MapChartTests.cs`).
2. When `UITesting=true`, sample apps include those shared test sources by linking the `.cs` files from `tests/SharedUITests/` via `build/UITestsLinks.Build.props` (rather than by adding a project reference to `SharedUITests`).
3. There are platform-specific exceptions to this wiring, notably the WinUI/Uno setup, but the overall goal is the same: the shared UI test code becomes part of the sample app being tested.
4. The orchestrator in `tests/UITests/Program.cs` builds and starts each sample app, connects to it via Factos, and runs the shared tests.

**Run:**

```bash
# run against a specific platform
dotnet run --project tests/UITests/ -- --select avalonia-desktop
dotnet run --project tests/UITests/ -- --select wpf-net10
dotnet run --project tests/UITests/ -- --select maui --test-env "tf=net10.0-windows10.0.19041.0"
```

In Debug mode the app-to-run is configured in `tests/UITests/Program.cs` (set the `appToRun` variable). In Release mode the arguments come from the CI pipeline.

## CI pipeline

All tests are executed automatically on every pull request via the GitHub Actions workflow (`.github/workflows/livecharts.yml`). The pipeline:

1. **Packs** NuGet packages for every platform library.
2. **Runs core tests** on `net8.0` and `net462` (Windows).
3. **Runs snapshot tests** (Windows).
4. **Runs UI tests** across Windows, Linux, macOS, browser, Android and iOS.

You do not need to run every platform locally; the CI will cover all of them. At a minimum, run the core tests and, if your changes affect rendering, the snapshot tests before submitting.

---

# Coding style

Please ensure that any code you add to this repository satisfies the coding style, the root of this repo contains a 
[editor config](https://github.com/beto-rodriguez/LiveCharts2/blob/master/.editorconfig) file that will warn you in 
visual studio if you are violating the coding style.

The coding style of this repository is based on [dot net runtime coding style](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md) 
but there are a few exceptions:

~~10. We only use var when the type is explicitly named on the right-hand side, typically due to either new or an explicit cast, e.g. 
`var stream = new FileStream(...)` not `var stream = OpenStandardInput()`.~~

10. Feel free to use `var` anywhere.

18. When using a single-statement if, we follow these conventions:

* ~~Never use single-line form (for example: if (source == null) throw new ArgumentNullException("source");)~~

* Please use single line `if` expressions. If the line is too long, please create a new line every time the width of the line could exceed the editor window size. For example, the following line is valid: 
```
if (model == null)
    throw new Exception(
        $"A {nameof(ObservablePoint)} can not be null, instead set to null to " +
        $"any of its properties.");
```


* Using braces is always accepted, and required if any block of an if/else if/.../else compound statement uses braces ~~or if a single statement body spans multiple lines.~~ (just break the lines as in the previous sample)

* Braces may be omitted only if the body of every block associated with an if/else if/.../else compound statement is placed on a single line.

Early `return`, `continue` and `break` should be used when possible.

# Naming files

The documentation is generated automatically, it is important that all the files are named the same as the object name.

for the following class, the file must be named `Hello.cs`

```
public class Hello
{
  public string World { get; set; }
}
```

When using generics, just ignore them. For example, the following class must also be named `Hello.cs`: 

```
public class Hello<T>
{
  public T World { get; set; }
}
```

If you have generic and a non-generic definition that share the same name, please place both classes in the same file. This should only 
happen when both objects are related to each other by inheritance.

The following `Hello.cs` must contain both, inheritance is required.

```
public class Hello
{
  public string World { get; set; }
}

public class Hello<T> : Hello
{
  public T World2 { get; set; }
}
```
