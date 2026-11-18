using Noesis;

namespace NoesisSample.Axes.Shared;
public partial class View : SampleView
{
    public View()
    {
        GUI.LoadComponent(this, "/NoesisSample;component/Axes/Shared/View.xaml");
    }
}
