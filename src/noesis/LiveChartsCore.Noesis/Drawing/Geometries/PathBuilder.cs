using Noesis;

namespace LiveChartsCore.Noesis.Drawing.Geometries;

/// <summary>Builds map geometry from projected coordinates.</summary>
public sealed class PathBuilder
{
    private readonly List<PathCommand> _commands = new();
    private StreamGeometry? _geometry;
    private bool _hasContour;

    /// <summary>Clears the path.</summary>
    public void Reset()
    {
        _commands.Clear();
        _geometry = null;
        _hasContour = false;
    }

    /// <summary>Starts a contour.</summary>
    public void MoveTo(float x, float y)
    {
        _hasContour = IsFinite(x) && IsFinite(y);

        if (_hasContour)
        {
            Add(new PathCommand(new Point(x, y), true, 0));
        }
    }

    /// <summary>Adds a line.</summary>
    public void LineTo(float x, float y)
    {
        if (!IsFinite(x) || !IsFinite(y))
        {
            _hasContour = false;

            return;
        }

        if (!_hasContour)
        {
            MoveTo(x, y);

            return;
        }

        Add(new PathCommand(new Point(x, y), false, 0));
    }

    /// <summary>Closes the contour.</summary>
    public void Close()
    {
        _hasContour = false;
    }

    /// <summary>Adds a circle.</summary>
    public void AddCircle(float x, float y, float radius)
    {
        if (!IsFinite(radius) || radius <= 0)
        {
            return;
        }

        MoveTo(x + radius, y);

        if (!_hasContour)
        {
            return;
        }

        Add(new PathCommand(new Point(x - radius, y), false, radius));
        Add(new PathCommand(new Point(x + radius, y), false, radius));
        Close();
    }

    internal StreamGeometry? Geometry
    {
        get
        {
            if (_commands.Count == 0)
            {
                return null;
            }

            if (_geometry is not null)
            {
                return _geometry;
            }

            _geometry = new StreamGeometry();

            using var path = _geometry.Open();

            foreach (var command in _commands)
            {
                if (command.StartsContour)
                {
                    path.BeginFigure(command.Point, true, true);
                }
                else if (command.Radius > 0)
                {
                    path.ArcTo(command.Point, new Size(command.Radius, command.Radius), 0, false, SweepDirection.Clockwise, true, false);
                }
                else
                {
                    path.LineTo(command.Point, true, false);
                }
            }

            return _geometry;
        }
    }

    private void Add(PathCommand command)
    {
        _commands.Add(command);
        _geometry = null;
    }

    private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

    private readonly struct PathCommand(Point point, bool startsContour, float radius)
    {
        public Point Point { get; } = point;

        public bool StartsContour { get; } = startsContour;

        public float Radius { get; } = radius;
    }
}
