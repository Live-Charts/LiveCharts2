// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsoleTests.Helpers;

/// <summary>
/// Compares a rendered string against a checked-in golden file. Set the
/// <c>LVC_GOLDEN_REGEN</c> env var to overwrite goldens with the current output —
/// review the diff before committing. Without the env var, a missing or stale golden
/// fails the test with a hint about how to regenerate.
/// </summary>
internal static class GoldenAssert
{
    public static void Matches(string actual, string goldenFileName, [CallerFilePath] string callerFile = "")
    {
        var goldenPath = ResolveGoldenPath(goldenFileName, callerFile);
        var regen = Environment.GetEnvironmentVariable("LVC_GOLDEN_REGEN") == "1";

        if (regen)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(goldenPath)!);
            File.WriteAllText(goldenPath, actual);
            // Don't pass on regen runs — force a second pass without the env var so the
            // suite is only "green" against committed goldens.
            Assert.Inconclusive($"Regenerated golden: {goldenPath}");
            return;
        }

        if (!File.Exists(goldenPath))
        {
            Assert.Fail(
                $"Golden file missing: {goldenPath}\n" +
                "Run with LVC_GOLDEN_REGEN=1 to generate, then review and commit.");
        }

        var expected = File.ReadAllText(goldenPath);
        if (expected == actual) return;

        // Side-write the actual output so the diff is reviewable without re-running with regen.
        var actualPath = goldenPath + ".actual";
        File.WriteAllText(actualPath, actual);
        Assert.Fail(
            $"Golden mismatch.\n  expected: {goldenPath}\n  actual:   {actualPath}\n" +
            "Review with a diff tool. To accept, replace the golden or rerun with LVC_GOLDEN_REGEN=1.");
    }

    /// <summary>
    /// Resolves goldens relative to the SOURCE folder of the calling test file, not the
    /// build output directory. This way regen writes to the checked-in tree and a normal
    /// run reads the same files (we still <c>CopyToOutput</c> them so CI without source
    /// access still works — that path is the fallback below).
    /// </summary>
    private static string ResolveGoldenPath(string fileName, string callerFile)
    {
        if (!string.IsNullOrEmpty(callerFile))
        {
            // Callers live alongside their goldens (Goldens/GoldenChartTests.cs sits next
            // to Goldens/<name>.txt), so the source path is just the caller's directory +
            // file name — no extra subfolder.
            var sourceDir = Path.GetDirectoryName(callerFile);
            if (sourceDir is not null)
            {
                var sourcePath = Path.Combine(sourceDir, fileName);
                if (File.Exists(sourcePath) || Environment.GetEnvironmentVariable("LVC_GOLDEN_REGEN") == "1")
                    return sourcePath;
            }
        }

        return Path.Combine(AppContext.BaseDirectory, "Goldens", fileName);
    }
}
