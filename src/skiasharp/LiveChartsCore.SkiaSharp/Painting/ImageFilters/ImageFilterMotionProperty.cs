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

using LiveChartsCore.Motion;

namespace LiveChartsCore.SkiaSharpView.Painting.ImageFilters;

/// <summary>
/// A motion property that animates an <see cref="ImageFilter"/>, interpolating between two filters
/// with <see cref="ImageFilter.Transitionate(ImageFilter?, ImageFilter?, float)"/> — the image-filter
/// counterpart of <see cref="Effects.PathEffectMotionProperty"/>. Assigning a new filter can soft-
/// transition; a self-animating filter (one carrying a looping <see cref="ImageFilter.Animation"/>)
/// keeps the rail running indefinitely, so the paint never needs an "is animating" flag.
/// </summary>
/// <param name="defaultValue">The default filter.</param>
public class ImageFilterMotionProperty(ImageFilter? defaultValue = null)
    : MotionProperty<ImageFilter?>(defaultValue)
{
    /// <inheritdoc cref="MotionProperty{T}.CanTransitionate"/>
    protected override bool CanTransitionate => (FromValue ?? ToValue) is not null;

    /// <inheritdoc cref="MotionProperty{T}.OnGetMovement(float)"/>
    // The instance Transitionate is protected, so go through the assembly-internal static one
    // (it also handles the default-filter fallback when an endpoint is null).
    protected override ImageFilter? OnGetMovement(float progress) =>
        ImageFilter.Transitionate(FromValue ?? ToValue, ToValue, progress);
}
