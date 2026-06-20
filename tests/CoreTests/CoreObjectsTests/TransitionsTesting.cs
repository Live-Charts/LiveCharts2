using System;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.CoreObjectsTests;

[TestClass]
public class TransitionsTesting
{
    [TestMethod]
    public void InfiniteAnimation()
    {
        var r = new RectangleGeometry();
        var a = (Animatable)r;
        var duration = TimeSpan.FromSeconds(1);
        var easing = EasingFunctions.Lineal;

        r.SetTransition(
            new Animation(easing, duration, int.MaxValue),
            DrawnGeometry.YProperty, DrawnGeometry.XProperty,
            BoundedDrawnGeometry.WidthProperty, BoundedDrawnGeometry.HeightProperty);

        void DrawFrame(long time)
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = time;
            a.IsValid = true;
        }

        float DoTransition(float from, float to, float start, float end, long time, Func<float, float> easing)
        {
            var p = (time - start) / (end - start);
            if (p > 1) p = 1;
            return from + easing(p) * (to - from);
        }

        var time = 50;
        DrawFrame(time);

        // transition from 50 to 100 for each property
        r.Y = 50;
        r.X = 50;
        r.Width = 50;
        r.Height = 50;
        r.CompleteTransition(
            DrawnGeometry.YProperty, DrawnGeometry.XProperty,
            BoundedDrawnGeometry.WidthProperty, BoundedDrawnGeometry.HeightProperty);

        r.Y = 100;
        r.X = 100;
        r.Width = 100;
        r.Height = 100;

        CoreMotionCanvas.DebugElapsedMilliseconds = time;
        var startTime = 50;
        time = startTime;

        while (time <= duration.TotalMilliseconds * 2 + startTime)
        {
            DrawFrame(time);

            var x = r.X;
            var y = r.Y;
            var w = r.Width;
            var h = r.Height;
            var l = Math.Truncate((time - startTime) / duration.TotalMilliseconds);
            if ((time - startTime) % duration.TotalMilliseconds == 0 && time != startTime) l--;
            var laps = (long)(l * duration.TotalMilliseconds);
            Assert.IsTrue(x == DoTransition(50, 100, 50, 1050, time - laps, easing));
            Assert.IsTrue(y == DoTransition(50, 100, 50, 1050, time - laps, easing));
            Assert.IsTrue(w == DoTransition(50, 100, 50, 1050, time - laps, easing));
            Assert.IsTrue(h == DoTransition(50, 100, 50, 1050, time - laps, easing));

            Assert.IsTrue(!a.IsValid);
            time += 500;
        }

        // not completed yet because the duration of the animation in this case is infinite
        Assert.IsTrue(!a.IsValid);

        // clean the debug time
        CoreMotionCanvas.DebugElapsedMilliseconds = -1;
    }

    [TestMethod]
    public void AnimationComplete()
    {
        var r = new RectangleGeometry();
        var a = (Animatable)r;
        var duration = TimeSpan.FromSeconds(1);
        var easing = EasingFunctions.Lineal;

        r.SetTransition(
            new Animation(easing, duration),
            DrawnGeometry.YProperty, DrawnGeometry.XProperty,
            BoundedDrawnGeometry.WidthProperty, BoundedDrawnGeometry.HeightProperty);

        void DrawFrame(long time)
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = time;
            a.IsValid = true;

            // Calling the property getter moves the transition with the current animatable time
            var x = r.X;
            var y = r.Y;
            var w = r.Width;
            var h = r.Height;
        }

        var time = 0;
        DrawFrame(time);

        // transition from 50 to 100 for each property
        r.Y = 0;
        r.X = 0;
        r.Width = 0;
        r.Height = 0;
        r.CompleteTransition(
            DrawnGeometry.YProperty, DrawnGeometry.XProperty,
            BoundedDrawnGeometry.WidthProperty, BoundedDrawnGeometry.HeightProperty);
        DrawFrame(time);

        Assert.IsTrue(a.IsValid);

        r.Y = 100;
        DrawFrame(time);
        var p = r.GetPropertyDefinition(nameof(r.Y)).GetMotion(r);

        time += 500;
        DrawFrame(time);
        Assert.IsTrue(!p.IsCompleted);

        time += 500;
        DrawFrame(time);
        Assert.IsTrue(!p.IsCompleted);

        time += 500;
        DrawFrame(time);
        Assert.IsTrue(p.IsCompleted);

        // clean the debug time
        CoreMotionCanvas.DebugElapsedMilliseconds = -1;
    }

    [TestMethod]
    public void KeyFramesTest()
    {
        var f = EasingFunctions.BuildFunctionUsingKeyFrames(
            [
                new KeyFrame { Time = 0, Value = 0, EasingFunction = EasingFunctions.Lineal },
                new KeyFrame { Time = 0.5f, Value = 1, EasingFunction = EasingFunctions.Lineal },
                new KeyFrame { Time = 1, Value = 0, EasingFunction = EasingFunctions.Lineal },
            ]);

        var r = new[] { 0, 0.2f, 0.4f, 0.6f, 0.8f, 1, 0.8f, 0.6f, 0.4f, 0.2f, 0 };
        var i = 0;

        for (float t = 0; t <= 1; t += 0.1f)
        {
            var p = f(t);
            Assert.IsTrue(Math.Abs(p - r[i++]) < 0.000001);
        }

        f = EasingFunctions.BuildFunctionUsingKeyFrames(
            [
                new KeyFrame { Time = 0, Value = 0, EasingFunction = EasingFunctions.Lineal },
                new KeyFrame { Time = 0.30f, Value = 0, EasingFunction = EasingFunctions.Lineal },
                new KeyFrame { Time = 0.80f, Value = 1, EasingFunction = EasingFunctions.Lineal },
                new KeyFrame { Time = 1, Value = 1, EasingFunction = EasingFunctions.Lineal },
            ]);

        r = [0, 0, 0, 0, 0.2f, 0.4f, 0.6f, 0.8f, 1, 1];
        i = 0;

        for (float t = 0; t <= 1; t += 0.1f)
        {
            var p = f(t);
            Assert.IsTrue(Math.Abs(p - r[i++]) < 0.000001);
        }
    }

    [TestMethod]
    public void NotFinishedAnimation()
    {
        var r = new RectangleGeometry();
        var a = (Animatable)r;
        var duration = TimeSpan.FromMilliseconds(100);
        var easing = EasingFunctions.Lineal;

        r.SetTransition(
            new Animation(easing, duration, int.MaxValue), DrawnGeometry.XProperty);

        void DrawFrame(long time)
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = time;
            a.IsValid = true;
        }

        var time = 0;
        DrawFrame(time);

        r.X = 0;
        r.CompleteTransition(DrawnGeometry.XProperty);

        r.X = 100;

        CoreMotionCanvas.DebugElapsedMilliseconds = time;
        time = 0;

        var start = 0d;
        var end = 100d;

        var sv = r.X;
        var dv = 100 - sv;

        while (time <= duration.TotalMilliseconds)
        {
            DrawFrame(time);

            if (time == 80)
            {
                sv = r.X;
                dv = 200 - sv;

                r.X = 200;
                start = time;
                end = time + 100;
            }

            var x = r.X;

            var p = (time - start) / (end - start);
            Assert.IsTrue(Math.Abs(x - (sv + p * dv)) < 0.001);

            time += 10;
        }

        // clean the debug time
        CoreMotionCanvas.DebugElapsedMilliseconds = -1;
    }
}
