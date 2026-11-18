using System;
using System.Collections.Generic;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Drawing.Layouts;
using LiveChartsCore.Kernel;
using LiveChartsCore.Noesis.Drawing;
using LiveChartsCore.Noesis.Drawing.Geometries;
using LiveChartsCore.Noesis.Drawing.Layouts;
using LiveChartsCore.Noesis.Painting;
using LiveChartsCore.Noesis;

namespace ViewModelsSamples.General.TemplatedTooltips;

public class CustomTooltip : DefaultTooltip
{
    // the Initialize method is used to set up the tooltip animations, colors, etc.
    protected override void Initialize(Chart chart)
    {
        var theme = chart.GetTheme();

        // the Wedge property, defines the size of the triangle that points to the target point.
        Wedge = 5;

        Geometry.Fill = theme.TooltipBackgroundPaint;
        Geometry.Stroke = new SolidColorPaint(new LvcColor(180, 180, 180, 100), 3);
        Geometry.Wedge = Wedge;
        Geometry.WedgeThickness = 2;

        this.Animate(
            new Animation(
                EasingFunctions.BounceOut,
                TimeSpan.FromMilliseconds(500)));
    }

    // the GetLayout method is used to define the content of the tooltip,
    // it is called every time the tooltip changes.
    protected override Layout<NoesisDrawingContext> GetLayout(IEnumerable<ChartPoint> foundPoints, Chart chart)
    {
        var theme = chart.GetTheme();

        var layout = new StackLayout
        {
            Padding = new(10),
            Orientation = ContainerOrientation.Vertical,
            HorizontalAlignment = Align.Middle,
            VerticalAlignment = Align.Middle
        };

        var i = 0;
        foreach (var point in foundPoints)
        {
            i++;

            var series = point.Context.Series;
            var geometryPoint = (GeometryPoint)point.Context.DataSource!;

            var miniature =
                (IDrawnElement<NoesisDrawingContext>)series.GetMiniatureGeometry(point);

            var label = new LabelGeometry
            {
                Text = point.Coordinate.PrimaryValue.ToString("C2"),
                Paint = theme.TooltipTextPaint,
                TextSize = 15,
                Padding = new Padding(8, 0),
                VerticalAlign = Align.Start,
                HorizontalAlign = Align.Start
            };

            var customContent = new VariableSVGPathGeometry
            {
                Fill = theme.TooltipTextPaint,
                Width = 30,
                Height = 30,
                SVGPath = geometryPoint.Geometry
            };

            var sp = new StackLayout
            {
                Orientation = ContainerOrientation.Horizontal,
                Padding = new Padding(0, 4),
                VerticalAlignment = Align.Middle,
                HorizontalAlignment = Align.Middle,
                Children =
                {
                    miniature,
                    label,
                    customContent
                }
            };

            layout.Children.Add(sp);
        }

        return layout;
    }

    public override void Show(IEnumerable<ChartPoint> foundPoints, Chart chart)
    {
        base.Show(foundPoints, chart);

        // write code here to add custom behavior when the tooltip is shown.
    }

    public override void Hide(Chart chart)
    {
        base.Hide(chart);

        // write code here to add custom behavior when the tooltip is hidden.
    }
}
