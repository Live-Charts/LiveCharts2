using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.SeriesTests;

[TestClass]
public class GeneralSeriesTests
{
    [TestMethod]
    public void MustHandleDataChanges()
    {
        RunCartesianTestFor(new ColumnSeries<int> { Name = "column" });
        RunCartesianTestFor(new CandlesticksSeries<int> { Name = "candle" });
        RunCartesianTestFor(new HeatSeries<int> { Name = "heat" });
        RunCartesianTestFor(new LineSeries<int> { Name = "line" });
        RunPieTest();
        RunPolarTestFor(new PolarLineSeries<int> { Name = "polar line" });
        RunCartesianTestFor(new ScatterSeries<int> { Name = "scatter" });
        RunCartesianTestFor(new StackedAreaSeries<int> { Name = "stacked area" });
        RunCartesianTestFor(new StackedColumnSeries<int> { Name = "stacked column" });
        RunCartesianHorzontalTestFor(new StackedRowSeries<int> { Name = "stacked row" });
        RunCartesianTestFor(new StackedStepAreaSeries<int> { Name = "stacked step area" });
        RunCartesianTestFor(new StepLineSeries<int> { Name = "step line" });
    }

    private void RunCartesianTestFor<TVisual, TLabel>(
        Series<int, TVisual, TLabel> series)
            where TVisual : DrawnGeometry, new()
            where TLabel : BaseLabelGeometry, new()
    {
        var values = new List<int> { 1 };
        series.Values = values;

        var chart = new SKCartesianChart
        {
            Width = 1000,
            Height = 1000,
            Series = new[] { series },
            YAxes = new[] { new Axis { MinLimit = 0, MaxLimit = 10 } }
        };

        _ = chart.GetImage();
        Assert.IsTrue(series.everFetched.Count == 1, $"Basic case for {series.Name}");

        values.Add(2);
        _ = chart.GetImage();
        var totalY = series.everFetched.Sum(x => series.ConvertToTypedChartPoint(x).Visual.Y);
        Assert.IsTrue(series.everFetched.Count == 2, $"Add case for {series.Name}");

        values[1] = 3;
        _ = chart.GetImage();
        // there are still 2 elements, but the shapes must have different position in the canvas
        // the size is not tested here, there is another test for that
        var newTotalY = series.everFetched.Sum(x => series.ConvertToTypedChartPoint(x).Visual.Y);
        Assert.IsTrue(series.everFetched.Count == 2 && totalY != newTotalY, $"Replace case for {series.Name}");

        values.RemoveAt(1);
        _ = chart.GetImage();
        Assert.IsTrue(series.everFetched.Count == 1, $"Delete case for {series.Name}");
    }

    private void RunPolarTestFor<TVisual, TLabel>(
        Series<int, TVisual, TLabel> series)
            where TVisual : DrawnGeometry, new()
            where TLabel : BaseLabelGeometry, new()
    {
        var values = new List<int> { 1 };
        series.Values = values;

        var chart = new SKPolarChart
        {
            Width = 1000,
            Height = 1000,
            Series = new[] { series },
            RadiusAxes = new[] { new PolarAxis { MinLimit = 0, MaxLimit = 10 } }
        };

        _ = chart.GetImage();
        Assert.IsTrue(series.everFetched.Count == 1, $"Basic case for {series.Name}");

        values.Add(2);
        _ = chart.GetImage();
        var totalY = series.everFetched.Sum(x => series.ConvertToTypedChartPoint(x).Visual.Y);
        Assert.IsTrue(series.everFetched.Count == 2, $"Add case for {series.Name}");

        values[1] = 3;
        _ = chart.GetImage();
        // there are still 2 elements, but the shapes must have different position in the canvas
        // the size is not tested here, there is another test for that
        var newTotalY = series.everFetched.Sum(x => series.ConvertToTypedChartPoint(x).Visual.Y);
        Assert.IsTrue(series.everFetched.Count == 2 && totalY != newTotalY, $"Replace case for {series.Name}");

        values.RemoveAt(1);
        _ = chart.GetImage();
        Assert.IsTrue(series.everFetched.Count == 1, $"Delete case for {series.Name}");
    }

    private void RunCartesianHorzontalTestFor<TVisual, TLabel>(
        Series<int, TVisual, TLabel> series)
            where TVisual : DrawnGeometry, new()
            where TLabel : BaseLabelGeometry, new()
    {
        var values = new List<int> { 1 };
        series.Values = values;

        var chart = new SKCartesianChart
        {
            Width = 1000,
            Height = 1000,
            Series = new[] { series },
            YAxes = new[] { new Axis { MinLimit = 0, MaxLimit = 10 } }
        };

        _ = chart.GetImage();
        Assert.IsTrue(series.everFetched.Count == 1, $"Basic case for {series.Name}");

        values.Add(2);
        _ = chart.GetImage();
        var totalX = series.everFetched.Sum(x => series.ConvertToTypedChartPoint(x).Visual.X);
        Assert.IsTrue(series.everFetched.Count == 2, $"Add case for {series.Name}");

        values[1] = 3;
        _ = chart.GetImage();
        chart.SaveImage($"{series.Name} 3.png");
        // there are still 2 elements, but the shapes must have different position in the canvas
        // the size is not tested here, there is another test for that
        var newTotalX = series.everFetched.Sum(x => series.ConvertToTypedChartPoint(x).Visual.X);
        Assert.IsTrue(series.everFetched.Count == 2 && totalX != newTotalX, $"Replace case for {series.Name}");

        values.RemoveAt(1);
        _ = chart.GetImage();
        Assert.IsTrue(series.everFetched.Count == 1, $"Delete case for {series.Name}");
    }

    private void RunPieTest()
    {
        var seriesCollection = new ObservableCollection<ISeries>(new[] { 1, 2, 3 }.AsPieSeries());

        var chart = new SKPieChart
        {
            Width = 1000,
            Height = 1000,
            Series = seriesCollection
        };

        int getSum() => seriesCollection.Sum(x => ((PieSeries<int>)x).everFetched.Count);

        _ = chart.GetImage();
        Assert.IsTrue(getSum() == 3, $"Basic case for pie");

        seriesCollection.Add(new PieSeries<int> { Values = new[] { 2 } });
        _ = chart.GetImage();
        Assert.IsTrue(getSum() == 4, $"Basic case for pie");

        seriesCollection.RemoveAt(1);
        _ = chart.GetImage();
        Assert.IsTrue(getSum() == 3, $"Delete case for pie");
    }
}
