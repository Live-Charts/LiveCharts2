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
using System.Collections.Generic;
using LiveChartsCore.Kernel;

namespace LiveChartsCore.SkiaSharpView;

/// <summary>
/// Defines a DateTime axis.
/// </summary>
public class DateTimeAxis : Axis
{
    static DateTimeAxis()
    {
        LiveChartsSkiaSharp.EnsureInitialized();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DateTimeAxis"/> class.
    /// </summary>
    /// <param name="unit">The unit of the axis (hours, days, months, years).</param>
    /// <param name="formatter">The labels formatter.</param>
    public DateTimeAxis(TimeSpan unit, Func<DateTime, string> formatter)
    {
        UnitWidth = unit.Ticks;
        Labeler = value => formatter(value.AsDate());
        MinStep = unit.Ticks;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the axis should group its time units into adaptive,
    /// multi-level labels: a fine unit on the first line (seconds, minutes, hours, days, months or
    /// years — whichever fits the visible range) and a coarser context on the second, re-tiering as
    /// you zoom. Default is false.
    /// </summary>
    public bool GroupTimeUnits { get; set => SetProperty(ref field, value); }

    /// <inheritdoc cref="CoreAxis{TTextGeometry, TLineGeometry}.GroupsSeparators"/>
    protected override bool GroupsSeparators => GroupTimeUnits;

    /// <inheritdoc cref="CoreAxis{TTextGeometry, TLineGeometry}.TryGroupSeparators"/>
    protected override bool TryGroupSeparators(
        Chart chart, double min, double max,
        out IEnumerable<double>? separators, out Func<double, string>? labeler)
    {
        if (GroupTimeUnits) return DateTimeGrouping.TryGroup(min, max, out separators, out labeler);

        separators = null;
        labeler = null;
        return false;
    }

    /// <inheritdoc cref="CoreAxis{TTextGeometry, TLineGeometry}.GetBandParityAnchor"/>
    protected override long GetBandParityAnchor(double min, double max, IReadOnlyList<double> separators) =>
        GroupTimeUnits
            ? DateTimeGrouping.GetCellOrdinal(min, max, separators[0])
            : base.GetBandParityAnchor(min, max, separators);
}
