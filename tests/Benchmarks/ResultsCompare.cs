using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Benchmarks;

// Reads BenchmarkDotNet `*-report-full.json` exports from a base run and a head run,
// matches benchmarks by (method, params), computes a % delta on the mean, and emits
// a markdown table.
internal static class ResultsCompare
{
    // A row is flagged only when its delta is BOTH:
    //   * beyond the headline percentage floor (|pct| > PctThreshold), and
    //   * statistically beyond the combined standard error (|pct| > NoiseFactor * relSE).
    // The percentage floor catches meaningful real moves; the noise gate filters out
    // jitter on small benches (Pie ~1 ms, Heat ~2 ms) where one bad sample on a shared
    // GitHub runner can fake a 10–15% swing.
    private const double PctThreshold = 0.15;
    private const double NoiseFactor = 3.0;

    public static int Run(ReadOnlySpan<string> args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("usage: compare <baseDir> <headDir> [outMarkdown]");
            return 64;
        }

        var baseResults = LoadAll(args[0]);
        var headResults = LoadAll(args[1]);
        var outPath = args.Length >= 3 ? args[2] : null;

        var rows = new List<Row>();
        foreach (var (key, head) in headResults)
        {
            baseResults.TryGetValue(key, out var @base);
            rows.Add(new Row(key, @base, head));
        }
        // Also list benchmarks that disappeared
        foreach (var (key, @base) in baseResults)
            if (!headResults.ContainsKey(key))
                rows.Add(new Row(key, @base, null));

        rows.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));

        var (hasInteresting, hasRegression) = Classify(rows);

        var md = RenderMarkdown(rows);
        if (outPath is not null)
            File.WriteAllText(outPath, md);
        else
            Console.Write(md);

        // Expose flags to the workflow so it can decide whether to post a comment
        // (any row significantly outside noise) and whether to fail (any row slower
        // beyond both the percentage floor and the noise gate).
        var githubOutput = Environment.GetEnvironmentVariable("GITHUB_OUTPUT");
        if (!string.IsNullOrEmpty(githubOutput))
        {
            File.AppendAllLines(githubOutput,
            [
                "has_interesting=" + (hasInteresting ? "true" : "false"),
                "has_regression=" + (hasRegression ? "true" : "false"),
            ]);
        }

        Console.WriteLine($"has_interesting={hasInteresting}");
        Console.WriteLine($"has_regression={hasRegression}");

        return 0;
    }

    private static (bool hasInteresting, bool hasRegression) Classify(List<Row> rows)
    {
        var interesting = false;
        var regression = false;
        foreach (var row in rows)
        {
            if (row.Base is null || row.Head is null) continue;
            if (!IsSignificant(row.Base, row.Head, out var pct)) continue;
            interesting = true;
            if (pct > 0) regression = true;
        }
        return (interesting, regression);
    }

    // A delta is "significant" when it is both beyond the percentage floor AND
    // beyond NoiseFactor x the combined relative standard error of base and head.
    private static bool IsSignificant(BenchmarkStats @base, BenchmarkStats head, out double pct)
    {
        pct = (head.MeanNs - @base.MeanNs) / @base.MeanNs;
        if (Math.Abs(pct) <= PctThreshold) return false;
        var relSe = Math.Sqrt(@base.StandardErrorNs * @base.StandardErrorNs
                              + head.StandardErrorNs * head.StandardErrorNs) / @base.MeanNs;
        return Math.Abs(pct) > NoiseFactor * relSe;
    }

    private static Dictionary<string, BenchmarkStats> LoadAll(string dir)
    {
        var result = new Dictionary<string, BenchmarkStats>(StringComparer.Ordinal);
        foreach (var file in Directory.EnumerateFiles(dir, "*-report-full.json", SearchOption.AllDirectories))
        {
            using var stream = File.OpenRead(file);
            using var doc = JsonDocument.Parse(stream);
            if (!doc.RootElement.TryGetProperty("Benchmarks", out var benchmarks)) continue;

            foreach (var b in benchmarks.EnumerateArray())
            {
                var fullName = b.GetProperty("FullName").GetString() ?? "";

                // If the benchmark errored out (worker crash, no results), Statistics
                // is serialised as null. Skip it so one bad row doesn't kill the diff.
                if (!b.TryGetProperty("Statistics", out var stats)
                    || stats.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
                {
                    Console.Error.WriteLine($"warn: no statistics for '{fullName}', skipping.");
                    continue;
                }

                var mean = stats.GetProperty("Mean").GetDouble();
                var stdErr = stats.GetProperty("StandardError").GetDouble();
                double? allocated = null;
                if (b.TryGetProperty("Memory", out var mem)
                    && mem.ValueKind is JsonValueKind.Object
                    && mem.TryGetProperty("BytesAllocatedPerOperation", out var alloc))
                {
                    allocated = alloc.GetDouble();
                }
                result[fullName] = new BenchmarkStats(mean, stdErr, allocated);
            }
        }
        return result;
    }

    private static string RenderMarkdown(List<Row> rows)
    {
        var sb = new StringBuilder();
        _ = sb.AppendLine("| Benchmark | Base (ms) | Head (ms) | Δ | Alloc base | Alloc head |");
        _ = sb.AppendLine("|---|---:|---:|---:|---:|---:|");
        foreach (var row in rows)
        {
            var @base = row.Base;
            var head = row.Head;
            var baseMs = @base is null ? "—" : FormatMs(@base.MeanNs);
            var headMs = head is null ? "—" : FormatMs(head.MeanNs);
            var delta = DeltaCell(@base, head);
            var baseAlloc = @base?.Allocated is null ? "—" : FormatBytes(@base.Allocated.Value);
            var headAlloc = head?.Allocated is null ? "—" : FormatBytes(head.Allocated.Value);
            // No backticks around the name — comment-progress injects this table into a JS
            // template literal and backticks would close it, producing a syntax error.
            _ = sb.AppendLine(CultureInfo.InvariantCulture, $"| {Short(row.Key)} | {baseMs} | {headMs} | {delta} | {baseAlloc} | {headAlloc} |");
        }
        return sb.ToString();
    }

    private static string DeltaCell(BenchmarkStats? @base, BenchmarkStats? head)
    {
        if (@base is null || head is null) return "—";
        var significant = IsSignificant(@base, head, out var pct);
        var str = (pct * 100).ToString("+0.0;-0.0;0.0", CultureInfo.InvariantCulture) + "%";
        if (!significant) return str;
        return (pct > 0 ? "🔴 " : "🟢 ") + str;
    }

    private static string FormatMs(double ns) => (ns / 1_000_000.0).ToString("N3", CultureInfo.InvariantCulture);

    private static string FormatBytes(double bytes)
    {
        if (bytes < 1024) return $"{bytes:N0} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024:N1} KB";
        return $"{bytes / (1024.0 * 1024):N1} MB";
    }

    // Drop the `Benchmarks.` namespace prefix to keep table width readable.
    private static string Short(string fullName) =>
        fullName.StartsWith("Benchmarks.", StringComparison.Ordinal) ? fullName[11..] : fullName;

    private sealed record BenchmarkStats(double MeanNs, double StandardErrorNs, double? Allocated);
    private sealed record Row(string Key, BenchmarkStats? Base, BenchmarkStats? Head);
}
