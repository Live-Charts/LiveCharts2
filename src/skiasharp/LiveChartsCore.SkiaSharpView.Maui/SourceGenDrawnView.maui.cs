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
        Loaded += (_, _) => OnDrawnViewLoaded();
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

    private void OnDrawnViewUnloadedInternal()
    {
        OnDrawnViewUnloaded();
#if IOS || MACCATALYST
        // Maui doesn't auto-DisconnectHandler when an Element leaves the visual tree.
        // On Apple, ChartViewHandler.ConnectHandler attaches PointerController's
        // UIKit gesture recognizers (UILongPress/UIPinch/UIPan/UIHover) to the
        // platform UIView; UIKit holds the recognizers' selector target (the
        // controller) strongly, the handler subscribes to the controller via
        // instance methods, and Handler.VirtualView pins the chart — leaking
        // every chart removed from the visual tree (#1725). Other platforms
        // exhibit the same +=/-= pattern but don't leak in practice (no
        // equivalent native-peer pinning), so scope this to Apple.
        //
        // Gate on Window == null: on a TabbedPage (iOS only) MAUI fires Unloaded
        // for the inactive tab's chart even though the element is still mounted
        // in the window — UITabBarController just hides the platform UIView.
        // DisconnectHandler in that case nulls the handler and MAUI never raises
        // Loaded again when the tab is reselected, so the chart goes permanently
        // blank (#2297). Window is null only when the chart has truly detached
        // from the window, which is the case we need to release the chain in.
        if (Window is null)
            Handler?.DisconnectHandler();
#endif
    }
}
