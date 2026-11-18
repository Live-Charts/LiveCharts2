using LiveChartsCore.Drawing;
using LiveChartsCore.Geo;
using LiveChartsCore.Motion;
using LiveChartsCore.Noesis.Drawing.Geometries;
using LiveChartsCore.Painting;

namespace LiveChartsCore.Noesis;

internal sealed class DefaultGeoTooltip : IGeoMapTooltip
{
    private DrawnTask? _task;

    public void Show(GeoTooltipPoint point, GeoMapChart chart)
    {
        Hide(chart);

        var theme = chart.GetTheme();
        var label = new LabelGeometry
        {
            Text = $"{point.Land.Name}: {point.Value:N2}",
            TextSize = 16,
            Padding = new Padding(10),
            Paint = theme.TooltipTextPaint,
            HorizontalAlign = Align.Start,
            VerticalAlign = Align.Start,
            X = point.LandCenter.X,
            Y = point.LandCenter.Y
        };
        var size = label.Measure();
        var background = new RoundedRectangleGeometry
        {
            X = label.X,
            Y = label.Y,
            Width = size.Width,
            Height = size.Height,
            Fill = theme.TooltipBackgroundPaint
        };

        _task = chart.Canvas.AddGeometry(CanvasZone.Overlay, background, label);
        _task.ZIndex = 10100;
        chart.Canvas.Invalidate();
    }

    public void Hide(GeoMapChart chart)
    {
        if (_task is null)
        {
            return;
        }

        chart.Canvas.RemovePaintTask(_task);
        _task = null;
        chart.Canvas.Invalidate();
    }
}
