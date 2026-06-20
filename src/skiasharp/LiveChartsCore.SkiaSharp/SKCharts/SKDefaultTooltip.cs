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
using LiveChartsCore.Drawing;
using LiveChartsCore.Drawing.Layouts;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.Motion;
using LiveChartsCore.Painting;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Drawing.Layouts;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.ImageFilters;
using SkiaSharp;

namespace LiveChartsCore.SkiaSharpView.SKCharts;

/// <summary>
/// Defines the default tooltip.
/// </summary>
public class SKDefaultTooltip : Container<PopUpGeometry>, IChartTooltip
{
    private bool _isInitialized;
    private object? _themeId;
    private DrawnTask? _drawnTask;
    private const int Py = 4;
    private const int Px = 8;

    /// <summary>
    /// Gets or sets the easing function.
    /// </summary>
    public Func<float, float> Easing { get; set; } = EasingFunctions.EaseOut;

    /// <summary>
    /// Gets or sets the animations speed.
    /// </summary>
    public TimeSpan AnimationsSpeed { get; set; } = TimeSpan.FromMilliseconds(150);

    /// <summary>
    /// Gets or sets the wedge.
    /// </summary>
    public int Wedge { get; set; } = 10;

    /// <inheritdoc cref="IChartTooltip.Show(IEnumerable{ChartPoint}, Chart)" />
    public virtual void Show(IEnumerable<ChartPoint> foundPoints, Chart chart)
    {
        var theme = chart.GetTheme();

        if (!_isInitialized || _themeId != theme.ThemeId || chart.View.TooltipBackgroundPaint is not null)
        {
            Initialize(chart);
            _isInitialized = true;
            _themeId = theme.ThemeId;
        }

        if (_drawnTask is null || _drawnTask.IsEmpty)
        {
            _drawnTask = chart.Canvas.AddGeometry(CanvasZone.Overlay, this);
            _drawnTask.ZIndex = 10100;
        }

        Opacity = 1;
        ScaleTransform = new LvcPoint(1, 1);

        var layout = GetLayout(foundPoints, chart);

        // align the wedge with the default placement
        var placement = chart.AutoToolTipsInfo.ToolTipPlacement;
        AlignWedge(placement, layout);

        Content = (IDrawnElement<SkiaSharpDrawingContext>)layout;

        var size = Measure();
        var location = foundPoints.GetTooltipLocation(size, chart);

        // if the placement was corrected by the GetTooltipLocation() function
        // we need to update the wedge
        // this happens when the tooltip is out of the chart bounds,
        // this tooltip has a wedge that points to the point, so we need to align it again.

        if (placement != chart.AutoToolTipsInfo.ToolTipPlacement)
        {
            AlignWedge(chart.AutoToolTipsInfo.ToolTipPlacement, layout);
            size = Measure();
            location = foundPoints.GetTooltipLocation(size, chart);
        }

        X = location.X;
        Y = location.Y;

        chart.Canvas.Invalidate();
    }

    /// <inheritdoc cref="IChartTooltip.Hide"/>
    public virtual void Hide(Chart chart)
    {
        if (chart is null) return;
        Opacity = 0f;
        ScaleTransform = new LvcPoint(0.85f, 0.85f);

        chart.Canvas.Invalidate();
    }

    /// <summary>
    /// Gets the content of the tooltip.
    /// </summary>
    /// <param name="foundPoints">The points to show.</param>
    /// <param name="chart">The chart.</param>
    /// <returns>The content layout.</returns>
    protected virtual Layout<SkiaSharpDrawingContext> GetLayout(
        IEnumerable<ChartPoint> foundPoints, Chart chart)
    {
        var theme = chart.GetTheme();

        var textSize = (float)chart.View.TooltipTextSize;
        if (textSize < 0) textSize = theme.TooltipTextSize;

        var fontPaint =
            chart.View.TooltipTextPaint ??
            theme.TooltipTextPaint ??
            new SolidColorPaint(new SKColor(28, 49, 58));

        var stackLayout = new StackLayout
        {
            Orientation = ContainerOrientation.Vertical,
            HorizontalAlignment = Align.Middle,
            VerticalAlignment = Align.Middle
        };

        var tableLayout = new TableLayout
        {
            HorizontalAlignment = Align.Middle,
            VerticalAlignment = Align.Middle
        };

        var lw = (float)LiveCharts.DefaultSettings.MaxTooltipsAndLegendsLabelsWidth;

        var i = 0;
        foreach (var point in foundPoints)
        {
            var series = point.Context.Series;

            if (i == 0)
            {
                var title = point.Context.Series.GetSecondaryToolTipText(point) ?? string.Empty;

                if (title != LiveCharts.IgnoreToolTipLabel)
                {
                    stackLayout.Children.Add(
                        new LabelGeometry
                        {
                            Text = point.Context.Series.GetSecondaryToolTipText(point) ?? string.Empty,
                            Paint = fontPaint,
                            TextSize = textSize,
                            Padding = new Padding(0, 0, 0, 8),
                            MaxWidth = lw,
                            VerticalAlign = Align.Start,
                            HorizontalAlign = Align.Start
                        });
                }
            }

            var content = point.Context.Series.GetPrimaryToolTipText(point) ?? string.Empty;
            var rtl = LiveChartsSkiaSharp.DefaultTextSettings.IsRTL;

            if (content != LiveCharts.IgnoreToolTipLabel)
            {
                var skiaMiniature = (IDrawnElement<SkiaSharpDrawingContext>)series.GetMiniatureGeometry(point);
                _ = tableLayout.AddChild(skiaMiniature, i, rtl ? 3 : 0);

                if (point.Context.Series.Name != LiveCharts.IgnoreSeriesName)
                    _ = tableLayout.AddChild(new LabelGeometry
                    {
                        Text = point.Context.Series.Name ?? string.Empty,
                        Paint = fontPaint,
                        TextSize = textSize,
                        Padding = new Padding(10, 0),
                        MaxWidth = lw,
                        VerticalAlign = Align.Start,
                        HorizontalAlign = Align.Start
                    }, i, 1, horizontalAlign: rtl ? Align.End : Align.Start);

                _ = tableLayout.AddChild(new LabelGeometry
                {
                    Text = content,
                    Paint = fontPaint,
                    TextSize = textSize,
                    Padding = new Padding(8, 2),
                    MaxWidth = lw,
                    VerticalAlign = Align.Start,
                    HorizontalAlign = Align.Start
                }, i, rtl ? 0 : 2, horizontalAlign: Align.End);

                i++;
            }
        }

        stackLayout.Children.Add(tableLayout);

        return stackLayout;
    }

    /// <summary>
    /// Called to initialize the tooltip.
    /// </summary>
    protected virtual void Initialize(Chart chart)
    {
        var theme = chart.GetTheme();

        var backgroundPaint =
            chart.View.TooltipBackgroundPaint ??
            theme.TooltipBackgroundPaint ??
            new SolidColorPaint(new SKColor(235, 235, 235, 230))
            {
                ImageFilter = new DropShadow(2, 2, 6, 6, new SKColor(50, 0, 0, 100))
            };

        Geometry.Fill = backgroundPaint;
        Geometry.Wedge = Wedge;
        Geometry.WedgeThickness = 3;

        Geometry.Fill = backgroundPaint;

        this.Animate(
            new Animation(Easing, AnimationsSpeed),
                OpacityProperty,
                ScaleTransformProperty,
                XProperty,
                YProperty);
    }

    private void AlignWedge(PopUpPlacement placement, Layout<SkiaSharpDrawingContext> layout)
    {
        Geometry.Placement = placement;

        switch (placement)
        {
            case PopUpPlacement.Top:
                layout.Padding = new Padding(Px, Py, Px, Py + Wedge); break;
            case PopUpPlacement.Bottom:
                layout.Padding = new Padding(Px, Py + Wedge, Px, Py); break;
            case PopUpPlacement.Left:
                layout.Padding = new Padding(Px, Py, Px + Wedge, Py); break;
            case PopUpPlacement.Right:
                layout.Padding = new Padding(Px + Wedge, Py, Px, Py); break;
            default: break;
        }
    }
}
