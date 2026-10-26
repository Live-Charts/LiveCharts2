using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Measure;
using Noesis;

namespace LiveChartsGeneratedCode;

public partial class SourceGenCartesianChart
{
    /// <summary>Initializes Cartesian pointer handling.</summary>
    public SourceGenCartesianChart()
    {
        MouseWheel += OnMouseWheel;
    }

    private void OnMouseWheel(object sender, MouseWheelEventArgs args)
    {
        var position = args.GetPosition(this);

        ((CartesianChartEngine)CoreChart).Zoom(
            ZoomMode,
            new LvcPoint(position.X, position.Y),
            args.Delta > 0 ? ZoomDirection.ZoomIn : ZoomDirection.ZoomOut);
    }
}
