using LiveChartsCore.Drawing;
using LiveChartsCore.Drawing.Layouts;

namespace LiveChartsCore.Console.Drawing.Layouts;

public class TableLayout : CoreTableLayout<ConsoleDrawingContext>
{
    public new TableLayout AddChild(
        IDrawnElement<ConsoleDrawingContext> drawable,
        int row,
        int column,
        Align? horizontalAlign = null,
        Align? verticalAlign = null)
    {
        _ = base.AddChild(drawable, row, column, horizontalAlign, verticalAlign);
        return this;
    }
}
