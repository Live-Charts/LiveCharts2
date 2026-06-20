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

using LiveChartsCore.Geo;
using LiveChartsCore.Measure;

namespace LiveChartsGeneratedCode;

// ==============================================================================
// MAUI-specific code for SourceGenMapChart. Drawn-view scaffolding (ChartView
// base, MotionCanvas hosting, Loaded/Unloaded wiring with Apple handler-disconnect
// for #1725, CoreCanvas/ControlSize/IsDarkMode/InvokeOnUIThread) inherited from
// SourceGenDrawnView. Only the wheel-to-zoom and modifier-free pointer handlers
// live here.
// ==============================================================================

/// <inheritdoc cref="IGeoMapView"/>
public abstract partial class SourceGenMapChart : SourceGenDrawnView, IGeoMapView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SourceGenMapChart"/> class.
    /// </summary>
    protected SourceGenMapChart()
    {
        InitializeChartControl();
    }

    /// <inheritdoc />
    protected override void OnDrawnViewSizeChanged() => CoreChart?.Update();

    /// <inheritdoc />
    protected override void OnDrawnViewLoaded() => CoreChart?.Load();

    /// <inheritdoc />
    protected override void OnDrawnViewUnloaded() => CoreChart?.Unload();

    internal override void OnPressed(object? sender, LiveChartsCore.Native.Events.PressedEventArgs args) =>
        CoreChart?.InvokePointerDown(args.Location, isSecondaryAction: false);

    internal override void OnMoved(object? sender, LiveChartsCore.Native.Events.ScreenEventArgs args) =>
        CoreChart?.InvokePointerMove(args.Location);

    internal override void OnReleased(object? sender, LiveChartsCore.Native.Events.PressedEventArgs args) =>
        CoreChart?.InvokePointerUp(args.Location, isSecondaryAction: false);

    internal override void OnExited(object? sender, LiveChartsCore.Native.Events.EventArgs args) =>
        CoreChart?.InvokePointerLeft();

    internal override void OnScrolled(object? sender, LiveChartsCore.Native.Events.ScrollEventArgs args) =>
        CoreChart?.InvokePointerWheel(
            args.Location,
            args.ScrollDelta > 0 ? ZoomDirection.ZoomIn : ZoomDirection.ZoomOut);
}
