using Noesis;

namespace NoesisSample.Financial.BasicOhlc;
public partial class View : SampleView
{
    public View()
    {
        GUI.LoadComponent(this, "/NoesisSample;component/Financial/BasicOhlc/View.xaml");
    }
}
