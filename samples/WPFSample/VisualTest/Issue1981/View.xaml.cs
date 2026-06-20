using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LiveChartsCore.Kernel.Sketches;

namespace WPFSample.VisualTest.Issue1981;

public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
    }

    public IEnumerable<IChartView> FindCharts(DependencyObject? parent = null)
    {
        parent ??= (DependencyObject)Content;

        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is IChartView typedChild)
                yield return typedChild;

            foreach (var descendant in FindCharts(child))
                yield return descendant;
        }
    }
}
