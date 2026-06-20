// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Drawing;
using LiveChartsCore.Motion;
using LiveChartsCore.Painting;

namespace LiveChartsCore.Console.Drawing;

public class ConsoleDrawingContext(CoreMotionCanvas motionCanvas, ConsoleSurface surface, LvcColor background)
    : DrawingContext
{
    public CoreMotionCanvas MotionCanvas { get; } = motionCanvas;
    public ConsoleSurface Surface { get; } = surface;
    public LvcColor Background { get; } = background;
    public LvcColor ActiveColor { get; set; } = new(255, 255, 255);

    public override void LogOnCanvas(string log)
    {
        // Log overlays are not useful in a one-shot console render; ignore.
    }

    internal override void OnBeginDraw()
    {
        Surface.Background = Background;
        Surface.Clear();
    }

    internal override void OnEndDraw() { }

    internal override void OnBeginZone(CanvasZone zone)
    {
        // Each zone carries a Clip rect. CanvasZone.NoClip uses LvcRectangle.Empty
        // (Width = Height = 0) which means "no clip"; CanvasZone.DrawMargin / XCrosshair /
        // YCrosshair carry the chart's draw-margin sub-rectangles. Translate the rect into a
        // surface-level clip so out-of-margin pixels (axis tail-end labels, series points
        // beyond the plot area) are rejected at SetPixel and don't bleed past the axes.
        var clip = zone.Clip;
        if (clip.Width <= 0 || clip.Height <= 0)
        {
            Surface.ResetClip();
        }
        else
        {
            Surface.SetClip((int)clip.X, (int)clip.Y, (int)clip.Width, (int)clip.Height);
        }
    }

    internal override void OnEndZone(CanvasZone zone) =>
        // Reset on every end so anything that draws between zones (or after the last zone)
        // inherits the full-surface clip rather than carrying a stale one.
        Surface.ResetClip();

    internal override void Draw(IDrawnElement drawable)
    {
        var opacity = ActiveOpacity;
        var element = (IDrawnElement<ConsoleDrawingContext>)drawable;

        if (ActiveLvcPaint is null)
        {
            if (element.Fill is not null) DrawByPaint(element.Fill, element, opacity);
            if (element.Stroke is not null) DrawByPaint(element.Stroke, element, opacity);
            if (element.Paint is not null) DrawByPaint(element.Paint, element, opacity);
        }
        else
        {
            if (ActiveLvcPaint.PaintStyle.HasFlag(PaintStyle.Fill))
            {
                if (element.Fill is null) DrawWithOpacity(element, opacity);
                else DrawByPaint(element.Fill, element, opacity);
            }
            if (ActiveLvcPaint.PaintStyle.HasFlag(PaintStyle.Stroke))
            {
                if (element.Stroke is null) DrawWithOpacity(element, opacity);
                else DrawByPaint(element.Stroke, element, opacity);
            }
        }
    }

    private void DrawWithOpacity(IDrawnElement<ConsoleDrawingContext> element, float opacity)
    {
        // ActiveColor was set by an outer SelectPaint call (otherwise we'd be in the
        // ActiveLvcPaint == null branch above). Modulate it by the geometry's opacity for
        // the duration of this Draw, then restore.
        var prevColor = ActiveColor;
        ActiveColor = ApplyOpacity(prevColor, opacity);
        element.Draw(this);
        ActiveColor = prevColor;
    }

    internal override void SelectPaint(Paint paint)
    {
        ActiveLvcPaint = paint;
        PaintMotionProperty.s_activePaint = paint;
        paint.OnPaintStarted(this, null);
    }

    internal override void ClearPaintSelection(Paint paint)
    {
        paint.OnPaintFinished(this, null);
        ActiveLvcPaint = null!;
        PaintMotionProperty.s_activePaint = null!;
    }

    private void DrawByPaint(Paint paint, IDrawnElement<ConsoleDrawingContext> element, float opacity)
    {
        var prevColor = ActiveColor;
        var prevPaint = ActiveLvcPaint;

        if (paint != MeasureTask.Instance)
        {
            ActiveLvcPaint = paint;
            paint.OnPaintStarted(this, element);  // sets ActiveColor from paint.Color
        }

        // Apply geometry opacity AFTER OnPaintStarted (which clobbers ActiveColor with the
        // paint's color). The chart engine sets ActiveOpacity (= geometry.Opacity) before
        // calling Draw — for separators being disposed during a zoom animation the engine
        // animates this from 1 down to 0, producing a fade-out instead of an abrupt
        // disappearance. Surface.SetPixel sees alpha < 255 and blends against Background.
        ActiveColor = ApplyOpacity(ActiveColor, opacity);

        element.Draw(this);

        paint.OnPaintFinished(this, element);

        ActiveColor = prevColor;
        ActiveLvcPaint = prevPaint;
    }

    private static LvcColor ApplyOpacity(LvcColor c, float opacity)
    {
        if (opacity >= 1f) return c;
        if (opacity <= 0f) return new LvcColor(c.R, c.G, c.B, 0);
        return new LvcColor(c.R, c.G, c.B, (byte)(c.A * opacity));
    }
}
