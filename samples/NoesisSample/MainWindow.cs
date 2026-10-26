using LiveChartsCore.Geo;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsGeneratedCode;
using Noesis;
using NoesisApp;

namespace NoesisSample;

public sealed class MainWindow : Window
{
    private readonly ListBox _sampleList;
    private readonly ContentControl _sampleContent;
    private readonly TextBlock _sampleTitle;
    private string _theme = "Light";

    public MainWindow()
    {
        GUI.LoadComponent(this, "/NoesisSample;component/MainWindow.xaml");

        _sampleList = (ListBox)FindName("SampleList");
        _sampleContent = (ContentControl)FindName("SampleContent");
        _sampleTitle = (TextBlock)FindName("SampleTitle");

        DataContext = this;

        _sampleList.SelectionChanged += OnSampleChanged;

        ((Button)FindName("ResetButton")).Click += OnResetExample;
        ((Button)FindName("ThemeButton")).Click += OnToggleTheme;

        var initial = Environment.GetEnvironmentVariable("LVC_SAMPLE") ?? SamplePaths[0];
        var index = Array.IndexOf(SamplePaths, initial);

        _sampleList.SelectedIndex = index < 0 ? 0 : index;
    }

    public string[] SamplePaths => ViewModelsSamples.Index.Samples;

    public string CurrentSample => (string)_sampleList.SelectedItem;

    public void ApplyTheme(string theme)
    {
        _theme = theme;

        var resources = Application.Current.Resources.MergedDictionaries;
        resources[0] = (ResourceDictionary)GUI.LoadXaml(
            $"/Noesis.GUI.Extensions;component/Theme/NoesisTheme.{theme}Blue.xaml");
        resources[1] = (ResourceDictionary)GUI.LoadXaml(
            $"/NoesisSample;component/Palette.{theme}.xaml");

        if (_sampleContent.Content is Visual view)
        {
            ApplyChartTheme(view);
        }
    }

    public void ApplyChartTheme(DependencyObject view)
    {
        if (view is SourceGenDrawnView chart)
        {
            chart.IsDarkMode = _theme == "Dark";
        }

        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(view); index++)
        {
            ApplyChartTheme(VisualTreeHelper.GetChild(view, index));
        }
    }

    public void SelectExample(int index)
    {
        _sampleList.SelectedIndex = index;
    }

    public void ResetExample()
    {
        ShowSample(CurrentSample);
    }

    public void VerifyCurrentExample()
    {
        if (_sampleContent.Content is not Visual view)
        {
            throw new InvalidOperationException($"Sample '{CurrentSample}' has no view.");
        }

        var charts = FindCharts(view).ToArray();

        foreach (var chart in charts)
        {
            if (chart is IGeoMapView map)
            {
                for (var x = 1; x < 10; x++)
                {
                    for (var y = 1; y < 6; y++)
                    {
                        _ = chart.GetPointsAt(new LiveChartsCore.Drawing.LvcPointD(map.ControlSize.Width * x / 10, map.ControlSize.Height * y / 6)).ToArray();
                    }
                }
            }

            if (chart.CoreCanvas.CountGeometries() == 0)
            {
                throw new InvalidOperationException($"Sample '{CurrentSample}' rendered no geometries.");
            }
        }

        Console.WriteLine($"Verified {CurrentSample}: {charts.Length} chart(s).");
    }

    private void ShowSample(string sample)
    {
        var viewType = GetType().Assembly.GetType($"NoesisSample.{sample.Replace('/', '.')}.View")
            ?? throw new InvalidOperationException($"No Noesis view exists for '{sample}'.");

        _sampleContent.Content = Activator.CreateInstance(viewType);
        _sampleTitle.Text = sample;
    }

    private static IEnumerable<IChartView> FindCharts(DependencyObject view)
    {
        if (view is IChartView chart)
        {
            yield return chart;
        }

        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(view); index++)
        {
            foreach (var child in FindCharts(VisualTreeHelper.GetChild(view, index)))
            {
                yield return child;
            }
        }
    }

    private void OnSampleChanged(object sender, SelectionChangedEventArgs args)
    {
        if (_sampleList.SelectedItem is string sample)
        {
            ShowSample(sample);
        }
    }

    private void OnResetExample(object sender, RoutedEventArgs args)
    {
        ResetExample();
    }

    private void OnToggleTheme(object sender, RoutedEventArgs args)
    {
        ApplyTheme(_theme == "Dark" ? "Light" : "Dark");
    }
}
