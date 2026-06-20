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
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView.Blazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace LiveChartsGeneratedCode;

// ==============================================================================
// Blazor-specific base class for cartesian / pie / polar controls. Drawn-view
// scaffolding (MotionCanvas ref capture, OnAfterRender/Dispose lifecycle,
// CoreCanvas / ControlSize / InvokeOnUIThread) lives in SourceGenDrawnView.blazor.cs.
// Chart-specific plumbing (observer lifecycle, BuildRenderTree for chart events,
// callback Parameters) lives here.
// ==============================================================================

/// <inheritdoc cref="IChartView" />
public abstract partial class SourceGenChart : SourceGenDrawnView, IChartView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SourceGenChart"/> class.
    /// </summary>
    protected SourceGenChart()
    {
        _observer = new(ConfigureObserver, () => CoreChart?.Update());
        InitializeObservedProperties();

        // will be initialized in OnAfterRender, because we need the canvas element reference
        CoreChart = null!;
    }

    /// <summary>
    /// Builds the render tree.
    /// </summary>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        base.BuildRenderTree(builder);
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
        StartObserving();

        CoreChart.Canvas.Sync = SyncContext;
        CoreChart.Load();
    }

    /// <inheritdoc />
    protected override void OnDrawnViewUnloaded()
    {
        StopObserving();
        CoreChart.Unload();
    }

    LvcColor IChartView.BackColor { get; }

    /// <summary>
    /// Gets or sets the pointer down callback.
    /// </summary>
    [Parameter]
    public EventCallback<PointerEventArgs> OnPointerDownCallback { get; set; }

    /// <summary>
    /// Gets or sets the pointer move callback.
    /// </summary>
    [Parameter]
    public EventCallback<PointerEventArgs> OnPointerMoveCallback { get; set; }

    /// <summary>
    /// Gets or sets the pointer up callback.
    /// </summary>
    [Parameter]
    public EventCallback<PointerEventArgs> OnPointerUpCallback { get; set; }

    /// <summary>
    /// Called when the pointer goes down.
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnPointerDown(PointerEventArgs e)
    {
        CoreChart?.InvokePointerDown(new LvcPoint((float)e.OffsetX, (float)e.OffsetY), e.Button == 2);
        _ = OnPointerDownCallback.InvokeAsync(e);
    }

    /// <summary>
    /// Called when the pointer moves.
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnPointerMove(PointerEventArgs e)
    {
        CoreChart?.InvokePointerMove(new LvcPoint((float)e.OffsetX, (float)e.OffsetY));
        _ = OnPointerMoveCallback.InvokeAsync(e);
    }

    /// <summary>
    /// Called when the pointer goes up.
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnPointerUp(PointerEventArgs e)
    {
        CoreChart?.InvokePointerUp(new LvcPoint((float)e.OffsetX, (float)e.OffsetY), e.Button == 2);
        _ = OnPointerUpCallback.InvokeAsync(e);
    }

    /// <summary>
    /// Called then mouse wheel moves.
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnWheel(WheelEventArgs e) { }

    /// <summary>
    /// Called when the pointer leaves the control.
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnPointerOut(PointerEventArgs e) => CoreChart?.InvokePointerLeft();
}
