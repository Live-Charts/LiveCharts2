using Noesis;
using LiveChartsCore.Noesis;

namespace NoesisSample.Polar.Basic;
public partial class View : SampleView
{
    public View()
    {
        GUI.LoadComponent(this, "/NoesisSample;component/Polar/Basic/View.xaml");
    }

#if UI_TESTING
    public PolarChart Chart => (PolarChart)Content!;
#endif
}
