using LiveChartsCore.Drawing;
using LiveChartsCore.Geo;
using LiveChartsCore.Measure;
using Noesis;

namespace LiveChartsGeneratedCode;

public abstract partial class SourceGenMapChart : SourceGenDrawnView, IGeoMapView
{
    /// <inheritdoc />
    public object SyncContext
    {
        get => CoreCanvas.Sync;
        set => SetValue(SyncContextProperty, new LiveChartsCore.Noesis.SynchronizationLock(value));
    }

    /// <summary>
    /// Identifies the synchronization lock used for chart updates.
    /// </summary>
    public static readonly DependencyProperty SyncContextProperty = DependencyProperty.Register(
        nameof(SyncContext),
        typeof(object),
        typeof(SourceGenMapChart),
        new PropertyMetadata(null, OnSynchronizationLockChanged));

    private static void OnSynchronizationLockChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        if (args.NewValue is not LiveChartsCore.Noesis.SynchronizationLock sync)
        {
            return;
        }

        var chart = (SourceGenMapChart)sender;

        chart.CoreCanvas.Sync = sync.Value;
        chart.CoreChart?.Update();
    }

    /// <summary>Initializes a Noesis map control.</summary>
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
    protected override void OnDrawnViewSizeChanged()
    {
        CoreChart?.Update();
    }

    /// <inheritdoc />
    protected override void OnDrawnViewLoaded()
    {
        CoreChart.Load();
    }

    /// <inheritdoc />
    protected override void OnDrawnViewUnloaded()
    {
        CoreChart.Unload();
    }

    private void OnMouseWheel(object sender, MouseWheelEventArgs args)
    {
        var position = args.GetPosition(this);

        CoreChart.InvokePointerWheel(
            new LvcPoint(position.X, position.Y),
            args.Delta > 0 ? ZoomDirection.ZoomIn : ZoomDirection.ZoomOut);
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs args)
    {
        var position = args.GetPosition(this);

        CaptureMouse();

        CoreChart.InvokePointerDown(new(position.X, position.Y), false);
    }

    private void OnMouseMove(object sender, MouseEventArgs args)
    {
        var position = args.GetPosition(this);

        CoreChart.InvokePointerMove(new(position.X, position.Y));
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs args)
    {
        var position = args.GetPosition(this);

        CoreChart.InvokePointerUp(new(position.X, position.Y), false);

        ReleaseMouseCapture();
    }

    private void OnMouseLeave(object sender, MouseEventArgs args)
    {
        CoreChart.InvokePointerLeft();
    }
}
