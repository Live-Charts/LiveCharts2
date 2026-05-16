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

namespace LiveChartsCore.Kernel;

internal static class GestureHelpers
{
    // Two presses qualify as a double-tap (and therefore as a "secondary"
    // press — used to enter zoom-by-zone mode on platforms without a native
    // right-click) when they happen within DoubleTapMaxIntervalMs of each
    // other AND the second press lands within sqrt(DoubleTapMaxDistanceSq)
    // pixels of the first. The distance guard prevents two clearly different
    // taps in quick succession from being merged into a phantom double-tap;
    // 12px is roughly the touch-target slop iOS/Android allow on their own
    // double-tap recognizers.
    public const int DoubleTapMaxIntervalMs = 500;
    public const float DoubleTapMaxDistanceSq = 144f;

    public static bool IsDoubleTap(LvcPoint current, LvcPoint previous, TimeSpan elapsed)
    {
        if (elapsed.TotalMilliseconds >= DoubleTapMaxIntervalMs) return false;
        var dx = current.X - previous.X;
        var dy = current.Y - previous.Y;
        return dx * dx + dy * dy < DoubleTapMaxDistanceSq;
    }
}
