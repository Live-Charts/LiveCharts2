using System;
using System.Windows.Forms;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;
using ViewModelsSamples.General.MultiThreading2;

namespace WinFormsSample.General.MultiThreading2;

public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();
        Size = new System.Drawing.Size(600, 400);

        // the view model adds points from 10 background tasks,
        // it uses the passed delegate to change the values on the UI thread.
        var viewModel = new ViewModel(InvokeOnUIThread);

        var cartesianChart = new CartesianChart
        {
            Series = [
                new LineSeries<int>
                {
                    Values = viewModel.Values,
                    GeometryFill = null,
                    GeometryStroke = null,
                    LineSmoothness = 0
                }
            ],
            Location = new System.Drawing.Point(0, 0),
            Size = new System.Drawing.Size(600, 400),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        Controls.Add(cartesianChart);
    }

    private void InvokeOnUIThread(Action action)
    {
        // the control needs a handle to invoke on the UI thread.
        if (!IsHandleCreated) return;
        _ = BeginInvoke(action);
    }
}
