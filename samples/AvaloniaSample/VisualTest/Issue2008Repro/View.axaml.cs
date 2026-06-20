using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView.Avalonia;

namespace AvaloniaSample.VisualTest.Issue2008Repro;

public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
    }

    public Gauge TemplatedGauge => this.Find<Gauge>("templatedGauge")!;

    public IPieSeries? FindTemplatedGaugeSeries()
    {
        // walk the templated PieChart to find its first non-fill IPieSeries.
        var chart = FindDescendantPieChart(TemplatedGauge);
        return ((IPieChartView)chart!).Series
            .OfType<IPieSeries>()
            .FirstOrDefault(s => !s.IsFillSeries);
    }

    private static PieChart? FindDescendantPieChart(Visual root)
    {
        foreach (var child in root.GetVisualChildren())
        {
            if (child is PieChart c) return c;
            var hit = FindDescendantPieChart(child);
            if (hit is not null) return hit;
        }
        return null;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

public class Gauge : TemplatedControl
{
    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<Gauge, double>(nameof(Value), 0d);

    public static readonly StyledProperty<double> MaxValueProperty =
        AvaloniaProperty.Register<Gauge, double>(nameof(MaxValue), 100d);

    public static readonly StyledProperty<double> MinValueProperty =
        AvaloniaProperty.Register<Gauge, double>(nameof(MinValue), 0d);

    public static readonly StyledProperty<string> LabelProperty =
        AvaloniaProperty.Register<Gauge, string>(nameof(Label), string.Empty);

    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double MaxValue
    {
        get => GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    public double MinValue
    {
        get => GetValue(MinValueProperty);
        set => SetValue(MinValueProperty, value);
    }

    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }
}
