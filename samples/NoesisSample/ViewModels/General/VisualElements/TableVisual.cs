using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Noesis.Drawing.Geometries;
using LiveChartsCore.Noesis.Drawing.Layouts;
using LiveChartsCore.Noesis.Painting;
using LiveChartsCore.VisualElements;

namespace ViewModelsSamples.General.VisualElements;

public class TableVisual : Visual
{
    protected override TableLayout DrawnElement { get; } =
        new TableLayout
        {
            Cells = [
                new(0, 0, GetLabel("A")),
                new(0, 1, GetLabel("-")),
                new(0, 2, GetLabel("B")),
                new(1, 0, GetLabel("-")),
                new(1, 1, GetLabel("C")),
                new(1, 2, GetLabel("-"))
            ],
        };

    protected override void Measure(Chart chart)
    {
        DrawnElement.X = 400;
        DrawnElement.Y = 100;
    }

    private static LabelGeometry GetLabel(string text) =>
        new()
        {
            Text = text,
            TextSize = 20,
            Padding = new(10),
            Paint = new SolidColorPaint(DrawingColors.Black),
            VerticalAlign = Align.Start,
            HorizontalAlign = Align.Start
        };
}
