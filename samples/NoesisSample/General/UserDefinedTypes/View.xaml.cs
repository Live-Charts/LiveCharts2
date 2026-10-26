using Noesis;

namespace NoesisSample.General.UserDefinedTypes;
public partial class View : SampleView
{
    public View()
    {
        GUI.LoadComponent(this, "/NoesisSample;component/General/UserDefinedTypes/View.xaml");
    }
}
