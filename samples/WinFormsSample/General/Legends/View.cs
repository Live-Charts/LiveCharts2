using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;

namespace WinFormsSample.General.Legends;

public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(400, 350);

        var series = new ISeries[]
        {
            new ColumnSeries<double> { Name = "Peru", Values = [3, 7, 3] },
            new ColumnSeries<double> { Name = "Colombia", Values = [4, 6, 2] },
            new ColumnSeries<double> { Name = "Mexico", Values = [5, 5, 5] },
            new ColumnSeries<double> { Name = "Argentina", Values = [6, 4, 6] },
            new ColumnSeries<double> { Name = "Chile", Values = [7, 3, 7] },
            new ColumnSeries<double> { Name = "Ecuador", Values = [8, 2, 8] },
            new ColumnSeries<double> { Name = "Bolivia", Values = [9, 1, 9] },
            new ColumnSeries<double> { Name = "Venezuela", Values = [10, 0, 10] },
            new ColumnSeries<double> { Name = "Uruguay", Values = [11, 1, 11] },
            new ColumnSeries<double> { Name = "Paraguay", Values = [12, 2, 12] },
            new ColumnSeries<double> { Name = "Brazil", Values = [13, 3, 13] },
            new ColumnSeries<double> { Name = "Costa Rica", Values = [14, 4, 14] },
            new ColumnSeries<double> { Name = "Panama", Values = [15, 5, 15] },
            new ColumnSeries<double> { Name = "Honduras", Values = [16, 6, 16] }
        };

        var cartesianChart = new CartesianChart
        {
            Series = series,
            LegendPosition = LegendPosition.Left,
            Location = new System.Drawing.Point(0, 50),
            Size = new System.Drawing.Size(400, 300),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(cartesianChart);

        var b1 = new ComboBox { Text = "left", Location = new System.Drawing.Point(0, 0), Width = 120 };
        b1.Items.AddRange(["hidden", "top", "left", "right", "bottom"]);
        b1.SelectedValueChanged += (sender, e) =>
        {
            if (b1.SelectedItem.ToString() == "hidden") cartesianChart.LegendPosition = LegendPosition.Hidden;
            if (b1.SelectedItem.ToString() == "top") cartesianChart.LegendPosition = LegendPosition.Top;
            if (b1.SelectedItem.ToString() == "bottom") cartesianChart.LegendPosition = LegendPosition.Bottom;
            if (b1.SelectedItem.ToString() == "left") cartesianChart.LegendPosition = LegendPosition.Left;
            if (b1.SelectedItem.ToString() == "right") cartesianChart.LegendPosition = LegendPosition.Right;
        };
        Controls.Add(b1);
    }
}
