using System;
using System.Threading;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.SkiaSharpView;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.CoreObjectsTests;

[TestClass]
public class CanvasDrawThreadTests
{
    // Avalonia draws frames on its render thread (CoreMotionCanvas.DrawFrame, under the canvas
    // Sync lock) while unloading and paint changes run on the UI thread. RemovePaintTask disposes
    // the native SKPaints of the removed task, and it changed the canvas without that lock, so a
    // frame that was drawing the task used a freed SKPaint: a native crash in SkiaSharp
    // (SIGSEGV / 0xC0000005, seen on a GeoMap unloading). Here one thread draws frames in a loop
    // while another adds and removes a paint task, the two calls from that crash.
    [TestMethod]
    public void AddingAndRemovingPaintTasksWhileAFrameDraws()
    {
        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 300,
            Series = [new ColumnSeries<int>([.. Enumerable(200)]), new LineSeries<int>([.. Enumerable(200)])]
        };

        var coreChart = chart.CoreChart;
        coreChart.IsLoaded = true;
        coreChart.Measure();

        var canvas = chart.CoreCanvas;
        canvas.DisableAnimations = true;

        Exception? drawError = null;
        using var stop = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        var drawThread = StartDrawing(canvas, stop.Token, ex => drawError = ex);

        while (!stop.IsCancellationRequested && drawError is null)
        {
            var paint = new SolidColorPaint(SKColors.Red) { StrokeThickness = 3 };
            canvas.AddDrawableTask(paint);
            // geometries added after the paint is on the canvas: AddGeometryToPaintTask is locked too
            for (var i = 0; i < 20; i++)
                paint.AddGeometryToPaintTask(canvas, new RectangleGeometry { X = i, Y = i, Width = 50, Height = 50 });

            Thread.SpinWait(500);
            canvas.RemovePaintTask(paint);
            paint.DisposeTask();
        }

        drawThread.Join();

        Assert.IsNull(drawError, $"drawing a frame failed while paint tasks were added and removed: {drawError}");
    }

    // The same race through a paint's geometries, the way MapFactory.Dispose tears a GeoMap down on
    // unload: it removes every land from the layer paints (RemoveGeometryFromPaintTask, which frees
    // the land's cached SKPath) and clears them while those paints are still on the canvas, and the
    // Paint geometry methods changed the set DrawFrame enumerates without the canvas lock.
    [TestMethod]
    public void RemovingGeometriesFromAPaintOnTheCanvasWhileAFrameDraws()
    {
        var chart = new SKCartesianChart { Width = 400, Height = 300, Series = [new LineSeries<int>([.. Enumerable(50)])] };
        var coreChart = chart.CoreChart;
        coreChart.IsLoaded = true;
        coreChart.Measure();

        var canvas = chart.CoreCanvas;
        canvas.DisableAnimations = true;

        var paint = new SolidColorPaint(SKColors.Red) { StrokeThickness = 3 };
        canvas.AddDrawableTask(paint);

        Exception? drawError = null;
        using var stop = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        var drawThread = StartDrawing(canvas, stop.Token, ex => drawError = ex);

        while (!stop.IsCancellationRequested && drawError is null)
        {
            var geometries = new RectangleGeometry[40];
            for (var i = 0; i < geometries.Length; i++)
            {
                geometries[i] = new RectangleGeometry { X = i, Y = i, Width = 50, Height = 50 };
                paint.AddGeometryToPaintTask(canvas, geometries[i]);
            }

            Thread.SpinWait(500);

            // the paint stays on the canvas, as the layer paints do in MapFactory.Dispose
            for (var i = 0; i < geometries.Length / 2; i++)
                paint.RemoveGeometryFromPaintTask(canvas, geometries[i]);
            paint.ClearGeometriesFromPaintTask(canvas);
        }

        drawThread.Join();

        Assert.IsNull(drawError, $"drawing a frame failed while a paint's geometries were removed: {drawError}");
    }

    private static Thread StartDrawing(LiveChartsCore.Motion.CoreMotionCanvas canvas, CancellationToken stop, Action<Exception> onError)
    {
        var thread = new Thread(() =>
        {
            try
            {
                using var surface = SKSurface.Create(new SKImageInfo(400, 300));
                while (!stop.IsCancellationRequested)
                    canvas.DrawFrame(new SkiaSharpDrawingContext(canvas, surface.Canvas, SKColor.Empty));
            }
            catch (Exception ex)
            {
                // any exception on the drawing thread is a failure, report it to the test
                onError(ex);
            }
        })
        { IsBackground = true, Name = "render thread (test)" };

        thread.Start();
        return thread;
    }

    private static int[] Enumerable(int count)
    {
        var values = new int[count];
        for (var i = 0; i < count; i++) values[i] = i % 17;
        return values;
    }
}
