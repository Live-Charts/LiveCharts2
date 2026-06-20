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
using System.IO;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView.Drawing;
using SkiaSharp;

namespace LiveChartsCore.SkiaSharpView.SKCharts;

/// <summary>
/// A chart that is able to generate images or draw to a given canvas.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="InMemorySkiaSharpChart"/> class.
/// </remarks>
public abstract class InMemorySkiaSharpChart(IDrawnView? drawnView = null)
{
    private readonly IDrawnView? _drawnView = drawnView;

    static InMemorySkiaSharpChart()
    {
        _ = LiveChartsSkiaSharp.EnsureInitialized();
    }

    /// <inheritdoc cref="IDrawnView.CoreCanvas"/>
    public CoreMotionCanvas CoreCanvas { get; } = new();

    internal bool ExplicitDisposing { get; set; }

    /// <summary>
    /// Gets or sets the background.
    /// </summary>
    /// <value>
    /// The background.
    /// </value>
    public SKColor Background { get; set; } = SKColors.Empty;

    /// <summary>
    /// Gets or sets the height.
    /// </summary>
    /// <value>
    /// The height.
    /// </value>
    public int Height
    {
        get => _drawnView is null ? field : (int)_drawnView.ControlSize.Height;
        set { field = value; WarnSize(); }
    } = 600;

    /// <summary>
    /// Gets or sets the width.
    /// </summary>
    /// <value>
    /// The width.
    /// </value>
    public int Width
    {
        get => _drawnView is null ? field : (int)_drawnView.ControlSize.Width;
        set { field = value; WarnSize(); }
    } = 900;

    /// <summary>
    /// Gets the core chart.
    /// </summary>
    /// <returns>The chart.</returns>
    protected abstract Chart GetCoreChart();

    /// <summary>
    /// Gets the current <see cref="SKSurface"/>.
    /// </summary>
    /// <returns></returns>
    public virtual SKImage GetImage()
    {
        using var surface = SKSurface.Create(new SKImageInfo(Width, Height)) ??
            throw new Exception(
                $"Could not create a valid {nameof(SKSurface)}. This is probably because the image you are " +
                $"building does not have a valid size, if you are building a chart image from a control in the UI," +
                $"ensure the control is loaded before requesting an image from it.");

        using var canvas = surface.Canvas;

        DrawOnCanvas(canvas);

        return surface.Snapshot();
    }

    /// <summary>
    /// Saves the image to the specified path.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="format">The format.</param>
    /// <param name="quality">The quality, an integer from 0 to 100.</param>
    /// <returns></returns>
    public virtual void SaveImage(Stream stream, SKEncodedImageFormat format = SKEncodedImageFormat.Png, int quality = 80)
    {
        using var image = GetImage();
        using var data = image.Encode(format, quality);
        data.SaveTo(stream);
    }

    /// <summary>
    /// Saves the image to the specified path.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="format">The format.</param>
    /// <param name="quality">The quality, an integer from 0 to 100.</param>
    /// <returns></returns>
    public virtual void SaveImage(string path, SKEncodedImageFormat format = SKEncodedImageFormat.Png, int quality = 80)
    {
        using var stream = File.OpenWrite(path);
        SaveImage(stream, format, quality);
    }

    /// <summary>
    /// Draws the image to the specified canvas.
    /// </summary>
    /// <param name="canvas">The canvas.</param>
    public virtual void SaveImage(SKCanvas canvas) =>
        DrawOnCanvas(canvas);

    /// <summary>
    /// Draws the chart to the specified canvas.
    /// </summary>
    /// <param name="canvas">The canvas.</param>
    public virtual void DrawOnCanvas(SKCanvas canvas)
    {
        var coreChart = GetCoreChart();

        if (coreChart is null || coreChart is not Chart skiaChart)
            throw new Exception("Something is missing :(");

        var bg = Background == SKColors.Empty
            ? coreChart.GetTheme().VirtualBackroundColor.AsSKColor()
            : Background;

        if (_drawnView is not null)
        {
            _drawnView.CoreCanvas.DrawFrame(
                new SkiaSharpDrawingContext(CoreCanvas, canvas, bg));
            return;
        }

        skiaChart.Canvas.DisableAnimations = true;

        skiaChart.IsLoaded = true;
        skiaChart._isFirstDraw = true;
        if (ExplicitDisposing) skiaChart.DisableTooltipCache = true;

        skiaChart.Measure();

        skiaChart.Canvas.DrawFrame(
            new SkiaSharpDrawingContext(CoreCanvas, canvas, bg));

        if (!ExplicitDisposing)
            skiaChart.Unload();
    }

    private void WarnSize()
    {
#if DEBUG
        if (_drawnView is null) return;
        throw new InvalidOperationException(
           $"The chart image dimensions are ignored when built from an {nameof(IDrawnView)} instance. " +
           $"If you need the chart in a specific size, please use the parameterless contructor and build " +
           $"the chart from there, or resize the chart in UI first to the desired size.");
#endif
    }
}
