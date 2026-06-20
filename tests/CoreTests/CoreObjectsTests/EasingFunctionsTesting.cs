using System;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.CoreObjectsTests;

[TestClass]
public class EasingFunctionsTesting
{
    [TestMethod]
    public void LinealIsIdentity()
    {
        var f = EasingFunctions.Lineal;
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(0.5f) - 0.5f) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void QuadraticInBoundary()
    {
        var f = EasingFunctions.QuadraticIn;
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
        Assert.IsTrue(Math.Abs(f(0.5f) - 0.25f) < 0.001f);
    }

    [TestMethod]
    public void QuadraticOutBoundary()
    {
        var f = EasingFunctions.QuadraticOut;
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void QuadraticInOutBoundary()
    {
        var f = EasingFunctions.QuadraticInOut;
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void CubicInBoundary()
    {
        var f = EasingFunctions.CubicIn;
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void CubicOutBoundary()
    {
        var f = EasingFunctions.CubicOut;
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void CubicInOutBoundary()
    {
        var f = EasingFunctions.CubicInOut;
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void CircleInBoundary()
    {
        var f = EasingFunctions.CircleIn;
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void CircleOutBoundary()
    {
        var f = EasingFunctions.CircleOut;
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void CircleInOutBoundary()
    {
        var f = EasingFunctions.CircleInOut;
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void ExponentialInBoundary()
    {
        var f = EasingFunctions.ExponentialIn;
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.01f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void ExponentialOutBoundary()
    {
        var f = EasingFunctions.ExponentialOut;
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.01f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void ExponentialInOutBoundary()
    {
        var f = EasingFunctions.ExponentialInOut;
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.01f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void SinInBoundary()
    {
        var f = EasingFunctions.SinIn;
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void SinOutBoundary()
    {
        var f = EasingFunctions.SinOut;
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void SinInOutBoundary()
    {
        var f = EasingFunctions.SinInOut;
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void BackInBoundary()
    {
        var f = EasingFunctions.BackIn;
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void BackOutBoundary()
    {
        var f = EasingFunctions.BackOut;
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void BackInOutBoundary()
    {
        var f = EasingFunctions.BackInOut;
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void BounceInBoundary()
    {
        var f = EasingFunctions.BounceIn;
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void BounceOutBoundary()
    {
        var f = EasingFunctions.BounceOut;
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void BounceInOutBoundary()
    {
        var f = EasingFunctions.BounceInOut;
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void ElasticInBoundary()
    {
        var f = EasingFunctions.ElasticIn;
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void ElasticOutBoundary()
    {
        var f = EasingFunctions.ElasticOut;
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void ElasticInOutBoundary()
    {
        var f = EasingFunctions.ElasticInOut;
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void PolinominalInBoundary()
    {
        var f = EasingFunctions.PolinominalIn;
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void PolinominalOutBoundary()
    {
        var f = EasingFunctions.PolinominalOut;
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void PolinominalInOutBoundary()
    {
        var f = EasingFunctions.PolinominalInOut;
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void EaseBoundary()
    {
        var f = EasingFunctions.Ease;
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void EaseInBoundary()
    {
        var f = EasingFunctions.EaseIn;
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void EaseOutBoundary()
    {
        var f = EasingFunctions.EaseOut;
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void EaseInOutBoundary()
    {
        var f = EasingFunctions.EaseInOut;
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void CustomBackInWithOvershoot()
    {
        var f = EasingFunctions.BuildCustomBackIn(1.70158f);
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void CustomBackOutWithOvershoot()
    {
        var f = EasingFunctions.BuildCustomBackOut(1.70158f);
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void CustomBackInOutWithOvershoot()
    {
        var f = EasingFunctions.BuildCustomBackInOut(1.70158f);
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void CustomElasticInWithAmplitudeAndPeriod()
    {
        var f = EasingFunctions.BuildCustomElasticIn(1, 0.3f);
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void CustomElasticOutWithAmplitudeAndPeriod()
    {
        var f = EasingFunctions.BuildCustomElasticOut(1, 0.3f);
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void CustomElasticInOutWithAmplitudeAndPeriod()
    {
        var f = EasingFunctions.BuildCustomElasticInOut(1, 0.3f);
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void CustomPolinominalInWithExponent()
    {
        var f = EasingFunctions.BuildCustomPolinominalIn(4);
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void CustomPolinominalOutWithExponent()
    {
        var f = EasingFunctions.BuildCustomPolinominalOut(4);
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void CustomPolinominalInOutWithExponent()
    {
        var f = EasingFunctions.BuildCustomPolinominalInOut(4);
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void BuildCubicBezierBoundary()
    {
        var f = EasingFunctions.BuildCubicBezier(0.25f, 0.1f, 0.25f, 1f);
        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.001f);
        Assert.IsTrue(Math.Abs(f(1) - 1) < 0.001f);
    }

    [TestMethod]
    public void BuildFunctionUsingKeyFrames()
    {
        var keyFrames = new KeyFrame[]
        {
            new() { Time = 0, Value = 0, EasingFunction = EasingFunctions.Lineal },
            new() { Time = 1, Value = 100, EasingFunction = EasingFunctions.Lineal }
        };
        var f = EasingFunctions.BuildFunctionUsingKeyFrames(keyFrames);

        Assert.IsTrue(Math.Abs(f(0) - 0) < 0.1f);
        Assert.IsTrue(Math.Abs(f(1) - 100) < 0.1f);
        Assert.IsTrue(Math.Abs(f(0.5f) - 50) < 0.1f);
    }

    [TestMethod]
    public void BuildFunctionUsingKeyFramesThrowsWithLessThanTwo()
    {
        var keyFrames = new KeyFrame[]
        {
            new() { Time = 0, Value = 0, EasingFunction = EasingFunctions.Lineal }
        };
        Assert.ThrowsExactly<Exception>(() => EasingFunctions.BuildFunctionUsingKeyFrames(keyFrames));
    }

    [TestMethod]
    public void BuildFunctionUsingKeyFramesAddsLastFrameIfNotAtOne()
    {
        var keyFrames = new KeyFrame[]
        {
            new() { Time = 0, Value = 0, EasingFunction = EasingFunctions.Lineal },
            new() { Time = 0.5f, Value = 50, EasingFunction = EasingFunctions.Lineal }
        };
        var f = EasingFunctions.BuildFunctionUsingKeyFrames(keyFrames);

        // Should extend the value to t=1
        Assert.IsTrue(Math.Abs(f(1) - 50) < 0.1f);
    }

    [TestMethod]
    public void EasingFunctionsAreMonotonicForMostFunctions()
    {
        // Lineal, SinOut, and CubicOut should be monotonically increasing
        var monotonic = new[]
        {
            ("Lineal", EasingFunctions.Lineal),
            ("SinOut", EasingFunctions.SinOut),
            ("CubicOut", EasingFunctions.CubicOut),
            ("CircleOut", EasingFunctions.CircleOut)
        };

        foreach (var (name, f) in monotonic)
        {
            var prev = f(0);
            for (var t = 0.01f; t <= 1.0f; t += 0.01f)
            {
                var current = f(t);
                Assert.IsTrue(current >= prev - 0.001f, $"{name} is not monotonic at t={t}");
                prev = current;
            }
        }
    }
}
