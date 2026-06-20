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

using System.Windows.Input;
using LiveChartsCore.Drawing;
using LiveChartsCore.Geo;
using LiveChartsCore.Measure;

namespace LiveChartsGeneratedCode;

// ==============================================================================
// WPF-specific code for SourceGenMapChart. Drawn-view scaffolding (MotionCanvas
// hosting, size / load / unload wiring, CoreCanvas / ControlSize / DesignerMode
// etc) is inherited from SourceGenDrawnView; only the map's wheel-to-zoom and
// modifier-free pointer handlers live here.
// ==============================================================================

/// <inheritdoc cref="IGeoMapView" />
public abstract partial class SourceGenMapChart : SourceGenDrawnView, IGeoMapView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SourceGenMapChart"/> class.
    /// </summary>
    protected SourceGenMapChart()
    {
        InitializeChartControl();

        MouseWheel += OnMouseWheel;
        MouseDown += OnMouseDown;
        MouseMove += OnMouseMove;
        MouseUp += OnMouseUp;
        MouseLeave += OnMouseLeave;
    }

    /// <inheritdoc />
    protected override void OnDrawnViewSizeChanged() => CoreChart?.Update();

    /// <inheritdoc />
    protected override void OnDrawnViewLoaded() => CoreChart?.Load();

    /// <inheritdoc />
    protected override void OnDrawnViewUnloaded() => CoreChart?.Unload();

    private void OnMouseWheel(object? sender, MouseWheelEventArgs e)
    {
        var p = e.GetPosition(this);
        CoreChart?.InvokePointerWheel(
            new LvcPoint((float)p.X, (float)p.Y),
            e.Delta > 0 ? ZoomDirection.ZoomIn : ZoomDirection.ZoomOut);
    }

    private void OnMouseDown(object? sender, MouseButtonEventArgs e)
    {
        var p = e.GetPosition(this);
        CoreChart?.InvokePointerDown(new LvcPoint((float)p.X, (float)p.Y), isSecondaryAction: false);
    }

    private void OnMouseMove(object? sender, MouseEventArgs e)
    {
        var p = e.GetPosition(this);
        CoreChart?.InvokePointerMove(new LvcPoint((float)p.X, (float)p.Y));
    }

    private void OnMouseUp(object? sender, MouseButtonEventArgs e)
    {
        var p = e.GetPosition(this);
        CoreChart?.InvokePointerUp(new LvcPoint((float)p.X, (float)p.Y), isSecondaryAction: false);
    }

    private void OnMouseLeave(object? sender, MouseEventArgs e) =>
        CoreChart?.InvokePointerLeft();
}
