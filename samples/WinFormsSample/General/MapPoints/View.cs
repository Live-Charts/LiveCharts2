using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WinForms;
using SkiaSharp;

namespace WinFormsSample.General.MapPoints;

public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(400, 400);

        var columnSeries = new ColumnSeries<int>
        {
            Values = [2, 5, 4, 6, 8, 3, 2, 4, 6],
            ShowDataLabels = true,
            DataLabelsPosition = DataLabelsPosition.Top
        };

        columnSeries.PointMeasured += OnPointMeasured;

        var cartesianChart = new CartesianChart
        {
            Series = [columnSeries],
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(400, 400),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(cartesianChart);
    }

    private void OnPointMeasured(ChartPoint point)
    {
        if (point.Context.Visual is null) return;
        point.Context.Visual.Fill = GetPaint(point.Index);
    }

    private SolidColorPaint GetPaint(int index)
    {
        var paints = new SolidColorPaint[]
        {
            new(SKColors.Red),
            new(SKColors.Green),
            new(SKColors.Blue),
            new(SKColors.Yellow)
        };
        return paints[index % paints.Length];
    }
}
