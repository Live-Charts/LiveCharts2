using LiveChartsCore.Drawing;
using LiveChartsCore.Measure;
using Noesis;

namespace LiveChartsCore.Noesis.Drawing.Geometries;

/// <summary>A tooltip surface with a pointer wedge.</summary>
public class PopUpGeometry : BoundedDrawnGeometry, IDrawnElement<NoesisDrawingContext>
{
    /// <summary>Gets or sets wedge size.</summary>
    public double Wedge { get; set; } = 15;

    /// <summary>Gets or sets wedge width.</summary>
    public double WedgeThickness { get; set; } = 2;

    /// <summary>Gets or sets corner radius.</summary>
    public double BorderRadius { get; set; } = 10;

    /// <summary>Gets or sets wedge placement.</summary>
    public PopUpPlacement Placement { get; set; } = PopUpPlacement.Bottom;

    /// <inheritdoc />
    public virtual void Draw(NoesisDrawingContext context)
    {
        var wedge = (float)Wedge;
        var x = X + (Placement == PopUpPlacement.Right ? wedge : 0);
        var y = Y + (Placement == PopUpPlacement.Bottom ? wedge : 0);
        var width = Width - (Placement is PopUpPlacement.Right or PopUpPlacement.Left ? wedge : 0);
        var height = Height - (Placement is PopUpPlacement.Top or PopUpPlacement.Bottom ? wedge : 0);
        var half = wedge * (float)WedgeThickness / 2;
        var geometry = new StreamGeometry();

        context.Canvas.DrawRoundedRectangle(context.Fill, context.Stroke, new Rect(x, y, width, height), (float)BorderRadius, (float)BorderRadius);

        using (var path = geometry.Open())
        {
            switch (Placement)
            {
                case PopUpPlacement.Bottom:
                    path.BeginFigure(new Point(x + width / 2 - half, y), true, true);
                    path.LineTo(new Point(x + width / 2, y - wedge), true, false);
                    path.LineTo(new Point(x + width / 2 + half, y), true, false);
                    break;
                case PopUpPlacement.Top:
                    path.BeginFigure(new Point(x + width / 2 - half, y + height), true, true);
                    path.LineTo(new Point(x + width / 2, y + height + wedge), true, false);
                    path.LineTo(new Point(x + width / 2 + half, y + height), true, false);
                    break;
                case PopUpPlacement.Left:
                    path.BeginFigure(new Point(x + width, y + height / 2 - half), true, true);
                    path.LineTo(new Point(x + width + wedge, y + height / 2), true, false);
                    path.LineTo(new Point(x + width, y + height / 2 + half), true, false);
                    break;
                case PopUpPlacement.Right:
                    path.BeginFigure(new Point(x, y + height / 2 - half), true, true);
                    path.LineTo(new Point(x - wedge, y + height / 2), true, false);
                    path.LineTo(new Point(x, y + height / 2 + half), true, false);
                    break;
            }
        }

        context.Retain(geometry);
        context.Canvas.DrawGeometry(context.Fill, context.Stroke, geometry);
    }
}
