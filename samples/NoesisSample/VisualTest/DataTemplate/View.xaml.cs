using System.Net.WebSockets;
using Noesis;
using LiveChartsCore.Kernel.Sketches;

namespace NoesisSample.VisualTest.DataTemplate;
public partial class View : SampleView
{
    public View()
    {
        GUI.LoadComponent(this, "/NoesisSample;component/VisualTest/DataTemplate/View.xaml");
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
