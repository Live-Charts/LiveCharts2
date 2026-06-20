using System;
using System.Collections;
using System.Linq;
using CoreTests.CoreObjectsTests;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsGeneratedCode;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.SeriesTests;

[TestClass]
public class VisibilityTests
{
    // Issue #2054: a series declared with IsVisible=false never reached the theme-application
    // loop (which iterated VisibleSeries), leaving its paints null. The legend still listed it
    // (IsVisibleAtLegend defaults to true) and the miniature crashed in SkiaSharp with
    // "Value cannot be null. (Parameter 'paint')" at the first draw.
    [TestMethod]
    public void InvisibleSeriesWithLegend_DoesNotCrash_Issue2054()
    {
        var cartesian = new SKCartesianChart
        {
            Width = 500,
            Height = 500,
            LegendPosition = LegendPosition.Top,
            Series = [new LineSeries<double> { IsVisible = false }],
            ExplicitDisposing = true
        };
        _ = cartesian.GetImage();

        var pie = new SKPieChart
        {
            Width = 500,
            Height = 500,
            LegendPosition = LegendPosition.Top,
            Series = [new PieSeries<double> { IsVisible = false, Values = [1] }],
            ExplicitDisposing = true
        };
        _ = pie.GetImage();

        var polar = new SKPolarChart
        {
            Width = 500,
            Height = 500,
            LegendPosition = LegendPosition.Top,
            Series = [new PolarLineSeries<double> { IsVisible = false }],
            ExplicitDisposing = true
        };
        _ = polar.GetImage();
    }

    [TestMethod]
    public void VisibilityChangingTest()
    {
        TestObservablesChanging(new CartesianSut(new BoxSeries<int>(), "box"));
        TestObservablesChanging(new CartesianSut(new ColumnSeries<int>(), "colum"));
        TestObservablesChanging(new CartesianSut(new CandlesticksSeries<int>(), "candle"));
        TestObservablesChanging(new CartesianSut(new HeatSeries<int>(), "heat"));
        TestObservablesChanging(new CartesianSut(new LineSeries<int>(), "line"));
        TestObservablesChanging(new CartesianSut(new RowSeries<int>(), "row"));
        TestObservablesChanging(new CartesianSut(new ScatterSeries<int>(), "scatter"));
        TestObservablesChanging(new CartesianSut(new StepLineSeries<int>(), "step line"));
        TestObservablesChanging(new PieSut(new PieSeries<int>(), "pie"));

        // ToDo: polar series needs to be updated, it is also not passing the Memory test...
        // TestObservablesChanging(new PolarSut(new PolarLineSeries<ObservableValue>(), "polar"));

        // stacked series are irrelevant for this test because they inherit from some type above.
    }

    private static void TestObservablesChanging<T>(ChartSut<T> sut)
        where T : IList
    {
        _ = ChangingPaintTasks.DrawChart(sut.Chart, true);

        sut.Series.IsVisible = true;
        _ = ChangingPaintTasks.DrawChart(sut.Chart, true);

        sut.Series.IsVisible = false;
        _ = ChangingPaintTasks.DrawChart(sut.Chart, true);

        var values = sut.Series.Fetch(sut.Chart.CoreChart);
        var first = values.First();

        Assert.IsTrue(
            first.Context.Visual is null &&
            first.Context.Label is null);
    }

    private class CartesianSut(
        ISeries series,
        string name)
            : ChartSut<int[]>(new SKCartesianChart
            {
                Series = [series],
                AnimationsSpeed = TimeSpan.FromMilliseconds(10),
                EasingFunction = EasingFunctions.Lineal,
                Width = 1000,
                Height = 1000
            },
            series,
            name,
            [1, 2, 3])
    { }

    private class PieSut(
        ISeries series,
        string name)
            : ChartSut<int[]>(new SKPieChart
            {
                Series = [series],
                AnimationsSpeed = TimeSpan.FromMilliseconds(10),
                EasingFunction = EasingFunctions.Lineal,
                Width = 1000,
                Height = 1000
            },
            series,
            name,
            [1, 2, 3])
    { }

    private class PolarSut(
        ISeries series,
        string name)
            : ChartSut<int[]>(new SKPolarChart
            {
                Series = [series],
                AnimationsSpeed = TimeSpan.FromMilliseconds(10),
                EasingFunction = EasingFunctions.Lineal,
                Width = 1000,
                Height = 1000
            },
            series,
            name,
            [1, 2, 3])
    { }

    private abstract class ChartSut<T>
        where T : IEnumerable
    {
        public SourceGenSKChart Chart { get; set; }
        public ISeries Series { get; set; }
        public T Values { get; set; }

        protected ChartSut(
            SourceGenSKChart chart,
            ISeries series,
            string name,
            T initialValues)
        {
            series.Name = name;
            series.Values = Values = initialValues;
            series.DataLabelsPaint = new SolidColorPaint(SKColors.Black);
            Series = series;
            Chart = chart;
        }
    }
}
