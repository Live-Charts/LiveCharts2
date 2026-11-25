using System;
using Eto.Forms;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Eto;
using ViewModelsSamples.General.MultiThreading2;

namespace EtoFormsSample.General.MultiThreading2;

public class View : Panel
{
    private readonly CartesianChart cartesianChart;

    public View()
    {
        // the view model adds points from 10 background tasks,
        // it uses the passed delegate to change the values on the UI thread.
        var viewModel = new ViewModel(InvokeOnUIThread);

        cartesianChart = new CartesianChart
        {
            Series =
            [
                new LineSeries<int>
                {
                    Values = viewModel.Values,
                    GeometryFill = null,
                    GeometryStroke = null,
                    LineSmoothness = 0
                }
            ]
        };

        Content = cartesianChart;
    }

    private static void InvokeOnUIThread(Action action) =>
        Application.Instance.AsyncInvoke(action);
}
