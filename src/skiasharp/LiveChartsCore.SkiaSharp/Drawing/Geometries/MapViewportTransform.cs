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

using SkiaSharp;

namespace LiveChartsCore.SkiaSharpView.Drawing.Geometries;

/// <summary>
/// Holds shared viewport transform state for all land geometries.
/// A single instance is shared across all geometries — update once, affects all draws.
/// </summary>
public class MapViewportTransform
{
    /// <summary>
    /// Gets or sets the zoom level.
    /// </summary>
    public float Zoom { get; set; } = 1f;

    /// <summary>
    /// Gets or sets the pan X offset.
    /// </summary>
    public float PanX { get; set; }

    /// <summary>
    /// Gets or sets the pan Y offset.
    /// </summary>
    public float PanY { get; set; }

    /// <summary>
    /// Gets or sets the center X of the control.
    /// </summary>
    public float CenterX { get; set; }

    /// <summary>
    /// Gets or sets the center Y of the control.
    /// </summary>
    public float CenterY { get; set; }

    /// <summary>
    /// Gets whether this transform is non-identity.
    /// </summary>
    public bool IsActive
    {
        get
        {
            const float epsilon = 1e-5f;
            var zoomIsIdentity = System.Math.Abs(Zoom - 1f) < epsilon;
            var panXIsIdentity = System.Math.Abs(PanX) < epsilon;
            var panYIsIdentity = System.Math.Abs(PanY) < epsilon;
            return !(zoomIsIdentity && panXIsIdentity && panYIsIdentity);
        }
    }

    /// <summary>
    /// Builds the SKMatrix for the current viewport transform.
    /// </summary>
    public SKMatrix GetMatrix()
    {
        var tx = CenterX * (1 - Zoom) + PanX;
        var ty = CenterY * (1 - Zoom) + PanY;
        return new SKMatrix(Zoom, 0, tx, 0, Zoom, ty, 0, 0, 1);
    }

    /// <summary>
    /// Inverse-transforms a screen point to base (unzoomed) coordinates.
    /// </summary>
    public (float x, float y) InverseTransform(float screenX, float screenY)
    {
        var tx = CenterX * (1 - Zoom) + PanX;
        var ty = CenterY * (1 - Zoom) + PanY;
        return ((screenX - tx) / Zoom, (screenY - ty) / Zoom);
    }
}
