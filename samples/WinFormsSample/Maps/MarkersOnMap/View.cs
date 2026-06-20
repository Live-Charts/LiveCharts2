using System.Windows.Forms;
using LiveChartsCore.Geo;
using LiveChartsCore.SkiaSharpView.WinForms;
using ViewModelsSamples.Maps.MarkersOnMap;

namespace WinFormsSample.Maps.MarkersOnMap;

public partial class View : UserControl
{
    public GeoMap Chart;

    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(50, 50);

        var vm = new ViewModel();

        var chart = new GeoMap
        {
            Series = vm.Series,
            VisualElements = vm.VisualElements,
            MapProjection = MapProjection.Mercator,
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(50, 50),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(chart);
        Chart = chart;
    }
}
