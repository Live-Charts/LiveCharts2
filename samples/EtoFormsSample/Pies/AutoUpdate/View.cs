using System;
using System.Collections.ObjectModel;
using System.Linq;
using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;

namespace EtoFormsSample.Pies.AutoUpdate;

public class View : Panel
{
    public readonly PieChart Chart;
    private readonly Random _random = new();

    public View()
    {
        var seriesCollection = new ObservableCollection<ISeries>
        {
            new PieSeries<int>{ Values= Fetch() },
            new PieSeries<int>{ Values= Fetch() },
        };

        Chart = new PieChart
        {
            Series = seriesCollection
        };

        var b1 = new Button { Text = "Add series" };
        b1.Click += (sender, e) => seriesCollection.Add(new PieSeries<int> { Values = Fetch() });

        var b2 = new Button { Text = "Remove series" };
        b2.Click += (sender, e) =>
        {
            if (seriesCollection.Count > 0)
                seriesCollection.Remove(Chart.Series.First());
        };

        var b3 = new Button { Text = "Update all" };
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

        var buttons = new StackLayout(b1, b2, b3) { Orientation = Orientation.Horizontal, Padding = 2, Spacing = 4 };

        Content = new DynamicLayout(buttons, Chart);
    }

    private int[] Fetch()
    {
        return [_random.Next(1, 10)];
    }
}
