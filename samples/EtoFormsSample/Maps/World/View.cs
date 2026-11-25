using Eto.Forms;
using LiveChartsCore.Geo;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Eto;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.SkiaSharpView.VisualElements;
using ViewModelsSamples.Maps.World;

namespace EtoFormsSample.Maps.World;

public class View : Panel
{
    public GeoMap Chart;

    public View()
    {
        // the view model changes the values every 500ms
        var viewModel = new ViewModel();

        var chart = new GeoMap
        {
            Series = viewModel.Series,
            MapProjection = MapProjection.Mercator,
            LegendPosition = LegendPosition.Right,
            Title = new DrawnLabelVisual(
                new LabelGeometry
                {
                    Text = "World population by country",
                    TextSize = 20,
                    Padding = new LiveChartsCore.Drawing.Padding(12)
                }),
            Legend = new SKHeatLegend
            {
                BadgePadding = new LiveChartsCore.Drawing.Padding(20, 16, 8, 16)
            }
        };

        Content = chart;
        Chart = chart;
    }
}
