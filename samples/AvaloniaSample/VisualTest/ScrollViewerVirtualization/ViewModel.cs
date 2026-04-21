using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace AvaloniaSample.VisualTest.ScrollViewerVirtualization;

// Simulates a view model that loads series data asynchronously (e.g., from a server).
// This is a visual test for https://github.com/Live-Charts/LiveCharts2/issues/1986
public class ViewModel : INotifyPropertyChanged
{
    private IEnumerable<ISeries> _series1 = [];
    private IEnumerable<ISeries> _series2 = [];

    public IEnumerable<ISeries> Series1
    {
        get => _series1;
        private set { _series1 = value; OnPropertyChanged(); }
    }

    public IEnumerable<ISeries> Series2
    {
        get => _series2;
        private set { _series2 = value; OnPropertyChanged(); }
    }

    public async Task LoadDataAsync(int delayMs = 1000)
    {
        await Task.Delay(delayMs);

        Series1 =
        [
            new LineSeries<double> { Values = [5, 10, 8, 4, 9, 6, 11] },
            new ColumnSeries<double> { Values = [3, 7, 4, 8, 2, 6, 5] }
        ];

        Series2 =
        [
            new LineSeries<double> { Values = [8, 3, 6, 9, 4, 7, 5] },
            new ColumnSeries<double> { Values = [6, 2, 8, 4, 9, 3, 7] }
        ];
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
