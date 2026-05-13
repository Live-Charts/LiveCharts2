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
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.ImageFilters;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.Themes;
using LiveChartsCore.VisualElements;
using LiveChartsCore.VisualStates;
using SkiaSharp;

namespace LiveChartsCore.SkiaSharpView;

/// <summary>
/// Defines the light theme extensions.
/// </summary>
public static class ThemesExtensions
{
    /// <summary>
    /// Adds the default theme.
    /// </summary>
    /// <param name="settings">The settings.</param>
    /// <param name="requestedTheme">Indicates the theme kind.</param>
    /// <param name="themeSettings">The adittional theme settings.</param>
    /// <returns>The current LiveCharts settings.</returns>
    public static LiveChartsSettings AddDefaultTheme(
        this LiveChartsSettings settings,
        Action<Theme>? themeSettings = null,
        LvcThemeKind requestedTheme = LvcThemeKind.Unknown)
    {
        return settings
            .HasTheme(theme =>
            {
                _ = theme
                    .OnInitialized(() =>
                    {
                        theme.RequestedTheme = requestedTheme;
                        theme.AnimationsSpeed = TimeSpan.FromMilliseconds(800);
                        theme.EasingFunction = EasingFunctions.ExponentialOut;

                        if (theme.IsDark)
                        {
                            theme.Colors = ColorPalletes.MaterialDesign200;
                            theme.VirtualBackroundColor = new(0, 0, 0);
                            theme.TooltipBackgroundPaint =
                                new SolidColorPaint(new(45, 45, 45, 230))
                                {
                                    ImageFilter = new DropShadow(4, 4, 12, 12, new(0, 0, 0, 255))
                                };
                            theme.TooltipTextPaint = new SolidColorPaint(new(245, 245, 245));
                            theme.LegendTextPaint = new SolidColorPaint(new(245, 245, 245));
                        }
                        else
                        {
                            theme.Colors = ColorPalletes.MaterialDesign500;
                            theme.VirtualBackroundColor = new(255, 255, 255);
                            theme.TooltipBackgroundPaint =
                                new SolidColorPaint(new(235, 235, 235, 230))
                                {
                                    ImageFilter = new DropShadow(2, 2, 6, 6, new(0, 0, 0, 100))
                                };
                            theme.TooltipTextPaint = new SolidColorPaint(new(30, 30, 30));
                            theme.LegendTextPaint = new SolidColorPaint(new(30, 30, 30));
                        }
                    })
                    .HasDefaultTooltip(() => new SKDefaultTooltip())
                    .HasDefaultLegend(() => new SKDefaultLegend())
                    .HasRuleForAxes(axis =>
                    {
                        axis.TextSize = 16;
                        axis.ShowSeparatorLines = true;
                        axis.NamePaint = new SolidColorPaint(theme.IsDark ? new(235, 235, 235) : new(35, 35, 35));
                        axis.LabelsPaint = new SolidColorPaint(theme.IsDark ? new(200, 200, 200) : new(70, 70, 70));

                        SKColor lineColor = theme.IsDark ? new(90, 90, 90) : new(235, 235, 235);

                        if (axis is ICartesianAxis cartesian)
                        {
                            axis.SeparatorsPaint = cartesian.Orientation == AxisOrientation.X
                                ? null
                                : new SolidColorPaint(lineColor);
                            cartesian.Padding = new Padding(12);
                        }
                        else if (axis is IPolarAxis polar)
                        {
                            polar.LabelsBackground = theme.IsDark ? new(0, 0, 0) : new(255, 255, 255);
                            axis.SeparatorsPaint = new SolidColorPaint(lineColor);
                        }
                        else
                        {
                            axis.SeparatorsPaint = new SolidColorPaint(lineColor);
                        }
                    })
                    .HasRuleForAnySeries(series =>
                    {
                        series.Name = LiveCharts.IgnoreSeriesName;

                        if (series.ShowDataLabels)
                            series.DataLabelsPaint = theme.IsDark
                                ? new SolidColorPaint(new(245, 245, 245))
                                : new SolidColorPaint(new(45, 45, 45));

                        _ = series.HasState("Hover", [
                                (nameof(DrawnGeometry.Opacity), 0.8f)
                            ]);
                    })
                    .HasRuleForLineSeries(lineSeries =>
                    {
                        // Order-dependent: write Stroke first (no-op when the user
                        // touched it — gated by CanSetProperty), then read the resulting
                        // Stroke back as the series identity color for Fill and marker
                        // defaults. That makes the area Fill, legend miniature, and
                        // per-point markers all match the line whether it's user-set or
                        // theme-rotated. Keeps theme switches (dark/light) correct
                        // because the second-apply Stroke is rewritten to the new
                        // palette before we derive from it. See #2064.
                        var paletteColor = theme.GetSeriesColor(lineSeries).AsSKColor();

                        lineSeries.Stroke = new SolidColorPaint(paletteColor, 4);

                        var identityColor = lineSeries.Stroke is SolidColorPaint actualStroke
                            ? actualStroke.Color
                            : paletteColor;

                        lineSeries.Fill = new SolidColorPaint(identityColor.WithAlpha(50));
                        lineSeries.GeometrySize = 12;
                        lineSeries.GeometryStroke = new SolidColorPaint(identityColor, 4);
                        lineSeries.GeometryFill =
                            new SolidColorPaint(theme.IsDark ? new(30, 30, 30) : new(250, 250, 250));

                        if (lineSeries.ShowError)
                            lineSeries.ErrorPaint = theme.IsDark
                                ? new SolidColorPaint(new(245, 245, 245))
                                : new SolidColorPaint(new(45, 45, 45));

                        _ = lineSeries.HasState("Hover", [
                                (nameof(DrawnGeometry.ScaleTransform), new LvcPoint(1.35f, 1.35f))
                            ]);
                    })
                    .HasRuleForStepLineSeries(steplineSeries =>
                    {
                        var paletteColor = theme.GetSeriesColor(steplineSeries).AsSKColor();

                        steplineSeries.Stroke = new SolidColorPaint(paletteColor, 4);

                        var identityColor = steplineSeries.Stroke is SolidColorPaint actualStroke
                            ? actualStroke.Color
                            : paletteColor;

                        steplineSeries.Fill = new SolidColorPaint(identityColor.WithAlpha(50));
                        steplineSeries.GeometrySize = 12;
                        steplineSeries.GeometryStroke = new SolidColorPaint(identityColor, 4);
                        steplineSeries.GeometryFill =
                            new SolidColorPaint(theme.IsDark ? new(30, 30, 30) : new(250, 250, 250));

                        _ = steplineSeries.HasState("Hover", [
                                (nameof(DrawnGeometry.ScaleTransform), new LvcPoint(1.35f, 1.35f))
                            ]);
                    })
                    .HasRuleForStackedLineSeries(stackedLine =>
                    {
                        var color = theme.GetSeriesColor(stackedLine).AsSKColor();

                        stackedLine.GeometrySize = 0;
                        stackedLine.GeometryStroke = null;
                        stackedLine.GeometryFill = null;
                        stackedLine.Stroke = null;
                        stackedLine.Fill = new SolidColorPaint(color);
                    })
                    .HasRuleForBarSeries(barSeries =>
                    {
                        var color = theme.GetSeriesColor(barSeries).AsSKColor();

                        barSeries.Stroke = null;
                        barSeries.Fill = new SolidColorPaint(color);
                        barSeries.Rx = 3;
                        barSeries.Ry = 3;

                        if (barSeries.ShowDataLabels)
                            barSeries.DataLabelsPaint =
                                barSeries.DataLabelsPosition == DataLabelsPosition.Middle
                                    ? theme.IsDark
                                        ? new SolidColorPaint(new(45, 45, 45))
                                        : new SolidColorPaint(new(245, 245, 245))
                                    : theme.IsDark
                                        ? new SolidColorPaint(new(245, 245, 245))
                                        : new SolidColorPaint(new(45, 45, 45));

                        if (barSeries.ShowError)
                            barSeries.ErrorPaint = theme.IsDark
                                ? new SolidColorPaint(new(245, 245, 245))
                                : new SolidColorPaint(new(45, 45, 45));
                    })
                    .HasRuleForStackedBarSeries(stackedBarSeries =>
                    {
                        var color = theme.GetSeriesColor(stackedBarSeries).AsSKColor();

                        stackedBarSeries.Stroke = null;
                        stackedBarSeries.Fill = new SolidColorPaint(color);
                        stackedBarSeries.Rx = 0;
                        stackedBarSeries.Ry = 0;
                        stackedBarSeries.DataLabelsPosition = DataLabelsPosition.Middle;

                        if (stackedBarSeries.ShowDataLabels)
                            stackedBarSeries.DataLabelsPaint =
                                theme.IsDark
                                    ? new SolidColorPaint(new(45, 45, 45))
                                    : new SolidColorPaint(new(245, 245, 245));
                    })
                    .HasRuleForStackedStepLineSeries(stackedStep =>
                    {
                        var color = theme.GetSeriesColor(stackedStep).AsSKColor();

                        stackedStep.GeometrySize = 0;
                        stackedStep.GeometryStroke = null;
                        stackedStep.GeometryFill = null;
                        stackedStep.Stroke = null;
                        stackedStep.Fill = new SolidColorPaint(color);
                    })
                    .HasRuleForBoxSeries(boxSeries =>
                    {
                        var color = theme.GetSeriesColor(boxSeries).AsSKColor();

                        boxSeries.MaxBarWidth = 60;
                        boxSeries.Stroke =
                            new SolidColorPaint(theme.IsDark ? new(220, 220, 220) : new(30, 30, 30), 2);
                        boxSeries.Fill = new SolidColorPaint(color);
                    })
                    .HasRuleForHeatSeries(heatSeries =>
                    {
                        // ... rules here
                    })
                    .HasRuleForFinancialSeries(financialSeries =>
                    {
                        financialSeries.UpFill = new SolidColorPaint(new(139, 195, 74, 255));
                        financialSeries.UpStroke = new SolidColorPaint(new(139, 195, 74, 255), 3);
                        financialSeries.DownFill = new SolidColorPaint(new(239, 83, 80, 255));
                        financialSeries.DownStroke = new SolidColorPaint(new(239, 83, 80, 255), 3);
                    })
                    .HasRuleForScatterSeries(scatterSeries =>
                    {
                        var color = theme.GetSeriesColor(scatterSeries).AsSKColor();

                        scatterSeries.Stroke = null;
                        scatterSeries.Fill = new SolidColorPaint(color.WithAlpha(200));

                        if (scatterSeries.ShowError)
                            scatterSeries.ErrorPaint = theme.IsDark
                                ? new SolidColorPaint(new(245, 245, 245))
                                : new SolidColorPaint(new(45, 45, 45));
                    })
                    .HasRuleForPieSeries(pieSeries =>
                    {
                        var color = theme.GetSeriesColor(pieSeries).AsSKColor();

                        pieSeries.Stroke = null;
                        pieSeries.Fill = new SolidColorPaint(color);

                        if (pieSeries.ShowDataLabels)
                            pieSeries.DataLabelsPaint =
                                pieSeries.DataLabelsPosition == PolarLabelsPosition.Outer
                                    ? theme.IsDark
                                        ? new SolidColorPaint(new(245, 245, 245))
                                        : new SolidColorPaint(new(45, 45, 45))
                                    : theme.IsDark
                                        ? new SolidColorPaint(new(45, 45, 45))
                                        : new SolidColorPaint(new(245, 245, 245));

                        _ = pieSeries.HasState("Hover", [
                            (nameof(DoughnutGeometry.PushOut), (float)pieSeries.HoverPushout),
                            (nameof(DoughnutGeometry.Opacity), 0.8f)
                        ]);
                    })
                    .HasRuleForPolarLineSeries(polarLine =>
                    {
                        var paletteColor = theme.GetSeriesColor(polarLine).AsSKColor();

                        polarLine.Stroke = new SolidColorPaint(paletteColor, 4);

                        var identityColor = polarLine.Stroke is SolidColorPaint actualStroke
                            ? actualStroke.Color
                            : paletteColor;

                        polarLine.Fill = new SolidColorPaint(identityColor.WithAlpha(50));
                        polarLine.GeometrySize = 12;
                        polarLine.GeometryStroke = new SolidColorPaint(identityColor, 4);
                        polarLine.GeometryFill =
                            new SolidColorPaint(theme.IsDark ? new(30, 30, 30) : new(250, 250, 250));

                        _ = polarLine.HasState("Hover", [
                                (nameof(DrawnGeometry.ScaleTransform), new LvcPoint(1.35f, 1.35f))
                            ]);
                    })
                    .HasRuleForGaugeSeries(gaugeSeries =>
                    {
                        var color = theme.GetSeriesColor(gaugeSeries).AsSKColor();

                        gaugeSeries.Stroke = null;
                        gaugeSeries.Fill = new SolidColorPaint(color);
                        gaugeSeries.DataLabelsPosition = PolarLabelsPosition.ChartCenter;
                        gaugeSeries.DataLabelsPaint =
                            new SolidColorPaint(theme.IsDark ? new(200, 200, 200) : new(70, 70, 70));
                        gaugeSeries.CornerRadius = 8;
                    })
                    .HasRuleForGaugeFillSeries(gaugeFill =>
                    {
                        gaugeFill.Fill =
                            new SolidColorPaint(theme.IsDark ? new(255, 255, 255, 30) : new(30, 30, 30, 10));
                    })
                    .HasRuleFor<BaseLabelVisual>(label =>
                    {
                        label.Paint =
                            new SolidColorPaint(theme.IsDark ? new(200, 200, 200) : new(30, 30, 30));
                    })
                    .HasRuleFor<BaseNeedleVisual>(needle =>
                    {
                        needle.Width = 20;
                        needle.Fill =
                            new SolidColorPaint(theme.IsDark ? new(200, 200, 200) : new(30, 30, 30));
                    })
                    .HasRuleFor<BaseAngularTicksVisual>(ticks =>
                    {
                        ticks.Stroke =
                            new SolidColorPaint(theme.IsDark ? new(200, 200, 200) : new(30, 30, 30));
                        ticks.LabelsPaint =
                            new SolidColorPaint(theme.IsDark ? new(200, 200, 200) : new(30, 30, 30));
                    });

                if (themeSettings is not null)
                    themeSettings(theme);
            });
    }

    /// <summary>
    /// Adds the light theme.
    /// </summary>
    /// <param name="settings">The settings.</param>
    /// /// <param name="themeSettings">The adittional theme settings.</param>
    /// <returns>The current LiveCharts settings.</returns>
    public static LiveChartsSettings AddLightTheme(
        this LiveChartsSettings settings,
        Action<Theme>? themeSettings = null) =>
            settings.AddDefaultTheme(themeSettings, LvcThemeKind.Light);

    /// <summary>
    /// Adds the dark theme.
    /// </summary>
    /// <param name="settings">The settings.</param>
    /// /// <param name="themeSettings">The adittional theme settings.</param>
    /// <returns>The current LiveCharts settings.</returns>
    public static LiveChartsSettings AddDarkTheme(
        this LiveChartsSettings settings,
        Action<Theme>? themeSettings = null) =>
            settings.AddDefaultTheme(themeSettings, LvcThemeKind.Dark);
}
