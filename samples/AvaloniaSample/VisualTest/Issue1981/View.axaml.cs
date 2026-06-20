using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using LiveChartsCore.Kernel.Sketches;

namespace AvaloniaSample.VisualTest.Issue1981;

public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
    }

    public IEnumerable<IChartView> FindCharts(Visual? parent = null)
    {
        parent ??= (Visual)Content!;

        foreach (var child in parent.GetVisualChildren())
        {
            if (child is IChartView chart)
                yield return chart;

            foreach (var descendant in FindCharts(child))
                yield return descendant;
        }
    }


    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
