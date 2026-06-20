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

using System;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.CoreObjectsTests;

[TestClass]
public class GestureHelpersTests
{
    [TestMethod]
    public void IsDoubleTap_NearbyAndFastEnough_ReturnsTrue()
    {
        // Same spot, well inside the 500ms window — classic double-tap.
        var first = new LvcPoint(100, 100);
        var second = new LvcPoint(102, 99); // 2px+1px ≈ √5 ≈ 2.2px
        Assert.IsTrue(GestureHelpers.IsDoubleTap(second, first, TimeSpan.FromMilliseconds(200)));
    }

    [TestMethod]
    public void IsDoubleTap_TooFarApart_ReturnsFalse()
    {
        // Two clearly distinct taps in quick succession must NOT register as a
        // double-tap — this is the regression for the missing spatial guard.
        var first = new LvcPoint(100, 100);
        var second = new LvcPoint(200, 200); // ~141px diagonal
        Assert.IsFalse(GestureHelpers.IsDoubleTap(second, first, TimeSpan.FromMilliseconds(100)));
    }

    [TestMethod]
    public void IsDoubleTap_SlowSecondTap_ReturnsFalse()
    {
        // Same spot but past the 500ms window — too slow to count.
        var first = new LvcPoint(100, 100);
        var second = new LvcPoint(101, 101);
        Assert.IsFalse(GestureHelpers.IsDoubleTap(second, first, TimeSpan.FromMilliseconds(600)));
    }

    [TestMethod]
    public void IsDoubleTap_AtThreshold_RejectsToAvoidEdgeCase()
    {
        // The check is strict-less-than: a tap that lands exactly on the radius
        // boundary should not count as a double-tap. Pick (12, 0) so dx²+dy²
        // equals DoubleTapMaxDistanceSq (144).
        var first = new LvcPoint(0, 0);
        var second = new LvcPoint(12, 0);
        Assert.IsFalse(GestureHelpers.IsDoubleTap(second, first, TimeSpan.FromMilliseconds(50)));
    }
}
