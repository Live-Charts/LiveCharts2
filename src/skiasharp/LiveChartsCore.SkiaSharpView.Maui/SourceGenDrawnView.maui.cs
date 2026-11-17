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

using System;
using LiveChartsCore.Drawing;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace LiveChartsGeneratedCode;

/// <inheritdoc cref="SourceGenDrawnView" />
public abstract partial class SourceGenDrawnView : ChartView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SourceGenDrawnView"/> class.
    /// </summary>
    protected SourceGenDrawnView()
    {
        Content = new MotionCanvas();

        SizeChanged += (_, _) => OnDrawnViewSizeChanged();
        Loaded += (_, _) => OnDrawnViewLoadedInternal();
        Unloaded += (_, _) => OnDrawnViewUnloadedInternal();
    }

    private MotionCanvas CanvasView => (MotionCanvas)Content;

    /// <inheritdoc cref="IDrawnView.CoreCanvas" />
    public CoreMotionCanvas CoreCanvas => CanvasView.CanvasCore;

    /// <inheritdoc cref="IDrawnView.ControlSize" />
    public LvcSize ControlSize => new() { Width = (float)Width, Height = (float)Height };

    /// <summary>MAUI has no built-in designer signal.</summary>
    public virtual bool DesignerMode => false;

    /// <summary>Whether MAUI is in dark mode.</summary>
    public virtual bool IsDarkMode =>
        Application.Current?.RequestedTheme == AppTheme.Dark;

    /// <summary>Marshals an action onto the MAUI main thread.</summary>
    public void InvokeOnUIThread(Action action) =>
        MainThread.BeginInvokeOnMainThread(action);

    private void OnDrawnViewLoadedInternal()
    {
#if IOS || MACCATALYST
        // put back the gesture recognizers released when the chart left the window (see below).
        (Handler as LiveChartsCore.SkiaSharpView.Maui.Handlers.ChartViewHandler)?.RestorePointerController();
#endif
        OnDrawnViewLoaded();
    }

    private void OnDrawnViewUnloadedInternal()
    {
        OnDrawnViewUnloaded();
#if IOS || MACCATALYST
        // Maui doesn't release anything when an Element leaves the visual tree. On Apple,
        // ChartViewHandler attaches PointerController's UIKit gesture recognizers to the platform
        // UIView; UIKit holds the recognizers' target (the controller) strongly, the handler
        // subscribes to the controller, and Handler.VirtualView pins the chart, so every chart
        // removed from the visual tree leaked (#1725). Releasing the recognizers breaks that chain.
        //
        // Release them only, don't disconnect the handler: MAUI doesn't create a new handler when
        // the same view is added back (it reuses the parent's), so a disconnected chart never raised
        // Loaded again and stayed blank, e.g. views reused as tabs by swapping a ContentView's
        // content (#2404). Loaded restores them.
        //
        // Gate on Window == null: on a TabbedPage (iOS only) MAUI fires Unloaded for the inactive
        // tab's chart even though it is still mounted in the window (UITabBarController just hides
        // the UIView) and doesn't raise Loaded when the tab is reselected (#2297), so the
        // recognizers must stay there. Window is null only when the chart left the window.
        if (Window is null)
            (Handler as LiveChartsCore.SkiaSharpView.Maui.Handlers.ChartViewHandler)?.ReleasePointerController();
#endif
    }
}
