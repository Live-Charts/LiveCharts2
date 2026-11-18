using System;
using System.Threading.Tasks;
using Noesis;
using LiveChartsCore.Noesis;

namespace NoesisSample.Test.MotionCanvasDispose;
public partial class UserControl1 : SampleView
{
    public UserControl1()
    {
        GUI.LoadComponent(this, "/NoesisSample;component/Test/MotionCanvasDispose/UserControl1.xaml");

        Loaded += UserControl1_Loaded;
    }

    private async void UserControl1_Loaded(object sender, Noesis.RoutedEventArgs e)
    {
        var canvas = (MotionCanvas)FindName("canvas");
        await Task.Delay(TimeSpan.FromMilliseconds(30)); // workaround to wait for the canvas to be ready...
        ViewModelsSamples.Test.MotionCanvasDispose.ViewModel.Generate(canvas.CanvasCore);
    }
}
