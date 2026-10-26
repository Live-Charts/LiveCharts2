using System.Collections.Generic;
using LiveChartsCore.Kernel;

namespace ViewModelsSamples.General.VisualElements;

public class ViewModel
{
    public IEnumerable<IChartElement> VisualElements { get; set; } = [
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
        ];
}
