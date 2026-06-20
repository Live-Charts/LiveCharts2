using System;
using System.Threading;
using System.Threading.Tasks;
using LiveChartsCore.Kernel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.CoreObjectsTests;

[TestClass]
public class ActionThrottlerTester
{
    [TestMethod]
    public async Task SameThread()
    {
        var runs = 0;

        Func<Task> action =
            () => Task.Run(() =>
            {
                runs++;
            });

        var throttler = new ActionThrottler(action, TimeSpan.FromMilliseconds(50));

        throttler.Call();
        throttler.Call();
        throttler.Call();
        throttler.Call();
        throttler.Call();
        throttler.Call();

        await Task.Delay(TimeSpan.FromSeconds(0.5));

        Assert.IsTrue(runs == 1 && throttler.Calls == 6);
    }

    [TestMethod]
    public async Task MultipleThreads()
    {
        var runs = 0;

        Func<Task> action =
            () => Task.Run(() =>
            {
                runs++;
            });

        var throttler = new ActionThrottler(action, TimeSpan.FromMilliseconds(50));

        new Thread(() => throttler.Call()).Start();
        new Thread(() => throttler.Call()).Start();
        new Thread(() => throttler.Call()).Start();

        await Task.WhenAll(
            Task.Factory.StartNew(() => throttler.Call()),
            Task.Factory.StartNew(() => throttler.Call()),
            Task.Factory.StartNew(() => throttler.Call()),
            Task.Run(() => throttler.Call()),
            Task.Run(() => throttler.Call()),
            Task.Run(() => throttler.Call()));

        await Task.Delay(TimeSpan.FromSeconds(0.5));

        var calls = throttler.Calls;
        Assert.IsTrue(runs == 1 && throttler.Calls == 9);
    }
}
