using System;
using Noesis;
using ViewModelsSamples.General.MultiThreading2;

namespace NoesisSample.General.MultiThreading2;
public partial class View : SampleView
{
    public View()
    {
        GUI.LoadComponent(this, "/NoesisSample;component/General/MultiThreading2/View.xaml");

        var vm = new ViewModel(InvokeOnUIThread);
        DataContext = vm;
    }

    // this method takes another function as an argument.
    // the idea is that we are invoking the passed action in the UI thread
    // but the UI framework will let the view model how to do this.
    // we will pass the InvokeOnUIThread method to our view model so the view model knows how
    // to invoke an action in the UI thred.
    private void InvokeOnUIThread(Action action)
    {
        Dispatcher.Invoke(action);
    }
}
