using System;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using LiveChartsCore.SkiaSharpView.Painting.ImageFilters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.CoreObjectsTests;

// SkiaPaint.PathEffect and SkiaPaint.ImageFilter are motion-backed: an effect/filter that
// carries a looping Animation animates on the rail (the paint reads the interpolated value each
// frame, stays invalid so the canvas keeps drawing, and loops past the end). Static effects
// (Animation == null) keep their previous create-once behavior — covered by the existing tests.
[TestClass]
public class AnimatedPaintEffectsTests
{
    // Guarantees the shared debug clock is restored even if an assertion throws, so a failure
    // can't leak driven time into other tests.
    [TestCleanup]
    public void ResetClock() => CoreMotionCanvas.DebugElapsedMilliseconds = -1;

    // A minimal self-animating path effect: Transitionate maps progress straight to a "phase"
    // so the test can read the interpolated value. No native SKPathEffect needed (we don't draw).
    private sealed class TestEffect : PathEffect
    {
        private static readonly object s_key = new();
        public float Phase { get; }
        public TestEffect(float phase, Animation? animation) : base(s_key)
        {
            Phase = phase;
            Animation = animation;
        }
        public override PathEffect Clone() => new TestEffect(Phase, Animation);
        public override void CreateEffect() { }
        public override PathEffect Transitionate(float progress, PathEffect? target) => new TestEffect(progress, Animation);
    }

    private sealed class TestFilter : ImageFilter
    {
        private static readonly object s_key = new();
        public float Radius { get; }
        public TestFilter(float radius, Animation? animation) : base(s_key)
        {
            Radius = radius;
            Animation = animation;
        }
        public override ImageFilter Clone() => new TestFilter(Radius, Animation);
        public override void CreateFilter() { }
        protected override ImageFilter Transitionate(float progress, ImageFilter target) => new TestFilter(progress, Animation);
    }

    [TestMethod]
    public void PathEffect_AnimatesOnTheRail_AndLoops()
    {
        var loop = new Animation(EasingFunctions.Lineal, TimeSpan.FromSeconds(1), int.MaxValue);

        CoreMotionCanvas.DebugElapsedMilliseconds = 0;
        var paint = new SolidColorPaint(SKColors.Blue) { PathEffect = new TestEffect(0, loop) };

        paint.IsValid = true;
        CoreMotionCanvas.DebugElapsedMilliseconds = 250;
        Assert.AreEqual(0.25f, ((TestEffect)paint.PathEffect!).Phase, 1e-4f, "should interpolate to 25% of the cycle");
        Assert.IsFalse(paint.IsValid, "an animating effect must keep the paint invalid (continuous frames)");

        paint.IsValid = true;
        CoreMotionCanvas.DebugElapsedMilliseconds = 750;
        Assert.AreEqual(0.75f, ((TestEffect)paint.PathEffect!).Phase, 1e-4f);

        // Overshoot a cycle boundary (real frames don't land exactly on it): must loop, not stop.
        paint.IsValid = true;
        CoreMotionCanvas.DebugElapsedMilliseconds = 1300;
        Assert.AreEqual(0.30f, ((TestEffect)paint.PathEffect!).Phase, 1e-3f, "must wrap to 30% of the next cycle");
        Assert.IsFalse(paint.IsValid, "an infinite animation never settles");

        CoreMotionCanvas.DebugElapsedMilliseconds = -1;
    }

    [TestMethod]
    public void ImageFilter_AnimatesOnTheRail_AndLoops()
    {
        var loop = new Animation(EasingFunctions.Lineal, TimeSpan.FromSeconds(1), int.MaxValue);

        CoreMotionCanvas.DebugElapsedMilliseconds = 0;
        var paint = new SolidColorPaint(SKColors.Red) { ImageFilter = new TestFilter(0, loop) };

        paint.IsValid = true;
        CoreMotionCanvas.DebugElapsedMilliseconds = 400;
        Assert.AreEqual(0.40f, ((TestFilter)paint.ImageFilter!).Radius, 1e-4f);
        Assert.IsFalse(paint.IsValid);

        paint.IsValid = true;
        CoreMotionCanvas.DebugElapsedMilliseconds = 1700; // 1.7 cycles → wraps to 70%
        Assert.AreEqual(0.70f, ((TestFilter)paint.ImageFilter!).Radius, 1e-3f);
        Assert.IsFalse(paint.IsValid);

        CoreMotionCanvas.DebugElapsedMilliseconds = -1;
    }

    [TestMethod]
    public void StaticEffect_DoesNotAnimateOrInvalidate()
    {
        // No Animation → the motion has nothing to run: the value is returned as-is and the paint
        // is left settled (the create-once behavior every existing effect relies on).
        CoreMotionCanvas.DebugElapsedMilliseconds = 0;
        var paint = new SolidColorPaint(SKColors.Green) { PathEffect = new TestEffect(0.5f, animation: null) };

        paint.IsValid = true;
        CoreMotionCanvas.DebugElapsedMilliseconds = 500;
        Assert.AreEqual(0.5f, ((TestEffect)paint.PathEffect!).Phase, 1e-4f, "a static effect returns its assigned value unchanged");
        Assert.IsTrue(paint.IsValid, "a static effect must not keep invalidating the canvas");

        CoreMotionCanvas.DebugElapsedMilliseconds = -1;
    }
}
