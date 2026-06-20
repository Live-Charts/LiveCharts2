using LiveChartsCore.Kernel.Sketches;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace WinUISample.VisualTest.Issue1981;

public sealed partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
    }

    public IEnumerable<IChartView> FindCharts(DependencyObject? parent = null)
    {
        parent ??= (DependencyObject)Content!;

        var count = VisualTreeHelper.GetChildrenCount(parent);

        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is IChartView chart)
                yield return chart;

            foreach (var descendant in FindCharts(child))
                yield return descendant;
        }
    }
}
