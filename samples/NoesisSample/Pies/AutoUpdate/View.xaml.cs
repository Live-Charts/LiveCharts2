using System.Threading.Tasks;
using Noesis;
using LiveChartsCore;
using LiveChartsCore.Noesis;
using ViewModelsSamples.Pies.AutoUpdate;

namespace NoesisSample.Pies.AutoUpdate;
public partial class View : SampleView
{

    public View()
    {
        GUI.LoadComponent(this, "/NoesisSample;component/Pies/AutoUpdate/View.xaml");
    }

#if UI_TESTING
    public PieChart Chart => (PieChart)FindName("chart");
#endif
}
