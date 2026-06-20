namespace MauiSample.Test.NullContext;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class View : ContentPage
{
    public View()
    {
        InitializeComponent();
    }

    public void SetNullContext() =>
        BindingContext = null;
}
