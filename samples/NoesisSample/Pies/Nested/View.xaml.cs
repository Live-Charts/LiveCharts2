using Noesis;

namespace NoesisSample.Pies.Nested;
public partial class View : SampleView
{
    public View()
    {
        GUI.LoadComponent(this, "/NoesisSample;component/Pies/Nested/View.xaml");
    }
}
