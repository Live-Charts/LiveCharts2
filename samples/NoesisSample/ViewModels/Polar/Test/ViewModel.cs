using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.Noesis;
using LiveChartsCore.Noesis.Painting;
using LiveChartsCore.Drawing;

namespace ViewModelsSamples.Polar.Test;

public class ViewModel : INotifyPropertyChanged
{
    private bool _fitToBounds = false;
    private double _initialRotation = 15;
    private double _innerRadius = 50;
    private double _totalAngle = 360;
    private double _labelsAngle = -60;

    public IEnumerable<ISeries> Series { get; set; } = new ObservableCollection<ISeries>
    {
        new PolarLineSeries<double>
        {
            Values = new ObservableCollection<double> { 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 },
            ShowDataLabels = true,
            GeometrySize = 30,
            DataLabelsSize = 15,
            DataLabelsPosition = PolarLabelsPosition.Middle,
            DataLabelsRotation = LiveCharts.CotangentAngle,
            IsClosed = true
        }
    };

    public PolarAxis[] RadialAxes { get; set; } =
    [
        new PolarAxis
        {
            LabelsAngle = -60,
            MaxLimit = 30 // null to let the chart autoscale (defualt is null) // mark
        }
    ];

    public PolarAxis[] AngleAxes { get; set; } =
    [
        new PolarAxis
        {
            LabelsRotation = LiveCharts.TangentAngle
        }
    ];

    public bool FitToBounds
    {
        get => _fitToBounds;
        set
        {
            _fitToBounds = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FitToBounds)));
        }
    }

    public double InitialRotation
    {
        get => _initialRotation;
        set
        {
            _initialRotation = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InitialRotation)));
        }
    }

    public double InnerRadius
    {
        get => _innerRadius;
        set
        {
            _innerRadius = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InnerRadius)));
        }
    }

    public double TotalAngle
    {
        get => _totalAngle;
        set
        {
            _totalAngle = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalAngle)));
        }
    }

    public double LabelsAngle
    {
        get => _labelsAngle;
        set
        {
            _labelsAngle = value;
            RadialAxes[0].LabelsAngle = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LabelsAngle)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
