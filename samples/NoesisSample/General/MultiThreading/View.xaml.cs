using Noesis;

namespace NoesisSample.General.MultiThreading;
public partial class View : SampleView
{
    public View()
    {
        GUI.LoadComponent(this, "/NoesisSample;component/General/MultiThreading/View.xaml");

        ((LiveChartsCore.Noesis.CartesianChart)Content).SyncContext =
            ((ViewModelsSamples.General.MultiThreading.ViewModel)DataContext).Sync;
    }
}
