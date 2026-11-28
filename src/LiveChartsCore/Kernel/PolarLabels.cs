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
using System.Linq;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;

namespace LiveChartsCore.Kernel;

// Where the labels of a polar chart go, shared by the axes and the step of a radius axis.
internal static class PolarLabels
{
    // The angle in degrees of the line the radius labels are drawn along. LabelsAngle when it is set, otherwise
    // halfway between the first two spokes of the angle axis: a spoke is where the points of the series and the
    // angle labels are, the labels there would sit under the points and the last one on top of an angle label.
    internal static double GetRadiusLabelsAngle(IPolarAxis radiusAxis, PolarChartEngine chart, PolarScaler scaler)
    {
        if (!double.IsNaN(radiusAxis.LabelsAngle)) return radiusAxis.LabelsAngle;

        var angleAxis = chart.AngleAxes[0];
        var bounds = angleAxis.VisibleDataBounds ?? angleAxis.DataBounds;
        var min = angleAxis.MinLimit ?? bounds.Min;

        double first, second;

        var custom = angleAxis.CustomSeparators?.Where(x => x >= min).OrderBy(x => x).Take(2).ToArray();
        if (custom is not null)
        {
            // fewer than two custom spokes, there is no gap to place the labels in, they go along the first one
            if (custom.Length == 0) return scaler.GetAngle(min);
            if (custom.Length == 1) return scaler.GetAngle(custom[0]);
            first = custom[0];
            second = custom[1];
        }
        else
        {
            var step = angleAxis.GetTick(chart).Value;
            if (step < angleAxis.MinStep) step = angleAxis.MinStep;
            if (angleAxis.ForceStepToMin) step = angleAxis.MinStep;
            if (step <= 0) return scaler.GetAngle(min);

            first = Math.Truncate(min / step) * step;
            if (first < min) first += step;
            second = first + step;
        }

        return scaler.GetAngle((first + second) * 0.5);
    }
}
