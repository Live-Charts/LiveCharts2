using System.Linq;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Drawing.Layouts;
using LiveChartsCore.Noesis.Drawing;
using LiveChartsCore.Noesis.Drawing.Layouts;
using LiveChartsCore.Noesis;

namespace ViewModelsSamples.General.TemplatedLegends;

public class CustomLegend : DefaultLegend
{
    protected override Layout<NoesisDrawingContext> GetLayout(Chart chart)
    {
        var theme = chart.GetTheme();

        var stackLayout = new StackLayout
        {
            Orientation = ContainerOrientation.Vertical,
            Padding = new Padding(15, 4),
            HorizontalAlignment = Align.Start,
            VerticalAlignment = Align.Middle,
        };

        foreach (var series in chart.Series.Where(x => x.IsVisibleAtLegend))
            stackLayout.Children.Add(new LegendItem(series, theme.TooltipTextPaint));

        return stackLayout;
    }
}
