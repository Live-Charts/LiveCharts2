using System.Collections.Generic;
using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using ViewModelsSamples.Events.Tutorial;

namespace EtoFormsSample.Events.Tutorial;

public class View : Panel
{
    private readonly ISeries[] _series;
    private readonly CartesianChart _chart;
    private readonly HashSet<ChartPoint> _activePoints = new();

    public View()
    {
        var viewModel = new ViewModel();

        _series =
        [
            new ColumnSeries<double>
            {
                Values = viewModel.Values1,
                Stroke = new SolidColorPaint(SKColors.Transparent)
            },
            new ColumnSeries<double>
            {
                Values = viewModel.Values2,
                Stroke = new SolidColorPaint(SKColors.Transparent)
            }
        ];

        _chart = new CartesianChart
        {
            Series = _series,
            FindingStrategy = FindingStrategy.ExactMatch
        };

        _chart.MouseDown += CartesianChart_MouseDown;
        _chart.HoveredPointsChanged += OnHoveredChanged;

        Content = _chart;
    }

    private void CartesianChart_MouseDown(object sender, MouseEventArgs e)
    {
        var foundPoints = _chart.GetPointsAt(new LiveChartsCore.Drawing.LvcPointD(e.Location.X, e.Location.Y));
        foreach (var point in foundPoints)
        {
            var visual = point.Context.Visual!;
            if (!_activePoints.Contains(point))
            {
                visual.Fill = new SolidColorPaint { Color = SKColors.Yellow };
                _activePoints.Add(point);
            }
            else
            {
                visual.Fill = null;
                _activePoints.Remove(point);
            }
        }
    }

    private void OnHoveredChanged(IChartView chart, IEnumerable<ChartPoint> newItems, IEnumerable<ChartPoint> oldItems)
    {
        foreach (var hovered in newItems ?? new List<ChartPoint>())
        {
            hovered.Context.Visual!.Stroke = new SolidColorPaint(SKColors.Black, 3);
        }
        foreach (var hovered in oldItems ?? new List<ChartPoint>())
        {
            hovered.Context.Visual!.Stroke = null;
        }
    }
}
