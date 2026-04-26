using System.ComponentModel;

namespace AvaloniaSample;

public class MainWindowViewModel : INotifyPropertyChanged
{
    public MainWindowViewModel()
    {
        Samples = [..
            ViewModelsSamples.Index.Samples,
            "VisualTest/Issue1986Repro"
        ];
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
