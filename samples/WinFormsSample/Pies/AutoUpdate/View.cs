using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;

namespace WinFormsSample.Pies.AutoUpdate;

public partial class View : UserControl
{
    public readonly PieChart Chart;
    private readonly Random _random = new();

    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(100, 100);

        var seriesCollection = new ObservableCollection<ISeries>
        {
            new PieSeries<int>{ Values= Fetch() },
            new PieSeries<int>{ Values= Fetch() },
        };

        Chart = new PieChart
        {
            Series = seriesCollection,
            Location = new System.Drawing.Point(0, 50),
            Size = new System.Drawing.Size(100, 50),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(Chart);

        var b1 = new Button { Text = "Add series", Location = new System.Drawing.Point(0, 0) };
        b1.Click += (sender, e) => seriesCollection.Add(new PieSeries<int> { Values = Fetch() });
        Controls.Add(b1);

        var b2 = new Button { Text = "Remove series", Location = new System.Drawing.Point(80, 0) };
        b2.Click += (sender, e) => seriesCollection.Remove(Chart.Series.First());
        Controls.Add(b2);

        var b3 = new Button { Text = "Update all", Location = new System.Drawing.Point(160, 0) };
        b3.Click += (sender, e) =>
        {
            foreach (var series in Chart.Series)
            {
                if (series is PieSeries<int> pieSeries)
                {
                    pieSeries.Values = Fetch();
                }
            }
        };
        Controls.Add(b3);
    }

    private int[] Fetch()
    {
        return [
            _random.Next(1, 10)
        ];
    }
}
