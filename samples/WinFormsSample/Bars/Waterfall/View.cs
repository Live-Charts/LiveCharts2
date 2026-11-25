using System;
using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;

namespace WinFormsSample.Bars.Waterfall;

public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(50, 50);

        var series = new ISeries[]
        {
            new RangeColumnSeries<RangeValue>
            {
                Name = "Cash flow",
                Values =
                [
                    new(0, 100),    // Start balance
                    new(100, 150),  // Sales (+50)
                    new(150, 180),  // Other income (+30)
                    new(180, 130),  // Costs (-50)
                    new(130, 110),  // Tax (-20)
                    new(0, 110)     // End balance
                ],
                YToolTipLabelFormatter = point =>
                {
                    var from = point.Coordinate.TertiaryValue;
                    var to = point.Coordinate.PrimaryValue;
                    var delta = to - from;
                    var sign = delta >= 0 ? "+" : "−";
                    return $"{from:N0} → {to:N0} ({sign}{Math.Abs(delta):N0})";
                }
            }
        };

        var cartesianChart = new CartesianChart
        {
            Series = series,
            XAxes = [new Axis { Labels = ["Start", "Sales", "Other income", "Costs", "Tax", "End"] }],
            YAxes = [new Axis { Name = "Balance", MinLimit = 0 }],
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(50, 50),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(cartesianChart);
    }
}
