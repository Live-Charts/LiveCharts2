using Noesis;
using LiveChartsCore.Noesis;

namespace NoesisSample.Treemaps.Basic;
public partial class View : SampleView
{
    public View()
    {
        GUI.LoadComponent(this, "/NoesisSample;component/Treemaps/Basic/View.xaml");
    }

#if UI_TESTING
    public TreemapChart Chart => (TreemapChart)Content!;
#endif
}
