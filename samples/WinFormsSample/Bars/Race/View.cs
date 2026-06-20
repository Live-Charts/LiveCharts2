using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WinForms;
using LiveChartsCore.Themes;
using SkiaSharp;

namespace WinFormsSample.Bars.Race;

public partial class View : UserControl
{
    private readonly Random _r = new();
    private readonly RowSeries<PilotInfo> _series;

    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(50, 50);

        _series = new RowSeries<PilotInfo>
        {
            Values = Fetch(),
            DataLabelsTranslate = new LiveChartsCore.Drawing.LvcPoint(-1, 0),
            DataLabelsPaint = new SolidColorPaint(SKColors.Black),
            DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.End,
            DataLabelsFormatter = point => ((PilotInfo)point.Context.DataSource).Name,
        };

        _series.PointMeasured += OnPointMeasured;

        var yAxis = new Axis { IsVisible = false };
        var xAxis = new Axis { IsVisible = false };

        var cartesianChart = new CartesianChart
        {
            Series = [_series],
            YAxes = [yAxis],
            TooltipPosition = LiveChartsCore.Measure.TooltipPosition.Hidden,
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(50, 50),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(cartesianChart);

        _ = StartRace();
    }

    private async Task StartRace()
    {
        await Task.Delay(1000);

        // to keep this sample simple, we run the next infinite loop
        // in a real application you should stop the loop/task when the view is disposed

        while (true)
        {
            // do a random change to the data
            foreach (var item in _series.Values)
                item.Value += _r.Next(0, 100);

            _series.Values = [.. _series.Values.OrderBy(x => x.Value)];

            await Task.Delay(100);
        }
    }

    private void OnPointMeasured(ChartPoint point)
    {
        // assign the pilot color to the bar
        var pilot = (PilotInfo)point.Context.DataSource;
        if (point.Context.Visual is null || pilot is null) return;
        point.Context.Visual.Fill = pilot.Paint;
    }

    private static PilotInfo[] Fetch()
    {
        var paints = Enumerable.Range(0, 7)
            .Select(i => new SolidColorPaint(ColorPalletes.MaterialDesign500[i].AsSKColor()))
            .ToArray();

        return [
            new PilotInfo("Tsunoda",   500,  paints[0]),
            new PilotInfo("Sainz",     450,  paints[1]),
            new PilotInfo("Riccardo",  520,  paints[2]),
            new PilotInfo("Bottas",    550,  paints[3]),
            new PilotInfo("Perez",     660,  paints[4]),
            new PilotInfo("Verstapen", 920,  paints[5]),
            new PilotInfo("Hamilton",  1000, paints[6])
        ];
    }

    public class PilotInfo : ObservableValue
    {
        public PilotInfo(string name, int value, SolidColorPaint paint)
        {
            Name = name;
            Paint = paint;
            Value = value;
        }
        public string Name { get; set; }
        public SolidColorPaint Paint { get; set; }
    }
}
