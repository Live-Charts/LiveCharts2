// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Console.Painting;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.Painting;
using LiveChartsCore.Themes;
using LiveChartsCore.VisualStates;
using LvcEasings = LiveChartsCore.EasingFunctions;

namespace LiveChartsCore.Console;

public static class LiveChartsConsole
{
    internal static LiveChartsSettings EnsureInitialized()
    {
        LiveCharts.Configure(settings =>
        {
            if (!LiveCharts.DefaultSettings.HasBackedDefined) _ = settings.AddConsole();
            if (!LiveCharts.DefaultSettings.HasThemeDefined) _ = settings.AddConsoleDefaultTheme();
            if (!LiveCharts.DefaultSettings.HasMappersDefined) _ = settings.AddDefaultMappers();
        });
        return LiveCharts.DefaultSettings;
    }

    public static LiveChartsSettings AddConsole(this LiveChartsSettings settings) =>
        settings.HasProvider(new ConsoleProvider());

    public static LiveChartsSettings AddConsoleDefaultTheme(this LiveChartsSettings settings) =>
        settings
            .HasTheme(theme => theme
                .OnInitialized(() =>
                {
                    theme.AnimationsSpeed = TimeSpan.FromMilliseconds(500);
                    theme.EasingFunction = LvcEasings.ExponentialOut;
                    theme.Colors = ColorPalletes.MaterialDesign500;
                    theme.VirtualBackroundColor = new(0, 0, 0);
                })
                .HasDefaultTooltip(() => new ConsoleTooltip())
                .HasDefaultLegend(() => new ConsoleLegend())
                .HasRuleForAxes(axis =>
                {
                    // TextSize is informational here — the axis layout reserves space using the
                    // LabelGeometry size which is driven by the render-mode glyph dimensions.
                    axis.TextSize = Drawing.Geometries.LabelGeometry.GlyphPixelsH;
                    axis.ShowSeparatorLines = true;
                    axis.LabelsPaint = new SolidColorPaint(new(180, 180, 180));

                    // Axis name (rendered when axis.Name is set). Slightly brighter than the
                    // tick labels so the title reads as a header above/beside them. Engine
                    // reserves space for this via DrawMargin when Name is non-null.
                    axis.NamePaint = new SolidColorPaint(new(220, 220, 220));
                    axis.NameTextSize = Drawing.Geometries.LabelGeometry.GlyphPixelsH;

                    var lineColor = new LvcColor(80, 80, 80);

                    if (axis is ICartesianAxis cartesian)
                    {
                        axis.SeparatorsPaint = cartesian.Orientation == AxisOrientation.X
                            ? null
                            : new SolidColorPaint(lineColor);

                        cartesian.Padding = new Padding(2);
                    }
                    else
                    {
                        axis.SeparatorsPaint = new SolidColorPaint(lineColor);
                    }
                })
                .HasRuleForAnySeries(series =>
                {
                    // Series.OnPointerEnter / OnPointerLeft do VisualStates["Hover"] without
                    // a TryGetValue, so a missing Hover state throws KeyNotFoundException
                    // during tooltip dispatch. We don't want any visual effect on hover
                    // (terminal pixels are too coarse for scale/opacity changes to read), so
                    // register Hover with no setters — SetState then iterates an empty list
                    // and just records that the state is active.
                    _ = series.HasState("Hover", []);
                })
                .HasRuleForLineSeries(line =>
                {
                    var color = theme.GetSeriesColor(line);
                    // If the user pre-set Stroke (typically to control StrokeThickness),
                    // mutate the existing paint's color in-place rather than reassigning —
                    // the setter is rejected by CanSetProperty since user-set tracking marks
                    // Stroke as touched, so the theme's assignment would be a no-op and
                    // we'd be left with the user's placeholder color.
                    if (line.Stroke is SolidColorPaint existing)
                        existing.Color = color;
                    else
                        line.Stroke = new SolidColorPaint(color, 1f);
                    line.Fill = null;
                    line.GeometrySize = 0; // hide default point geometries.
                    line.GeometryStroke = null;
                    line.GeometryFill = null;
                })
                .HasRuleForStepLineSeries(step =>
                {
                    var color = theme.GetSeriesColor(step);
                    if (step.Stroke is SolidColorPaint existing)
                        existing.Color = color;
                    else
                        step.Stroke = new SolidColorPaint(color, 1f);
                    step.Fill = null;
                    step.GeometrySize = 0;
                    step.GeometryStroke = null;
                    step.GeometryFill = null;
                })
                .HasRuleForStackedLineSeries(stacked =>
                {
                    // The base line rule nulls Fill — for stacked area we want the band visible.
                    // ILineSeries is a sibling of IStackedLineSeries (per the SeriesProperties
                    // bitmask check in Theme.cs), so this rule fires after HasRuleForLineSeries
                    // and overwrites just the Fill assignment.
                    var color = theme.GetSeriesColor(stacked);
                    stacked.Fill = new SolidColorPaint(color);
                })
                .HasRuleForStackedStepLineSeries(stacked =>
                {
                    var color = theme.GetSeriesColor(stacked);
                    stacked.Fill = new SolidColorPaint(color);
                })
                .HasRuleForBarSeries(bar =>
                {
                    var color = theme.GetSeriesColor(bar);
                    bar.Stroke = null;
                    bar.Fill = new SolidColorPaint(color);
                    bar.Rx = 1;
                    bar.Ry = 1;
                    // Hover dims the individual bar by ~30% — at terminal pixel density a
                    // scale-up barely reads (a 1.35x scale on a 4-pixel-wide bar still looks
                    // like a 4-pixel-wide bar after rounding), but opacity blends visibly
                    // toward the background.
                    _ = bar.HasState("Hover", [(nameof(DrawnGeometry.Opacity), 0.7f)]);
                })
                .HasRuleForStackedBarSeries(stacked =>
                {
                    // IStackedBarSeries is a sibling of IBarSeries, not a child — without this
                    // rule stacked column/row series get no Stroke/Fill set and render nothing.
                    var color = theme.GetSeriesColor(stacked);
                    stacked.Stroke = null;
                    stacked.Fill = new SolidColorPaint(color);
                    stacked.Rx = 0;
                    stacked.Ry = 0;
                    _ = stacked.HasState("Hover", [(nameof(DrawnGeometry.Opacity), 0.7f)]);
                })
                .HasRuleForScatterSeries(scatter =>
                {
                    var color = theme.GetSeriesColor(scatter);
                    scatter.Stroke = null;
                    scatter.Fill = new SolidColorPaint(color);
                    scatter.GeometrySize = 8;
                    _ = scatter.HasState("Hover", [(nameof(DrawnGeometry.Opacity), 0.7f)]);
                })
                .HasRuleForFinancialSeries(financial =>
                {
                    // Default candle colors don't matter much here — the series toggles UpFill/
                    // DownFill internally based on (open, close). We just provide them.
                    financial.UpFill = new SolidColorPaint(new(120, 220, 120));
                    financial.UpStroke = new SolidColorPaint(new(120, 220, 120), 1f);
                    financial.DownFill = new SolidColorPaint(new(220, 120, 120));
                    financial.DownStroke = new SolidColorPaint(new(220, 120, 120), 1f);
                    _ = financial.HasState("Hover", [(nameof(DrawnGeometry.Opacity), 0.7f)]);
                })
                .HasRuleForBoxSeries(box =>
                {
                    var color = theme.GetSeriesColor(box);
                    box.Stroke = new SolidColorPaint(color, 1f);
                    box.Fill = null;
                    _ = box.HasState("Hover", [(nameof(DrawnGeometry.Opacity), 0.7f)]);
                })
                .HasRuleForPieSeries(pie =>
                {
                    var color = theme.GetSeriesColor(pie);
                    pie.Stroke = null;
                    pie.Fill = new SolidColorPaint(color);

                    // Match SkiaSharp's default HoverPushout (20) — at terminal pixel
                    // densities with a typical 1200x660 Sixel surface and a ~300px pie
                    // radius, 8 read as ~3% of the radius and was barely perceptible.
                    pie.HoverPushout = 20;
                    _ = pie.HasState("Hover", [
                        (nameof(BaseDoughnutGeometry.PushOut), (float)pie.HoverPushout),
                        (nameof(DrawnGeometry.Opacity), 0.8f),
                    ]);
                })
                .HasRuleForPolarLineSeries(polar =>
                {
                    var color = theme.GetSeriesColor(polar);
                    polar.Stroke = new SolidColorPaint(color, 1f);
                    polar.Fill = null;
                    polar.GeometrySize = 0;
                    polar.GeometryStroke = null;
                    polar.GeometryFill = null;
                }));
}

