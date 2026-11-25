using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WinForms;
using SkiaSharp;

namespace WinFormsSample.Events.Tutorial;

public partial class View : UserControl
{
    private readonly ISeries[] _series;
    private readonly CartesianChart _chart;
    private readonly HashSet<ChartPoint> _activePoints = [];

    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(400, 400);

        // Example data for demonstration
        _series = [
            new ColumnSeries<double>
            {
                Values = [1, 5, 4, 3],
                Stroke = new SolidColorPaint(SKColors.Transparent)
            },
            new ColumnSeries<double>
            {
                Values = [3, 2, 6, 2],
                Stroke = new SolidColorPaint(SKColors.Transparent)
            }
        ];

        _chart = new CartesianChart
        {
            Series = _series,
            FindingStrategy = FindingStrategy.ExactMatch,
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(400, 400),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        _chart.MouseDown += CartesianChart_MouseDown;
        _chart.HoveredPointsChanged += OnHoveredChanged;

        Controls.Add(_chart);
    }

    private void CartesianChart_MouseDown(object sender, MouseEventArgs e)
    {
        var foundPoints = _chart.GetPointsAt(new LvcPointD(e.Location.X, e.Location.Y));

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
                // clear the fill to the default value
                visual.Fill = null;
                _activePoints.Remove(point);
            }

            Trace.WriteLine($"found {point.Context.DataSource}");
        }
    }

    private void OnHoveredChanged(IChartView chart, IEnumerable<ChartPoint> newItems, IEnumerable<ChartPoint> oldItems)
    {
        // the NewPoints contains the new hovered points // mark
        foreach (var hovered in newItems ?? [])
        {
            // in this case, we will set a black stroke on the drawn gemetry. // mark
            hovered.Context.Visual!.Stroke = new SolidColorPaint(SKColors.Black, 3);
        }

        // the OldPoints contains the points that are not hovered anymore // mark
        foreach (var hovered in oldItems ?? [])
        {
            // now, we will clear the stroke. // mark
            hovered.Context.Visual!.Stroke = null;
        }

        Trace.WriteLine(
            $"hovered, {newItems?.Count()} added, {oldItems?.Count()} removed");
    }
}
