using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.Kernel.Sketches;
using Noesis;
using SynchronizationLock = LiveChartsCore.Noesis.SynchronizationLock;

namespace LiveChartsGeneratedCode;

/// <summary>
/// Connects chart observation and pointer events to Noesis controls.
/// </summary>
public abstract partial class SourceGenChart : SourceGenDrawnView, IChartView
{
    private readonly Dictionary<ISeries, ContentPresenter> _seriesPresenters = new();
    private bool _isPointerDown;
    private bool _isObserving;

    /// <inheritdoc />
    public object SyncContext
    {
        get => CoreCanvas.Sync;
        set => SetValue(SyncContextProperty, new SynchronizationLock(value));
    }

    /// <summary>
    /// Identifies the synchronization lock used for chart updates.
    /// </summary>
    public static readonly DependencyProperty SyncContextProperty = DependencyProperty.Register(
        nameof(SyncContext),
        typeof(object),
        typeof(SourceGenChart),
        new PropertyMetadata(null, OnSynchronizationLockChanged));

    private static void OnSynchronizationLockChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        if (args.NewValue is not SynchronizationLock sync)
        {
            return;
        }

        var chart = (SourceGenChart)sender;

        chart.CoreCanvas.Sync = sync.Value;
        chart.CoreChart?.Update();
    }

    /// <summary>Initializes a Noesis chart control.</summary>
    protected SourceGenChart()
    {
        InitializeChartControl();
        InitializeObservedProperties();

        MouseDown += OnMouseDown;
        MouseMove += OnMouseMove;
        MouseUp += OnMouseUp;
        MouseLeave += OnMouseLeave;
        LostMouseCapture += OnLostMouseCapture;
    }

    LvcColor IChartView.BackColor =>
        Background is SolidColorBrush brush
            ? LvcColor.FromArgb(brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B)
            : CoreCanvas._virtualBackgroundColor;

    /// <inheritdoc />
    protected override void OnDrawnViewSizeChanged()
    {
        CoreChart?.Update();
    }

    /// <inheritdoc />
    protected override void OnDrawnViewLoaded()
    {
        _isObserving = true;

        StartObserving();
        CoreChart.Load();
    }

    /// <inheritdoc />
    protected override void OnDrawnViewUnloaded()
    {
        _isObserving = false;

        ReleasePointer();
        StopObserving();

        CoreChart.Unload();
    }

    private void AddUIElement(object item)
    {
        if (item is ISeries series && _seriesPresenters.ContainsKey(series))
        {
            return;
        }

        if (Content is Panel panel && item is FrameworkElement element && VisualTreeHelper.GetParent(element) is null)
        {
            panel.Children.Add(element);
        }
    }

    private void RemoveUIElement(object item)
    {
        if (Content is not Panel panel)
        {
            return;
        }

        if (item is ISeries series && _seriesPresenters.Remove(series, out var presenter))
        {
            panel.Children.Remove(presenter);

            return;
        }

        if (item is FrameworkElement element)
        {
            panel.Children.Remove(element);
        }
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs args)
    {
        CaptureMouse();

        _isPointerDown = true;

        var position = args.GetPosition(this);
        var commandArgs = new PointerCommandArgs(this, new(position.X, position.Y), args);

        if (PointerPressedCommand?.CanExecute(commandArgs) == true)
        {
            PointerPressedCommand.Execute(commandArgs);
        }

        CoreChart.InvokePointerDown(new(position.X, position.Y), args.ChangedButton == MouseButton.Right);
    }

    private void OnMouseMove(object sender, MouseEventArgs args)
    {
        var position = args.GetPosition(this);
        var commandArgs = new PointerCommandArgs(this, new(position.X, position.Y), args);

        if (PointerMoveCommand?.CanExecute(commandArgs) == true)
        {
            PointerMoveCommand.Execute(commandArgs);
        }

        CoreChart.InvokePointerMove(new(position.X, position.Y));
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs args)
    {
        var position = args.GetPosition(this);
        var commandArgs = new PointerCommandArgs(this, new(position.X, position.Y), args);

        _isPointerDown = false;

        if (PointerReleasedCommand?.CanExecute(commandArgs) == true)
        {
            PointerReleasedCommand.Execute(commandArgs);
        }

        CoreChart.InvokePointerUp(new(position.X, position.Y), args.ChangedButton == MouseButton.Right);

        ReleaseMouseCapture();
    }

    private void OnMouseLeave(object sender, MouseEventArgs args)
    {
        CoreChart.InvokePointerLeft();
    }

    private void OnLostMouseCapture(object sender, MouseEventArgs args)
    {
        ReleasePointer();
    }

    private void ReleasePointer()
    {
        if (!_isPointerDown)
        {
            return;
        }

        _isPointerDown = false;

        var position = Mouse.GetPosition(this);

        CoreChart.InvokePointerUp(new(position.X, position.Y), false);
    }

    private ISeries InflateSeriesTemplate(object item)
    {
        var panel = (Panel)Content;
        var presenter = new ContentPresenter
        {
            Content = item,
            ContentTemplate = SeriesTemplate,
            IsHitTestVisible = false,
            Width = 0,
            Height = 0
        };

        panel.Children.Add(presenter);
        presenter.ApplyTemplate();
        presenter.Measure(new Size(float.PositiveInfinity, float.PositiveInfinity));

        if (VisualTreeHelper.GetChildrenCount(presenter) == 0 ||
            VisualTreeHelper.GetChild(presenter, 0) is not FrameworkElement element ||
            element is not ISeries series)
        {
            panel.Children.Remove(presenter);

            throw new InvalidOperationException("The series template must contain a LiveCharts series.");
        }

        element.DataContext = item;

        _seriesPresenters.Add(series, presenter);

        return series;
    }

    private static object GetSeriesSource(ISeries series) => ((FrameworkElement)series).DataContext;
}
