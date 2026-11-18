using LiveChartsCore;
using LiveChartsCore.Noesis.Drawing.Geometries;
using LiveChartsCore.Noesis.Painting;
using LiveChartsCore.Themes;
using LiveChartsCore.VisualElements;
using LiveChartsCore.Drawing;

namespace ViewModelsSamples.General.VisualElements;

public class ThemedVisual : Visual
{
    private object _themeId = new();
    private static bool _isThemeRegistered;

    protected override RectangleGeometry DrawnElement { get; } =
        new RectangleGeometry();

    protected override void Measure(Chart chart)
    {
        if (!_isThemeRegistered) RegisterTheme();

        ApplyTheme(chart.GetTheme());

        DrawnElement.X = 600;
        DrawnElement.Y = 100;
        DrawnElement.Width = 40;
        DrawnElement.Height = 40;
    }

    private void ApplyTheme(Theme theme)
    {
        if (theme.ThemeId == _themeId) return;

        theme.ApplyStyleTo<ThemedVisual>(this);
        _themeId = theme.ThemeId;
    }

    private void RegisterTheme()
    {
        // now lets define the default style for the visual
        // ideally you should do this in the application startup

        var theme = LiveCharts.DefaultSettings.GetTheme();

        theme.HasRuleFor<ThemedVisual>(visual =>
        {
            visual.DrawnElement.Fill =
                theme.IsDark
                    ? new SolidColorPaint(DrawingColors.White)
                    : new SolidColorPaint(DrawingColors.Black);
        });

        _isThemeRegistered = true;
    }
}
