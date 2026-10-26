using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;

namespace ViewModelsSamples.Axes.MatchScale;

public static class CustomScaleExtensions
{
    // the DrawMarginDefined event is called once the chart // mark
    // has defined the area where the series will be drawn // mark
    // ignoring the axes, titles, and legends // mark
    // this is where we modify the axes limits to define our custom scale // mark
    public static void DoubleY(this ICartesianChartView chart) =>
        ((CartesianChartEngine)chart.CoreChart).DrawMarginDefined += OnDrawMarginDefined;

    // we are defining the limits of the Y axes to take the double of pixels per unit of data // mark
    private static void OnDrawMarginDefined(CartesianChartEngine chart)
    {
        var x = chart.XAxes[0];
        var y = chart.YAxes[0];

        var xMin = x.MinLimit ?? x.DataBounds.Min;
        var xMax = x.MaxLimit ?? x.DataBounds.Max;

        // validating the limits ensures that when the chart is empty
        // the axis limits use a rasonable default value.
        AxisLimit.ValidateLimits(ref xMin, ref xMax, x.MinStep);

        x.SetLimits(xMin, xMax, notify: false);

        var xScreenDataRatio = chart.DrawMarginSize.Width / (xMax - xMin);

        // with some omitted Algebra, we know that the required range
        // in the Y axis needs to satisfy the next formula:
        // yDelta = height / (xScreenDataRatio * timesScale)

        const int timesScale = 2;
        var yDelta = chart.DrawMarginSize.Height / (xScreenDataRatio * timesScale);
        var midY = GetMidPoint(y);

        // finally we set the limits of the Y axis
        // to +-0.5 * yDelta from the mid point
        y.SetLimits(
            midY - 0.5f * yDelta,
            midY + 0.5f * yDelta,
            notify: false);

        // it is important to use notify: false // mark
        // to avoid the chart to update once we set the limits. // mark
    }

    private static double GetMidPoint(ICartesianAxis axis)
    {
        var min = axis.MinLimit ?? axis.DataBounds.Min;
        var max = axis.MaxLimit ?? axis.DataBounds.Max;
        return (min + max) * 0.5f;
    }
}
