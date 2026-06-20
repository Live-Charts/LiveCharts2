using System.Windows.Forms;
using LiveChartsCore.Geo;
using LiveChartsCore.SkiaSharpView.WinForms;
using ViewModelsSamples.Maps.OrthographicWorld;

namespace WinFormsSample.Maps.OrthographicWorld;

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
            MapProjection = MapProjection.Orthographic,
            Location = new System.Drawing.Point(0, 40),
            Size = new System.Drawing.Size(50, 10),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(chart);
        Chart = chart;

        int x = 0;
        var bAmericas = new Button { Text = "Americas", Location = new System.Drawing.Point(x, 0), AutoSize = true };
        bAmericas.Click += (_, _) => Chart.CoreChart.RotateTo(-95, 35);
        Controls.Add(bAmericas);
        x += bAmericas.Width + 4;

        var bEurope = new Button { Text = "Europe", Location = new System.Drawing.Point(x, 0), AutoSize = true };
        bEurope.Click += (_, _) => Chart.CoreChart.RotateTo(15, 50);
        Controls.Add(bEurope);
        x += bEurope.Width + 4;

        var bAsia = new Button { Text = "Asia", Location = new System.Drawing.Point(x, 0), AutoSize = true };
        bAsia.Click += (_, _) => Chart.CoreChart.RotateTo(100, 35);
        Controls.Add(bAsia);
        x += bAsia.Width + 4;

        var bAfrica = new Button { Text = "Africa", Location = new System.Drawing.Point(x, 0), AutoSize = true };
        bAfrica.Click += (_, _) => Chart.CoreChart.RotateTo(20, 5);
        Controls.Add(bAfrica);
        x += bAfrica.Width + 4;

        var bOceania = new Button { Text = "Oceania", Location = new System.Drawing.Point(x, 0), AutoSize = true };
        bOceania.Click += (_, _) => Chart.CoreChart.RotateTo(135, -25);
        Controls.Add(bOceania);
    }
}
