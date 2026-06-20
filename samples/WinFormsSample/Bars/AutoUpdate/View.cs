using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;

namespace WinFormsSample.Bars.AutoUpdate;

public partial class View : UserControl
{
    private readonly CartesianChart _cartesianChart;
    private readonly ObservableCollection<ISeries> _seriesCollection;
    private readonly ObservableCollection<ObservableValue> _juanaValues;
    private readonly Random _random = new();

    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(100, 100);

        _juanaValues = [
            new(2), new(5), new(4)
        ];

        _seriesCollection = [
            new ColumnSeries<ObservableValue>
            {
                Name = "Juana",
                Values = _juanaValues
            },
            new ColumnSeries<ObservableValue>
            {
                Name = "Mary",
                Values = [new(5), new(4), new(1)]
            }
        ];

        _cartesianChart = new CartesianChart
        {
            Series = _seriesCollection,
            Location = new System.Drawing.Point(0, 50),
            Size = new System.Drawing.Size(100, 50),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(_cartesianChart);

        var b1 = new Button { Text = "Add item", Location = new System.Drawing.Point(0, 0) };
        b1.Click += (sender, e) => { AddItem(); };
        Controls.Add(b1);

        var b2 = new Button { Text = "Replace item", Location = new System.Drawing.Point(80, 0) };
        b2.Click += (sender, e) => { ReplaceItem(); };
        Controls.Add(b2);

        var b3 = new Button { Text = "Remove item", Location = new System.Drawing.Point(160, 0) };
        b3.Click += (sender, e) => { RemoveItem(); };
        Controls.Add(b3);

        var b4 = new Button { Text = "Add series", Location = new System.Drawing.Point(240, 0) };
        b4.Click += (sender, e) => { AddSeries(); };
        Controls.Add(b4);

        var b5 = new Button { Text = "Remove series", Location = new System.Drawing.Point(320, 0) };
        b5.Click += (sender, e) => { RemoveSeries(); };
        Controls.Add(b5);
    }

    private void AddSeries()
    {
        _seriesCollection.Add(
            new ColumnSeries<ObservableValue>
            {
                Name = $"User #{_seriesCollection.Count}",
                Values = FetchVales()
            });
    }

    private void RemoveSeries()
    {
        if (_seriesCollection.Count <= 1) return;
        _seriesCollection.RemoveAt(_seriesCollection.Count - 1);
    }

    private void AddItem()
    {
        var newPoint = new ObservableValue { Value = _random.Next(0, 10) };
        _juanaValues.Add(newPoint);
    }

    private void RemoveItem()
    {
        if (_juanaValues.Count < 2) return;
        _juanaValues.RemoveAt(0);
    }

    private void ReplaceItem()
    {
        if (_juanaValues.Count < 2) return;
        var randomIndex = _random.Next(0, _juanaValues.Count - 1);
        _juanaValues[randomIndex] = new ObservableValue(_random.Next(1, 10));
    }

    private ObservableValue[] FetchVales()
    {
        return [
            new(_random.Next(0, 10)),
            new(_random.Next(0, 10)),
            new(_random.Next(0, 10))
        ];
    }

    public CartesianChart Chart => _cartesianChart;
}
