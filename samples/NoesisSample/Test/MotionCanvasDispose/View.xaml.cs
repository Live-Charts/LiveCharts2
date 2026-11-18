using Noesis;

namespace NoesisSample.Test.MotionCanvasDispose;
public partial class View : SampleView
{
    public View()
    {
        GUI.LoadComponent(this, "/NoesisSample;component/Test/MotionCanvasDispose/View.xaml");
    }

    private void Button_Click(object sender, Noesis.RoutedEventArgs e)
    {
        var content = (ContentControl)FindName("content");
        content.Content = new UserControl1();
    }
}
