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
using LiveChartsCore.Kernel;
using LiveChartsCore.Painting;
using LiveChartsCore.VisualElements;

namespace LiveChartsCore.Geo;

/// <summary>
/// Wraps a <see cref="VisualElement"/> and positions it at a geographic
/// (longitude, latitude) on a <see cref="GeoMapChart"/>. Each measure pass
/// re-projects the coordinate via the chart's current projector and writes
/// the result onto the inner visual's <see cref="VisualElement.X"/> /
/// <see cref="VisualElement.Y"/>, so the overlay follows zoom, pan, and
/// orthographic rotation. When the coordinate isn't visible (e.g. the back
/// hemisphere on <see cref="MapProjection.Orthographic"/>), the inner
/// visual is removed from the UI for the frame so it doesn't ghost at
/// its last on-screen position.
/// </summary>
public class GeoVisualElement : ChartElement
{
    /// <summary>
    /// Initializes a new <see cref="GeoVisualElement"/> wrapping the given
    /// inner visual.
    /// </summary>
    /// <param name="visual">The visual to position at (Longitude, Latitude).</param>
    public GeoVisualElement(VisualElement visual)
    {
        Visual = visual ?? throw new ArgumentNullException(nameof(visual));
    }

    /// <summary>Gets the wrapped visual element.</summary>
    public VisualElement Visual { get; }

    /// <summary>Gets or sets the longitude (in degrees) where the visual is anchored.</summary>
    public double Longitude { get; set => SetProperty(ref field, value); }

    /// <summary>Gets or sets the latitude (in degrees) where the visual is anchored.</summary>
    public double Latitude { get; set => SetProperty(ref field, value); }

    /// <inheritdoc cref="ChartElement.Invalidate(Chart)"/>
    public override void Invalidate(Chart chart)
    {
        if (chart is not GeoMapChart geoMap)
            throw new InvalidOperationException(
                $"{nameof(GeoVisualElement)} can only be used on a {nameof(GeoMapChart)}.");

        var pixel = geoMap.Project(Longitude, Latitude);
        if (pixel is null)
        {
            // Off the visible region (e.g. orthographic back hemisphere).
            // Pull the inner visual off the canvas for this frame so it
            // doesn't render at its last on-screen position.
            Visual.RemoveFromUI(chart);
            return;
        }

        Visual.X = pixel.Value.X;
        Visual.Y = pixel.Value.Y;
        Visual.Invalidate(chart);

        // Chart.AddVisual normally calls RemoveOldPaints after Invalidate to
        // drop paint tasks the visual no longer references (e.g. user swapped
        // Fill from green to red — green paint stays on the canvas otherwise).
        // The wrapped visual goes through our Invalidate, not AddVisual, so
        // mirror that step explicitly.
        Visual.RemoveOldPaints(chart.View);
    }

    /// <inheritdoc cref="ChartElement.RemoveFromUI(Chart)"/>
    // base.RemoveFromUI iterates GetPaintTasks() (which delegates to Visual),
    // so calling Visual.RemoveFromUI is enough; the base call would double-
    // remove the same paint tasks.
    public override void RemoveFromUI(Chart chart) => Visual.RemoveFromUI(chart);

    /// <inheritdoc cref="ChartElement.GetPaintTasks"/>
    protected internal override Paint?[] GetPaintTasks() => Visual.GetPaintTasks();
}
