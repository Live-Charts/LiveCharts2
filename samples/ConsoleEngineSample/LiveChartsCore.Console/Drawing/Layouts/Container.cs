using LiveChartsCore.Console.Drawing.Geometries;
using LiveChartsCore.Drawing;
using LiveChartsCore.Drawing.Layouts;

namespace LiveChartsCore.Console.Drawing.Layouts;

public class Container : Container<RoundedRectangleGeometry> { }

public class Container<TShape> : BaseContainer<TShape, ConsoleDrawingContext>
    where TShape : BoundedDrawnGeometry, IDrawnElement<ConsoleDrawingContext>, new()
{ }
