using System;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Generators;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Noesis.Drawing;
using LiveChartsCore.Noesis.Painting;

namespace ViewModelsSamples.General.DrawOnCanvas;

// in this example, we use the UpdateStartedCommand to add our custom // mark
// geometry to the chart, then we use the PointerPressedCommand to change the //mark
// size of the geometry when the user clicks on it. // mark

public partial class ViewModel
{
    private bool _isBig;
    private readonly MotionGeometry _geometry = new()
    {
        Diameter = 20,
        Fill = new SolidColorPaint(DrawingColors.Blue.WithAlpha(100)),
        Stroke = new SolidColorPaint(DrawingColors.Blue, strokeWidth: 3)
    };

    [RelayCommand]
    public void ChartUpdated(ChartCommandArgs args)
    {
        var chartView = (ICartesianChartView)args.Chart;

        // lets place the geometry at (5, 5) in the chart values scale
        var locationInChartValues = new LvcPointD(5, 5);
        var locationInPixels = chartView.ScaleDataToPixels(locationInChartValues);

        _geometry.X = (float)locationInPixels.X;
        _geometry.Y = (float)locationInPixels.Y;

        _geometry.Initialize(args);
    }

    [RelayCommand]
    public void ChartPressed(PointerCommandArgs args)
    {
        _geometry.Diameter = _isBig ? 20 : 70;
        _isBig = !_isBig;
        args.Chart.Invalidate();
    }
}

public partial class MotionGeometry : BoundedDrawnGeometry, IDrawnElement<NoesisDrawingContext>
{
    private bool _isInitialized;

    // The MotionProperty attribute makes the property animatable, // mark
    // both the property and the class must be marked as partial // mark
    // partial properties were introduced in C# 13 // mark
    // A motion property will animate any change in the property // mark
    // according to the Animation defined in the Animate method. // mark

    [MotionProperty]
    public partial float Diameter { get; set; }

    public void Draw(NoesisDrawingContext context)
    {
        context.Canvas.DrawEllipse(context.Fill, context.Stroke, new global::Noesis.Point(X, Y), Diameter, Diameter);
    }

    public void Initialize(ChartCommandArgs args)
    {
        if (_isInitialized) return;

        args.Chart.CoreChart.Canvas.AddGeometry(this);

        var animation = new Animation(
            easingFunction: EasingFunctions.BounceOut,
            duration: TimeSpan.FromMilliseconds(800));

        this.Animate(animation);

        _isInitialized = true;
    }
}
