using LiveChartsCore.Drawing;
using Noesis;

namespace LiveChartsCore.Noesis.Drawing.Geometries;

/// <summary>Draws an SVG geometry with Noesis.</summary>
public class BaseSVGPathGeometry : BoundedDrawnGeometry, IDrawnElement<NoesisDrawingContext>
{
    /// <summary>Creates an empty SVG geometry.</summary>
    public BaseSVGPathGeometry()
    {
    }

    /// <summary>Creates an SVG geometry.</summary>
    public BaseSVGPathGeometry(global::Noesis.Geometry path)
    {
        Path = path;
    }

    /// <summary>Gets or sets the path.</summary>
    public global::Noesis.Geometry? Path { get; set; }

    /// <summary>Gets or sets whether the path fills the geometry bounds.</summary>
    public bool FitToSize { get; set; }

    /// <inheritdoc />
    public virtual void Draw(NoesisDrawingContext context)
    {
        if (Path is null)
        {
            return;
        }

        var bounds = Path.Bounds;
        var sx = FitToSize && bounds.Width > 0 ? Width / bounds.Width : 1;
        var sy = FitToSize && bounds.Height > 0 ? Height / bounds.Height : 1;
        var group = new TransformGroup();

        group.Children.Add(new TranslateTransform(-bounds.X, -bounds.Y));
        group.Children.Add(new ScaleTransform { ScaleX = sx, ScaleY = sy });
        group.Children.Add(new TranslateTransform(X, Y));

        context.Retain(group);
        context.Canvas.PushTransform(group);
        context.Canvas.DrawGeometry(context.Fill, context.Stroke, Path);
        context.Canvas.Pop();
    }
}
