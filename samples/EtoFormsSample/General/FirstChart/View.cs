using Eto.Forms;
using SkiaSharp;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;

namespace EtoFormsSample.General.FirstChart;

public class View : Panel
{
    public View()
    {
        Content = new CartesianChart
        {
            LegendPosition = LegendPosition.Right,
            Series = [
                new LineSeries<int>
                {
                    Values = [5, 10, 8, 4],
                    Name = "Mary"
                },
                new ColumnSeries<int>
                {
                    Values = [4, 7, 3, 8],
                    Name = "Ana"
                }
            ]
        };
    }

#if UI_TESTING
    public CartesianChart Chart => (CartesianChart)Content;
#endif
}
