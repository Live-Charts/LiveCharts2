using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace AvaloniaSample.VisualTest.Issue1986Repro;

// Reproducer for https://github.com/Live-Charts/LiveCharts2/issues/1986
// Following busitech's reproduction steps: charts are placed inside a TabControl + ScrollViewer
// and series data is assigned asynchronously (after a delay, simulating a server fetch).
public class ViewModel : INotifyPropertyChanged
{
    private IEnumerable<ISeries> _cartesianA = [];
    private IEnumerable<ISeries> _cartesianB = [];
    private IEnumerable<ISeries> _cartesianC = [];
    private IEnumerable<ISeries> _pieA = [];
    private IEnumerable<ISeries> _pieB = [];

    public IEnumerable<ISeries> CartesianA { get => _cartesianA; private set { _cartesianA = value; OnPropertyChanged(); } }
    public IEnumerable<ISeries> CartesianB { get => _cartesianB; private set { _cartesianB = value; OnPropertyChanged(); } }
    public IEnumerable<ISeries> CartesianC { get => _cartesianC; private set { _cartesianC = value; OnPropertyChanged(); } }
    public IEnumerable<ISeries> PieA { get => _pieA; private set { _pieA = value; OnPropertyChanged(); } }
    public IEnumerable<ISeries> PieB { get => _pieB; private set { _pieB = value; OnPropertyChanged(); } }

    public async Task LoadDataAsync(int delayMs = 1000)
    {
        await Task.Delay(delayMs);

        CartesianA =
        [
            new LineSeries<double> { Values = [5, 10, 8, 4, 9, 6, 11] },
            new ColumnSeries<double> { Values = [3, 7, 4, 8, 2, 6, 5] }
        ];

        CartesianB =
        [
            new LineSeries<double> { Values = [8, 3, 6, 9, 4, 7, 5] }
        ];

        CartesianC =
        [
            new ColumnSeries<double> { Values = [6, 2, 8, 4, 9, 3, 7] }
        ];

        PieA =
        [
            new PieSeries<double> { Values = [4] },
            new PieSeries<double> { Values = [6] },
            new PieSeries<double> { Values = [3] }
        ];

        PieB =
        [
            new PieSeries<double> { Values = [10] },
            new PieSeries<double> { Values = [5] },
            new PieSeries<double> { Values = [7] }
        ];
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
