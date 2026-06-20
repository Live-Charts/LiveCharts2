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
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Painting;

namespace LiveChartsCore.Themes;

/// <summary>
/// Defines a style builder.
/// </summary>
public class Theme
{
    private readonly object _lightId = new();
    private readonly object _darkId = new();
    private bool _initialized = false;
    private bool _lastKnownDarkMode = false;
    internal LvcThemeKind _themeRequest = LvcThemeKind.Unknown;

    /// <summary>
    /// Gets the theme id.
    /// </summary>
    public object ThemeId => IsDark ? _darkId : _lightId;

    /// <summary>
    /// Gets a value indicating whether the theme is dark.
    /// When the <see cref="RequestedTheme"/> is Unknown, the theme is determined by the system settings.
    /// </summary>
    public bool IsDark
    {
        get => RequestedTheme == LvcThemeKind.Unknown
            ? field
            : RequestedTheme == LvcThemeKind.Dark;
        private set;
    }

    /// <summary>
    /// Gets or sets the theme request.
    /// </summary>
    public LvcThemeKind RequestedTheme { get; set; } = LvcThemeKind.Unknown;

    /// <summary>
    /// Gets or sets the theme colors.
    /// </summary>
    public LvcColor[] Colors { get; set; } = [];

    /// <summary>
    /// Gets or sets the virtual background color,
    /// it means the color to use to clear the canvas before drawing,
    /// if the control has a background color set, this property will be ignored.
    /// </summary>
    public LvcColor VirtualBackroundColor { get; set; } = new(255, 255, 255);

    /// <summary>
    /// Gets or sets the default easing function.
    /// </summary>
    /// <value>
    /// The default easing function.
    /// </value>
    public Func<float, float> EasingFunction { get; set; } = EasingFunctions.ExponentialOut;

    /// <summary>
    /// Gets or sets the default animations speed.
    /// </summary>
    /// <value>
    /// The default animations speed.
    /// </value>
    public TimeSpan AnimationsSpeed { get; set; } = TimeSpan.FromMilliseconds(800);

    /// <summary>
    /// Gets or sets the tooltip text size.
    /// </summary>
    public float TooltipTextSize { get; set; } = 16f;

    /// <summary>
    /// Gets or sets the default tooltip text paint.
    /// </summary>
    public Paint? TooltipTextPaint { get; set; }

    /// <summary>
    /// Gets or sets the default tooltip background paint.
    /// </summary>
    public Paint? TooltipBackgroundPaint { get; set; }

    /// <summary>
    /// Gets or sets the default legend background paint.
    /// </summary>
    public Paint? LegendBackgroundPaint { get; set; }

    /// <summary>
    /// Gets or sets the default legend text paint.
    /// </summary>
    public Paint? LegendTextPaint { get; set; }

    /// <summary>
    /// Gets or sets the default legend text size.
    /// </summary>
    public float LegendTextSize { get; set; } = 16;

    /// <summary>
    /// Called when the theme changes.
    /// </summary>
    public List<Action> Initialized { get; set; } = [];

    /// <summary>
    /// Gets or sets the axis builder.
    /// </summary>
    /// <value>
    /// The axis builder.
    /// </value>
    public List<Action<IPlane>> AxisBuilder { get; set; } = [];

    /// <summary>
    /// Gets or sets the draw margin frame getter.
    /// </summary>
    /// <value>
    /// The draw margin frame builder.
    /// </value>
    public Func<CoreDrawMarginFrame?>? DrawMarginFrameGetter { get; set; }

    /// <summary>
    /// Gets or sets the draw margin frame builder.
    /// </summary>
    public List<Action<CoreDrawMarginFrame>> DrawMarginFrameBuilder { get; set; } = [];

    /// <summary>
    /// Gets or sets the series builder.
    /// </summary>
    /// <value>
    /// The pie series builder.
    /// </value>
    public List<Action<ISeries>> SeriesBuilder { get; set; } = [];

    /// <summary>
    /// Gets or sets the pie series builder.
    /// </summary>
    /// <value>
    /// The pie series builder.
    /// </value>
    public List<Action<IPieSeries>> PieSeriesBuilder { get; set; } = [];

    /// <summary>
    /// Gets or sets the gauge series builder.
    /// </summary>
    /// <value>
    /// The pie series builder.
    /// </value>
    public List<Action<IPieSeries>> GaugeSeriesBuilder { get; set; } = [];

    /// <summary>
    /// Gets or sets the gauge fill series builder.
    /// </summary>
    /// <value>
    /// The pie series builder.
    /// </value>
    public List<Action<IPieSeries>> GaugeFillSeriesBuilder { get; set; } = [];

    /// <summary>
    /// Theme rules applied to sankey series. Mirrors the
    /// <see cref="PieSeriesBuilder"/> shape so the same defaults-system
    /// pattern (palette colors, default LinkFill alpha) works for sankey.
    /// </summary>
    public List<Action<ISankeySeries>> SankeySeriesBuilder { get; set; } = [];

    /// <summary>
    /// Gets or sets the Cartesian series builder.
    /// </summary>
    /// <value>
    /// The pie series builder.
    /// </value>
    public List<Action<ICartesianSeries>> CartesianSeriesBuilder { get; set; } = [];

    /// <summary>
    /// Gets or sets the stepline series builder.
    /// </summary>
    /// <value>
    /// The pie series builder.
    /// </value>
    public List<Action<IStepLineSeries>> StepLineSeriesBuilder { get; set; } = [];

    /// <summary>
    /// Gets or sets the  stacked stepline series builder.
    /// </summary>
    /// <value>
    /// The pie series builder.
    /// </value>
    public List<Action<IStepLineSeries>> StackedStepLineSeriesBuilder { get; set; } = [];

    /// <summary>
    /// Gets or sets the line series builder.
    /// </summary>
    /// <value>
    /// The pie series builder.
    /// </value>
    public List<Action<ILineSeries>> LineSeriesBuilder { get; set; } = [];

    /// <summary>
    /// Gets or sets the line series builder.
    /// </summary>
    /// <value>
    /// The polar series builder.
    /// </value>
    public List<Action<IPolarSeries>> PolarSeriesBuilder { get; set; } = [];

    /// <summary>
    /// Gets or sets the line series builder.
    /// </summary>
    /// <value>
    /// The polar series builder.
    /// </value>
    public List<Action<IPolarLineSeries>> PolarLineSeriesBuilder { get; set; } = [];

    /// <summary>
    /// Gets or sets the line series builder.
    /// </summary>
    /// <value>
    /// The polar series builder.
    /// </value>
    public List<Action<IPolarSeries>> StackedPolarSeriesBuilder { get; set; } = [];

    /// <summary>
    /// Gets or sets the line series builder.
    /// </summary>
    /// <value>
    /// The pie series builder.
    /// </value>
    public List<Action<IHeatSeries>> HeatSeriesBuilder { get; set; } = [];

    /// <summary>
    /// Gets or sets the financial series builder.
    /// </summary>
    /// <value>
    /// The pie series builder.
    /// </value>
    public List<Action<IFinancialSeries>> FinancialSeriesBuilder { get; set; } = [];

    /// <summary>
    /// Gets or sets the stacked line series builder.
    /// </summary>
    /// <value>
    /// The pie series builder.
    /// </value>
    public List<Action<ILineSeries>> StackedLineSeriesBuilder { get; set; } = [];

    /// <summary>
    /// Gets or sets the bar series builder.
    /// </summary>
    /// <value>
    /// The pie series builder.
    /// </value>
    public List<Action<IBarSeries>> BarSeriesBuilder { get; set; } = [];

    /// <summary>
    /// Gets or sets the column series builder.
    /// </summary>
    /// <value>
    /// The pie series builder.
    /// </value>
    public List<Action<IBarSeries>> ColumnSeriesBuilder { get; set; } = [];

    /// <summary>
    /// Gets or sets the row series builder.
    /// </summary>
    /// <value>
    /// The pie series builder.
    /// </value>
    public List<Action<IBarSeries>> RowSeriesBuilder { get; set; } = [];

    /// <summary>
    /// Gets or sets the stacked bar series builder.
    /// </summary>
    /// <value>
    /// The pie series builder.
    /// </value>
    public List<Action<IStackedBarSeries>> StackedBarSeriesBuilder { get; set; } = [];

    /// <summary>
    /// Gets or sets the stacked column series builder.
    /// </summary>
    /// <value>
    /// The pie series builder.
    /// </value>
    public List<Action<IStackedBarSeries>> StackedColumnSeriesBuilder { get; set; } = [];

    /// <summary>
    /// Gets or sets the stacked row series builder.
    /// </summary>
    /// <value>
    /// The pie series builder.
    /// </value>
    public List<Action<IStackedBarSeries>> StackedRowSeriesBuilder { get; set; } = [];

    /// <summary>
    /// Gets or sets the scatter series builder.
    /// </summary>
    /// <value>
    /// The pie series builder.
    /// </value>
    public List<Action<IScatterSeries>> ScatterSeriesBuilder { get; set; } = [];

    /// <summary>
    /// Gets or sets the error series builder.
    /// </summary>
    /// <value>
    /// The pie series builder.
    /// </value>
    public List<Action<IBoxSeries>> BoxSeriesBuilder { get; set; } = [];

    /// <summary>
    /// Gets or sets the visual element builder.
    /// </summary>
    public Dictionary<Type, object> ChartElementElementBuilder { get; set; } = [];

    /// <summary>
    /// Gets or sets the default tooltip.
    /// </summary>
    public Func<IChartTooltip> GetDefaultTooltip { get; set; } = () => throw new NotImplementedException();

    /// <summary>
    /// Gets or sets the default legend.
    /// </summary>
    public Func<IChartLegend> GetDefaultLegend { get; set; } = () => throw new NotImplementedException();

    internal void Setup(bool isUIDark)
    {
        IsDark = isUIDark;
        if (!_initialized || _lastKnownDarkMode != IsDark)
        {
            _lastKnownDarkMode = IsDark;
            _initialized = true;
            foreach (var rule in Initialized)
                rule();
        }
    }

    /// <summary>
    /// Applies the theme to an axis.
    /// </summary>
    /// <param name="axis">The axis.</param>
    public void ApplyStyleToAxis(IPlane axis)
    {
        foreach (var rule in AxisBuilder) rule(axis);
    }

    /// <summary>
    /// Applies the theme to a series.
    /// </summary>
    /// <param name="series">The series.</param>
    public virtual void ApplyStyleToSeries(ISeries series)
    {
        foreach (var rule in SeriesBuilder) rule(series);

        if ((series.SeriesProperties & SeriesProperties.PieSeries) == SeriesProperties.PieSeries)
        {
            if ((series.SeriesProperties & SeriesProperties.Gauge) != 0)
            {
                if ((series.SeriesProperties & SeriesProperties.GaugeFill) != 0)
                {
                    foreach (var rule in GaugeFillSeriesBuilder) rule((IPieSeries)series);
                }
                else
                {
                    foreach (var rule in GaugeSeriesBuilder) rule((IPieSeries)series);
                }
            }
            else
            {
                foreach (var rule in PieSeriesBuilder) rule((IPieSeries)series);
            }
        }

        if ((series.SeriesProperties & SeriesProperties.CartesianSeries) == SeriesProperties.CartesianSeries)
        {
            foreach (var rule in CartesianSeriesBuilder) rule((ICartesianSeries)series);
        }

        if ((series.SeriesProperties & SeriesProperties.Bar) == SeriesProperties.Bar &&
            (series.SeriesProperties & SeriesProperties.Stacked) != SeriesProperties.Stacked)
        {
            var barSeries = (IBarSeries)series;
            foreach (var rule in BarSeriesBuilder) rule(barSeries);

            if ((series.SeriesProperties & SeriesProperties.PrimaryAxisVerticalOrientation) == SeriesProperties.PrimaryAxisVerticalOrientation)
            {
                foreach (var rule in ColumnSeriesBuilder) rule(barSeries);
            }

            if ((series.SeriesProperties & SeriesProperties.PrimaryAxisHorizontalOrientation) == SeriesProperties.PrimaryAxisHorizontalOrientation)
            {
                foreach (var rule in RowSeriesBuilder) rule(barSeries);
            }
        }

        var stackedBarMask = SeriesProperties.Bar | SeriesProperties.Stacked;
        if ((series.SeriesProperties & stackedBarMask) == stackedBarMask)
        {
            var stackedBarSeries = (IStackedBarSeries)series;
            foreach (var rule in StackedBarSeriesBuilder) rule(stackedBarSeries);

            if ((series.SeriesProperties & SeriesProperties.PrimaryAxisVerticalOrientation) == SeriesProperties.PrimaryAxisVerticalOrientation)
            {
                foreach (var rule in StackedColumnSeriesBuilder) rule(stackedBarSeries);
            }

            if ((series.SeriesProperties & SeriesProperties.PrimaryAxisHorizontalOrientation) == SeriesProperties.PrimaryAxisHorizontalOrientation)
            {
                foreach (var rule in StackedRowSeriesBuilder) rule(stackedBarSeries);
            }
        }

        if ((series.SeriesProperties & SeriesProperties.Scatter) == SeriesProperties.Scatter)
        {
            foreach (var rule in ScatterSeriesBuilder) rule((IScatterSeries)series);
        }

        if ((series.SeriesProperties & SeriesProperties.StepLine) == SeriesProperties.StepLine)
        {
            var stepSeries = (IStepLineSeries)series;
            foreach (var rule in StepLineSeriesBuilder) rule(stepSeries);

            if ((series.SeriesProperties & SeriesProperties.Stacked) == SeriesProperties.Stacked)
            {
                foreach (var rule in StackedStepLineSeriesBuilder) rule(stepSeries);
            }
        }

        if ((series.SeriesProperties & SeriesProperties.BoxSeries) == SeriesProperties.BoxSeries)
        {
            foreach (var rule in BoxSeriesBuilder) rule((IBoxSeries)series);
        }

        if ((series.SeriesProperties & SeriesProperties.Line) == SeriesProperties.Line)
        {
            var lineSeries = (ILineSeries)series;
            foreach (var rule in LineSeriesBuilder) rule(lineSeries);

            if ((series.SeriesProperties & SeriesProperties.Stacked) == SeriesProperties.Stacked)
            {
                foreach (var rule in StackedLineSeriesBuilder) rule(lineSeries);
            }
        }

        if ((series.SeriesProperties & SeriesProperties.Polar) == SeriesProperties.Polar)
        {
            var polarSeries = (IPolarSeries)series;
            foreach (var rule in PolarSeriesBuilder) rule(polarSeries);

            if ((series.SeriesProperties & SeriesProperties.Stacked) == SeriesProperties.Stacked)
            {
                foreach (var rule in StackedPolarSeriesBuilder) rule(polarSeries);
            }
        }

        if ((series.SeriesProperties & SeriesProperties.PolarLine) == SeriesProperties.PolarLine)
        {
            var polarSeries = (IPolarLineSeries)series;
            foreach (var rule in PolarLineSeriesBuilder) rule(polarSeries);

            if ((series.SeriesProperties & SeriesProperties.Stacked) == SeriesProperties.Stacked)
            {
                foreach (var rule in StackedPolarSeriesBuilder) rule(polarSeries);
            }
        }

        if ((series.SeriesProperties & SeriesProperties.Heat) == SeriesProperties.Heat)
        {
            var heatSeries = (IHeatSeries)series;
            foreach (var rule in HeatSeriesBuilder) rule(heatSeries);
        }

        if ((series.SeriesProperties & SeriesProperties.Financial) == SeriesProperties.Financial)
        {
            var financialSeries = (IFinancialSeries)series;
            foreach (var rule in FinancialSeriesBuilder) rule(financialSeries);
        }

        if ((series.SeriesProperties & SeriesProperties.Sankey) == SeriesProperties.Sankey)
        {
            foreach (var rule in SankeySeriesBuilder) rule((ISankeySeries)series);
        }
    }

    /// <summary>
    /// Build a draw amrgin frame based on the theme.
    /// </summary>
    public void ApplyStyleToDrawMarginFrame(CoreDrawMarginFrame drawMarginFrame)
    {
        foreach (var rule in DrawMarginFrameBuilder)
            rule(drawMarginFrame);
    }

    /// <summary>
    /// Applies the theme to a visual element.
    /// </summary>
    /// <typeparam name="TChartElement">The typoe of the chart element.</typeparam>
    /// <param name="visualElement">The visual element.</param>
    public void ApplyStyleTo<TChartElement>(IChartElement visualElement)
        where TChartElement : IChartElement
    {
        if (!ChartElementElementBuilder.TryGetValue(typeof(TChartElement), out var builder)) return;

        ((Action<TChartElement>)builder)((TChartElement)visualElement);
    }

    /// <summary>
    /// Gets the color of a series according to the theme.
    /// </summary>
    /// <returns></returns>
    public LvcColor GetSeriesColor(ISeries series) =>
        Colors[series.SeriesId % Colors.Length];
}
