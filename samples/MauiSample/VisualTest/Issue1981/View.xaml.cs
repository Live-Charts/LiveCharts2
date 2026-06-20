using LiveChartsCore.Kernel.Sketches;

namespace MauiSample.VisualTest.Issue1981;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class View : ContentPage
{
    public View()
    {
        InitializeComponent();
    }

    public IEnumerable<IChartView> FindCharts(Element? parent = null)
    {
        parent ??= Content!;

        foreach (var child in parent.GetVisualTreeDescendants())
        {
            if (child is IChartView chart)
                yield return chart;
        }
    }
}
