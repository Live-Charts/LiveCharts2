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

using LiveChartsCore.Drawing;

namespace LiveChartsCore.Native.Events;

/// <summary>
/// Defines the pointer event args.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ScreenEventArgs"/> class.
/// </remarks>
/// <param name="location">The pointer location.</param>
/// <param name="isSecondaryPress">Indicates whether the action is secondary.</param>
/// <param name="originalEvent">The original event.</param>
public class PressedEventArgs(LvcPoint location, bool isSecondaryPress, object originalEvent) : ScreenEventArgs(location, originalEvent)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PressedEventArgs"/> class with
    /// an explicit <paramref name="isSyntheticRelease"/> flag.
    /// </summary>
    /// <param name="location">The pointer location.</param>
    /// <param name="isSecondaryPress">Indicates whether the action is secondary.</param>
    /// <param name="isSyntheticRelease">
    /// True when the release is not produced by a real user gesture but synthesized
    /// by the chart (e.g. when an ancestor steals pointer capture mid-drag and the
    /// chart needs to release its internal pan/drag state). Public release commands
    /// (PointerReleasedCommand) should ignore synthetic releases since the user has
    /// not actually lifted the pointer.
    /// </param>
    /// <param name="originalEvent">The original event.</param>
    public PressedEventArgs(LvcPoint location, bool isSecondaryPress, bool isSyntheticRelease, object originalEvent)
        : this(location, isSecondaryPress, originalEvent)
    {
        IsSyntheticRelease = isSyntheticRelease;
    }

    /// <summary>
    /// Gets a value indicating whether the action is a secondary press.
    /// </summary>
    public bool IsSecondaryPress { get; } = isSecondaryPress;

    /// <summary>
    /// Gets a value indicating whether this release was synthesized by the chart
    /// rather than produced by a real user gesture (e.g. raised on pointer capture
    /// loss so internal pan/drag state can be released). Consumers of public release
    /// hooks such as PointerReleasedCommand should skip synthetic releases.
    /// </summary>
    public bool IsSyntheticRelease { get; }
}
