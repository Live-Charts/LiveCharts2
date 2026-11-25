using Eto.Forms;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;
using ViewModelsSamples.Axes.Paging;

namespace EtoFormsSample.Axes.Paging;

public class View : Panel
{
    private readonly CartesianChart cartesianChart;
    private readonly Axis xAxis;
    private readonly int[] values;

    public View()
    {
        values = new ViewModel().Values;
        xAxis = new Axis();

        var series = new ISeries[]
        {
            new ColumnSeries<int> { Values = values }
        };

        cartesianChart = new CartesianChart
        {
            Series = series,
            XAxes = new[] { xAxis },
            ZoomMode = LiveChartsCore.Measure.ZoomAndPanMode.X,
        };

        var b1 = new Button { Text = "Page 1" };
        b1.Click += (sender, e) => GoToPage(0);
        var b2 = new Button { Text = "Page 2" };
        b2.Click += (sender, e) => GoToPage(1);
        var b3 = new Button { Text = "Page 3" };
        b3.Click += (sender, e) => GoToPage(2);
        var b4 = new Button { Text = "Clear" };
        b4.Click += (sender, e) => SeeAll();

        var buttons = new StackLayout(b1, b2, b3, b4) { Orientation = Orientation.Horizontal, Padding = 2, Spacing = 4 };
        Content = new DynamicLayout(buttons, cartesianChart);
    }

    private void GoToPage(int page)
    {
        if (page == 0) { xAxis.MinLimit = -0.5; xAxis.MaxLimit = 10.5; }
        else if (page == 1) { xAxis.MinLimit = 9.5; xAxis.MaxLimit = 20.5; }
        else if (page == 2) { xAxis.MinLimit = 19.5; xAxis.MaxLimit = 30.5; }
    }

    private void SeeAll()
    {
        xAxis.MinLimit = null;
        xAxis.MaxLimit = null;
    }
}
