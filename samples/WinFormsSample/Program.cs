using System;
using System.Windows.Forms;
using LiveChartsCore; // mark
using ViewModelsSamples;

namespace WinFormsSample;

static class Program
{
    [STAThread]
    static void Main()
    {
        // LiveCharts configuration section: // mark
        LiveCharts.Configure(c => c // mark
            .AddLiveChartsAppSettings()); // mark

        //_ = Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        var form = new Form1();

#if UI_TESTING
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(Factos.SGTests).TypeHandle);
        Factos.WinForms.SetupExtensions.UseFactosApp(form);
#endif

        Application.Run(form);
    }
}
