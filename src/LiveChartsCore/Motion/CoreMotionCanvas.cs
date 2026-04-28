// The MIT License(MIT)
//
// Copyright(c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using LiveChartsCore.Drawing;
using LiveChartsCore.Painting;

namespace LiveChartsCore.Motion;

/// <summary>
/// Defines a canvas that is able to animate the shapes inside it.
/// </summary>
public class CoreMotionCanvas : IDisposable
{
    private static readonly Stopwatch s_clock = new();
    private int _frames = 0;
    private Stopwatch? _fspSw;
    private int _jitteredDrawCount;
    private double _totalDrawTime = 0;
    private double _lastDrawTime = 0;
    private double _lastKnownFPS = 0;
    private double _averageRenderTime = 0;
    private double _totalFrames = double.Epsilon;
    private double _totalSeconds = 0;
    internal long _lastFrameTimestamp;
    internal TimeSpan _nextFrameDelay = s_baseFrameDelay;
    private static readonly double s_ticksPerMillisecond = Stopwatch.Frequency / 1000d;
    private static readonly TimeSpan s_baseFrameDelay = TimeSpan.FromMilliseconds(1000d / LiveCharts.RenderingSettings.LiveChartsRenderLoopFPS);
    private static readonly long s_jitterThreshold = s_baseFrameDelay.Ticks / 2;
    internal LvcColor _virtualBackgroundColor;
    internal static string? s_externalRenderer;
    internal static string? s_rendererName;
    internal static string? s_tickerName;

    static CoreMotionCanvas()
    {
        s_clock.Start();
    }

    /// <summary>
    /// The frame request handler delegate.
    /// </summary>
    /// <param name="context"></param>
    public delegate void FrameRequestHandler(DrawingContext context);

    internal CanvasZone[] Zones
    {
        get
        {
            if (field.Length > 0)
                return field;

            // if the zones are not initialized, we create them
            field = CanvasZone.CreateZones();

            return field;
        }
        set => field = value;
    } = [];

    /// <summary>
    /// Gets the clock elapsed time in milliseconds.
    /// </summary>
    public static long ElapsedMilliseconds =>
        DebugElapsedMilliseconds > -1
            ? DebugElapsedMilliseconds
            : s_clock.ElapsedMilliseconds;

    internal static long DebugElapsedMilliseconds { get; set; } = -1;
    internal static bool IsTesting { get; set; }

    // Lets a platform view announce that the chart is visible again after a period
    // off-screen (e.g., scrolled into view, tab activated). Bumping the frame
    // timestamp prevents Chart.IsRendering() from suppressing the next measure
    // request that would otherwise be blocked because the canvas has not painted.
    // Used by the Avalonia view; see https://github.com/Live-Charts/LiveCharts2/issues/1986
    internal void NotifyPlatformVisible() =>
        _lastFrameTimestamp = s_clock.ElapsedTicks;

    internal bool DisableAnimations { get; set; }

    /// <summary>
    /// Occurs when the visual is invalidated.
    /// </summary>
    public event Action<CoreMotionCanvas>? Invalidated;

    /// <summary>
    /// Occurs when all the visuals in the canvas are valid.
    /// </summary>
    public event Action<CoreMotionCanvas>? Validated;

    /// <summary>
    /// Returns true if the visual is valid.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
    /// </value>
    public bool IsValid { get; private set; }

    /// <summary>
    /// Gets the synchronize object.
    /// </summary>
    /// <value>
    /// The synchronize.
    /// </value>
    public object Sync { get; internal set => field = value ?? new object(); } = new();

    /// <summary>
    /// Gets the name of the renderer associated with the current canvas instance.
    /// </summary>
    public string RendererName => s_rendererName ?? "unknown renderer";

    /// <summary>
    /// Draws the frame.
    /// </summary>
    /// <param name="context">The context.</param>
    public void DrawFrame<TDrawingContext>(TDrawingContext context)
        where TDrawingContext : DrawingContext
    {
#if DEBUG
        if (LiveCharts.EnableLogging)
            Trace.WriteLine(
                $"[core canvas frame drawn] ".PadRight(60) +
                $"thread: {Environment.CurrentManagedThreadId}");
#endif

        var showFps = LiveCharts.RenderingSettings.ShowFPS;
        var drawStartTime = s_clock.ElapsedTicks;
        _lastFrameTimestamp = drawStartTime;

        lock (Sync)
        {
            context.OnBeginDraw();

            var isValid = true;

            var toRemoveGeometries = new List<Tuple<Paint, IDrawnElement>>();

            foreach (var zone in Zones)
            {
                context.OnBeginZone(zone);

                foreach (var task in zone.EnumerateTasks())
                {
                    if (DisableAnimations) task.CompleteTransition(null);
                    task.IsValid = true;

                    context.SelectPaint(task);

                    foreach (var geometry in task.GetGeometries(this))
                    {
                        if (geometry is null) continue;
                        if (DisableAnimations) geometry.CompleteTransition(null);

                        geometry.IsValid = true;

                        if (!task.IsPaused)
                        {
                            context.ActiveOpacity = geometry.Opacity;
                            context.Draw(geometry);
                        }

                        isValid = isValid && geometry.IsValid;

                        if (geometry.IsValid && geometry.RemoveOnCompleted)
                            toRemoveGeometries.Add(
                                new Tuple<Paint, IDrawnElement>(task, geometry));
                    }

                    isValid = isValid && task.IsValid;

                    if (task.RemoveOnCompleted && task.IsValid)
                        zone.RemoveTask(task);

                    context.ClearPaintSelection(task);
                }

                context.OnEndZone(zone);
            }

            foreach (var tuple in toRemoveGeometries)
            {
                tuple.Item1.RemoveGeometryFromPaintTask(this, tuple.Item2);

                // if we removed at least one geometry, we need to redraw the control
                // to ensure it is not present in the next frame
                isValid = false;
            }

            if (showFps)
            {
                MeasureFPS(drawStartTime);

                var sb = new StringBuilder();

#if DEBUG
                sb.Append($"[~~ {_lastKnownFPS:N2} ~~] FPS (DEBUG)");
#else
                sb.Append($"[ {_lastKnownFPS:N2} ] FPS");
#endif
                sb.Append($"`[ {_lastDrawTime:N2}ms / {_averageRenderTime:N2}ms ] render time last / avrg");

                if (s_externalRenderer is null)
                {
                    sb.Append($"`{s_rendererName}");
                    var isVSynced = LiveCharts.RenderingSettings.UseGPU && LiveCharts.RenderingSettings.TryUseVSync;
                    sb.Append($"`[ {(isVSynced ? "VSync" : "VSync disabled")} ] handled by {s_tickerName}");
                }
                else
                {
                    sb.Append($"`{s_externalRenderer}");
                    sb.Append($"`{s_tickerName}");
                }

                if (_jitteredDrawCount > 0)
                    sb.Append(
                            $"`[ {_jitteredDrawCount} ] jittered draws");

                context.LogOnCanvas(sb.ToString());
            }

            IsValid = isValid;

            context.OnEndDraw();
        }

        if (IsValid)
        {
            Validated?.Invoke(this);

            if (showFps)
            {
                // restart the count when the canvas is valid.
                _frames = 0;
                _fspSw = null;
                _totalDrawTime = 0;
                _totalFrames = 0;
                _totalSeconds = 0;
            }
        }

        if (!LiveCharts.RenderingSettings.TryUseVSync)
        {
            var timeInDrawOperation = s_clock.ElapsedTicks - drawStartTime;
            var delay = s_baseFrameDelay.Ticks - timeInDrawOperation;

            TimeSpan frameDelay;

            if (delay <= s_jitterThreshold)
            {
                _jitteredDrawCount++;
                frameDelay = s_baseFrameDelay;
            }
            else
            {
                frameDelay = new TimeSpan(delay);
            }

            _nextFrameDelay = frameDelay;
        }
    }

    /// <summary>
    /// Gets the drawables count.
    /// </summary>
    public int DrawablesCount => Zones.Sum(x => x.CountTasks());

    /// <summary>
    /// Invalidates this instance.
    /// </summary>
    public void Invalidate()
    {
        IsValid = false;
        Invalidated?.Invoke(this);
    }

    /// <summary>
    /// Adds a drawable task.
    /// </summary>
    /// <param name="task">The task.</param>
    /// <param name="style">The paint style.</param>
    /// <param name="zone">The zone.</param>
    public void AddDrawableTask(Paint task, PaintStyle style = PaintStyle.Undefined, int zone = CanvasZone.NoClip)
    {
        if (task == Paint.Default) return;

        if (style != PaintStyle.Undefined)
            task.PaintStyle = style;

        Zones[zone].AddTask(task);
    }

    /// <summary>
    /// Adds a geometry (or geometries) to the canvas.
    /// </summary>
    /// <returns>
    /// The task created to manage the geometries.
    /// </returns>
    public DrawnTask AddGeometry(params IDrawnElement[] geometries)
    {
        var task = new DrawnTask(this, geometries);

        Zones[CanvasZone.NoClip].AddTask(task);

        return task;
    }

    /// <summary>
    /// Removes the paint task.
    /// </summary>
    /// <param name="task">The task.</param>
    public void RemovePaintTask(Paint task)
    {
        var geometriesWithOwnPaints = task.GetGeometries(this)
                .Where(x => (x.Fill ?? x.Stroke ?? x.Paint) is not null);

        foreach (var geometry in geometriesWithOwnPaints)
            geometry.DisposePaints();

        task.ReleaseCanvas(this);

        foreach (var zone in Zones)
            if (zone.RemoveTask(task))
                break;
    }

    /// <summary>
    /// Clears the canvas and tasks.
    /// </summary>
    public void Clear()
    {
        Clean();
        Invalidate();
    }

    /// <summary>
    /// Counts the geometries.
    /// </summary>
    /// <returns></returns>
    public int CountGeometries()
    {
        var count = 0;

        foreach (var task in Zones.SelectMany(x => x.EnumerateTasks()))
            foreach (var geometry in task.GetGeometries(this))
                count++;

        return count;
    }

    /// <summary>
    /// Counts the paint tasks in the canvas.
    /// </summary>
    /// <returns>the paints count in all zones.</returns>
    public int CountPaintTasks()
    {
        var count = 0;
        foreach (var zone in Zones)
            count += zone.CountTasks();
        return count;
    }

    /// <summary>
    /// Checks if the canvas contains a paint task.
    /// </summary>
    /// <param name="task">The task.</param>
    /// <returns>A value indicating if the task is contained.</returns>
    public bool ContainsPaintTask(Paint task)
    {
        foreach (var zone in Zones)
            if (zone.ContainsTask(task))
                return true;

        return false;
    }


    /// <summary>
    /// Releases the resources.
    /// </summary>
    public void Dispose()
    {
        lock (Sync)
        {
            Clean();
            IsValid = true;
        }
    }

    private void Clean()
    {
        foreach (var zone in Zones)
        {
            foreach (var task in zone.EnumerateTasks())
            {
                var geometriesWithOwnPaints = task.GetGeometries(this)
                    .Where(x => (x.Fill ?? x.Stroke ?? x.Paint) is not null);

                foreach (var geometry in geometriesWithOwnPaints)
                    geometry.DisposePaints();

                task.DisposeTask();
                task.ReleaseCanvas(this);
            }

            zone.ClearTasks();
        }

        Zones = [];
    }

    private void MeasureFPS(long drawStartTime)
    {
        if (s_clock.ElapsedMilliseconds < 3000)
            return; // we only start measuring after 3 seconds, to improve accuracy.

        _totalDrawTime += (s_clock.ElapsedTicks - drawStartTime) / s_ticksPerMillisecond;

        if (_fspSw is null)
        {
            _fspSw = new();
            _fspSw.Start();
        }

        const int logEach = 20;
        _frames++;

        if (_frames % logEach == 0)
        {
            var elapsedSeconds = _fspSw.ElapsedMilliseconds / 1000d;

            _totalFrames += logEach;
            _totalSeconds += elapsedSeconds;
            _lastKnownFPS = _totalFrames / _totalSeconds;
            _averageRenderTime = _totalDrawTime / _totalFrames;

            // it not exactly the last frame time, is the 20th frame time
            // so we can actually read the time in the log.
            _lastDrawTime = (s_clock.ElapsedTicks - drawStartTime) / s_ticksPerMillisecond;

            _fspSw.Restart();
        }
    }
}
