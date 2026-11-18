using LiveChartsCore.Drawing;
using Noesis;

namespace LiveChartsCore.Noesis.Drawing.Geometries;

/// <summary>A projected map land.</summary>
public class LandAreaGeometry : BaseVectorGeometry, IDrawnElement<NoesisDrawingContext>
{
    private PathBuilder? _path;

    /// <summary>Gets or sets the shared viewport.</summary>
    public MapViewportTransform? ViewportTransform { get; set; }

    /// <summary>Invalidates the projected path.</summary>
    public void MarkPathDirty() => _path = null;

    /// <summary>Sets a projected path.</summary>
    public void SetBasePath(PathBuilder path) => _path = path;

    /// <summary>Clears and returns the projected path.</summary>
    public PathBuilder GetOrResetBasePath()
    {
        _path ??= new PathBuilder();
        _path.Reset();

        return _path;
    }

    /// <inheritdoc />
    public void Draw(NoesisDrawingContext context)
    {
        var geometry = GetPath().Geometry;

        if (geometry is null)
        {
            return;
        }

        var viewport = ViewportTransform;

        if (viewport is not null && viewport.IsActive)
        {
            var group = new TransformGroup();

            group.Children.Add(new ScaleTransform { ScaleX = viewport.Zoom, ScaleY = viewport.Zoom, CenterX = viewport.CenterX, CenterY = viewport.CenterY });
            group.Children.Add(new TranslateTransform(viewport.PanX, viewport.PanY));
            context.Retain(group);
            context.Canvas.PushTransform(group);
        }

        context.Retain(geometry);
        context.Canvas.DrawGeometry(context.Fill, context.Stroke, geometry);

        if (viewport is not null && viewport.IsActive)
        {
            context.Canvas.Pop();
        }
    }

    /// <inheritdoc />
    public override bool ContainsPoint(float x, float y)
    {
        if (ViewportTransform is { IsActive: true } viewport)
        {
            (x, y) = viewport.InverseTransform(x, y);
        }

        return GetPath().Geometry?.FillContains(new Point(x, y)) == true;
    }

    private PathBuilder GetPath()
    {
        if (_path is not null)
        {
            return _path;
        }

        _path = new PathBuilder();

        var first = true;

        foreach (var segment in Commands)
        {
            if (first)
            {
                _path.MoveTo(segment.Xi, segment.Yi);
                first = false;
            }
            else
            {
                _path.LineTo(segment.Xi, segment.Yi);
            }
        }

        _path.Close();

        return _path;
    }
}
