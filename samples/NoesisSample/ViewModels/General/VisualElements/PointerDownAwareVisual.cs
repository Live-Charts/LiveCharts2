using System.Diagnostics;
using LiveChartsCore;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.Noesis.Drawing.Geometries;
using LiveChartsCore.Noesis.Painting;
using LiveChartsCore.VisualElements;
using LiveChartsCore.Drawing;

namespace ViewModelsSamples.General.VisualElements;

public class PointerDownAwareVisual : Visual
{
    public PointerDownAwareVisual()
    {
        PointerDown += OnPointerDown;
    }

    private void OnPointerDown(IInteractable visual, VisualElementEventArgs visualElementsArgs)
    {
        var location = visualElementsArgs.PointerLocation;
        Trace.WriteLine($"Pointer down at {location.X}, {location.Y}");
    }

    protected override RectangleGeometry DrawnElement { get; } =
        new RectangleGeometry { Fill = new SolidColorPaint(DrawingColors.Red) };

    protected override void Measure(Chart chart)
    {
        DrawnElement.X = 150;
        DrawnElement.Y = 100;
        DrawnElement.Width = 40;
        DrawnElement.Height = 40;
    }
}
