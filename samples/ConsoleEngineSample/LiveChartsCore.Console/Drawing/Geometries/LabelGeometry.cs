// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Drawing;

namespace LiveChartsCore.Console.Drawing.Geometries;

public class LabelGeometry : BaseLabelGeometry, IDrawnElement<ConsoleDrawingContext>
{
    /// <summary>Sub-pixels per character, horizontally — set by <see cref="InMemoryConsoleChart"/> based on render mode.</summary>
    internal static int GlyphPixelsW = 1;

    /// <summary>Sub-pixels per character, vertically.</summary>
    internal static int GlyphPixelsH = 2;

    public virtual void Draw(ConsoleDrawingContext context)
    {
        if (string.IsNullOrEmpty(Text)) return;

        var size = MeasuredSize();
        var dx = HorizontalAlign switch
        {
            Align.Start => 0f,
            Align.Middle => -size.Width / 2f,
            Align.End => -size.Width,
            _ => 0f
        };
        var dy = VerticalAlign switch
        {
            Align.Start => 0f,
            Align.Middle => -GlyphPixelsH / 2f,
            Align.End => -GlyphPixelsH,
            _ => 0f
        };

        var x = (int)(X + dx);
        var y = (int)(Y + dy);

        // Cell-grid modes emit ANSI text glyphs; Sixel renders labels as bitmap pixels into
        // the image so they ship with the rest of the frame and don't need a second cell-text
        // pass (which would flicker between the Sixel write and the overlay write).
        if (context.Surface.Mode == ConsoleRenderMode.Sixel)
            BitmapFont.DrawText(context.Surface, x, y, Text, context.ActiveColor);
        else
            context.Surface.DrawText(x, y, Text, context.ActiveColor);
    }

    public override LvcSize Measure() => MeasuredSize();

    private LvcSize MeasuredSize()
    {
        var w = (Text?.Length ?? 0) * GlyphPixelsW + Padding.Left + Padding.Right;
        var h = GlyphPixelsH + Padding.Top + Padding.Bottom;
        return new LvcSize(w, h);
    }
}
