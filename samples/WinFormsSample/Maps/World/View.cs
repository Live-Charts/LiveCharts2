using System.Windows.Forms;
using LiveChartsCore.Geo;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.SkiaSharpView.WinForms;
using ViewModelsSamples.Maps.World;

namespace WinFormsSample.Maps.World;

public partial class View : UserControl
{
    public GeoMap Chart;

    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(50, 50);

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
            },
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(50, 50),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(chart);
        Chart = chart;
    }
}
