using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LiveChartsCore.SkiaSharpView.Avalonia;

namespace AvaloniaSample.Test.NullContext;

public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public CartesianChart CartesianChart => this.FindControl<CartesianChart>("cartesianChart")!;
    public PieChart PieChart => this.FindControl<PieChart>("pieChart")!;
    public PolarChart PolarChart => this.FindControl<PolarChart>("polarChart")!;

    public void SetNullContext() =>
        DataContext = null;
}
