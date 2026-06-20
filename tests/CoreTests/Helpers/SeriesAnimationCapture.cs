using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Motion;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.Helpers;

// Drives an SK chart's CoreChart.Measure() in a controlled-time loop and captures every
// visible point's geometry state at each sample timestamp. Pattern modeled on
// CoreObjectsTests.TransitionsTesting (which exercises raw RectangleGeometry) and
// OtherTests.AnimationsSpeedPropagationTests.RuntimeAnimationsSpeedChange_Interpolates_AtNewSpeed
// (which exercises a single visual property across a single transition).
//
// Two flavors of capture:
//   - The default Frame captures X/Y/W/H from any BoundedDrawnGeometry (covers
//     Bar/Line/StepLine/Scatter/Area/StepArea/Polar/Heat series — anything whose
//     visual is a bounded rect or marker)
//   - Specialized frames (CandlestickFrame, BoxFrame, PieFrame) capture additional
//     motion properties unique to each visual: OHLC values for candlesticks, quartile
//     values for boxes, arc parameters for pie slices
//
// Caller responsibility:
//   - Build a chart with EasingFunctions.Lineal so interpolation is predictable
//   - Fix axis MinLimit/MaxLimit so a re-measure doesn't shift visuals
//   - Wrap the call in try/finally and reset CoreMotionCanvas.DebugElapsedMilliseconds = -1
internal static class SeriesAnimationCapture
{
    public readonly struct Frame
    {
        public Frame(long timeMs, float x, float y, float width, float height)
        {
            TimeMs = timeMs;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public long TimeMs { get; }
        public float X { get; }
        public float Y { get; }
        public float Width { get; }
        public float Height { get; }
    }

    public readonly struct CandlestickFrame
    {
        // BaseCandlestickGeometry extends DrawnGeometry (no Height) — the vertical
        // extent is implicit between Y (high) and Low. Width is the body width.
        public CandlestickFrame(long timeMs, float x, float y, float width,
            float open, float close, float low)
        {
            TimeMs = timeMs;
            X = x; Y = y; Width = width;
            Open = open; Close = close; Low = low;
        }

        public long TimeMs { get; }
        public float X { get; }
        public float Y { get; }            // high (top of the candle)
        public float Width { get; }
        public float Open { get; }
        public float Close { get; }
        public float Low { get; }          // bottom wick
    }

    public readonly struct BoxFrame
    {
        // BaseBoxGeometry extends DrawnGeometry (no Height) — vertical extent is
        // implicit between Y (max) and Min.
        public BoxFrame(long timeMs, float x, float y, float width,
            float third, float first, float min, float median)
        {
            TimeMs = timeMs;
            X = x; Y = y; Width = width;
            Third = third; First = first; Min = min; Median = median;
        }

        public long TimeMs { get; }
        public float X { get; }
        public float Y { get; }            // max (top whisker)
        public float Width { get; }
        public float Third { get; }        // third quartile
        public float First { get; }        // first quartile
        public float Min { get; }          // min (bottom whisker)
        public float Median { get; }
    }

    public readonly struct HeatFrame
    {
        // Heat cells: their rect dimensions are static (set once on creation, never
        // animated). What DOES animate is the cell color via ColorMotionProperty —
        // each byte of R/G/B/A interpolates between two colors in the heat gradient.
        // Bytes are stored as floats so the same reflection-based serializer pipeline
        // handles them.
        public HeatFrame(long timeMs, float x, float y, float width, float height,
            float r, float g, float b, float a)
        {
            TimeMs = timeMs;
            X = x; Y = y; Width = width; Height = height;
            R = r; G = g; B = b; A = a;
        }

        public long TimeMs { get; }
        public float X { get; }
        public float Y { get; }
        public float Width { get; }
        public float Height { get; }
        public float R { get; }
        public float G { get; }
        public float B { get; }
        public float A { get; }
    }

    public readonly struct MapLandFrame
    {
        // GeoMap land shapes don't have meaningful X/Y/W/H — they're path-based
        // BaseVectorGeometry instances. What animates is each land's Fill paint,
        // specifically the R/G/B/A of the SolidColorPaint that HeatLandSeries
        // assigns based on the land's value. Bytes stored as floats so the
        // reflection-based serializer handles them uniformly.
        public MapLandFrame(long timeMs, float r, float g, float b, float a)
        {
            TimeMs = timeMs;
            R = r; G = g; B = b; A = a;
        }

        public long TimeMs { get; }
        public float R { get; }
        public float G { get; }
        public float B { get; }
        public float A { get; }
    }

    public readonly struct AxisFrame
    {
        // Captures the per-separator state of an axis: the separator line endpoints
        // (X..X1, Y..Y1 all animatable) and the label's position + text size. A "missing"
        // visual (e.g. SeparatorsPaint=null on the axis) is encoded with zeros; the
        // baseline pins the actual configured visuals — change the axis to add/remove a
        // paint and you've changed the contract, so re-baseline deliberately.
        public AxisFrame(long timeMs,
            float separatorX, float separatorY, float separatorX1, float separatorY1,
            float labelX, float labelY, float labelTextSize)
        {
            TimeMs = timeMs;
            SeparatorX = separatorX; SeparatorY = separatorY;
            SeparatorX1 = separatorX1; SeparatorY1 = separatorY1;
            LabelX = labelX; LabelY = labelY; LabelTextSize = labelTextSize;
        }

        public long TimeMs { get; }
        public float SeparatorX { get; }
        public float SeparatorY { get; }
        public float SeparatorX1 { get; }
        public float SeparatorY1 { get; }
        public float LabelX { get; }
        public float LabelY { get; }
        public float LabelTextSize { get; }
    }

    public readonly struct PieFrame
    {
        public PieFrame(long timeMs, float x, float y, float width, float height,
            float centerX, float centerY, float startAngle, float sweepAngle,
            float pushOut, float innerRadius)
        {
            TimeMs = timeMs;
            X = x; Y = y; Width = width; Height = height;
            CenterX = centerX; CenterY = centerY;
            StartAngle = startAngle; SweepAngle = sweepAngle;
            PushOut = pushOut; InnerRadius = innerRadius;
        }

        public long TimeMs { get; }
        public float X { get; }
        public float Y { get; }
        public float Width { get; }
        public float Height { get; }
        public float CenterX { get; }
        public float CenterY { get; }
        public float StartAngle { get; }
        public float SweepAngle { get; }
        public float PushOut { get; }
        public float InnerRadius { get; }
    }

    // ---- capture -----------------------------------------------------------

    public static List<Frame[]> CaptureTrajectory(
        IEnumerable<ChartPoint> points,
        long startMs,
        long endMs,
        long stepMs)
    {
        return CaptureTrajectory(points, (t, p) =>
        {
            var v = (BoundedDrawnGeometry)p.Context.Visual!;
            return new Frame(t, v.X, v.Y, v.Width, v.Height);
        }, startMs, endMs, stepMs);
    }

    public static List<TFrame[]> CaptureTrajectory<TFrame>(
        IEnumerable<ChartPoint> points,
        Func<long, ChartPoint, TFrame> selector,
        long startMs,
        long endMs,
        long stepMs)
        where TFrame : struct
    {
        // Caller's selector picks the concrete geometry; we just filter null visuals.
        var pointList = points.Where(p => p.Context.Visual is not null).ToList();
        var trajectory = new List<TFrame[]>();

        for (var t = startMs; t <= endMs; t += stepMs)
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = t;

            // Property getters interpolate to DebugElapsedMilliseconds when read.
            var frame = pointList.Select(p => selector(t, p)).ToArray();
            trajectory.Add(frame);
        }

        return trajectory;
    }

    // Walks the axis's per-chart active-separator dictionary at each timestamp and
    // captures separator line endpoints + label position. The separator dictionary is
    // keyed by the formatted value (e.g. "10", "20"); we iterate in insertion order so
    // the per-frame array shape is stable across timestamps. Caller is responsible for
    // fixing MinLimit/MaxLimit so the active-separator set doesn't churn between
    // measures.
    public static List<AxisFrame[]> CaptureAxisTrajectory(
        LiveChartsCore.SkiaSharpView.Axis axis,
        LiveChartsCore.Chart chart,
        long startMs,
        long endMs,
        long stepMs)
    {
        if (!axis.activeSeparators.TryGetValue(chart, out var separators))
            throw new InvalidOperationException(
                "Axis has no active separators for this chart — has the chart been measured at least once?");

        // Snapshot key order so per-frame arrays line up.
        var keys = separators.Keys.ToArray();

        var trajectory = new List<AxisFrame[]>();

        for (var t = startMs; t <= endMs; t += stepMs)
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = t;

            var frame = new AxisFrame[keys.Length];
            for (var i = 0; i < keys.Length; i++)
            {
                var sep = separators[keys[i]];
                var s = sep.Separator;
                var l = sep.Label;

                frame[i] = new AxisFrame(
                    timeMs: t,
                    separatorX: s?.X ?? 0f,
                    separatorY: s?.Y ?? 0f,
                    separatorX1: s?.X1 ?? 0f,
                    separatorY1: s?.Y1 ?? 0f,
                    labelX: l?.X ?? 0f,
                    labelY: l?.Y ?? 0f,
                    labelTextSize: l?.TextSize ?? 0f);
            }
            trajectory.Add(frame);
        }

        return trajectory;
    }

    // ---- assertion ---------------------------------------------------------

    // Snapshot-style assertion: compares the captured trajectory against a JSON baseline
    // committed at tests/CoreTests/AnimationBaselines/{baselineName}.json. Mirrors the
    // workflow in tests/SnapshotTests/Extensions.cs#AssertSnapshotMatches:
    //   - First run with no baseline: writes the new trajectory to AnimationBaselinesNew/
    //     and fails with a "review and commit" message
    //   - Subsequent runs: deserializes the baseline and compares frame-by-frame within
    //     `tolerance` per float dimension. On mismatch writes both [EXPECTED] and
    //     [RESULT] to AnimationBaselinesDiff/ for inspection
    //
    // Re-baseline workflow: delete tests/CoreTests/AnimationBaselines/{baselineName}.json,
    // re-run the test, move the regenerated file from AnimationBaselinesNew/ back into place.
    //
    // TFrame must be a struct with a `long TimeMs` property and any number of `float`
    // properties — all float properties are serialized (lowercased + first-letter shortened
    // to a JSON-friendly key) and compared with tolerance.
    public static void AssertTrajectoryMatches<TFrame>(
        List<TFrame[]> trajectory,
        string baselineName,
        float tolerance = 0.5f)
        where TFrame : struct
    {
        // Sanitize the caller-supplied stem so it can only contribute a file-name
        // segment to subsequent Path.Combine calls — guards against the (test-only)
        // case of a rooted/path-shaped baselineName silently dropping the diff/new
        // directory prefix. Path.GetFileName strips any directory parts.
        var safeStem = Path.GetFileName(baselineName);

        var baseDir = AppContext.BaseDirectory;
        var baselineDir = Path.Combine(baseDir, "AnimationBaselines");
        var newDir = Path.Combine(baseDir, "AnimationBaselinesNew");
        var diffDir = Path.Combine(baseDir, "AnimationBaselinesDiff");
        var baselinePath = Path.Combine(baselineDir, $"{safeStem}.json");
        var newPath = Path.Combine(newDir, $"{safeStem}.json");

        _ = Directory.CreateDirectory(newDir);

        var serialized = Serialize(trajectory);
        File.WriteAllText(newPath, serialized);

        if (!File.Exists(baselinePath))
        {
            Assert.Fail(
                $"Animation baseline not found for '{baselineName}'. " +
                $"A new trajectory was written to '{newPath}'. Review it and commit " +
                $"to tests/CoreTests/AnimationBaselines/{baselineName}.json.");
            return;
        }

        var expected = Deserialize<TFrame>(File.ReadAllText(baselinePath));

        var mismatch = Compare(expected, trajectory, tolerance);
        if (mismatch is null) return;

        _ = Directory.CreateDirectory(diffDir);
        File.Copy(baselinePath, Path.Combine(diffDir, $"{safeStem}[EXPECTED].json"), overwrite: true);
        File.Copy(newPath, Path.Combine(diffDir, $"{safeStem}[RESULT].json"), overwrite: true);

        Assert.Fail(
            $"Trajectory '{baselineName}' diverges from baseline: {mismatch}. " +
            $"See AnimationBaselinesDiff/{baselineName}[EXPECTED|RESULT].json.");
    }

    // ---- reflection-based ser/de/compare ----------------------------------

    private readonly struct FrameSchema
    {
        public FrameSchema(PropertyInfo timeMs, (string JsonKey, PropertyInfo Prop)[] floats, ConstructorInfo ctor, int[] ctorOrder)
        {
            TimeMs = timeMs;
            Floats = floats;
            Ctor = ctor;
            CtorOrder = ctorOrder;
        }

        public PropertyInfo TimeMs { get; }
        public (string JsonKey, PropertyInfo Prop)[] Floats { get; }
        public ConstructorInfo Ctor { get; }
        // CtorOrder[i] = index into Floats array for the i-th constructor parameter
        // (after the leading TimeMs parameter). Allows reading JSON dimensions and
        // passing them to the ctor in the right order.
        public int[] CtorOrder { get; }
    }

    private static readonly ConcurrentDictionary<Type, FrameSchema> s_schemaCache = new();

    private static FrameSchema GetSchema<TFrame>() where TFrame : struct
    {
        return s_schemaCache.GetOrAdd(typeof(TFrame), static t =>
        {
            var props = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var timeMs = props.FirstOrDefault(p => p.PropertyType == typeof(long) && p.Name == "TimeMs")
                ?? throw new InvalidOperationException($"{t.Name} must have a 'long TimeMs' property");

            var floatProps = props.Where(p => p.PropertyType == typeof(float)).ToArray();
            // JSON key: PascalCase -> camelCase, with common abbreviations:
            //   Width -> w, Height -> h, otherwise first letter lowercase.
            var floats = floatProps
                .Select(p => (JsonKey: AbbreviateKey(p.Name), Prop: p))
                .ToArray();

            // Match the constructor: TimeMs is param 0; remaining float params follow.
            var ctor = t.GetConstructors().Single();
            var ctorParams = ctor.GetParameters();
            if (ctorParams.Length != 1 + floats.Length)
                throw new InvalidOperationException(
                    $"{t.Name} constructor must take (long timeMs, [float ...]) — got {ctorParams.Length} params");

            var order = new int[floats.Length];
            for (var i = 0; i < floats.Length; i++)
            {
                var paramName = ctorParams[i + 1].Name!;
                var match = Array.FindIndex(floats, f => string.Equals(f.Prop.Name, paramName, StringComparison.OrdinalIgnoreCase));
                if (match < 0)
                    throw new InvalidOperationException(
                        $"{t.Name}: constructor param '{paramName}' has no matching public property");
                order[i] = match;
            }

            return new FrameSchema(timeMs, floats, ctor, order);
        });
    }

    private static string AbbreviateKey(string propName)
    {
        return propName switch
        {
            "Width" => "w",
            "Height" => "h",
            "X" => "x",
            "Y" => "y",
            _ => char.ToLowerInvariant(propName[0]) + propName.Substring(1),
        };
    }

    private static string Serialize<TFrame>(List<TFrame[]> trajectory) where TFrame : struct
    {
        var schema = GetSchema<TFrame>();
        var sb = new StringBuilder();
        _ = sb.Append('[');
        _ = sb.Append('\n');
        for (var i = 0; i < trajectory.Count; i++)
        {
            _ = sb.Append("  [");
            var frame = trajectory[i];
            for (var j = 0; j < frame.Length; j++)
            {
                if (j > 0) _ = sb.Append(',');
                var f = frame[j];
                _ = sb.Append("{\"t\":").Append(((long)schema.TimeMs.GetValue(f)!).ToString(CultureInfo.InvariantCulture));
                foreach (var (jsonKey, prop) in schema.Floats)
                {
                    _ = sb.Append(",\"").Append(jsonKey).Append("\":").Append(F((float)prop.GetValue(f)!));
                }
                _ = sb.Append('}');
            }
            _ = sb.Append(']');
            if (i < trajectory.Count - 1) _ = sb.Append(',');
            _ = sb.Append('\n');
        }
        _ = sb.Append(']');
        _ = sb.Append('\n');
        return sb.ToString();

        static string F(float v) => v.ToString("0.00", CultureInfo.InvariantCulture);
    }

    private static List<TFrame[]> Deserialize<TFrame>(string json) where TFrame : struct
    {
        var schema = GetSchema<TFrame>();
        using var doc = JsonDocument.Parse(json);
        var trajectory = new List<TFrame[]>();
        foreach (var frameElement in doc.RootElement.EnumerateArray())
        {
            var frame = new List<TFrame>();
            foreach (var pointElement in frameElement.EnumerateArray())
            {
                var timeMs = pointElement.GetProperty("t").GetInt64();
                var values = new object?[1 + schema.Floats.Length];
                values[0] = timeMs;
                for (var i = 0; i < schema.Floats.Length; i++)
                {
                    var floatIdx = schema.CtorOrder[i];
                    var jsonKey = schema.Floats[floatIdx].JsonKey;
                    values[i + 1] = pointElement.GetProperty(jsonKey).GetSingle();
                }
                frame.Add((TFrame)schema.Ctor.Invoke(values));
            }
            trajectory.Add([.. frame]);
        }
        return trajectory;
    }

    private static string? Compare<TFrame>(List<TFrame[]> expected, List<TFrame[]> actual, float tolerance) where TFrame : struct
    {
        var schema = GetSchema<TFrame>();

        if (expected.Count != actual.Count)
            return $"frame count {expected.Count} expected, got {actual.Count}";

        for (var i = 0; i < expected.Count; i++)
        {
            var e = expected[i];
            var a = actual[i];
            if (e.Length != a.Length)
                return $"frame {i}: point count {e.Length} expected, got {a.Length}";

            for (var j = 0; j < e.Length; j++)
            {
                var eTime = (long)schema.TimeMs.GetValue(e[j])!;
                var aTime = (long)schema.TimeMs.GetValue(a[j])!;
                if (eTime != aTime)
                    return $"frame {i} point {j}: TimeMs {eTime} expected, got {aTime}";

                foreach (var (jsonKey, prop) in schema.Floats)
                {
                    var ev = (float)prop.GetValue(e[j])!;
                    var av = (float)prop.GetValue(a[j])!;
                    if (Math.Abs(ev - av) > tolerance)
                        return $"frame {i} (t={aTime}) point {j}: {prop.Name} expected {ev}, got {av}";
                }
            }
        }
        return null;
    }
}
