using Noesis;

namespace NoesisSample.General.Sections;
public partial class View : SampleView
{
    public View()
    {
        GUI.LoadComponent(this, "/NoesisSample;component/General/Sections/View.xaml");
    }
}
