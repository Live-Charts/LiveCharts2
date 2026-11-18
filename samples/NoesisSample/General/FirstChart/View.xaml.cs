using Noesis;
using LiveChartsCore.Noesis;

namespace NoesisSample.General.FirstChart;
public partial class View : SampleView
{
    public View()
    {
        GUI.LoadComponent(this, "/NoesisSample;component/General/FirstChart/View.xaml");
    }

#if UI_TESTING
    public CartesianChart Chart => (CartesianChart)Content;
#endif
}
