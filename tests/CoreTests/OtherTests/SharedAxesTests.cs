// The MIT License(MIT)
//
// Copyright(c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Linq;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.OtherTests;

[TestClass]
public class SharedAxesTests
{
    // SharedAxes.Set links a group of axes so they zoom/pan together and a
    // measure on one chart contributes its data bounds to the others. The
    // feature had no dedicated coverage; these tests pin down the contract
    // (group wiring, SetLimits propagation, GetLimits aggregation, zoom
    // propagation). #2159's shared-axes regression lives separately in
    // Issue2159ZoomLimitsTests.

    // Two charts whose X axes are a shared group, each carrying its own
    // line series so the axes get real data bounds after a measure.
    private static (Axis ax1, Axis ax2, CartesianChartEngine core1, CartesianChartEngine core2)
        CreateSharedCharts(double[] data1, double[] data2)
    {
        var ax1 = new Axis();
        var ax2 = new Axis();

        SharedAxes.Set(ax1, ax2);

        var chart1 = new SKCartesianChart
        {
            Width = 800,
            Height = 400,
            ZoomMode = ZoomAndPanMode.Both,
            XAxes = [ax1],
            YAxes = [new Axis()],
            Series = [new LineSeries<double> { Values = data1 }]
        };
        var chart2 = new SKCartesianChart
        {
            Width = 800,
            Height = 400,
            ZoomMode = ZoomAndPanMode.Both,
            XAxes = [ax2],
            YAxes = [new Axis()],
            Series = [new LineSeries<double> { Values = data2 }]
        };

        _ = chart1.GetImage();
        _ = chart2.GetImage();

        return (ax1, ax2, (CartesianChartEngine)chart1.CoreChart, (CartesianChartEngine)chart2.CoreChart);
    }

    [TestMethod]
    public void Set_WiresSharedWithBothWays_AndExcludesSelf()
    {
        var a = new Axis();
        var b = new Axis();
        var c = new Axis();

        SharedAxes.Set(a, b, c);

        var aShared = a.SharedWith!.ToList();
        var bShared = b.SharedWith!.ToList();
        var cShared = c.SharedWith!.ToList();

        // each axis is shared with every OTHER axis in the group, never itself
        CollectionAssert.AreEquivalent(new ICartesianAxis[] { b, c }, aShared);
        CollectionAssert.AreEquivalent(new ICartesianAxis[] { a, c }, bShared);
        CollectionAssert.AreEquivalent(new ICartesianAxis[] { a, b }, cShared);
    }

    [TestMethod]
    public void SetLimits_PropagatesAcrossTheSharedGroup()
    {
        var a = new Axis();
        var b = new Axis();
        var c = new Axis();
        SharedAxes.Set(a, b, c);

        ((ICartesianAxis)a).SetLimits(5, 25);

        // the limits set on one axis must land on every sibling
        Assert.AreEqual(5, b.MinLimit);
        Assert.AreEqual(25, b.MaxLimit);
        Assert.AreEqual(5, c.MinLimit);
        Assert.AreEqual(25, c.MaxLimit);
    }

    [TestMethod]
    public void SetLimits_WithPropagateSharedFalse_StaysLocal()
    {
        var a = new Axis();
        var b = new Axis();
        SharedAxes.Set(a, b);

        ((ICartesianAxis)a).SetLimits(5, 25, propagateShared: false);

        Assert.AreEqual(5, a.MinLimit);
        Assert.IsNull(b.MinLimit,
            "propagateShared: false must keep the change off the shared siblings.");
        Assert.IsNull(b.MaxLimit,
            "propagateShared: false must keep the change off the shared siblings.");
    }

    [TestMethod]
    public void GetLimits_UnionsViewAndDataBoundsAcrossSharedGroup()
    {
        // chart1 X data spans indices [0, 9], chart2 X data spans [0, 4].
        var (ax1, ax2, _, _) = CreateSharedCharts(
            [1, 2, 3, 4, 5, 6, 7, 8, 9, 10],
            [1, 2, 3, 4, 5]);

        // a pin on one axis must surface as the view max of the whole group
        ax1.MaxLimit = 100;

        var limits = ((ICartesianAxis)ax2).GetLimits();

        Assert.AreEqual(100, limits.Max,
            "GetLimits() must surface a sibling's pinned MaxLimit as the group view max.");
        // ax2's own series only reaches X index 4; seeing 9 proves the wider
        // sibling's data bounds were unioned in (via AppendLimits + GetLimits).
        Assert.IsTrue(limits.DataMax >= 9 - 1e-6,
            $"GetLimits() must union DataMax across the shared group; got {limits.DataMax}.");
    }

    [TestMethod]
    public void Zoom_OnOneChart_PropagatesLimitsToSharedSibling()
    {
        var (ax1, ax2, core1, _) = CreateSharedCharts(
            [1, 2, 3, 4, 5, 6, 7, 8, 9, 10],
            [1, 2, 3, 4, 5]);

        core1.Zoom(ZoomAndPanMode.Both, new LvcPoint(400, 200), ZoomDirection.ZoomIn);

        Assert.IsNotNull(ax1.MinLimit, "ZoomIn must assign the zoomed axis a concrete MinLimit.");
        Assert.AreEqual(ax1.MinLimit, ax2.MinLimit,
            "A zoom on one chart must propagate the new MinLimit to its shared sibling.");
        Assert.AreEqual(ax1.MaxLimit, ax2.MaxLimit,
            "A zoom on one chart must propagate the new MaxLimit to its shared sibling.");
    }
}
