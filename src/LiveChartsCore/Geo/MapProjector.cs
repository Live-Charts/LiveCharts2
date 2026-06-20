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

namespace LiveChartsCore.Geo;

/// <summary>
/// The map projector class.
/// </summary>
public abstract class MapProjector
{
    /// <summary>
    /// Gets the map width.
    /// </summary>
    public float MapWidth { get; protected set; }

    /// <summary>
    /// Gets the map height.
    /// </summary>
    public float MapHeight { get; protected set; }

    /// <summary>
    /// Gets the x offset width.
    /// </summary>
    public float XOffset { get; protected set; }

    /// <summary>
    /// Gets the y offset.
    /// </summary>
    public float YOffset { get; protected set; }

    /// <summary>
    /// Projects the given point.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <returns></returns>
    public abstract float[] ToMap(double[] point);

    /// <summary>
    /// Allocation-free projection. Default implementation forwards to the
    /// array-based <see cref="ToMap(double[])"/> for back-compat with custom
    /// projectors that only override the array-based overload; built-in
    /// projectors override this to skip the per-point allocation entirely.
    /// During orthographic rotation this is called tens of thousands of
    /// times per frame, so the difference is material.
    /// </summary>
    /// <param name="longitude">The longitude.</param>
    /// <param name="latitude">The latitude.</param>
    /// <param name="x">Projected screen X.</param>
    /// <param name="y">Projected screen Y.</param>
    public virtual void ToMap(double longitude, double latitude, out float x, out float y)
    {
        var r = ToMap([longitude, latitude]);
        x = r[0];
        y = r[1];
    }

    /// <summary>
    /// Determines whether a point at the given longitude and latitude is visible
    /// in this projection. Always returns true for flat projections.
    /// </summary>
    /// <param name="longitude">The longitude.</param>
    /// <param name="latitude">The latitude.</param>
    /// <returns>True if the point is visible.</returns>
    public virtual bool IsVisible(double longitude, double latitude) => true;

    /// <summary>
    /// Inverse of <see cref="ToMap(double, double, out float, out float)"/> —
    /// converts a screen-space pixel back to (longitude, latitude). Returns
    /// false when the pixel is outside the projection's visible region
    /// (e.g. the back hemisphere on Orthographic).
    /// </summary>
    /// <param name="screenX">Pixel X.</param>
    /// <param name="screenY">Pixel Y.</param>
    /// <param name="longitude">The longitude, if recoverable.</param>
    /// <param name="latitude">The latitude, if recoverable.</param>
    /// <returns>True if the pixel maps back to a valid coordinate.</returns>
    public abstract bool ToCoordinates(float screenX, float screenY, out double longitude, out double latitude);
}
