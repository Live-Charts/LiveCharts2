using System;
using Eto.Forms;
using LiveChartsCore; // mark
using ViewModelsSamples;

#if UI_TESTING
using Factos.EtoForms;
#endif

namespace EtoFormsSample;

static class Program
{
    [STAThread]
    static void Main()
    {
        // LiveCharts configuration section: // mark
        LiveCharts.Configure(c => c // mark
            .AddLiveChartsAppSettings()); // mark

        var platform = Eto.Platform.Detect;

        var form = new Form1();

#if UI_TESTING
        form.UseFactosApp();
#endif

        new Application(platform).Run(form);
    }
}
