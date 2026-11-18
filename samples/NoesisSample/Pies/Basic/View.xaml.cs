using Noesis;
using LiveChartsCore.Noesis;

namespace NoesisSample.Pies.Basic;
public partial class View : SampleView
{
    public View()
    {
        GUI.LoadComponent(this, "/NoesisSample;component/Pies/Basic/View.xaml");
    }

#if UI_TESTING
    public PieChart Chart => (PieChart)Content!;
#endif
}
