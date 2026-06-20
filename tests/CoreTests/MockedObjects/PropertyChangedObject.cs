using System.ComponentModel;

namespace CoreTests.MockedObjects;

public class PropertyChangedObject : INotifyPropertyChanged
{
    private double value;

    public double Value { get => value; set { this.value = value; OnPropertyChanged(nameof(Value)); } }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
