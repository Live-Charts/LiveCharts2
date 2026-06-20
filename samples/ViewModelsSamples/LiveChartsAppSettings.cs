using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;

namespace ViewModelsSamples;

public static partial class CustomLiveChartsExtensions
{
    public static LiveChartsSettings AddLiveChartsAppSettings(this LiveChartsSettings settings) =>
        settings
            // use skia sharp to draw
            .AddSkiaSharp()

            // register mappers for primitives and LiveCharts objects
            .AddDefaultMappers()

            // adds a theme that is light or dark depending on the OS theme;
            // OS theme awareness is not supported in WPF, WinForms and ETO.
            // themes registered here are handled by livecharts and can be used
            // across all the supported UI frameworks, but you could also
            // use XAML based themes if your platform supports them.
            .AddDefaultTheme(
                // if neccessary, override the default theme, for more info see:
                // https://github.com/beto-rodriguez/LiveCharts2/blob/master/samples/ViewModelsSamples/LiveChartsThemeExtensions.cs

                //theme =>
                //    theme.OnInitialized(() =>
                //    {
                //        theme.AnimationsSpeed = TimeSpan.FromSeconds(1);
                //        theme.EasingFunction = EasingFunctions.BounceOut;
                //        theme.Colors = theme.IsDark
                //            ? ColorPalletes.MaterialDesign200
                //            : ColorPalletes.MaterialDesign800;
                //    })
            )

            // loads the recommended settings, this is equivalent to calling
            // .AddSkiaSharp().AddDefaultMappers().AddDefaultTheme()
            .UseDefaults()

            // register your own mappers, this is useful to use custom objects
            // in your plots, for example the next line registers a mapper for the
            // City record, this mapper will be used to convert the City to a coordinate.
            // you can learn more about mappers at:
            // https://livecharts.dev/docs/{{ platform }}/{{ version }}/Overview.Mappers
            .HasMap<City>(
                // the maper takes the City and its index in the collection,
                // and returns a coordinate, in this case we are using the
                // City.Population as Y and the index as the X.
                (city, index) => new(index, city.Population))

            // RTL (right-to-left) shaping is handled already in the library by Harfbuzz,
            // but we can enable RTL to force RTL tooltips and legends.
            // RTL settings are enabled when the system culture is RTL,
            // but you can also force the use of RTL settings by calling:
            //.UseRightToLeftSettings()
        ;
}
