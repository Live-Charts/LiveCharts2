using Noesis;

namespace NoesisSample.General.VisualElements;
public partial class View : SampleView
{
    public View()
    {
        GUI.LoadComponent(this, "/NoesisSample;component/General/VisualElements/View.xaml");
    }
}
