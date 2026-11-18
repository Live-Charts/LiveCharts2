using System.Threading.Tasks;
using Noesis;
using ViewModelsSamples.StepLines.AutoUpdate;

namespace NoesisSample.StepLines.AutoUpdate;
public partial class View : SampleView
{
    private bool? isStreaming = false;

    public View()
    {
        GUI.LoadComponent(this, "/NoesisSample;component/StepLines/AutoUpdate/View.xaml");
    }

    private async void Button_Click(object sender, Noesis.RoutedEventArgs e)
    {
        var vm = (ViewModel)DataContext;
        isStreaming = isStreaming is null ? true : !isStreaming;

        while (isStreaming.Value)
        {
            vm.RemoveItem();
            vm.AddItem();
            await Task.Delay(1000);
        }
    }
}
