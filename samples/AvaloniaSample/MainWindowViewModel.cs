using System;
using System.ComponentModel;

namespace AvaloniaSample;

public class MainWindowViewModel : INotifyPropertyChanged
{
    public MainWindowViewModel()
    {
        Samples = ViewModelsSamples.Index.Samples;

        // Dev-loop hook: LVC_SAMPLE selects an initial sample by path
        // (e.g. LVC_SAMPLE=Lines/Basic). Lets agents/scripts
        // launch the app pointed at a specific repro without UI navigation.
        var initial = Environment.GetEnvironmentVariable("LVC_SAMPLE");
        if (!string.IsNullOrWhiteSpace(initial)
            && Type.GetType($"AvaloniaSample.{initial.Replace('/', '.')}.View") is not null)
        {
            if (Array.IndexOf(Samples, initial) < 0)
                Samples = [.. Samples, initial];
            SelectedSample = initial;
        }
    }

    public string[] Samples { get; set; }

    public string? SelectedSample
    {
        get => field;
        set
        {
            if (field == value) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSample)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
