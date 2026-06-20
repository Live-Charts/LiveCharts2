using System.Windows.Forms;
using LiveChartsCore.Geo;
using LiveChartsCore.SkiaSharpView.WinForms;
using ViewModelsSamples.Maps.RotatingOrthographic;

namespace WinFormsSample.Maps.RotatingOrthographic;

public partial class View : UserControl
{
    public GeoMap Chart;
    private readonly ViewModel _vm = new();

    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(50, 50);

        var chart = new GeoMap
        {
            Series = _vm.Series,
            MapProjection = MapProjection.Orthographic,
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(50, 50),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(chart);
        Chart = chart;

        Load           += (_, _) => _vm.Start((lon, lat) => chart.CoreChart.RotateTo(lon, lat));
        HandleDestroyed += (_, _) => _vm.Stop();
    }
}
