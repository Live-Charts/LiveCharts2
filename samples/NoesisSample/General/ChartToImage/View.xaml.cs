using Noesis;

namespace NoesisSample.General.ChartToImage;

public partial class View : SampleView
{
    public View()
    {
        GUI.LoadComponent(this, "/NoesisSample;component/General/ChartToImage/View.xaml");

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs args)
    {
        var app = (App)NoesisApp.Application.Current;

        app.ExportChart((FrameworkElement)FindName("cartesianChart"), "CartesianImageFromControl.bmp");
        app.ExportChart((FrameworkElement)FindName("pieChart"), "PieImageFromControl.bmp");
    }
}
