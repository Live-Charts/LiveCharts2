using LiveChartsCore.SkiaSharpView.Painting;

namespace ViewModelsSamples.Test.NullContext;

public class ViewModel
{
    public int[] Values { get; set; } = [1, 2, 3, 4, 5];
    public SolidColorPaint LegendTextPaint { get; set; } = new SolidColorPaint(new(0, 0, 0));
    public SolidColorPaint LegendBgPaint { get; set; } = new SolidColorPaint(new(0, 0, 0));
    public SolidColorPaint TooltipTextPaint { get; set; } = new SolidColorPaint(new(0, 0, 0));
    public SolidColorPaint TooltipBgPaint { get; set; } = new SolidColorPaint(new(0, 0, 0));
}
