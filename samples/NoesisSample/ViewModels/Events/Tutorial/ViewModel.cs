using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.Measure;
using LiveChartsCore.Noesis.Painting;
using LiveChartsCore.Drawing;

namespace ViewModelsSamples.Events.Tutorial;

public partial class ViewModel
{
    private readonly HashSet<ChartPoint> _activePoints = [];

    public double[] Values1 { get; set; } = [1, 5, 4, 3];
    public double[] Values2 { get; set; } = [3, 2, 6, 2];

    public FindingStrategy Strategy { get; } = FindingStrategy.ExactMatch;

    [RelayCommand]
    public void OnPressed(PointerCommandArgs args)
    {
        var foundPoints = args.Chart.GetPointsAt(args.PointerPosition);

        foreach (var point in foundPoints)
        {
            var visual = point.Context.Visual!;

            if (!_activePoints.Contains(point))
            {
                visual.Fill = new SolidColorPaint { Color = DrawingColors.Yellow };
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

    [RelayCommand]
    public void OnHoveredPointsChanged(HoverCommandArgs args)
    {
        // the NewPoints contains the new hovered points // mark
        foreach (var hovered in args.NewPoints ?? [])
        {
            // in this case, we will set a black stroke on the drawn gemetry. // mark
            hovered.Context.Visual!.Stroke = new SolidColorPaint(DrawingColors.Black, 3);
        }

        // the OldPoints contains the points that are not hovered anymore // mark
        foreach (var hovered in args.OldPoints ?? [])
        {
            // now, we will clear the stroke. // mark
            hovered.Context.Visual!.Stroke = null;
        }

        Trace.WriteLine(
            $"hovered, {args.NewPoints?.Count()} added, {args.OldPoints?.Count()} removed");
    }
}
