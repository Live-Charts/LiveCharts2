using System.Threading.Tasks;
using Noesis;
using LiveChartsCore.Noesis;
using ViewModelsSamples.Bars.AutoUpdate;

namespace NoesisSample.Bars.AutoUpdate;
public partial class View : SampleView
{
    public View()
    {
        GUI.LoadComponent(this, "/NoesisSample;component/Bars/AutoUpdate/View.xaml");
    }

#if UI_TESTING
    public CartesianChart Chart => chart;
#endif
}
