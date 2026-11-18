using LiveChartsCore.Drawing;
using LiveChartsCore.Noesis.Painting;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore.Kernel;

namespace ViewModelsSamples.General.MapPoints;

public partial class ViewModel
{
    public int[] Values { get; set; } =
        [2, 5, 4, 6, 8, 3, 2, 4, 6];

    [RelayCommand]
    private void OnPointMeasured(ChartPoint point)
    {
        // the PointMeasured command/event is called every time a point is measured,
        // this happens when the chart loads, rezizes or when the data changes, this method
        // is called for every point in the series.

        if (point.Context.Visual is null) return;

        // here we can customize the visual of the point, for example we can set
        // a different color for each point.
        point.Context.Visual.Fill = GetPaint(point.Index);
    }

    private SolidColorPaint GetPaint(int index)
    {
        var paints = new SolidColorPaint[]
        {
            new(DrawingColors.Red),
            new(DrawingColors.Green),
            new(DrawingColors.Blue),
            new(DrawingColors.Yellow)
        };

        return paints[index % paints.Length];
    }
}
