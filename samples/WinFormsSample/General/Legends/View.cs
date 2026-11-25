using System.Linq;
using System.Windows.Forms;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;
using ViewModelsSamples.General.Legends;

namespace WinFormsSample.General.Legends;

public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(400, 350);

        var viewModel = new ViewModel();

        var series = viewModel.Data
            .Select(data => new ColumnSeries<double> { Name = data.Name, Values = data.Values })
            .ToArray();

        var cartesianChart = new CartesianChart
        {
            Series = series,
            LegendPosition = viewModel.SelectedPosition,
            Location = new System.Drawing.Point(0, 50),
            Size = new System.Drawing.Size(400, 300),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(cartesianChart);

        var b1 = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new System.Drawing.Point(0, 0),
            Width = 120
        };
        b1.Items.AddRange([.. viewModel.Positions.Cast<object>()]);
        b1.SelectedItem = viewModel.SelectedPosition;
        b1.SelectedValueChanged += (sender, e) =>
            cartesianChart.LegendPosition = (LegendPosition)b1.SelectedItem;
        Controls.Add(b1);
    }
}
