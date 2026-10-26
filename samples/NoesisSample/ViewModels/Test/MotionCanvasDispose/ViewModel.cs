using System;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.Motion;
using LiveChartsCore.Noesis.Drawing.Geometries;
using LiveChartsCore.Noesis.Painting;
using LiveChartsCore.Drawing;

namespace ViewModelsSamples.Test.MotionCanvasDispose;

public partial class ViewModel : ObservableObject
{
    public static void Generate(CoreMotionCanvas canvas)
    {
        var r = new Random();
        var p = new SolidColorPaint(DrawingColors.Blue, 3);

        canvas.AddDrawableTask(p, LiveChartsCore.Painting.PaintStyle.Fill);

        for (var i = 0; i < 1000; i++)
        {
            var circle = new CircleGeometry { X = r.Next(15, 285), Y = r.Next(15, 285), Width = 5, Height = 5 };
            circle.Animate(EasingFunctions.ElasticOut, TimeSpan.FromSeconds(1));
            p.AddGeometryToPaintTask(canvas, circle);

            circle.X = r.Next(15, 285);
            circle.Y = r.Next(15, 285);
        }

        canvas.Invalidate();
    }
}
