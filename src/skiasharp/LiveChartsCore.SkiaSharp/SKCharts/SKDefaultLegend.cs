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
using System.Linq;
using LiveChartsCore.Drawing;
using LiveChartsCore.Drawing.Layouts;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.Motion;
using LiveChartsCore.Painting;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Drawing.Layouts;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace LiveChartsCore.SkiaSharpView.SKCharts;

/// <summary>
/// Defines the default legend for a chart.
/// </summary>
public class SKDefaultLegend : Container, IChartLegend
{
    private bool _isInitialized;
    private object? _themeId;
    private DrawnTask? _drawnTask;

    /// <summary>
    /// Gets or sets the easing function.
    /// </summary>
    public Func<float, float> Easing { get; set; } = EasingFunctions.EaseOut;

    /// <summary>
    /// Gets or sets the animations speed.
    /// </summary>
    public TimeSpan AnimationsSpeed { get; set; } = TimeSpan.FromMilliseconds(150);

    /// <inheritdoc cref="IChartLegend.Draw(Chart)"/>
    public virtual void Draw(Chart chart)
    {
        var theme = chart.GetTheme();

        if (!_isInitialized || _themeId != theme.ThemeId || chart.View.LegendBackgroundPaint is not null)
        {
            Initialize(chart);
            _themeId = theme.ThemeId;
            _isInitialized = true;
        }

        if (_drawnTask is null || _drawnTask.IsEmpty)
        {
            _drawnTask = chart.Canvas.AddGeometry(CanvasZone.Overlay, this);
            _drawnTask.ZIndex = 10099;
        }

        var legendPosition = chart.GetLegendPosition();

        X = legendPosition.X;
        Y = legendPosition.Y;

        if (chart.LegendPosition == LegendPosition.Hidden && _drawnTask is not null)
        {
            chart.Canvas.RemovePaintTask(_drawnTask);
            _drawnTask = null;
        }
    }

    /// <inheritdoc cref="IChartLegend.Measure(Chart)"/>
    public virtual LvcSize Measure(Chart chart)
    {
        Content = (IDrawnElement<SkiaSharpDrawingContext>)GetLayout(chart);

        return Measure();
    }

    /// <inheritdoc cref="IChartLegend.Hide(Chart)"/>
    public virtual void Hide(Chart chart)
    {
        if (_drawnTask is not null)
        {
            chart.Canvas.RemovePaintTask(_drawnTask);
            _drawnTask = null;
        }
    }

    /// <summary>
    /// Gets the content of the legend.
    /// </summary>
    /// <param name="chart">The chart.</param>
    /// <returns>The content layout.</returns>
    protected virtual Layout<SkiaSharpDrawingContext> GetLayout(Chart chart)
    {
        var theme = chart.GetTheme();

        var textSize = (float)chart.View.LegendTextSize;
        if (textSize < 0) textSize = theme.LegendTextSize;

        var fontPaint =
            chart.View.LegendTextPaint ??
            theme.LegendTextPaint ??
            new SolidColorPaint(new SKColor(30, 30, 30, 255));

        var rtl = LiveChartsSkiaSharp.DefaultTextSettings.IsRTL;

        var stackLayout = new StackLayout
        {
            Padding = new Padding(15, 4),
            HorizontalAlignment = rtl ? Align.End : Align.Start,
            VerticalAlignment = Align.Middle,
            Orientation = chart.LegendPosition is LegendPosition.Left or LegendPosition.Right
                ? ContainerOrientation.Vertical
                : ContainerOrientation.Horizontal
        };

        if (stackLayout.Orientation == ContainerOrientation.Horizontal)
        {
            stackLayout.MaxWidth = chart.ControlSize.Width;
            stackLayout.MaxHeight = double.MaxValue;
        }
        else
        {
            stackLayout.MaxWidth = double.MaxValue;
            stackLayout.MaxHeight = chart.ControlSize.Height;
        }

        foreach (var series in chart.Series.Where(x => x.IsVisibleAtLegend))
        {
            var miniature = (IDrawnElement<SkiaSharpDrawingContext>)series.GetMiniatureGeometry(null);
            var label = new LabelGeometry
            {
                Text = series.Name ?? string.Empty,
                Paint = fontPaint,
                TextSize = textSize,
                Padding = new Padding(8, 2, 8, 2),
                MaxWidth = (float)LiveCharts.DefaultSettings.MaxTooltipsAndLegendsLabelsWidth,
                VerticalAlign = Align.Start,
                HorizontalAlign = Align.Start
            };

            stackLayout.Children.Add(new StackLayout
            {
                Padding = new Padding(12, 6),
                VerticalAlignment = Align.Middle,
                HorizontalAlignment = Align.Middle,
                Children = rtl
                    ? [label, miniature]
                    : [miniature, label]
            });
        }

        return stackLayout;
    }

    /// <summary>
    /// Called to initialize the tooltip.
    /// </summary>
    protected virtual void Initialize(Chart chart)
    {
        Geometry.Fill =
            chart.View.LegendBackgroundPaint ??
            chart.GetTheme().LegendBackgroundPaint;
    }
}
