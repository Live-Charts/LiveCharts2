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
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.VisualElements;

namespace LiveChartsCore.Themes;

/// <summary>
/// Defines the LiveCharts theme extensions.
/// </summary>
public static class LiveChartsthemeExtensions
{
    /// <summary>
    ///  Defines a style builder for <see cref="IChartView"/> objects.
    /// </summary>
    /// <param name="theme">The theme.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns></returns>
    public static Theme HasRuleForAxes(this Theme theme, Action<IPlane> predicate)
    {
        theme.AxisBuilder.Add(predicate);
        return theme;
    }

    /// <summary>
    /// Defines a style builder for <see cref="CoreDrawMarginFrame"/> objects.
    /// </summary>
    /// <param name="theme">The theme.</param>
    /// <param name="getter">The getter.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns></returns>
    public static Theme HasRuleForDrawMarginFrame(this Theme theme, Func<CoreDrawMarginFrame?> getter, Action<CoreDrawMarginFrame> predicate)
    {
        theme.DrawMarginFrameGetter = getter;
        theme.DrawMarginFrameBuilder.Add(predicate);
        return theme;
    }

    /// <summary>
    ///  Defines a style builder for <see cref="ISeries"/> objects.
    /// </summary>
    /// <param name="theme">The theme.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns></returns>
    public static Theme HasRuleForAnySeries(this Theme theme, Action<ISeries> predicate)
    {
        theme.SeriesBuilder.Add(predicate);
        return theme;
    }

    /// <summary>
    ///  Defines a style builder for <see cref="IPieSeries"/> objects.
    /// </summary>
    /// <param name="theme">The theme.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns></returns>
    public static Theme HasRuleForPieSeries(this Theme theme, Action<IPieSeries> predicate)
    {
        theme.PieSeriesBuilder.Add(predicate);
        return theme;
    }

    /// <summary>
    ///  Defines a style builder for <see cref="ISankeySeries"/> objects.
    /// </summary>
    /// <param name="theme">The theme.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns></returns>
    public static Theme HasRuleForSankeySeries(this Theme theme, Action<ISankeySeries> predicate)
    {
        theme.SankeySeriesBuilder.Add(predicate);
        return theme;
    }

    /// <summary>
    ///  Defines a style builder for <see cref="IPieSeries"/> objects when used as gauges.
    /// </summary>
    /// <param name="theme">The theme.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns></returns>
    public static Theme HasRuleForGaugeSeries(this Theme theme, Action<IPieSeries> predicate)
    {
        theme.GaugeSeriesBuilder.Add(predicate);
        return theme;
    }

    /// <summary>
    ///  Defines a style builder for <see cref="IPieSeries"/> objects when used as gauges fills.
    /// </summary>
    /// <param name="theme">The theme.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns></returns>
    public static Theme HasRuleForGaugeFillSeries(this Theme theme, Action<IPieSeries> predicate)
    {
        theme.GaugeFillSeriesBuilder.Add(predicate);
        return theme;
    }

    /// <summary>
    ///  Defines a style builder for <see cref="ILineSeries"/> objects.
    /// </summary>
    /// <param name="theme">The theme.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns></returns>
    public static Theme HasRuleForLineSeries(this Theme theme, Action<ILineSeries> predicate)
    {
        theme.LineSeriesBuilder.Add(predicate);
        return theme;
    }

    /// <summary>
    ///  Defines a style builder for <see cref="IStepLineSeries"/> objects.
    /// </summary>
    /// <param name="theme">The theme.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns></returns>
    public static Theme HasRuleForStepLineSeries(this Theme theme, Action<IStepLineSeries> predicate)
    {
        theme.StepLineSeriesBuilder.Add(predicate);
        return theme;
    }

    /// <summary>
    ///  Defines a style builder for stacked <see cref="IStepLineSeries"/> objects.
    /// </summary>
    /// <param name="theme">The theme.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns></returns>
    public static Theme HasRuleForStackedStepLineSeries(this Theme theme, Action<IStepLineSeries> predicate)
    {
        theme.StackedStepLineSeriesBuilder.Add(predicate);
        return theme;
    }

    /// <summary>
    ///  Defines a style builder for stacked <see cref="IStepLineSeries"/> objects.
    /// </summary>
    /// <param name="theme">The theme.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns></returns>
    public static Theme HasRuleForBoxSeries(this Theme theme, Action<IBoxSeries> predicate)
    {
        theme.BoxSeriesBuilder.Add(predicate);
        return theme;
    }

    /// <summary>
    ///  Defines a style builder for tacked <see cref="ILineSeries"/> objects.
    /// </summary>
    /// <param name="theme">The theme.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns></returns>
    public static Theme HasRuleForStackedLineSeries(this Theme theme, Action<ILineSeries> predicate)
    {
        theme.StackedLineSeriesBuilder.Add(predicate);
        return theme;
    }

    /// <summary>
    ///  Defines a style builder for <see cref="IBarSeries"/> objects.
    /// </summary>
    /// <param name="theme">The theme.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns></returns>
    public static Theme HasRuleForBarSeries(this Theme theme, Action<IBarSeries> predicate)
    {
        theme.BarSeriesBuilder.Add(predicate);
        return theme;
    }

    /// <summary>
    ///  Defines a style builder for vertical <see cref="IBarSeries"/> objects.
    /// </summary>
    /// <param name="theme">The theme.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns></returns>
    public static Theme HasRuleForColumnSeries(this Theme theme, Action<IBarSeries> predicate)
    {
        theme.ColumnSeriesBuilder.Add(predicate);
        return theme;
    }

    /// <summary>
    ///  Defines a style builder for horizontal <see cref="IBarSeries"/> objects.
    /// </summary>
    /// <param name="theme">The theme.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns></returns>
    public static Theme HasRuleForRowSeries(this Theme theme, Action<IBarSeries> predicate)
    {
        theme.ColumnSeriesBuilder.Add(predicate);
        return theme;
    }

    /// <summary>
    ///  Defines a style builder for stacked <see cref="IBarSeries"/> objects.
    /// </summary>
    /// <param name="theme">The theme.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns></returns>
    public static Theme HasRuleForStackedBarSeries(this Theme theme, Action<IStackedBarSeries> predicate)
    {
        theme.StackedBarSeriesBuilder.Add(predicate);
        return theme;
    }

    /// <summary>
    ///  Defines a style builder for stacked vertical <see cref="IBarSeries"/> objects.
    /// </summary>
    /// <param name="theme">The theme.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns></returns>
    public static Theme HasRuleForStackedColumnSeries(this Theme theme, Action<IStackedBarSeries> predicate)
    {
        theme.StackedColumnSeriesBuilder.Add(predicate);
        return theme;
    }

    /// <summary>
    ///  Defines a style builder for stacked horizontal <see cref="IBarSeries"/> objects.
    /// </summary>
    /// <param name="theme">The theme.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns></returns>
    public static Theme HasRuleForStackedRowSeries(this Theme theme, Action<IStackedBarSeries> predicate)
    {
        theme.StackedRowSeriesBuilder.Add(predicate);
        return theme;
    }

    /// <summary>
    ///  Defines a style builder for <see cref="IScatterSeries"/> objects.
    /// </summary>
    /// <param name="theme">The theme.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns></returns>
    public static Theme HasRuleForScatterSeries(this Theme theme, Action<IScatterSeries> predicate)
    {
        theme.ScatterSeriesBuilder.Add(predicate);
        return theme;
    }

    /// <summary>
    ///  Defines a style builder for <see cref="IHeatSeries"/> objects.
    /// </summary>
    /// <param name="theme">The theme.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns></returns>
    public static Theme HasRuleForHeatSeries(this Theme theme, Action<IHeatSeries> predicate)
    {
        theme.HeatSeriesBuilder.Add(predicate);
        return theme;
    }

    /// <summary>
    ///  Defines a style builder for <see cref="IFinancialSeries"/> objects.
    /// </summary>
    /// <param name="theme">The theme.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns></returns>
    public static Theme HasRuleForFinancialSeries(this Theme theme, Action<IFinancialSeries> predicate)
    {
        theme.FinancialSeriesBuilder.Add(predicate);
        return theme;
    }

    /// <summary>
    ///  Defines a style builder for <see cref="IPolarSeries"/> objects.
    /// </summary>
    /// <param name="theme">The theme.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns></returns>
    public static Theme HasRuleForPolaSeries(this Theme theme, Action<IPolarSeries> predicate)
    {
        theme.PolarSeriesBuilder.Add(predicate);
        return theme;
    }

    /// <summary>
    ///  Defines a style builder for <see cref="IPolarLineSeries"/> objects.
    /// </summary>
    /// <param name="theme">The theme.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns></returns>
    public static Theme HasRuleForPolarLineSeries(this Theme theme, Action<IPolarLineSeries> predicate)
    {
        theme.PolarLineSeriesBuilder.Add(predicate);
        return theme;
    }

    /// <summary>
    /// Defines the default tooltip.
    /// </summary>
    /// <param name="theme"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static Theme HasDefaultTooltip(this Theme theme, Func<IChartTooltip> predicate)
    {
        theme.GetDefaultTooltip = predicate;
        return theme;
    }

    /// <summary>
    /// Defines the default legend.
    /// </summary>
    /// <param name="theme"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static Theme HasDefaultLegend(this Theme theme, Func<IChartLegend> predicate)
    {
        theme.GetDefaultLegend = predicate;
        return theme;
    }

    /// <summary>
    /// Defines a style builder for <see cref= "VisualElement" /> objects.
    /// </summary>
    /// <typeparam name="TChartElement">The type of the chart element.</typeparam>
    /// <param name="theme">the theme.</param>
    /// <param name="predicate">the predicate.</param>
    /// <returns></returns>
    public static Theme HasRuleFor<TChartElement>(this Theme theme, Action<TChartElement> predicate)
        where TChartElement : ChartElement
    {
        theme.ChartElementElementBuilder.Add(typeof(TChartElement), predicate);
        return theme;
    }

    /// <summary>
    /// Defines the initialized action.
    /// </summary>
    /// <param name="theme">the theme.</param>
    /// <param name="predicate">the predicate.</param>
    /// <returns></returns>
    public static Theme OnInitialized(this Theme theme, Action predicate)
    {
        theme.Initialized.Add(predicate);
        return theme;
    }
}
