using LiveChartsCore.Drawing;
using LiveChartsCore.Motion;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Noesis;
using Noesis;

namespace LiveChartsGeneratedCode;

/// <summary>
/// Provides Noesis layout and dispatcher integration for drawn chart controls.
/// </summary>
public abstract partial class SourceGenDrawnView : UserControl
{
    private readonly MotionCanvas _motionCanvas = new();

    /// <summary>Initializes the chart host.</summary>
    protected SourceGenDrawnView()
    {
        // Register the UI dispatcher before chart updates can run on a worker thread.
        _ = Dispatcher;

        Content = _motionCanvas;

        SizeChanged += OnSizeChanged;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    /// <inheritdoc />
    public CoreMotionCanvas CoreCanvas => _motionCanvas.CanvasCore;

    /// <inheritdoc />
    public LvcSize ControlSize => new(ActualWidth, ActualHeight);

    /// <inheritdoc />
    public virtual bool DesignerMode => false;

    /// <inheritdoc />
    public virtual bool IsDarkMode
    {
        get => (bool)GetValue(IsDarkModeProperty);
        set => SetValue(IsDarkModeProperty, value);
    }

    /// <summary>
    /// Identifies the chart palette preference.
    /// </summary>
    public static readonly DependencyProperty IsDarkModeProperty = DependencyProperty.Register(
        nameof(IsDarkMode),
        typeof(bool),
        typeof(SourceGenDrawnView),
        new PropertyMetadata(false, OnThemeChanged));

    private static void OnThemeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        if (sender is IChartView chart)
        {
            chart.CoreChart?.Update();
        }
    }

    /// <inheritdoc />
    public void InvokeOnUIThread(Action action)
    {
        if (Dispatcher.CheckAccess())
        {
            action();

            return;
        }

        Dispatcher.BeginInvoke(action);
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs args)
    {
        OnDrawnViewSizeChanged();
    }

    private void OnLoaded(object sender, RoutedEventArgs args)
    {
        OnDrawnViewLoaded();
    }

    private void OnUnloaded(object sender, RoutedEventArgs args)
    {
        OnDrawnViewUnloaded();
    }
}
