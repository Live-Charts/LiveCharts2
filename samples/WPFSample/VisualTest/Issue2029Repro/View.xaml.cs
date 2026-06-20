using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace WPFSample.VisualTest.Issue2029Repro;

public partial class View : UserControl
{
    public View()
    {
        InitializeComponent();

        // Drive the repro automatically: TabItem1 → TabItem2 → TabItem1.
        // Reporter's full flow on 2.0.0-rc6.1 (closed as dup of #2049):
        //   * step 1 — TabItem2 reveals an abnormally large/blurry chart
        //     because the not-yet-selected MotionCanvas got Loaded at
        //     size 0 and then Loaded again with no Unloaded in between,
        //     so the WPF render-mode paint handler ended up subscribed
        //     twice and Canvas.Scale(DPI) was applied twice per frame
        //     (fixed by #2070 was only the NRE — this is the scale bug
        //     #2029 keeps complaining about).
        //   * step 2 — switching back to TabItem1 used to NRE inside
        //     CompositionTargetTicker.OnCompositonTargetRendering
        //     (fixed by #2070).
        Loaded += (_, _) =>
        {
            _ = Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() => tabControl.SelectedIndex = 1));
            _ = Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() => tabControl.SelectedIndex = 0));
        };
    }
}
