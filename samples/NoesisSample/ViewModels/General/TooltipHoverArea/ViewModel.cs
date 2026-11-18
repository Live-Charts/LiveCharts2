using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Drawing;
using LiveChartsCore.Measure;
using LiveChartsCore.Motion;
using LiveChartsCore.Noesis;
using LiveChartsCore.Noesis.Drawing.Geometries;

namespace ViewModelsSamples.General.TooltipHoverArea;

public class ViewModel
{
    // we use the CustomColumnSeries class to override the OnPointMeasured method.
    // when each point is measured, we want to replace the hover area with a custom hover area.
    public ISeries[] Series { get; set; } = [
        new CustomColumnSeries<double>
        {
            Name = "Asia",
            Values = [ 37.833, 27.058, 21.516, 20.742, 15.029 ]
        },
        new CustomColumnSeries<double>
        {
            Name = "Europe",
            Values = [ 12.537, 9.304, 5.383, 3.769, 3.223 ]
        }
    ];
}

public class CustomColumnSeries<T> : ColumnSeries<T>
{
    protected override void OnPointMeasured(ChartPoint point)
    {
        var rectangle = (RoundedRectangleGeometry)point.Context.Visual!;

        var x = GetTargetValue(rectangle, RoundedRectangleGeometry.XProperty);
        var y = GetTargetValue(rectangle, RoundedRectangleGeometry.YProperty);
        var w = GetTargetValue(rectangle, RoundedRectangleGeometry.WidthProperty);
        var h = GetTargetValue(rectangle, RoundedRectangleGeometry.HeightProperty);

        // we replace the point hover area with a custom hover area
        // the area also suggests the tooltip location, this location could change
        // when there is not enough space in the chart.

        var customHoverArea = new CustomHoverArea
        {
            X = x,
            Y = y,
            Width = w,
            Height = h,
            SuggestedTooltipLocation = new LvcPoint(x + w * 0.5f, y)
        };

        point.Context.HoverArea = customHoverArea;
    }

    private static float GetTargetValue(Animatable animatable, PropertyDefinition propertyDefinition)
    {
        // get the motion property in the shape that handles animations.
        var motionProperty = (MotionProperty<float>)propertyDefinition.GetMotion(animatable)!;

        // return the target value, it is the value where the animation will end.
        return motionProperty.ToValue;
    }
}

public class CustomHoverArea : RectangleHoverArea
{
    public override bool IsPointerOver(LvcPoint pointerLocation, FindingStrategy strategy)
    {
        // lets override the logic to determine if the pointer is over the hover area.
        // in this case, we ignore the strategy parameter, we only want to show the tooltip
        // when the pointer is exactly over the column.

        var isInsideXAxis = X <= pointerLocation.X && X + Width >= pointerLocation.X;
        var isInsideYAxis = Y <= pointerLocation.Y && Y + Height >= pointerLocation.Y;

        return isInsideXAxis && isInsideYAxis;
    }
}
