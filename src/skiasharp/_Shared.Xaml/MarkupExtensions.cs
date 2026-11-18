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

using System;
using System.Collections.Generic;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Painting;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using LiveChartsCore.SkiaSharpView.Painting.ImageFilters;
using LiveChartsCore.SkiaSharpView.TypeConverters;
using SkiaSharp;

#if AVALONIA_LVC
using Extension = Avalonia.Markup.Xaml.MarkupExtension;
namespace LiveChartsCore.SkiaSharpView.Avalonia;
#elif MAUI_LVC
using Microsoft.Maui.Controls;
using Extension = Microsoft.Maui.Controls.Xaml.IMarkupExtension;
namespace LiveChartsCore.SkiaSharpView.Maui;
#elif WINUI_LVC
using Extension = Microsoft.UI.Xaml.Markup.MarkupExtension;
namespace LiveChartsCore.SkiaSharpView.WinUI;
#elif NOESIS_LVC
using Extension = Noesis.MarkupExtension;
namespace LiveChartsCore.SkiaSharpView.Noesis;
#elif WPF_LVC
using Extension = System.Windows.Markup.MarkupExtension;
namespace LiveChartsCore.SkiaSharpView.WPF;
#endif

/// <summary>
/// The base LiveCharts extension.
/// </summary>
#if MAUI_LVC
[Microsoft.Maui.Controls.Xaml.AcceptEmptyServiceProvider]
#endif
public abstract class BaseExtension : Extension
{
    /// <summary>
    /// Gets the value to provide for the extension.
    /// </summary>
#if WINUI_LVC
    protected override object ProvideValue(Microsoft.UI.Xaml.IXamlServiceProvider serviceProvider)
#elif MAUI_LVC
    public object ProvideValue(IServiceProvider serviceProvider)
#else
    public override object ProvideValue(IServiceProvider serviceProvider)
#endif
    {
#pragma warning disable IDE0022 // Use expression body for method
        return OnValueRequested(serviceProvider);
#pragma warning restore IDE0022 // Use expression body for method
    }

    /// <summary>
    /// Gets the value to provide for the extension, <paramref name="serviceProvider"/> is
    /// the UI framework service provider, in case required use #IF directives to cast it
    /// to the UI framework type.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns></returns>
    protected abstract object OnValueRequested(object serviceProvider);
}

/// <summary>
/// The base skia paint extension.
/// </summary>
public abstract class BaseSkiaPaintExtention : BaseExtension
{
    /// <summary>
    /// Gets or sets a value indicating whether the paint should be antialias.
    /// </summary>
    public bool IsAntialias { get; set; } = true;

    /// <summary>
    /// Gets or sets the stroke width.
    /// </summary>
    public double StrokeWidth { get; set; } = 1f;

    /// <summary>
    /// Gets or sets the stroke cap.
    /// </summary>
    public SKStrokeCap StrokeCap { get; set; }

    /// <summary>
    /// Gets or sets the stroke miter.
    /// </summary>
    public double StrokeMiter { get; set; }

    /// <summary>
    /// Gets or sets the stroke join.
    /// </summary>
    public SKStrokeJoin StrokeJoin { get; set; }

    /// <summary>
    /// Gets or sets the image filter.
    /// </summary>
    public ImageFilter? ImageFilter { get; set; } = null;

    /// <summary>
    /// Gets or sets the path effect.
    /// </summary>
    public PathEffect? PathEffect { get; set; } = null;

    /// <summary>
    /// Gets or sets the font family.
    /// </summary>
    public string? FontFamily { get; set; }

    /// <summary>
    /// Gets or sets the font weight.
    /// </summary>
    public SKFontStyleWeight FontWeight { get; set; } = SKFontStyleWeight.Normal;

    /// <summary>
    /// Gets or sets the font width.
    /// </summary>
    public SKFontStyleWidth FontWidth { get; set; } = SKFontStyleWidth.Normal;

    /// <summary>
    /// Gets or sets the font slant.
    /// </summary>
    public SKFontStyleSlant FontSlant { get; set; } = SKFontStyleSlant.Upright;

    /// <summary>
    /// Gets or sets a character to match the typeface to use.
    /// </summary>
    public string? TypefaceMatchesChar { get; set; } = null;

    /// <summary>
    /// Maps the properties to the paint.
    /// </summary>
    /// <param name="paint">The paint.</param>
    protected void MapProperties(SkiaPaint paint)
    {
        paint.IsAntialias = IsAntialias;
        paint.StrokeThickness = (float)StrokeWidth;
        paint.StrokeCap = StrokeCap;
        paint.StrokeMiter = (float)StrokeMiter;
        paint.StrokeJoin = StrokeJoin;
        paint.ImageFilter = ImageFilter;
        paint.PathEffect = PathEffect;

#pragma warning disable CS0618 // Type or member is obsolete
        if (FontWeight != SKFontStyleWeight.Normal || FontWidth != SKFontStyleWidth.Normal || FontSlant != SKFontStyleSlant.Upright)
            paint.SKFontStyle = new SKFontStyle(FontWeight, FontWidth, FontSlant);
#pragma warning restore CS0618 // Type or member is obsolete

        if (FontFamily is not null)
        {
            paint.SKTypeface = SKTypeface.FromFamilyName(FontFamily);
        }
        else if (TypefaceMatchesChar is not null && TypefaceMatchesChar.Length > 0)
        {
            paint.SKTypeface = SKFontManager.Default.MatchCharacter(TypefaceMatchesChar[0]);
        }
    }

    /// <summary>
    /// Parses the point.
    /// </summary>
    protected static SKPoint ParsePoint(string? point, SKPoint @default)
    {
        if (string.IsNullOrWhiteSpace(point)) return @default;

        var split = point!.Split(',');

        if (split.Length != 2) return @default;
#pragma warning disable IDE0046 // Convert to conditional expression
        if (!float.TryParse(split[0], out var x) || !float.TryParse(split[1], out var y)) return @default;
#pragma warning restore IDE0046 // Convert to conditional expression

        return new SKPoint(x, y);
    }
}

/// <summary>
/// The solid color paint extension.
/// </summary>
public class SolidColorPaintExtension : BaseSkiaPaintExtention
{
    /// <summary>
    /// Gets or sets the color, default value is #00000000, and means to use the default color, normally
    /// defines by the element being painted, for example the series color.
    /// </summary>
    public string? Color { get; set; } = "#00000000";

    /// <inheritdoc/>
    protected override object OnValueRequested(object serviceProvider)
    {
        if (!SKColor.TryParse(Color, out var color))
            return new SolidColorPaint(SKColors.Transparent);

        var paint = new SolidColorPaint(color);

        MapProperties(paint);

        return paint;
    }
}

/// <summary>
/// The linear gradient paint extension.
/// </summary>
public class LinearGradientPaintExtension : BaseSkiaPaintExtention
{
    /// <summary>
    /// Gets or sets the colors separated by commas, default value is #00000000, and means to use the default color, normally
    /// defines by the element being painted, for example the series color.
    /// </summary>
    public string? Colors { get; set; } = "#00000000";

    /// <summary>
    /// Gets or sets the start point.
    /// </summary>
    public string? StartPoint { get; set; } = null;

    /// <summary>
    /// Gets or sets the end point.
    /// </summary>
    public string? EndPoint { get; set; } = null;

    /// <summary>
    /// Gets or sets the colors positions separated by commas,
    /// values should be in the range of 0 to 1,
    /// these values indicate the relative positions of the colors, you can set that argument to null to equally
    /// space the colors, default is null.
    /// </summary>
    public string? ColorPositions { get; set; } = null;

    /// <summary>
    /// Gets or sets the tile mode.
    /// </summary>
    public SKShaderTileMode TileMode { get; set; } = SKShaderTileMode.Clamp;

    /// <inheritdoc/>
    protected override object OnValueRequested(object serviceProvider)
    {
        var colorsHexArray = Colors?.Split(',');
        if (colorsHexArray is null || colorsHexArray.Length == 0)
            return new LinearGradientPaint(SKColors.Transparent, SKColors.Transparent);

        var colors = new SKColor[colorsHexArray.Length];
        for (var i = 0; i < colors.Length; i++)
        {
            colors[i] = SKColor.TryParse(colorsHexArray[i], out var color)
                ? color
                : SKColors.Transparent;
        }

        if (colors.Length == 0)
            return new LinearGradientPaint(SKColors.Transparent, SKColors.Transparent);

        var start = ParsePoint(StartPoint, LinearGradientPaint.DefaultStartPoint);
        var end = ParsePoint(EndPoint, LinearGradientPaint.DefaultEndPoint);

        var colorPositionsSplit = ColorPositions?.Split(',');
        float[]? colorPositions = null;

        if (colorPositionsSplit is not null && colorPositionsSplit.Length > 0)
        {
            colorPositions = new float[colorPositionsSplit.Length];
            for (var i = 0; i < colorPositionsSplit.Length; i++)
            {
                colorPositions[i] = float.TryParse(colorPositionsSplit[i], out var value)
                    ? value
                    : 0f;
            }
        }

        var paint = new LinearGradientPaint([.. colors], start, end, colorPositions, TileMode);

        MapProperties(paint);

        return paint;
    }
}

/// <summary>
/// The radial gradient paint extension.
/// </summary>
public class RadialGradientPaintExtension : BaseSkiaPaintExtention
{
    /// <summary>
    /// Gets or sets the colors separated by commas, default value is #00000000, and means to use the default color, normally
    /// defines by the element being painted, for example the series color.
    /// </summary>
    public string? Colors { get; set; } = "#00000000";

    /// <summary>
    /// Gets or sets the center point.
    /// </summary>
    public string? Center { get; set; } = null;

    /// <summary>
    /// Gets or sets the radius.
    /// </summary>
    public double Radius { get; set; } = 0.5f;

    /// <summary>
    /// Gets or sets the colors positions separated by commas,
    /// values should be in the range of 0 to 1,
    /// these values indicate the relative positions of the colors, you can set that argument to null to equally
    /// space the colors, default is null.
    /// </summary>
    public string? ColorPositions { get; set; } = null;

    /// <summary>
    /// Gets or sets the tile mode.
    /// </summary>
    public SKShaderTileMode TileMode { get; set; } = SKShaderTileMode.Clamp;

    /// <inheritdoc/>
    protected override object OnValueRequested(object serviceProvider)
    {
        var colorsHexArray = Colors?.Split(',');
        if (colorsHexArray is null || colorsHexArray.Length == 0)
            return new RadialGradientPaint(SKColors.Transparent, SKColors.Transparent);

        var colors = new SKColor[colorsHexArray.Length];
        for (var i = 0; i < colors.Length; i++)
        {
            colors[i] = SKColor.TryParse(colorsHexArray[i], out var color)
                ? color
                : SKColors.Transparent;
        }

        if (colors.Length == 0)
            return new RadialGradientPaint(SKColors.Transparent, SKColors.Transparent);

        var center = ParsePoint(Center, new(0.5f, 0.5f));

        var colorPositionsSplit = ColorPositions?.Split(',');
        float[]? colorPositions = null;

        if (colorPositionsSplit is not null && colorPositionsSplit.Length > 0)
        {
            colorPositions = new float[colorPositionsSplit.Length];
            for (var i = 0; i < colorPositionsSplit.Length; i++)
            {
                colorPositions[i] = float.TryParse(colorPositionsSplit[i], out var value)
                    ? value
                    : 0f;
            }
        }

        var paint = new RadialGradientPaint([.. colors], center, (float)Radius, colorPositions, TileMode);

        MapProperties(paint);

        return paint;
    }
}

/// <summary>
/// The frame extension.
/// </summary>
public class FrameExtension : BaseExtension
{
    /// <summary>
    /// Gets or sets the fill.
    /// </summary>
    public Paint? Fill { get; set; }

    /// <summary>
    /// Gets or sets the stroke.
    /// </summary>
    public Paint? Stroke { get; set; }

    /// <inheritdoc/>
    protected override object OnValueRequested(object serviceProvider)
    {
        return new DrawMarginFrame
        {
            Fill = Fill,
            Stroke = Stroke
        };
    }
}

/// <summary>
/// The from shared axes extension.
/// </summary>
public class FromSharedAxesExtension : BaseExtension
{
    /// <summary>
    /// Gets or sets the pair instance.
    /// </summary>
    public SharedAxesPair? AxesPair { get; set; }

    /// <summary>
    /// Gets or sets the element.
    /// </summary>
    public PairElement Element { get; set; }

    /// <inheritdoc/>
    protected override object OnValueRequested(object serviceProvider)
    {
#pragma warning disable IDE0046 // Convert to conditional expression
        if (AxesPair is null || AxesPair.First is null || AxesPair.Second is null)
            return Array.Empty<ICartesianAxis>();
#pragma warning restore IDE0046 // Convert to conditional expression

        return Element switch
        {
            PairElement.First => [AxesPair.First],
            PairElement.Second => [AxesPair.Second],
            _ => Array.Empty<ICartesianAxis>()
        };
    }

    /// <summary>
    /// The shared axes pair elements.
    /// </summary>
    public enum PairElement
    {
        /// <summary>
        /// The first element.
        /// </summary>
        First,

        /// <summary>
        /// The second element.
        /// </summary>
        Second
    }
}

/// <summary>
/// The drop shadow extension.
/// </summary>
#if MAUI_LVC
[ContentProperty(nameof(Value))]
#endif
public class ShadowExtension : BaseExtension
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShadowExtension"/> class.
    /// </summary>
    public ShadowExtension() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ShadowExtension"/> class.
    /// </summary>
    /// <param name="stringFormat">The string format example: "2,2,8,8,#000" or "2,2,8,#000" or "2,8,#000".</param>
    public ShadowExtension(string stringFormat)
    {
        Value = stringFormat;
    }

    /// <summary>
    /// The Shadow in string format example: "2,2,8,8,#000" or "2,2,8,#000" or "2,8,#000".
    /// </summary>
    public string Value { get; set; } = "0,0,4,4,#000";

    /// <inheritdoc/>
    protected override object OnValueRequested(object serviceProvider)
    {
        float Dx = 0, Dy = 0, SigmaX = 0, SigmaY = 0;
        var Color = "#000";

        if (Value is not null)
        {
            var split = Value.Split(',');

            if (split.Length == 5)
            {
                Dx = float.TryParse(split[0], out var dx) ? dx : 0f;
                Dy = float.TryParse(split[1], out var dy) ? dy : 0f;
                SigmaX = float.TryParse(split[2], out var sx) ? sx : 0f;
                SigmaY = float.TryParse(split[3], out var sy) ? sy : 0f;
                Color = split[4];
            }

            if (split.Length == 4)
            {
                Dx = float.TryParse(split[0], out var dx) ? dx : 0f;
                Dy = float.TryParse(split[1], out var dy) ? dy : 0f;
                SigmaX = SigmaY = float.TryParse(split[2], out var sx) ? sx : 0f;
                Color = split[3];
            }

            if (split.Length == 3)
            {
                Dx = Dy = float.TryParse(split[0], out var dx) ? dx : 0f;
                SigmaX = SigmaY = float.TryParse(split[1], out var dy) ? dy : 0f;
                Color = split[2];
            }

            if (split.Length == 2)
            {
                Dx = Dy = float.TryParse(split[0], out var dx) ? dx : 0f;
                SigmaX = SigmaY = float.TryParse(split[1], out var dy) ? dy : 0f;
                Color = "#555";
            }
        }

        if (!LvcColor.TryParse(Color!, out var color))
            color = new(0, 0, 0);

        return new LvcDropShadow(Dx, Dy, SigmaX, SigmaY, color);
    }
}

/// <summary>
/// The dashed extension.
/// </summary>
public class DashedExtension : BaseExtension
{
    private static readonly float[] s_defaultDashes = [2, 2];

    /// <summary>
    /// Gets or sets the dash array separated by commas.
    /// </summary>
    public string? Array { get; set; } = null;

    /// <summary>
    /// Gets or sets the dash phase.
    /// </summary>
    public double Phase { get; set; } = 0f;

    /// <inheritdoc/>
    protected override object OnValueRequested(object serviceProvider)
    {
        if (Array is null) return new DashEffect(s_defaultDashes);

        var dashArray = Array.Split(',');
        var dashes = new List<float>(dashArray.Length);

        foreach (var dash in dashArray)
        {
            if (float.TryParse(dash, out var value))
                dashes.Add(value);
        }

        return new DashEffect(
            dashes.Count == 0
                ? s_defaultDashes
                : [.. dashes],
            (float)Phase);
    }
}

/// <summary>
/// The padding extension.
/// </summary>
#if MAUI_LVC
[ContentProperty(nameof(Value))]
#endif
public class PaddingExtension : BaseExtension
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PaddingExtension"/> class.
    /// </summary>
    public PaddingExtension() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PaddingExtension"/> class with a string format.
    /// </summary>
    /// <param name="stringFormat">The string format, example: "0,0" or "10,20,30,40".</param>
    public PaddingExtension(string stringFormat)
    {
        Value = stringFormat;
    }

    /// <summary>
    /// The value in string format, example: "0,0" or "10,20,30,40".
    /// </summary>
    public string Value { get; set; } = "0,0";

    /// <inheritdoc/>
    protected override object OnValueRequested(object serviceProvider) =>
        PaddingTypeConverter.ParsePadding(Value);
}

/// <summary>
/// The margin extension.
/// </summary>
#if MAUI_LVC
[ContentProperty(nameof(Value))]
#endif
public class MarginExtension : BaseExtension
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MarginExtension"/> class.
    /// </summary>
    public MarginExtension() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MarginExtension"/> class with a string format.
    /// </summary>
    /// <param name="stringFormat">The string format, example: "0,0" or "10,20,30,40".</param>
    public MarginExtension(string stringFormat)
    {
        Value = stringFormat;
    }

    /// <summary>
    /// The value in string format, example: "0,0" or "10,20,30,40".
    /// </summary>
    public string Value { get; set; } = "0,0";

    /// <inheritdoc/>
    protected override object OnValueRequested(object serviceProvider) =>
        MarginTypeConverter.ParseMargin(Value);
}

/// <summary>
/// The point extension.
/// </summary>
#if MAUI_LVC
[ContentProperty(nameof(Value))]
#endif
public class PointExtension : BaseExtension
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PointExtension"/> class.
    /// </summary>
    public PointExtension() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PointExtension"/> class with a string format.
    /// </summary>
    /// <param name="stringFormat">The string format, example: "0,0" or "10,20".</param>
    public PointExtension(string stringFormat)
    {
        Value = stringFormat;
    }

    /// <summary>
    /// The value in string format, example: "0,0" or "10,20".
    /// </summary>
    public string Value { get; set; } = "0,0";

    /// <inheritdoc/>
    protected override object OnValueRequested(object serviceProvider) =>
        PointTypeConverter.ParsePoint(Value);
}

/// <summary>
/// The color array extension.
/// </summary>
#if MAUI_LVC
[ContentProperty(nameof(Values))]
#endif
public class ColorArrayExtension : BaseExtension
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ColorArrayExtension"/> class.
    /// </summary>
    public ColorArrayExtension() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorArrayExtension"/> class with a string format.
    /// </summary>
    /// <param name="stringFormat">The string format, example: "#000,#FFF,#F00".</param>
    public ColorArrayExtension(string stringFormat)
    {
        Values = stringFormat;
    }

    /// <summary>
    /// Gets or sets the values in string format, example: "#000,#FFF,#F00".
    /// </summary>
    public string Values { get; set; } = "#000";

    /// <inheritdoc/>
    protected override object OnValueRequested(object serviceProvider) =>
        HexToLvcColorArrayTypeConverter.Parse(Values);
}

/// <summary>
/// The color extension.
/// </summary>
#if MAUI_LVC
[ContentProperty(nameof(Hex))]
#endif
public class ColorExtension : BaseExtension
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ColorExtension"/> class.
    /// </summary>
    public ColorExtension() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorExtension"/> class with a string format.
    /// </summary>
    /// <param name="stringFormat">The string format, example: "#00000000" or "#FFF" or "#F00".</param>
    public ColorExtension(string stringFormat)
    {
        Hex = stringFormat;
    }

    /// <summary>
    /// The color in hex format, default value is #00000000.
    /// </summary>
    public string Hex { get; set; } = "#00000000";

    /// <inheritdoc/>
    protected override object OnValueRequested(object serviceProvider)
    {
        LvcColor? response = LvcColor.TryParse(Hex, out var c)
            ? c
            : null;

        return response!;
    }
}

#if WINUI_LVC
/// <summary>
/// The float extension, becase WinUI parses numeric strings as double.
/// </summary>
public class FloatExtension : BaseExtension
{
    /// <summary>
    /// 
    /// </summary>
    public string Value { get; set; } = "0";

    /// <inheritdoc/>
    protected override object OnValueRequested(object serviceProvider) =>
        float.TryParse(Value, out var value) ? value : 0f;
}

#endif

#if WINUI_LVC || AVALONIA_LVC || NOESIS_LVC
// a hacked extension because winui and avalonia do not support typeconverters from string to List<double>
// at least I was not able to find how to do it... this extension is just to create simple charts,
// used in the first chart sample.

/// <summary>
/// The float extension, becase WinUI parses numeric strings as double.
/// </summary>
public class ValuesExtension : BaseExtension
{
    /// <summary>
    /// Gets or sets the collection, items separated by commas.
    /// </summary>
    public string? FromString { get; set; }

    /// <inheritdoc/>
    protected override object OnValueRequested(object serviceProvider)
    {
        if (FromString is null)
            return new List<double>();

        var split = FromString.Split(',');
        var list = new List<double>(split.Length);
        foreach (var item in split)
        {
            if (double.TryParse(item, out var value))
                list.Add(value);
        }
        return list;
    }
}
#endif
