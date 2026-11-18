using System;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Noesis;

namespace ViewModelsSamples.VisualTest.TwoChartsOneSeries;

public partial class ViewModel : ObservableObject
{
    public ViewModel()
    {
        //var values = new int[100];
        //var r = new Random();
        //var t = 0;

        //for (var i = 0; i < 100; i++)
        //{
        //    t += r.Next(-90, 100);
        //    values[i] = t;
        //}

        //Series = new ISeries[] { new StepLineSeries<int> { Values = values } };

        var values = new ObservableValue[100];
        var r = new Random();
        var t = 0;

        for (var i = 0; i < 100; i++)
        {
            t += r.Next(-90, 100);
            values[i] = new(t);
        }

        Series = [new StepLineSeries<ObservableValue> { Values = values }];
    }

    public ISeries[] Series { get; set; }
}
