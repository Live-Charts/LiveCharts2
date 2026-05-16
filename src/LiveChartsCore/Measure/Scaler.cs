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
using LiveChartsCore.Kernel.Sketches;

namespace LiveChartsCore.Measure;

/// <summary>
/// Defines the scaler class, this class helps to scale from the data scale to the user interface scale and vise versa.
/// </summary>
public class Scaler
{
    private readonly double _deltaVal, _m, _mInv, _minPx, _maxPx, _deltaPx, _minVal, _maxVal;
    private readonly AxisOrientation _orientation;

    /// <summary>
    /// Initializes a new instance of the <see cref="Scaler"/> class.
    /// </summary>
    /// <param name="drawMarginLocation">The draw margin location.</param>
    /// <param name="drawMarginSize">Size of the draw margin.</param>
    /// <param name="axis">The axis.</param>
    /// <param name="bounds">Indicates the bounds to use.</param>
    /// <exception cref="Exception">The axis is not ready to be scaled.</exception>
    public Scaler(
        LvcPoint drawMarginLocation,
        LvcSize drawMarginSize,
        ICartesianAxis axis,
        Bounds? bounds = null)
    {
        if (axis.Orientation == AxisOrientation.Unknown)
            throw new Exception("The axis is not ready to be scaled.");

        _orientation = axis.Orientation;

        var max = bounds?.Max ?? axis.MaxLimit ?? axis.VisibleDataBounds.Max;
        var min = bounds?.Min ?? axis.MinLimit ?? axis.VisibleDataBounds.Min;

        if (axis.Orientation == AxisOrientation.X)
        {
            _minPx = drawMarginLocation.X;
            _maxPx = drawMarginLocation.X + drawMarginSize.Width;
            _deltaPx = _maxPx - _minPx;
        }
        else
        {
            _minPx = drawMarginLocation.Y;
            _maxPx = drawMarginLocation.Y + drawMarginSize.Height;
            _deltaPx = _maxPx - _minPx;

            // invert Y, because the y axis is inverted by the screen coordinates
            (max, min) = (min, max);
        }

        AxisLimit.ValidateLimits(ref max, ref min, axis.MinStep);

        if (axis.IsInverted)
            (max, min) = (min, max);

        _maxVal = max;
        _minVal = min;
        _deltaVal = _maxVal - _minVal;

        _m = _deltaPx / _deltaVal;
        _mInv = 1 / _m;

        if (!double.IsNaN(_m) && !double.IsInfinity(_m) &&
            !double.IsNaN(_mInv) && !double.IsInfinity(_mInv)) return;
        _m = 0;
        _mInv = 0;
    }

    internal Scaler()
    {
        _minPx = 0;
        _maxPx = 100;
        _deltaPx = _maxPx - _minPx;

        _maxVal = 0;
        _minVal = 100;
        _deltaVal = _maxVal - _minVal;

        _m = _deltaPx / _deltaVal;
        _mInv = 1 / _m;
    }

    /// <summary>
    /// Converts a given value (in chart values) to pixels.
    /// </summary>
    /// <param name="value">The value in chart values.</param>
    /// <returns></returns>
    public float ToPixels(double value) =>
        unchecked((float)(_minPx + (value - _minVal) * _m));

    /// <summary>
    /// Converts a given value (in pixels) to chart values.
    /// </summary>
    /// <param name="pixels">The value in pixels.</param>
    /// <returns></returns>
    public double ToChartValues(double pixels) =>
        _minVal + (pixels - _minPx) * _mInv;

    /// <summary>
    /// Measures an absolute value in pixels.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public float MeasureInPixels(double value)
    {
        unchecked
        {
            return Math.Abs(
                _orientation == AxisOrientation.X
                    ? (float)(_minPx + (value - _minVal) * _m - (_minPx + (0 - _minVal) * _m))
                    : (float)(_minPx + (0 - _minVal) * _m - (_minPx + (value - _minVal) * _m)));
        }
    }
}
