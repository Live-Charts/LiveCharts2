using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using LiveChartsCore.Defaults;
using LiveChartsCore.Measure;
using LiveChartsCore.Noesis;
using LiveChartsCore.Noesis.Extensions;
using LiveChartsCore.Noesis.Painting;
using LiveChartsCore.Drawing;

namespace ViewModelsSamples.Pies.Gauge;

public class ViewModel : INotifyPropertyChanged
{
    private double _initialRotation;
    private double _maxAngle;
    private double _innerRadius = 100;
    private double _offsetRadius = 10;
    private double _backgroundInnerRadius = 100;
    private double _backgroundOffsetRadius = 20;

    public ViewModel()
    {
        _maxAngle = 270;
        GaugeTotal = 60;
        _initialRotation = 135;

        Series = GaugeGenerator.BuildSolidGauge(
            new GaugeItem(10, SetStyle),
            new GaugeItem(25, SetStyle),
            new GaugeItem(50, SetStyle),
            new GaugeItem(50, SetStyle),
            new GaugeItem(GaugeItem.Background, SetBackgroundStyle));
    }

    private void SetStyle(PieSeries<ObservableValue> series)
    {
        series.InnerRadius = InnerRadius;
        series.RelativeInnerRadius = OffsetRadius;
        series.RelativeOuterRadius = OffsetRadius;
        series.DataLabelsFormatter = point => point.Coordinate.PrimaryValue.ToString();
        series.DataLabelsPosition = PolarLabelsPosition.Start;
        series.DataLabelsSize = 30;
    }

    private void SetBackgroundStyle(PieSeries<ObservableValue> series)
    {
        series.Fill = new SolidColorPaint(new LvcColor(0, 0, 0, 10));
        series.InnerRadius = BackgroundInnerRadius;
        series.RelativeInnerRadius = BackgroundOffsetRadius;
        series.RelativeOuterRadius = BackgroundOffsetRadius;
    }

    public IEnumerable<PieSeries<ObservableValue>> Series { get; set; }

    public double GaugeTotal { get; set; }

    public double InitialRotation { get => _initialRotation; set { _initialRotation = value; OnPropertyChanged(); } }

    public double MaxAngle { get => _maxAngle; set { _maxAngle = value; OnPropertyChanged(); } }

    public double BackgroundInnerRadius
    {
        get => _backgroundInnerRadius;
        set
        {
            _backgroundInnerRadius = value;
            foreach (var item in Series.Where(x => x.IsFillSeries))
                item.InnerRadius = value;
            OnPropertyChanged();
        }
    }

    public double BackgroundOffsetRadius
    {
        get => _backgroundOffsetRadius;
        set
        {
            _backgroundOffsetRadius = value;
            foreach (var item in Series.Where(x => x.IsFillSeries))
            {
                item.RelativeInnerRadius = value;
                item.RelativeOuterRadius = value;
            }
            OnPropertyChanged();
        }
    }

    public double InnerRadius
    {
        get => _innerRadius;
        set
        {
            _innerRadius = value;
            foreach (var item in Series.Where(x => !x.IsFillSeries))
                item.InnerRadius = value;
            OnPropertyChanged();
        }
    }

    public double OffsetRadius
    {
        get => _offsetRadius;
        set
        {
            _offsetRadius = value;
            foreach (var item in Series.Where(x => !x.IsFillSeries))
            {
                item.RelativeInnerRadius = value;
                item.RelativeOuterRadius = value;
            }
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
