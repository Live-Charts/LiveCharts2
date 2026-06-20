// The MIT License(MIT)
//
// Copyright(c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.Motion;
using LiveChartsCore.Painting;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.TypeConverters;
using SkiaSharp;

namespace LiveChartsCore.SkiaSharpView;

/// <summary>
/// Defines the default LiveCharts-SkiaSharp settings
/// </summary>
public static class LiveChartsSkiaSharp
{
    internal static MotionCanvasComposer.MotionCanvasRenderingFactoryDelegate MotionCanvasRenderingFactory { get; set; } =
        (settings) => throw new NotImplementedException(
            "No motion canvas rendering factory has been set, please use the method 'HasMotionCanvasRenderingFactory' to set one.");

    internal static TextSettings DefaultTextSettings { get; set; } = new();

    internal static LiveChartsSettings EnsureInitialized()
    {
        LiveCharts.Configure(settings => settings.UseDefaults());

        var defaultRenderSettings = LiveCharts.RenderingSettings;

#if __GPU_TRUE__
        defaultRenderSettings.UseGPU = true;
#endif
#if __GPU_FALSE__
        defaultRenderSettings.UseGPU = false;
#endif
#if __VSYNC_TRUE__
        defaultRenderSettings.TryUseVSync = true;
#endif
#if __VSYNC_FALSE__
        defaultRenderSettings.TryUseVSync = false;
#endif
#if __DIAGNOSE__
        defaultRenderSettings.ShowFPS = true;
#endif

        return LiveCharts.DefaultSettings;
    }

    /// <summary>
    /// Configures LiveCharts using the default settings for SkiaSharp.
    /// </summary>
    /// <param name="settings">The settings.</param>
    /// <returns>The settings.</returns>
    public static LiveChartsSettings UseDefaults(this LiveChartsSettings settings)
    {
        if (!LiveCharts.DefaultSettings.HasBackedDefined)
            _ = settings.AddSkiaSharp();

        if (!LiveCharts.DefaultSettings.HasThemeDefined)
            _ = settings.AddDefaultTheme();

        if (!LiveCharts.DefaultSettings.HasMappersDefined)
            _ = settings.AddDefaultMappers();

        return settings;
    }

    /// <summary>
    /// Adds SkiaSharp as the library backend.
    /// </summary>
    /// <param name="settings">The settings.</param>
    /// <returns>The current settings.</returns>
    public static LiveChartsSettings AddSkiaSharp(this LiveChartsSettings settings)
    {
        PropertyDefinition.Parsers[typeof(Paint)] = HexToPaintTypeConverter.Parse;
        PropertyDefinition.Parsers[typeof(LvcColor)] = HexToLvcColorTypeConverter.Parse;
        PropertyDefinition.Parsers[typeof(Margin)] = MarginTypeConverter.ParseMargin;
        PropertyDefinition.Parsers[typeof(Padding)] = PaddingTypeConverter.ParsePadding;
        PropertyDefinition.Parsers[typeof(LvcPointD)] = PointDTypeConverter.ParsePoint;
        PropertyDefinition.Parsers[typeof(LvcPoint)] = PointTypeConverter.ParsePoint;

        return settings.HasProvider(new SkiaSharpProvider());
    }

    /// <summary>
    /// Registers a global SKTypeface instance to use on any <see cref="SkiaPaint"/> that does not specify a typeface.
    /// </summary>
    /// <param name="settings">The current settings.</param>
    /// <param name="typeface">The typeface to load for text paints.</param>
    /// <returns>The current settings.</returns>
    [Obsolete(
        $"Use {nameof(HasTextSettings)} and set the {nameof(TextSettings.DefaultTypeface)} instead, " +
        $"LiveCharts now uses the {nameof(SKFontManager)} to look for a valid font when necessesary, " +
        $"Configuring a typeface could not be necessary anymore, but explicitly loading a typeface " +
        $"could improve text quality, e.g. SKTypeface.FromFamilyName(\"my-font\")")]
    public static LiveChartsSettings HasGlobalSKTypeface(this LiveChartsSettings settings, SKTypeface typeface)
    {
        DefaultTextSettings.DefaultTypeface = typeface;
        return settings;
    }

    /// <summary>
    /// Registers the text settings to use for SkiaSharp.
    /// </summary>
    /// <param name="settings">The current settings.</param>
    /// <param name="textSettings">The text settings to use for SkiaSharp text rendering.</param>
    /// <returns>The current settings.</returns>
    public static LiveChartsSettings HasTextSettings(
        this LiveChartsSettings settings, TextSettings textSettings)
    {
        DefaultTextSettings = textSettings;
        return settings;
    }

    /// <summary>
    /// Adds a render mode to the available render modes.
    /// </summary>
    /// <param name="settings">The current settings.</param>
    /// <param name="factory">The rendering factory.</param>
    /// <returns>The current settings.</returns>
    public static LiveChartsSettings HasRenderingFactory(
        this LiveChartsSettings settings, MotionCanvasComposer.MotionCanvasRenderingFactoryDelegate factory)
    {
        MotionCanvasRenderingFactory = factory;
        return settings;
    }

    /// <summary>
    /// Converts a <see cref="LvcColor"/> to a <see cref="SKColor"/> instance.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <param name="alphaOverrides">The alpha overrides.</param>
    /// <returns></returns>
    public static SKColor AsSKColor(this LvcColor color, byte? alphaOverrides = null) =>
        color == LvcColor.Empty
            ? SKColor.Empty
            : new(color.R, color.G, color.B, alphaOverrides ?? color.A);

    /// <summary>
    /// Creates a new color based on the 
    /// </summary>
    /// <param name="color">The color.</param>
    /// <param name="opacity">The opacity from 0 to 255.</param>
    /// <returns></returns>
    public static LvcColor WithOpacity(this LvcColor color, byte opacity) =>
        LvcColor.FromArgb(opacity, color);

    /// <summary>
    /// Converts a <see cref="SKColor"/> to a <see cref="LvcColor"/> intance.
    /// </summary>
    /// <param name="color">The color</param>
    /// <returns></returns>
    public static LvcColor AsLvcColor(this SKColor color) =>
        new(color.Red, color.Green, color.Blue, color.Alpha);

    /// <summary>
    /// Calculates the distance in pixels from the target <see cref="ChartPoint"/> to the given location in the UI.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="location">The location.</param>
    /// <returns>The distance in pixels.</returns>
    public static double GetDistanceTo(this ChartPoint target, LvcPoint location)
    {
        LvcPointD dataCoordinates;
        double x, y;

        if (target.Context is ICartesianChartView cartesianChart)
        {
            dataCoordinates = cartesianChart.ScalePixelsToData(new LvcPointD(location));

            var cartesianSeries = (ICartesianSeries)target.Context.Series;

            if (target.Context.Series.SeriesProperties.HasFlag(SeriesProperties.PrimaryAxisHorizontalOrientation))
            {
                var primaryAxis = cartesianChart.Core.YAxes[cartesianSeries.ScalesYAt];
                var secondaryAxis = cartesianChart.Core.XAxes[cartesianSeries.ScalesXAt];

                var drawLocation = cartesianChart.Core.DrawMarginLocation;
                var drawMarginSize = cartesianChart.Core.DrawMarginSize;
                var secondaryScale = new Scaler(drawLocation, drawMarginSize, primaryAxis);
                var primaryScale = new Scaler(drawLocation, drawMarginSize, secondaryAxis);

                var coordinate = target.Coordinate;

                x = secondaryScale.ToPixels(coordinate.SecondaryValue);
                y = primaryScale.ToPixels(coordinate.PrimaryValue);
            }
            else
            {
                var primaryAxis = cartesianChart.Core.YAxes[cartesianSeries.ScalesXAt];
                var secondaryAxis = cartesianChart.Core.XAxes[cartesianSeries.ScalesYAt];

                var drawLocation = cartesianChart.Core.DrawMarginLocation;
                var drawMarginSize = cartesianChart.Core.DrawMarginSize;

                var secondaryScale = new Scaler(drawLocation, drawMarginSize, secondaryAxis);
                var primaryScale = new Scaler(drawLocation, drawMarginSize, primaryAxis);

                var coordinate = target.Coordinate;

                x = secondaryScale.ToPixels(coordinate.SecondaryValue);
                y = primaryScale.ToPixels(coordinate.PrimaryValue);
            }
        }
        else if (target.Context is IPolarChartView polarChart)
        {
            dataCoordinates = polarChart.ScalePixelsToData(new LvcPointD(location));

            var polarSeries = (IPolarSeries)target.Context.Series;

            var angleAxis = polarChart.Core.AngleAxes[polarSeries.ScalesAngleAt];
            var radiusAxis = polarChart.Core.RadiusAxes[polarSeries.ScalesRadiusAt];

            var drawLocation = polarChart.Core.DrawMarginLocation;
            var drawMarginSize = polarChart.Core.DrawMarginSize;

            var scaler = new PolarScaler(
                drawLocation, drawMarginSize, angleAxis, radiusAxis,
                polarChart.Core.InnerRadius, polarChart.Core.InitialRotation, polarChart.Core.TotalAnge);

            var scaled = scaler.ToPixels(target);
            x = scaled.X;
            y = scaled.Y;
        }
        else
        {
            throw new NotImplementedException();
        }

        // calculate the distance
        var dx = dataCoordinates.X - x;
        var dy = dataCoordinates.Y - y;

        var distance = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));

        return distance;
    }
}
