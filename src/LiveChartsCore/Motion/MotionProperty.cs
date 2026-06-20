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

namespace LiveChartsCore.Motion;

/// <summary>
/// The <see cref="MotionProperty{T}"/> object tracks where a property of a <see cref="Animatable"/> is in a time line.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="MotionProperty{T}"/> class.
/// </remarks>
/// <param name="defaultValue">The default value.</param>
public abstract class MotionProperty<T>(T defaultValue) : IMotionProperty
{
    private float _startTime;
    private float _endTime;

    /// <summary>
    /// Gets the value where the transition began.
    /// </summary>
    public T FromValue { get; protected set; } = defaultValue;

    /// <summary>
    /// Gets the value where the transition finishes.
    /// </summary>
    public T ToValue { get; protected set; } = defaultValue;

    /// <inheritdoc cref="IMotionProperty.IsCompleted"/>
    public bool IsCompleted { get; private set; } = true;

    /// <summary>
    /// Gets the default value of the property.
    /// </summary>
    public T DefaultValue { get; } = defaultValue;

    object? IMotionProperty.FromValue => FromValue;

    object? IMotionProperty.ToValue => ToValue;

    /// <inheritdoc cref="IMotionProperty.Animation"/>
    public Animation? Animation { get; set; }

#if DEBUG
    /// <inheritdoc cref="IMotionProperty.LogGet"/>
    public Animatable? LogGet { get; set; }

    /// <inheritdoc cref="IMotionProperty.LogSet"/>
    public Animatable? LogSet { get; set; }
#endif

    /// <summary>
    /// Indicates whether the property transition can complete, this depends
    /// on <see cref="FromValue"/> and <see cref="ToValue"/> being null or any other
    /// state where the transition is not possible.
    /// </summary>
    protected abstract bool CanTransitionate { get; }

    /// <summary>
    /// Moves to the specified value.
    /// </summary>
    /// <param name="value">The value to move to.</param>
    /// <param name="animatable">The <see cref="Animatable"/> instance that is moving.</param>
    public void SetMovement(T value, Animatable animatable)
    {
#if DEBUG
        if (LogSet == animatable)
        {
            System.Diagnostics.Debug.WriteLine($"[MOTION SET]:");
        }
#endif

        FromValue = GetMovement(animatable);
        ToValue = value;

        SetTimeLine();

        animatable.IsValid = false;

#if DEBUG
        if (LogSet == animatable)
        {
            System.Diagnostics.Debug.WriteLine($"    {FromValue:N2}   =>   {ToValue:N2}");
        }
#endif
    }

    /// <summary>
    /// Gets the current movement in the <see cref="Animation"/>.
    /// </summary>
    /// <param name="animatable"></param>
    /// <returns></returns>
    public T GetMovement(Animatable animatable)
    {
        if (Animation is null || Animation.EasingFunction is null || !CanTransitionate)
        {
#if DEBUG
            if (LogGet == animatable)
                System.Diagnostics.Debug.WriteLine($"[MOTION GET] invalid transition state.");
#endif
            return ToValue;
        }

        var globalTime = CoreMotionCanvas.ElapsedMilliseconds;

        var deltaTime = _endTime - _startTime;
        var p = (globalTime - _startTime) / deltaTime;

        // A repeating animation usually OVERSHOOTS its end (real frames rarely land exactly on a
        // cycle boundary, so p jumps past 1). Advance the window by the whole elapsed cycles so the
        // loop stays seamless and phase-continuous, instead of completing. RepeatTimes defaults to 0,
        // so one-shot animations skip this; the exact p == 1 boundary is still handled below unchanged.
        if (deltaTime > 0 && p > 1 &&
            (Animation.RepeatTimes == int.MaxValue || Animation.RepeatTimes > Animation.RepeatCount))
        {
            var cycles = (int)((globalTime - _startTime) / deltaTime); // floor (elapsed is positive)

            if (Animation.RepeatTimes != int.MaxValue)
            {
                // Never fast-forward past the remaining repeats: a long frame stall must still
                // complete on time, not run forever. (Infinite repeats skip the counter to avoid
                // RepeatCount overflowing over a long-running animation.)
                var remaining = Animation.RepeatTimes - Animation.RepeatCount;
                if (cycles > remaining) cycles = remaining;
                Animation.RepeatCount += cycles;
            }

            _startTime += cycles * deltaTime;
            _endTime = _startTime + Animation.Duration;
            deltaTime = _endTime - _startTime;
            p = (globalTime - _startTime) / deltaTime; // > 1 if it should complete (cycles was clamped)
        }

        if (deltaTime <= 0 || p > 1)
        {
            // when deltaTime is <= 0:
            //  1. the animation duration is 0.
            //  2. the Finish method was called, it sets the _endTime to 0.

            // when p >= 1:
            //  1. the animation is completed.

            // when animation.ForceFinish is true:
            //  1. the animation was forced to finish

            // in both cases we return the ToValue, and return
            // before invalidating the animatable.

#if DEBUG
            if (LogGet == animatable)
                System.Diagnostics.Debug.WriteLine(
                    $"[MOTION GET] complete state, progress: {p:P2}." +
                    $"{(deltaTime <= 0 ? $" invalid delta {deltaTime:N2}" : string.Empty)}");
#endif

            if (!IsCompleted)
            {
                IsCompleted = true;
                OnCompleted();
            }

            return ToValue;
        }

        // at this points we are sure that the animatable has not finished at least with this property.
        // setting the IsValid to false will make the animatable draw again.
        animatable.IsValid = false;

        if (p >= 1)
        {
            // at this point the animation is completed
            p = 1;

            var requiresRepeat =
                Animation.RepeatTimes == int.MaxValue ||
                Animation.RepeatTimes >= ++Animation.RepeatCount;

            if (requiresRepeat)
            {
                _startTime = globalTime;
                _endTime = globalTime + Animation.Duration;
            }
        }

#if DEBUG
        if (LogGet == animatable)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[MOTION GET]    {FromValue:N2}   =>   {ToValue:N2}   @   {OnGetMovement(Animation.EasingFunction(p)):N2} [{p:p2}]");
        }
#endif

        return OnGetMovement(Animation.EasingFunction(p));
    }

    /// <summary>
    /// Called to get the movement at a specific progress.
    /// </summary>
    /// <param name="progress">The progress.</param>
    /// <returns></returns>
    protected abstract T OnGetMovement(float progress);

    /// <summary>
    /// Called when the animation is completed.
    /// </summary>
    protected virtual void OnCompleted() { }

    /// <inheritdoc cref="IMotionProperty.Finish"/>
    public void Finish() => _endTime = 0;

    /// <inheritdoc cref="IMotionProperty.CopyFrom(IMotionProperty)"/>
    public void CopyFrom(IMotionProperty source)
    {
        var typedSource = (MotionProperty<T>)source;

        FromValue = typedSource.FromValue;
        ToValue = typedSource.ToValue;
        _startTime = typedSource._startTime;
        _endTime = typedSource._endTime;
        Animation = typedSource.Animation;
    }

    private void SetTimeLine()
    {
        if (Animation is null) return;

        var globalTime = CoreMotionCanvas.ElapsedMilliseconds;

        IsCompleted = false;
        _startTime = globalTime;
        _endTime = globalTime + Animation.Duration;

        Animation.RepeatCount = 0;
    }
}
