using LiveChartsCore.Motion;
using LiveChartsCore.Noesis.Drawing;
using Noesis;

namespace LiveChartsCore.Noesis;

/// <summary>Draws chart geometry through Noesis.</summary>
public sealed class MotionCanvas : Grid
{
    private View? _view;
    private NoesisDrawingContext? _drawing;

    /// <summary>Initializes the canvas.</summary>
    public MotionCanvas()
    {
        LiveChartsNoesis.EnsureInitialized();

#if NOESIS_UNITY
        PPAAIn = 0;
#else
        GUI.LoadComponent(this, "/LiveChartsCore.Noesis;component/MotionCanvas.xaml");
#endif

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        SizeChanged += OnSizeChanged;
    }

    /// <summary>Gets the chart's drawing canvas.</summary>
    public CoreMotionCanvas CanvasCore { get; } = new();

    /// <summary>Gets the number of native drawing passes.</summary>
    public int RenderedFrames { get; private set; }

    /// <summary>Gets the number of geometry draws in the last frame.</summary>
    public int LastDrawCount { get; private set; }

    /// <inheritdoc />
    protected override void OnRender(global::Noesis.DrawingContext context)
    {
        base.OnRender(context);

        var drawing = new NoesisDrawingContext(context);

        _drawing = drawing;

        CanvasCore.DrawFrame(drawing);

        LastDrawCount = drawing.DrawCount;
        RenderedFrames++;
    }

    private void OnLoaded(object sender, RoutedEventArgs args)
    {
        _view = View;

        if (_view is not null)
        {
            _view.Rendering += OnRendering;
        }

        CoreMotionCanvas.s_rendererName = "Noesis drawing";
        CoreMotionCanvas.s_tickerName = "Noesis View.Rendering";

        CanvasCore.NotifyPlatformVisible();
        CanvasCore.Invalidate();
        InvalidateVisual();
    }

    private void OnUnloaded(object sender, RoutedEventArgs args)
    {
        if (_view is not null)
        {
            _view.Rendering -= OnRendering;
        }

        _view = null;
        _drawing = null;
    }

    private void OnRendering(object sender, global::Noesis.EventArgs args)
    {
        if (!CanvasCore.IsValid && IsVisible)
        {
            InvalidateVisual();
        }
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs args)
    {
        CanvasCore.Invalidate();
        InvalidateVisual();
    }
}
