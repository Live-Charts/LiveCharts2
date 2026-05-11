using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using LiveChartsCore.Kernel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.OtherTests;

[TestClass]
public class ActionDebouncerTests
{
    [TestMethod]
    public async Task RapidCalls_DoNotRaiseFirstChanceTaskCanceledExceptions()
    {
        // regression for #2006: each wheel tick on a zoomable Cartesian chart
        // calls _zoommingDebouncer.Debounce(...). The previous implementation
        // awaited Task.Delay(_delay, token) and cancelled the token on the next
        // call, which threw a TaskCanceledException that the catch block hid
        // from app code -- but VS still pauses on each one as a first-chance
        // exception, flooding the Exceptions window.

        var debouncer = new ActionDebouncer(TimeSpan.FromMilliseconds(300));

        var firstChanceCancels = 0;
        void Handler(object? sender, FirstChanceExceptionEventArgs e)
        {
            if (e.Exception is TaskCanceledException) firstChanceCancels++;
        }

        AppDomain.CurrentDomain.FirstChanceException += Handler;
        try
        {
            // simulate ~10 wheel ticks within the debounce window
            for (var i = 0; i < 10; i++)
            {
                debouncer.Debounce(() => { });
                await Task.Delay(20);
            }

            // let the final scheduled action settle
            await Task.Delay(400);
        }
        finally
        {
            AppDomain.CurrentDomain.FirstChanceException -= Handler;
        }

        Assert.AreEqual(
            0,
            firstChanceCancels,
            $"ActionDebouncer raised {firstChanceCancels} first-chance " +
            "TaskCanceledException(s); the debugger pauses on each one.");
    }

    [TestMethod]
    public async Task OnlyLastActionRuns_AfterRapidCalls()
    {
        // standard debouncer contract: only the final scheduled action should
        // fire once the input quiets down.
        var debouncer = new ActionDebouncer(TimeSpan.FromMilliseconds(100));

        var ranIndex = -1;
        for (var i = 0; i < 5; i++)
        {
            var captured = i;
            debouncer.Debounce(() => ranIndex = captured);
            await Task.Delay(20);
        }

        await Task.Delay(300);

        Assert.AreEqual(4, ranIndex,
            "only the last debounced action should run after the quiet period.");
    }
}
