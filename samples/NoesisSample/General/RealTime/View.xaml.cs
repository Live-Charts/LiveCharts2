using Noesis;

namespace NoesisSample.General.RealTime;
public partial class View : SampleView
{
    public View()
    {
        GUI.LoadComponent(this, "/NoesisSample;component/General/RealTime/View.xaml");

        ((LiveChartsCore.Noesis.CartesianChart)Content).SyncContext =
            ((ViewModelsSamples.General.RealTime.ViewModel)DataContext).Sync;
    }
}
