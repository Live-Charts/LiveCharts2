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
using LiveChartsCore.Motion;
using LiveChartsCore.Vortice;
using Vortice;
using Vortice.Direct2D1;

namespace LiveChartsGeneratedCode;

// ==============================================================================
// this file is the base class for this UI framework controls, in this file we
// define the UI framework specific code. 
// expanding this file in the solution explorer will show 2 more files:
//    - *.shared.cs:        shared code between all UI frameworks
//    - *.sgp.cs:           the source generated properties
// ==============================================================================

public abstract partial class SourceGenChart : IMyUIFrameworkControl
{
    public IMyUIFrameworkControl[] Children { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SourceGenChart"/> class.
    /// </summary>
    /// <exception cref="MotionCanvas"></exception>
    protected SourceGenChart()
    {
        var motionCanvas = new MotionCanvas();
        Children = [motionCanvas];

        InitializeChartControl();
        InitializeObservedProperties();

        StartObserving();
        CoreChart?.Load();
    }

    // just a sample for our small ui framework, dispose things on destruction.
    ~SourceGenChart()
    {
        StopObserving();
        CoreChart?.Unload();
    }

    public CoreMotionCanvas CoreCanvas => ((MotionCanvas)Children[0]).CanvasCore;
    public bool IsDarkMode => false;
    public bool DesignerMode => false;
    public LvcColor BackColor => new(255, 255, 255, 255);
    public LvcSize ControlSize => new()
    {
        Width = Application.Current!.MainWindow!.Bounds.Width,
        Height = Application.Current.MainWindow.Bounds.Height
    };

    public void InvokeOnUIThread(Action action) =>
        action();

    public void DrawFrame(ID2D1HwndRenderTarget renderTarget)
    {
        foreach (var control in Children)
            control.DrawFrame(renderTarget);
    }

    public void Measure() =>
        CoreChart.Measure();

    // ==============================================================================
    // toDo: implement pointer event in our sample ui framework.
    // ==============================================================================

    //private void OnMouseMove(...)
    //{
    //    notify livecharts that the pointer moved...
    //}

    //private void OnMouseDown(...)
    //{
    //    notify livecharts that the pointer is down...
    //}

    //private void OnMouseUp(...)
    //{
    //    notify livecharts that the pointer is up...
    //}

    //private void OnMouseLeave(...)
    //{
    //    notify livecharts that the pointer left the control...
    //}
}
