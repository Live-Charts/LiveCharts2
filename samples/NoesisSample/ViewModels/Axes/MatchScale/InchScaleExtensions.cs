using System;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Sketches;

namespace ViewModelsSamples.Axes.MatchScale;

public static class InchScaleExtensions
{
    // the DrawMarginDefined event is called once the chart // mark
    // has defined the area where the series will be drawn // mark
    // ignoring the axes, titles, and legends // mark
    // this is where we modify the axes limits to define our custom scale // mark
    public static void InchSeparator(this ICartesianChartView chart) =>
        ((CartesianChartEngine)chart.CoreChart).DrawMarginDefined += OnDrawMarginDefined;

    // we will force the axis step to be 1 inch long // mark
    private static void OnDrawMarginDefined(CartesianChartEngine chart)
    {
        var dataPerInch = GetDataPerInch((ICartesianChartView)chart.View);

        // knowing the data per inch, we can set an step that will, in this case 1 inch
        var inches = 1d;

        var x = chart.XAxes[0];
        var xMin = x.MinLimit ?? x.DataBounds.Min;
        var xMax = x.MaxLimit ?? x.DataBounds.Max;
        var xStep = inches * dataPerInch.X;

        x.SetLimits(xMin, xMax, xStep, notify: false);

        var y = chart.YAxes[0];
        var yMin = y.MinLimit ?? y.DataBounds.Min;
        var yMax = y.MaxLimit ?? y.DataBounds.Max;
        var yStep = inches * dataPerInch.Y;

        y.SetLimits(yMin, yMax, yStep, notify: false);

        // it is important to use notify: false // mark
        // to avoid the chart to update once we set the limits. // mark
    }

    private static LvcPointD GetDataPerInch(ICartesianChartView chart)
    {
        var p0 = chart.ScaleDataToPixels(new(0, 0));
        var p1 = chart.ScaleDataToPixels(new(1, 1));

        // calculate the distance between 0,0 and 1,1 in pixels
        var pux = Math.Abs(p0.X - p1.X);
        var puy = Math.Abs(p0.Y - p1.Y);

        var ppi = GetPixelsPerInch();

        // calculate the data per inch
        var dpix = ppi / pux;
        var dpiy = ppi / puy;

        return new LvcPointD(dpix, dpiy);
    }

    // this is an example, in a real scenario you should get the screen PPI
    private static double GetPixelsPerInch() => 96.0;
}
