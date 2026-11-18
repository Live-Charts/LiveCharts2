using System.Reflection;
using Noesis;

namespace NoesisSample;

public class SampleView : UserControl
{
    public SampleView()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    protected override bool ConnectEvent(object source, string eventName, string handlerName)
    {
        var eventInfo = source.GetType().GetEvent(eventName);
        var handler = GetType().GetMethod(
            handlerName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (eventInfo?.EventHandlerType is null || handler is null)
        {
            return base.ConnectEvent(source, eventName, handlerName);
        }

        eventInfo.AddEventHandler(source, handler.CreateDelegate(eventInfo.EventHandlerType, this));

        return true;
    }

    private void OnLoaded(object sender, RoutedEventArgs args)
    {
        if (NoesisApp.Application.Current.MainWindow is MainWindow window)
        {
            window.ApplyChartTheme(this);
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs args)
    {
        if (DataContext is IDisposable disposable)
        {
            disposable.Dispose();
        }

        var reading = DataContext?.GetType().GetProperty("IsReading");

        if (reading?.PropertyType == typeof(bool) && reading.CanWrite)
        {
            reading.SetValue(DataContext, false);
        }
    }
}
