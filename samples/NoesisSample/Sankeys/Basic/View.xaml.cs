using Noesis;
using LiveChartsCore.Noesis;

namespace NoesisSample.Sankeys.Basic;
public partial class View : SampleView
{
    public View()
    {
        GUI.LoadComponent(this, "/NoesisSample;component/Sankeys/Basic/View.xaml");
    }

#if UI_TESTING
    public SankeyChart Chart => (SankeyChart)Content!;
#endif
}
