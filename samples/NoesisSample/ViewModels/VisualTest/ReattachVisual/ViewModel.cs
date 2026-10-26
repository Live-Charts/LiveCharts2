using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.Noesis;
using LiveChartsCore.Noesis.Painting;
using LiveChartsCore.Drawing;

namespace ViewModelsSamples.VisualTest.ReattachVisual;

public partial class ViewModel : ObservableObject
{
    public ICollection<ISeries> Series { get; set; } = new ObservableCollection<ISeries>
    {
        new ColumnSeries<double>
        {
            Values = new ObservableCollection<double> { 2, 5, 4, -2, 4, -3, 5 }
        }
    };

    public IEnumerable<ISeries> PieSeries { get; set; } = new ObservableCollection<ISeries>
    {
        new PieSeries<double> { Values = new ObservableCollection<double> { 5 } },
        new PieSeries<double> { Values = new ObservableCollection<double> { 4 } },
        new PieSeries<double> { Values = new ObservableCollection<double> { 3 } }
    };

    public IEnumerable<ISeries> PolarSeries { get; set; } = new ObservableCollection<ISeries>
    {
        new PolarLineSeries<double>
        {
            Values = new ObservableCollection<double> { 2, 5, 4, -2, 4, -3, 5 }
        }
    };

    public IEnumerable<IChartElement> Sections { get; set; } = new ObservableCollection<IChartElement>
    {
        new RectangularSection()
        {
            Xi = 0,
            Xj = 4,
            Fill = new SolidColorPaint(DrawingColors.LightGray)
        }
    };
}
