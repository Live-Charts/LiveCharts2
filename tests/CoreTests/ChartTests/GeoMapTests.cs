using System.Linq;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.ChartTests;

[TestClass]
public class GeoMapTests
{
    // https://github.com/Live-Charts/LiveCharts2/issues/962
    //
    // Swapping the Series collection on a GeoMap should preserve the heat fill
    // on lands that exist in both the old and the new series. Before the fix,
    // GeoMapChart.Measure() painted the new series first and then called
    // Delete() on the departed series, which nulled Shape.Fill on every land
    // the old series had ever painted -- including ones the new series just
    // painted -- so shared lands appeared blank until the next measure.
    [TestMethod]
    public void GeoMap_SwappingSeries_PreservesFillOnSharedLands()
    {
        var chart = new SKGeoMap
        {
            Width = 400,
            Height = 400,
            Series = [
                new HeatLandSeries
                {
                    Lands = [
                        new() { Name = "fra", Value = 10 },
                        new() { Name = "usa", Value = 5 },
                    ]
                }
            ]
        };

        chart.CoreChart.Measure();

        var fra = chart.ActiveMap.FindLand("fra");
        var usa = chart.ActiveMap.FindLand("usa");
        Assert.IsNotNull(fra, "fra should exist in the world map");
        Assert.IsNotNull(usa, "usa should exist in the world map");
        Assert.IsTrue(
            fra.Data.All(d => d.Shape?.Fill is not null),
            "fra should be painted after the first measure");

        // Swap to a series that still contains "fra" but drops "usa".
        chart.Series = [
            new HeatLandSeries
            {
                Lands = [
                    new() { Name = "fra", Value = 1 },
                    new() { Name = "bra", Value = 99 },
                ]
            }
        ];

        chart.CoreChart.Measure();

        Assert.IsTrue(
            fra.Data.All(d => d.Shape?.Fill is not null),
            "fra is shared between the old and new series and must stay painted after swap (#962)");
        Assert.IsTrue(
            usa.Data.All(d => d.Shape?.Fill is null),
            "usa is no longer in any series and must be cleared after swap");
    }
}
