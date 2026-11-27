using Eto.Forms;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView.Eto;
using ViewModelsSamples.General.VisualElements;

namespace EtoFormsSample.General.VisualElements;

public class View : Panel
{
    public View()
    {
        var visualElements = new IChartElement[]
        {
            new RectangleVisual(),
            new ScaledRectangleVisual(),
            new PointerDownAwareVisual(),
            new SvgVisual(),
            new ThemedVisual(),
            new CustomVisual(),
            new AbsoluteVisual(),
            new StackedVisual(),
            new TableVisual(),
            new ContainerVisual(),
        };

        var cartesianChart = new CartesianChart
        {
            VisualElements = visualElements
        };

        Content = cartesianChart;
    }
}
