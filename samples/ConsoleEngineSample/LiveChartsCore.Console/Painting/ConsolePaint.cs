// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Drawing;
using LiveChartsCore.Painting;

namespace LiveChartsCore.Console.Painting;

public abstract class ConsolePaint(float strokeThickness = 1f, float strokeMiter = 0f)
    : Paint(strokeThickness, strokeMiter)
{
    internal override void OnPaintFinished(DrawingContext context, IDrawnElement? drawnElement) { }

    internal override void DisposeTask() { }
}
