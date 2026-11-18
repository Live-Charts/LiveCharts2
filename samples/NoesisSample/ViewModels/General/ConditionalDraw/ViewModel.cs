using System;
using System.Collections.ObjectModel;
using LiveChartsCore.Defaults;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore.Kernel;
using System.Threading.Tasks;

namespace ViewModelsSamples.General.ConditionalDraw;

public partial class ViewModel
{
    public ViewModel()
    {
        Randomize();
    }

    public ObservableCollection<ObservableValue> Values { get; set; } =
        [
            new(2),
            new(3),
            new(4)
        ];

    [RelayCommand]
    private void OnPointMeasured(ChartPoint point)
    {
        var ctx = point.Context;
        if (ctx.DataSource is not ObservableValue observable) return;

        var states = ctx.Series.VisualStates;

        if (observable.Value > 5)
        {
            states.SetState("Danger", ctx.Visual);
            states.SetState("LabelDanger", ctx.Label);
        }
        else
        {
            states.ClearState("Danger", ctx.Visual);
            states.ClearState("LabelDanger", ctx.Label);
        }
    }

    private async void Randomize()
    {
        var r = new Random();

        while (true)
        {
            await Task.Delay(3000);

            foreach (var item in Values)
            {
                item.Value = r.Next(0, 10);
            }
        }
    }
}
