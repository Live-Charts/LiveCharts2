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
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace LiveChartsGeneratedCode;

// ==============================================================================
// this file is the base class for this UI framework controls, in this file we
// define the UI framework specific code. 
// expanding this file in the solution explorer will show 2 more files:
//    - *.shared.cs:        shared code between all UI frameworks
//    - *.sgp.cs:           the source generated properties
// ==============================================================================

/// <inheritdoc cref="IChartView"/>
public abstract partial class SourceGenChart : ChartView, IChartView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SourceGenChart"/> class.
    /// </summary>
    protected SourceGenChart()
    {
        Content = new MotionCanvas();

        SizeChanged += (s, e) =>
            CoreChart.Update();

        InitializeChartControl();
        InitializeObservedProperties();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;

        if (Application.Current is not null)
            Application.Current.RequestedThemeChanged += (sender, args) => CoreChart?.ApplyTheme();
    }

    private MotionCanvas CanvasView => (MotionCanvas)Content;

    /// <inheritdoc cref="IDrawnView.CoreCanvas"/>
    public CoreMotionCanvas CoreCanvas => CanvasView.CanvasCore;

    bool IChartView.DesignerMode => false;
    bool IChartView.IsDarkMode => Application.Current?.RequestedTheme == AppTheme.Dark;
    LvcColor IChartView.BackColor
    {
        get
        {
            var c = (Background as SolidColorBrush)?.Color ?? BackgroundColor;
            return c is not null
                ? LvcColor.FromArgb((byte)(c.Alpha * 255), (byte)(c.Red * 255), (byte)(c.Green * 255), (byte)(c.Blue * 255))
                : CoreCanvas._virtualBackgroundColor;
        }
    }
    LvcSize IDrawnView.ControlSize => new() { Width = (float)Width, Height = (float)Height };

    private void OnLoaded(object? sender, EventArgs e)
    {
        StartObserving();
        CoreChart?.Load();
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
        StopObserving();
        CoreChart?.Unload();
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
        Handler?.DisconnectHandler();
#endif
    }

    private ISeries InflateSeriesTemplate(object item)
    {
        if (SeriesTemplate.CreateContent() is not View template)
            throw new InvalidOperationException("The template must be a View.");
        if (template is not ISeries series)
            throw new InvalidOperationException("The template is not a valid series.");

        template.BindingContext = item;

        return series;
    }

    private void AddUIElement(object item)
    {
        if (item is not View view) return;
        CanvasView.Children.Add(view);
    }

    private void RemoveUIElement(object item)
    {
        if (item is not View view) return;
        _ = CanvasView.Children.Remove(view);
    }

    internal override void OnPressed(object? sender, LiveChartsCore.Native.Events.PressedEventArgs args)
    {
        // not implemented yet?
        // https://github.com/dotnet/maui/issues/16202
        //if (Keyboard.Modifiers > 0) return;

        var cArgs = new PointerCommandArgs(this, new(args.Location.X, args.Location.Y), args);
        if (PressedCommand?.CanExecute(cArgs) == true)
            PressedCommand.Execute(cArgs);

        CoreChart.InvokePointerDown(args.Location, args.IsSecondaryPress);
    }

    internal override void OnMoved(object? sender, LiveChartsCore.Native.Events.ScreenEventArgs args)
    {
        var location = args.Location;

        var cArgs = new PointerCommandArgs(this, new(location.X, location.Y), args.OriginalEvent);
        if (MovedCommand?.CanExecute(cArgs) == true)
            MovedCommand.Execute(cArgs);

        CoreChart.InvokePointerMove(location);
    }

    internal override void OnReleased(object? sender, LiveChartsCore.Native.Events.PressedEventArgs args)
    {
        // Synthetic releases are raised by the shared PointerController on
        // PointerCaptureLost (see #1576). The user has not actually lifted the
        // pointer, so we must not invoke the public ReleasedCommand — only forward
        // to the core chart so internal pan/drag state can be released. WinUI/Uno
        // and WPF/Avalonia follow the same rule from their own platform paths.
        if (!args.IsSyntheticRelease)
        {
            var cArgs = new PointerCommandArgs(this, new(args.Location.X, args.Location.Y), args);
            if (ReleasedCommand?.CanExecute(cArgs) == true)
                ReleasedCommand.Execute(cArgs);
        }

        CoreChart.InvokePointerUp(args.Location, args.IsSecondaryPress);
    }

    internal override void OnExited(object? sender, LiveChartsCore.Native.Events.EventArgs args) =>
        CoreChart.InvokePointerLeft();

    void IChartView.InvokeOnUIThread(Action action) =>
        MainThread.BeginInvokeOnMainThread(action);
}
