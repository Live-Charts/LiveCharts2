using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ViewModelsSamples.Axes.Paging;

public partial class ViewModel : ObservableObject
{
    public int[] Values { get; set; } = Fetch();

    [ObservableProperty]
    public partial double MinLimit { get; set; } = double.NaN;

    [ObservableProperty]
    public partial double MaxLimit { get; set; } = double.NaN;

    [RelayCommand]
    public void GoToPage1()
    {
        MinLimit = -0.5;
        MaxLimit = 10.5;
    }

    [RelayCommand]
    public void GoToPage2()
    {
        MinLimit = 9.5;
        MaxLimit = 20.5;
    }

    [RelayCommand]
    public void GoToPage3()
    {
        MinLimit = 19.5;
        MaxLimit = 30.5;
    }

    [RelayCommand]
    public void SeeAll()
    {
        MinLimit = double.NaN;
        MaxLimit = double.NaN;
    }

    private static int[] Fetch()
    {
        var random = new Random();

        var trend = 100;
        var values = new List<int>();
        for (var i = 0; i < 100; i++)
        {
            trend += random.Next(-30, 50);
            values.Add(trend);
        }

        return [.. values];
    }
}
