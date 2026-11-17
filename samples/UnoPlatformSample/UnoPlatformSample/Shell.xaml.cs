namespace UnoPlatformSample;

// Minimal navigation shell used only by Factos UI-test builds (UITesting=true). It exposes a
// ContentControl region (via IContentControlProvider) that Uno.Extensions navigation mounts the
// root route into; Factos registers its FactosShell there. The lean WASM showcase never uses this
// type — it is excluded from non-UI-test builds in UnoPlatformSample.csproj.
public sealed partial class Shell : UserControl, IContentControlProvider
{
    public Shell()
    {
        this.InitializeComponent();
    }

    public ContentControl ContentControl => Splash;
}
