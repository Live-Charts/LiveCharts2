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
using System.Globalization;
using LiveChartsCore.Drawing;
using LiveChartsCore.Drawing.Layouts;
using LiveChartsCore.Geo;
using LiveChartsCore.Kernel;
using LiveChartsCore.Measure;
using LiveChartsCore.Motion;
using LiveChartsCore.Painting;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Drawing.Layouts;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.ImageFilters;
using LiveChartsCore.Themes;
using SkiaSharp;

namespace LiveChartsCore.SkiaSharpView.SKCharts;

/// <summary>
/// Defines the default geo map tooltip.
/// </summary>
public class SKDefaultGeoTooltip : Container<PopUpGeometry>, IGeoMapTooltip
{
    private bool _isInitialized;
    private bool _isOpen;
    private object? _themeId;
    private Paint? _lastBackgroundPaint;
    private Paint? _lastTextPaint;
    private DrawnTask? _drawnTask;
    private const int Py = 4;
    private const int Px = 8;

    /// <summary>
    /// Gets or sets the easing function.
    /// </summary>
    public Func<float, float> Easing { get; set; } = EasingFunctions.EaseOut;

    /// <summary>
    /// Gets or sets the animation speed.
    /// </summary>
    public TimeSpan AnimationsSpeed { get; set; } = TimeSpan.FromMilliseconds(150);

    /// <summary>
    /// Gets or sets the wedge size.
    /// </summary>
    public int Wedge { get; set; } = 10;

    /// <inheritdoc cref="IGeoMapTooltip.Show(GeoTooltipPoint, GeoMapChart)"/>
    public virtual void Show(GeoTooltipPoint point, GeoMapChart chart)
    {
        var theme = chart.GetTheme();
        var currentThemeId = theme.ThemeId;

        var bgPaint = chart.View.TooltipBackgroundPaint;
        var textPaint = chart.View.TooltipTextPaint;

        if (!_isInitialized || _themeId != currentThemeId ||
            bgPaint != _lastBackgroundPaint || textPaint != _lastTextPaint)
        {
            Initialize(chart, theme);
            _isInitialized = true;
            _themeId = currentThemeId;
            _lastBackgroundPaint = bgPaint;
            _lastTextPaint = textPaint;
        }

        if (_drawnTask is null || _drawnTask.IsEmpty)
        {
            _drawnTask = chart.View.CoreCanvas.AddGeometry(CanvasZone.Overlay, this);
            _drawnTask.ZIndex = 10100;
        }

        Opacity = 1;
        ScaleTransform = new LvcPoint(1, 1);
        _isOpen = true;

        // Determine placement from view setting, or auto-detect
        var preferredPlacement = chart.View.TooltipPosition switch
        {
            TooltipPosition.Bottom => PopUpPlacement.Bottom,
            TooltipPosition.Top => PopUpPlacement.Top,
            _ => PopUpPlacement.Top // Auto: default to top, flip if needed
        };

        var placement = preferredPlacement;
        var layout = GetLayout(point, chart, theme, placement);
        Content = (IDrawnElement<SkiaSharpDrawingContext>)layout;
        var size = Measure();

        float x, y;

        if (placement == PopUpPlacement.Bottom)
        {
            x = point.LandCenter.X - size.Width / 2f;
            y = point.LandCenter.Y;
        }
        else
        {
            x = point.LandCenter.X - size.Width / 2f;
            y = point.LandCenter.Y - size.Height;
        }

        // Auto-flip if out of bounds (only when not explicitly set)
        if (chart.View.TooltipPosition is TooltipPosition.Auto or TooltipPosition.Hidden)
        {
            if (placement == PopUpPlacement.Top && y < 0)
            {
                placement = PopUpPlacement.Bottom;
                layout = GetLayout(point, chart, theme, placement);
                Content = (IDrawnElement<SkiaSharpDrawingContext>)layout;
                size = Measure();
                y = point.LandCenter.Y;
            }
            else if (placement == PopUpPlacement.Bottom && y + size.Height > chart.View.ControlSize.Height)
            {
                placement = PopUpPlacement.Top;
                layout = GetLayout(point, chart, theme, placement);
                Content = (IDrawnElement<SkiaSharpDrawingContext>)layout;
                size = Measure();
                y = point.LandCenter.Y - size.Height;
            }
        }

        // Clamp horizontal
        var controlSize = chart.View.ControlSize;
        if (x < 0) x = 0;
        if (x + size.Width > controlSize.Width)
            x = controlSize.Width - size.Width;

        // Clamp bottom
        if (y + size.Height > controlSize.Height)
            y = controlSize.Height - size.Height;

        X = x;
        Y = y;

        Geometry.Placement = placement;
        chart.View.CoreCanvas.Invalidate();
    }

    /// <inheritdoc cref="IGeoMapTooltip.Hide(GeoMapChart)"/>
    public virtual void Hide(GeoMapChart chart)
    {
        if (!_isOpen) return;
        _isOpen = false;

        Opacity = 0f;
        ScaleTransform = new LvcPoint(0.85f, 0.85f);
        chart.View.CoreCanvas.Invalidate();
    }

    /// <summary>
    /// Gets the tooltip content layout.
    /// </summary>
    protected virtual Layout<SkiaSharpDrawingContext> GetLayout(
        GeoTooltipPoint point, GeoMapChart chart, Theme theme, PopUpPlacement placement)
    {
        var textSize = (float)chart.View.TooltipTextSize;
        if (textSize <= 0) textSize = theme.TooltipTextSize;

        var fontPaint =
            chart.View.TooltipTextPaint ??
            theme.TooltipTextPaint ??
            new SolidColorPaint(new SKColor(28, 49, 58));

        var lw = (float)LiveCharts.DefaultSettings.MaxTooltipsAndLegendsLabelsWidth;

        // Padding accounts for wedge on the appropriate side
        var padding = placement == PopUpPlacement.Bottom
            ? new Padding(Px, Py + Wedge, Px, Py)
            : new Padding(Px, Py, Px, Py + Wedge);

        var stackLayout = new StackLayout
        {
            Orientation = ContainerOrientation.Vertical,
            HorizontalAlignment = Align.Middle,
            VerticalAlignment = Align.Middle,
            Padding = padding
        };

        // Country name (title-cased since the data stores lowercase names)
        stackLayout.Children.Add(
            new LabelGeometry
            {
                Text = ToTitleCase(point.Land.Name),
                Paint = fontPaint,
                TextSize = textSize,
                Padding = new Padding(0, 0, 0, point.Values.Count > 0 ? 8 : 0),
                MaxWidth = lw,
                VerticalAlign = Align.Start,
                HorizontalAlign = Align.Start
            });

        // One labeled line per series that has a value for this land.
        var formatter = chart.MapView.TooltipFormatter;
        foreach (var v in point.Values)
        {
            stackLayout.Children.Add(
                new LabelGeometry
                {
                    Text = formatter is null ? FormatDefault(v) : formatter(v),
                    Paint = fontPaint,
                    TextSize = textSize,
                    MaxWidth = lw,
                    VerticalAlign = Align.Start,
                    HorizontalAlign = Align.Start
                });
        }

        return stackLayout;
    }

    private static string FormatDefault(GeoTooltipValue v) =>
        string.IsNullOrEmpty(v.Series.Name)
            ? v.Value.ToString("N2")
            : $"{v.Series.Name}: {v.Value:N2}";

    /// <summary>
    /// Called to initialize the tooltip.
    /// </summary>
    protected virtual void Initialize(GeoMapChart chart, Theme theme)
    {
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

        this.Animate(
            new Animation(Easing, AnimationsSpeed),
                OpacityProperty,
                ScaleTransformProperty,
                XProperty,
                YProperty);
    }

    private static string ToTitleCase(string text) =>
        CultureInfo.InvariantCulture.TextInfo.ToTitleCase(text);
}
