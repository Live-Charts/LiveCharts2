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
using LiveChartsCore.SkiaSharpView.Blazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace LiveChartsGeneratedCode;

// ==============================================================================
// Blazor-specific code for SourceGenMapChart. Drawn-view scaffolding inherited
// from SourceGenDrawnView (MotionCanvas ref capture, lifecycle, CoreCanvas etc).
// Only BuildRenderTree (with map-specific event wiring) and the wheel-to-zoom +
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
        CoreChart = null!;
    }

    /// <summary>
    /// Builds the render tree.
    /// </summary>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<MotionCanvas>(0);
        builder.AddAttribute(1, "OnPointerDownCallback", EventCallback.Factory.Create<PointerEventArgs>(this, OnPointerDown));
        builder.AddAttribute(2, "OnPointerMoveCallback", EventCallback.Factory.Create<PointerEventArgs>(this, OnPointerMove));
        builder.AddAttribute(3, "OnPointerUpCallback", EventCallback.Factory.Create<PointerEventArgs>(this, OnPointerUp));
        builder.AddAttribute(4, "OnPointerOutCallback", EventCallback.Factory.Create<PointerEventArgs>(this, OnPointerOut));
        builder.AddAttribute(5, "OnWheelCallback", EventCallback.Factory.Create<WheelEventArgs>(this, OnWheel));
        builder.AddComponentReferenceCapture(7, r => _motionCanvas = (MotionCanvas)r);
        builder.CloseComponent();
    }

    /// <inheritdoc />
    protected override void OnDrawnViewSizeChanged() => CoreChart?.Update();

    /// <inheritdoc />
    protected override void OnDrawnViewLoaded()
    {
        InitializeChartControl();
        CoreCanvas.Sync = SyncContext;
    }

    /// <inheritdoc />
    protected override void OnDrawnViewUnloaded() => CoreChart?.Unload();

    private void OnPointerDown(PointerEventArgs e) =>
        CoreChart?.InvokePointerDown(new LvcPoint((float)e.OffsetX, (float)e.OffsetY), isSecondaryAction: false);

    private void OnPointerMove(PointerEventArgs e) =>
        CoreChart?.InvokePointerMove(new LvcPoint((float)e.OffsetX, (float)e.OffsetY));

    private void OnPointerUp(PointerEventArgs e) =>
        CoreChart?.InvokePointerUp(new LvcPoint((float)e.OffsetX, (float)e.OffsetY), isSecondaryAction: false);

    private void OnPointerOut(PointerEventArgs e) =>
        CoreChart?.InvokePointerLeft();

    private void OnWheel(WheelEventArgs e) =>
        CoreChart?.InvokePointerWheel(
            new LvcPoint((float)e.OffsetX, (float)e.OffsetY),
            e.DeltaY < 0 ? ZoomDirection.ZoomIn : ZoomDirection.ZoomOut);
}
