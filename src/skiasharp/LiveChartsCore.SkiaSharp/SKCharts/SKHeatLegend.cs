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
using System.ComponentModel;
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
using LiveChartsCore.SkiaSharpView.TypeConverters;
using SkiaSharp;
using Container = LiveChartsCore.SkiaSharpView.Drawing.Layouts.Container;

namespace LiveChartsCore.SkiaSharpView.SKCharts;

/// <summary>
/// Defines the heat legend for a chart.
/// </summary>
public class SKHeatLegend : Container, IChartLegend
{
    private bool _isInitialized;
    private DrawnTask? _drawnTask;

    /// <summary>
    /// Gets or sets the badge margin.
    /// </summary>
    [TypeConverter(typeof(PaddingTypeConverter))]
    public Padding BadgePadding { get; set; } = new(10);

    /// <summary>
    /// Gets or sets the heat badge width, null for auto based on the text size.
    /// </summary>
    public double? BadgeWidth { get; set; } = null;

    /// <summary>
    /// Gets or sets the legend labels formatter.
    /// </summary>
    public Func<double, string> Formatter { get; set; } = Labelers.SixRepresentativeDigits;

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
        if (!_isInitialized)
        {
            Initialize(chart);
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

        var isVertical = chart.LegendPosition is LegendPosition.Left or LegendPosition.Right;

        var layout = new AbsoluteLayout
        {
            Padding = BadgePadding
        };

        var heatSeries = chart.EnumerateHeatLegendSources()
            .FirstOrDefault(x => x.IsVisibleAtLegend);

        var gradient = heatSeries?.HeatMap.Select(x => x.AsSKColor()).ToArray();

        if (heatSeries is null || gradient is null || gradient.Length == 0)
        {
            Hide(chart);
            return layout;
        }

        var minLabel = new LabelGeometry
        {
            Padding = new(8, 4),
            Text = Formatter(heatSeries.WeightBounds.Min),
            Paint = new SolidColorPaint(GetContrastingColor(gradient[0])),
            TextSize = textSize,
            MaxWidth = (float)LiveCharts.DefaultSettings.MaxTooltipsAndLegendsLabelsWidth,
            VerticalAlign = Align.Start,
            HorizontalAlign = Align.Start
        };

        var maxLabel = new LabelGeometry
        {
            Padding = new(8, 4),
            Text = Formatter(heatSeries.WeightBounds.Max),
            Paint = new SolidColorPaint(GetContrastingColor(gradient[gradient.Length - 1])),
            TextSize = textSize,
            MaxWidth = (float)LiveCharts.DefaultSettings.MaxTooltipsAndLegendsLabelsWidth,
            VerticalAlign = Align.Start,
            HorizontalAlign = Align.Start
        };

        var ms = minLabel.Measure();
        var xs = maxLabel.Measure();

        var auto = (float?)BadgeWidth ?? (isVertical
            ? Math.Max(xs.Width, ms.Width)
            : Math.Max(xs.Height, ms.Height));

        var s = isVertical
            ? BadgePadding.Top + BadgePadding.Bottom
            : BadgePadding.Left + BadgePadding.Right;

        layout.Children.Add(
            new RectangleGeometry
            {
                Width = isVertical ? (float)auto : chart.DrawMarginSize.Width - s,
                Height = isVertical ? chart.DrawMarginSize.Height - s : (float)auto,
                Fill = new LinearGradientPaint(
                    gradientStops: gradient,
                    startPoint: isVertical ? new(0.5f, 0f) : new(0, 0.5f),
                    endPoint: isVertical ? new(0.5f, 1f) : new(1, 0.5f))
            });

        layout.Children.Add(minLabel);
        layout.Children.Add(maxLabel);

        if (isVertical)
        {
            minLabel.X = Math.Abs(ms.Width - auto) * 0.5f;
            minLabel.Y = 0;

            maxLabel.X = Math.Abs(xs.Width - auto) * 0.5f;
            maxLabel.Y = chart.DrawMarginSize.Height - s - xs.Height;
        }
        else
        {
            minLabel.X = 0;
            minLabel.Y = Math.Abs(ms.Height - auto) * 0.5f;

            maxLabel.X = chart.DrawMarginSize.Width - s - xs.Width;
            maxLabel.Y = Math.Abs(xs.Height - auto) * 0.5f;
        }

        return new StackLayout
        {
            Padding = BadgePadding,
            Children = [layout]
        };
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

    private static SKColor GetContrastingColor(SKColor color)
    {
        var luma = (299 * color.Red + 587 * color.Green + 114 * color.Blue) / 1000;

        return luma > 128
            ? SKColors.Black
            : SKColors.White;
    }
}
