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

using Factos;
using LiveChartsCore.Kernel.Sketches;
using SharedUITests.Helpers;
using Xunit;

#if WINUI_UI_TESTING || UNO_UI_TESTING
using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
#endif

namespace SharedUITests;

public class ThemeTests
{
    public AppController App => AppController.Current;

#if WINUI_UI_TESTING || UNO_UI_TESTING
    // regression for https://github.com/Live-Charts/LiveCharts2/issues/2004
    //
    // The chart used to read Application.Current.RequestedTheme, which only
    // tracks the App.xaml-time setting and ignores element-level overrides
    // (FrameworkElement.RequestedTheme on a Window, page, ancestor, or the
    // chart itself — the standard WinUI 3 idiom for theme switching). The
    // fix routes IsDarkMode through this.ActualTheme so any link in the
    // resolution chain triggers the right palette. This canary sets
    // RequestedTheme on the chart directly (the most pessimistic case —
    // there is no Application.RequestedTheme=Dark to fall back on) and
    // asserts IsDarkMode flips with it.
    [AppTestMethod]
    public async Task WinUI_isDarkMode_honors_element_RequestedTheme()
    {
        var sut = await App.NavigateTo<Samples.General.FirstChart.View>();
        await sut.Chart.WaitUntilChartRenders();

        var chart = (FrameworkElement)sut.Chart;
        var view = (IChartView)sut.Chart;

        await RunOnDispatcherAsync(chart, () =>
        {
            chart.RequestedTheme = ElementTheme.Dark;
            Assert.True(
                view.IsDarkMode,
                "IsDarkMode must follow the chart's resolved ActualTheme; setting RequestedTheme=Dark on the chart (or any ancestor FrameworkElement) is the standard WinUI 3 way to opt a subtree into dark mode and Application.RequestedTheme — the property the chart used to read — does not change in response.");

            chart.RequestedTheme = ElementTheme.Light;
            Assert.False(
                view.IsDarkMode,
                "Flipping RequestedTheme back to Light must flip IsDarkMode back to false; otherwise the chart would stay locked on the first theme it observed and not respond to runtime theme toggles.");
        });
    }

    private static Task RunOnDispatcherAsync(FrameworkElement chart, Action work)
    {
        var tcs = new TaskCompletionSource<object?>();
        if (!chart.DispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                work();
                tcs.SetResult(null);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        }))
        {
            tcs.SetException(new InvalidOperationException("Failed to enqueue work on the chart's DispatcherQueue."));
        }
        return tcs.Task;
    }
#endif
}
