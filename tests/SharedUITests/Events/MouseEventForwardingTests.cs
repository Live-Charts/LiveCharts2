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
using SharedUITests.Helpers;
using Xunit;

namespace SharedUITests.Events;

public class MouseEventForwardingTests
{
    public AppController App => AppController.Current;

#if WINFORMS_UI_TESTING
    // regression for https://github.com/Live-Charts/LiveCharts2/issues/1209
    // mouse messages reach the inner SKControl, not the outer chart, so the
    // chart must explicitly re-raise them. before the fix, MouseClick and
    // MouseDoubleClick subscribers on the outer chart never fired.
    [AppTestMethod]
    public async Task Chart_should_re_raise_inner_mouse_events()
    {
        var sut = await App.NavigateTo<Samples.General.FirstChart.View>();
        await sut.Chart.WaitUntilChartRenders();

        var control = (System.Windows.Forms.Control)sut.Chart;
        var fired = new HashSet<string>();

        control.MouseDown += (_, _) => fired.Add("Down");
        control.MouseMove += (_, _) => fired.Add("Move");
        control.MouseUp += (_, _) => fired.Add("Up");
        control.MouseClick += (_, _) => fired.Add("Click");
        control.MouseDoubleClick += (_, _) => fired.Add("DoubleClick");

        await sut.Chart.RaiseInput(InputKind.Down, 50, 50);
        await sut.Chart.RaiseInput(InputKind.Move, 50, 51);
        await sut.Chart.RaiseInput(InputKind.Up, 50, 51);
        await sut.Chart.RaiseInput(InputKind.Click, 50, 51);
        await sut.Chart.RaiseInput(InputKind.DoubleClick, 50, 51);

        Assert.Contains("Down", fired);
        Assert.Contains("Move", fired);
        Assert.Contains("Up", fired);
        Assert.Contains("Click", fired);
        Assert.Contains("DoubleClick", fired);
    }
#endif
}
