using System.Windows.Forms;
using LiveChartsCore.SkiaSharpView.WinForms;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using SkiaSharp;
using LiveChartsCore.Drawing;
using ViewModelsSamples.Scatter.Custom;

namespace WinFormsSample.Scatter.Custom;

public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(50, 50);

        var viewModel = new ViewModel();
        var pinPath = SVGPoints.Pin;

        var series = new ISeries[]
        {
            // the second type argument is the geometry to draw.
            new ScatterSeries<ObservablePoint, HeartGeometry>
            {
                Values = viewModel.Values1,
                Stroke = null,
                GeometrySize = 40
            },

            // the variable SVG path geometry can change the path at runtime.
            new ScatterSeries<ObservablePoint, VariableSVGPathGeometry>
            {
                Values = viewModel.Values2,
                GeometrySvg = pinPath,
                GeometrySize = 40
            },

            // finally you can use skiasharp to draw your own geometries.
            new ScatterSeries<ObservablePoint, MyGeometry>
            {
                Values = viewModel.Values3,
                GeometrySize = 40
            }
        };

        var cartesianChart = new CartesianChart
        {
            Series = series,
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(50, 50),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(cartesianChart);
    }
}
