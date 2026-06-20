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

using Avalonia.Input;
using LiveChartsCore.Drawing;
using LiveChartsCore.Geo;
using LiveChartsCore.Measure;

namespace LiveChartsGeneratedCode;

// ==============================================================================
// Avalonia-specific code for SourceGenMapChart. Drawn-view scaffolding is
// inherited from SourceGenDrawnView; only the map's wheel-to-zoom and
// modifier-free pointer handlers live here.
// ==============================================================================

/// <inheritdoc cref="IGeoMapView" />
public partial class SourceGenMapChart : SourceGenDrawnView, IGeoMapView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SourceGenMapChart"/> class.
    /// </summary>
    public SourceGenMapChart()
    {
        InitializeChartControl();

        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;
        PointerExited += OnPointerExited;
        PointerWheelChanged += OnPointerWheelChanged;
    }

    /// <inheritdoc />
    protected override void OnDrawnViewSizeChanged() => CoreChart?.Update();

    /// <inheritdoc />
    protected override void OnDrawnViewLoaded() => CoreChart?.Load();

    /// <inheritdoc />
    protected override void OnDrawnViewUnloaded() => CoreChart?.Unload();

    /// <inheritdoc />
    protected override void OnDrawnViewReturnedToViewport() => CoreChart?.Update();

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var p = e.GetPosition(this);
        CoreChart?.InvokePointerDown(new LvcPoint((float)p.X, (float)p.Y), isSecondaryAction: false);
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var p = e.GetPosition(this);
        CoreChart?.InvokePointerMove(new LvcPoint((float)p.X, (float)p.Y));
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var p = e.GetPosition(this);
        CoreChart?.InvokePointerUp(new LvcPoint((float)p.X, (float)p.Y), isSecondaryAction: false);
    }

    private void OnPointerExited(object? sender, PointerEventArgs e) =>
        CoreChart?.InvokePointerLeft();

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        e.Handled = true;

        var p = e.GetPosition(this);
        CoreChart?.InvokePointerWheel(
            new LvcPoint((float)p.X, (float)p.Y),
            e.Delta.Y > 0 ? ZoomDirection.ZoomIn : ZoomDirection.ZoomOut);
    }
}
