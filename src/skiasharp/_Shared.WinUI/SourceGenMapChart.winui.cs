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
using LiveChartsCore.Geo;
using LiveChartsCore.Measure;
using Microsoft.UI.Xaml.Input;

namespace LiveChartsGeneratedCode;

// ==============================================================================
// WinUI / Uno-WinUI specific code for SourceGenMapChart. Drawn-view scaffolding
// (MotionCanvas hosting, lifecycle, CoreCanvas / ControlSize / theme awareness /
// DispatcherQueue dispatch) inherited from SourceGenDrawnView. Only the
// wheel-to-zoom and modifier-free pointer handlers live here.
// ==============================================================================

/// <inheritdoc cref="IGeoMapView"/>
public abstract partial class SourceGenMapChart : SourceGenDrawnView, IGeoMapView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SourceGenMapChart"/> class.
    /// </summary>
    public SourceGenMapChart()
    {
        InitializeChartControl();

        PointerWheelChanged += OnPointerWheelChanged;
        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;
        PointerExited += OnPointerExited;
    }

    /// <inheritdoc />
    protected override void OnDrawnViewSizeChanged() => CoreChart?.Update();

    /// <inheritdoc />
    protected override void OnDrawnViewLoaded() => CoreChart?.Load();

    /// <inheritdoc />
    protected override void OnDrawnViewUnloaded() => CoreChart?.Unload();

    private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        var pp = e.GetCurrentPoint(this);
        var p = pp.Position;
        var delta = pp.Properties.MouseWheelDelta;
        CoreChart?.InvokePointerWheel(
            new LvcPoint((float)p.X, (float)p.Y),
            delta > 0 ? ZoomDirection.ZoomIn : ZoomDirection.ZoomOut);
        e.Handled = true;
    }

    private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var p = e.GetCurrentPoint(this).Position;
        CoreChart?.InvokePointerDown(new LvcPoint((float)p.X, (float)p.Y), isSecondaryAction: false);
    }

    private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        var p = e.GetCurrentPoint(this).Position;
        CoreChart?.InvokePointerMove(new LvcPoint((float)p.X, (float)p.Y));
    }

    private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        var p = e.GetCurrentPoint(this).Position;
        CoreChart?.InvokePointerUp(new LvcPoint((float)p.X, (float)p.Y), isSecondaryAction: false);
    }

    private void OnPointerExited(object sender, PointerRoutedEventArgs e) =>
        CoreChart?.InvokePointerLeft();
}
