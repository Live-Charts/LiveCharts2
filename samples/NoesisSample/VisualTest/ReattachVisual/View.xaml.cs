using Noesis;
using LiveChartsCore.Noesis;

namespace NoesisSample.VisualTest.ReattachVisual;
public partial class View : SampleView
{
    private bool _isInVisualTree = true;

    public View()
    {
        GUI.LoadComponent(this, "/NoesisSample;component/VisualTest/ReattachVisual/View.xaml");
    }

    private void Button_Click(object sender, Noesis.RoutedEventArgs e)
    {
        var parent = (Grid)FindName("parent");
        var chart = (CartesianChart)FindName("chart");

        if (_isInVisualTree)
        {
            parent.Children.Remove(chart);
            _isInVisualTree = false;
            return;
        }

        parent.Children.Add(chart);
        _isInVisualTree = true;
    }
}
