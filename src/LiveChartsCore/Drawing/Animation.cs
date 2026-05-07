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

namespace LiveChartsCore.Drawing;

/// <summary>
/// Defines an animation.
/// </summary>
public class Animation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Animation"/> class.
    /// </summary>
    public Animation()
    {
        EasingFunction = t => t;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Animation"/> class.
    /// </summary>
    /// <param name="easingFunction">The easing function.</param>
    /// <param name="duration">The duration.</param>
    public Animation(Func<float, float>? easingFunction, TimeSpan duration)
    {
        EasingFunction = easingFunction;
        Duration = (long)duration.TotalMilliseconds;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Animation"/> class.
    /// </summary>
    /// <param name="easingFunction">The easing function.</param>
    /// <param name="duration">The duration.</param>
    /// <param name="repeatTimes">The repeat times.</param>
    public Animation(Func<float, float> easingFunction, TimeSpan duration, int repeatTimes)
    {
        EasingFunction = easingFunction;
        Duration = (long)duration.TotalMilliseconds;
        RepeatTimes = repeatTimes;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Animation"/> class.
    /// </summary>
    /// <param name="easingFunction">The easing function.</param>
    /// <param name="duration">The duration.</param>
    /// <param name="repeatTimes">The repeat times.</param>
    public Animation(Func<float, float> easingFunction, long duration, int repeatTimes)
    {
        EasingFunction = easingFunction;
        Duration = duration;
        RepeatTimes = repeatTimes;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Animation"/> class.
    /// </summary>
    /// <param name="animation">The animation.</param>
    public Animation(Animation animation)
    {
        EasingFunction = animation.EasingFunction;
        Duration = animation.Duration;
        RepeatTimes = animation.RepeatTimes;
    }

    /// <summary>
    /// Gets or sets the easing function.
    /// </summary>
    public Func<float, float>? EasingFunction { get; set; }

    /// <summary>
    /// Gets or sets the duration of the transition in Milliseconds.
    /// </summary>
    public long Duration { get; set; }

    /// <summary>
    /// Gets or sets how many times the animation needs to repeat before it is completed,
    /// use int.MaxValue to repeat it indefinitely number of times.
    /// </summary>
    public int RepeatTimes { get; set; }

    internal int RepeatCount { get; set; } = 0;
}
