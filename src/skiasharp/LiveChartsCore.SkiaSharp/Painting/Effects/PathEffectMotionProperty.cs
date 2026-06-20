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

namespace LiveChartsCore.SkiaSharpView.Painting.Effects;

/// <summary>
/// A motion property that animates a <see cref="PathEffect"/>, interpolating between two effects
/// with <see cref="PathEffect.Transitionate(float, PathEffect)"/> — the same way
/// <see cref="PaintMotionProperty"/> animates whole paints. Assigning a new effect can soft-
/// transition; a self-animating effect (one carrying a looping <see cref="PathEffect.Animation"/>)
/// keeps the rail running indefinitely, so the paint never needs an "is animating" flag.
/// </summary>
/// <param name="defaultValue">The default effect.</param>
public class PathEffectMotionProperty(PathEffect? defaultValue = null)
    : MotionProperty<PathEffect?>(defaultValue)
{
    /// <inheritdoc cref="MotionProperty{T}.CanTransitionate"/>
    // A single assigned effect can animate against itself (it maps progress → its own shape,
    // e.g. a dash phase), so a non-null target alone is enough.
    protected override bool CanTransitionate => (FromValue ?? ToValue) is not null;

    /// <inheritdoc cref="MotionProperty{T}.OnGetMovement(float)"/>
    // Go through the assembly-internal static Transitionate (like ImageFilterMotionProperty) so the
    // null-endpoint → default-effect handling is consistent across both rails.
    protected override PathEffect? OnGetMovement(float progress) =>
        PathEffect.Transitionate(FromValue ?? ToValue, ToValue, progress);
}
