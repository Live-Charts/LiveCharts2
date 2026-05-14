using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.OtherTests;

[TestClass]
public class BlazorWasmToolsCheckTests
{
    [TestMethod]
    public void TargetsFile_GuardsAgainstMissingWasmToolsWorkload()
    {
        // Issue #2229: a fresh Blazor WASM template + `dotnet add package
        // LiveChartsCore.SkiaSharpView.Blazor` crashed at first render with
        // `ReferenceError: Module is not defined` whenever the wasm-tools
        // workload was missing. SkiaSharp's native libs were silently
        // dropped; the SDK only emits a single buried warning that the
        // user doesn't see.
        //
        // The fix is a .targets file shipped in buildTransitive/ of the
        // package: when RuntimeIdentifier=browser-wasm but
        // WasmNativeWorkloadAvailable!=true, raise LVC0001 so the build
        // fails with the install command. Removing or weakening any of
        // these tokens reopens the regression — so they must all stay.

        var targets = Path.Combine(
            RepoRoot(),
            "src", "skiasharp", "LiveChartsCore.SkiaSharpView.Blazor",
            "build", "LiveChartsCore.SkiaSharpView.Blazor.targets");

        Assert.IsTrue(File.Exists(targets),
            $"Missing build-time guard from PR #2229 fix: {targets}");

        var content = File.ReadAllText(targets);

        StringAssert.Contains(content, "Code=\"LVC0001\"");
        StringAssert.Contains(content, "'$(RuntimeIdentifier)' == 'browser-wasm'");
        StringAssert.Contains(content, "'$(WasmNativeWorkloadAvailable)' != 'true'");
        StringAssert.Contains(content, "dotnet workload install wasm-tools");

        // The csproj must also pack the file at the consumer-visible path,
        // otherwise the guard ships in source but not in the NuGet.
        var csproj = Path.Combine(
            RepoRoot(),
            "src", "skiasharp", "LiveChartsCore.SkiaSharpView.Blazor",
            "LiveChartsCore.SkiaSharpView.Blazor.csproj");

        var csprojContent = File.ReadAllText(csproj);
        StringAssert.Contains(csprojContent,
            "PackagePath=\"buildTransitive\\$(AssemblyName).targets\"");
    }

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(System.AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Directory.Build.props")))
            dir = dir.Parent;

        return dir?.FullName
            ?? throw new System.InvalidOperationException(
                "Could not locate repo root (Directory.Build.props) from " +
                System.AppContext.BaseDirectory);
    }
}
