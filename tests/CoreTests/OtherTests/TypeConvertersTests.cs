using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using LiveChartsCore.Drawing;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.TypeConverters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.OtherTests;

[TestClass]
public class TypeConvertersTests
{
    // The numeric converters (Margin/Padding/Point/PointD/Values/StringToDoubleArray)
    // call float.TryParse / double.TryParse without passing a CultureInfo, which means
    // they honor the current culture's decimal separator. Pin the culture for the
    // duration of this test class so inputs like "12.5" are interpreted the same way
    // on every machine (e.g. fr-FR uses ',' by default and would otherwise fail).
    private static CultureInfo? s_originalCulture;
    private static CultureInfo? s_originalUICulture;

    [ClassInitialize]
    public static void ClassInit(TestContext _)
    {
        s_originalCulture = Thread.CurrentThread.CurrentCulture;
        s_originalUICulture = Thread.CurrentThread.CurrentUICulture;
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
    }

    [ClassCleanup]
    public static void ClassCleanupMethod()
    {
        if (s_originalCulture is not null) Thread.CurrentThread.CurrentCulture = s_originalCulture;
        if (s_originalUICulture is not null) Thread.CurrentThread.CurrentUICulture = s_originalUICulture;
    }

    [TestMethod]
    public void HexToLvcColor_ConvertsKnownHexStrings()
    {
        var converter = new HexToLvcColorTypeConverter();

        Assert.IsTrue(converter.CanConvertFrom(null, typeof(string)));
        Assert.IsFalse(converter.CanConvertFrom(null, typeof(int)));

        var red = (LvcColor)converter.ConvertFrom(null, null, "#FF0000")!;
        Assert.AreEqual((byte)255, red.R);
        Assert.AreEqual((byte)0, red.G);
        Assert.AreEqual((byte)0, red.B);
        Assert.AreEqual((byte)255, red.A);

        var withAlpha = (LvcColor)converter.ConvertFrom(null, null, "#8000FF00")!;
        Assert.AreEqual((byte)128, withAlpha.A);
        Assert.AreEqual((byte)0, withAlpha.R);
        Assert.AreEqual((byte)255, withAlpha.G);
        Assert.AreEqual((byte)0, withAlpha.B);
    }

    [TestMethod]
    public void HexToLvcColor_ReturnsTransparentFallbackOnInvalidInput()
    {
        var converter = new HexToLvcColorTypeConverter();

        var fallback = (LvcColor)converter.ConvertFrom(null, null, "not-a-color")!;
        Assert.AreEqual((byte)0, fallback.R);
        Assert.AreEqual((byte)0, fallback.G);
        Assert.AreEqual((byte)0, fallback.B);
        Assert.AreEqual((byte)0, fallback.A);
    }

    [TestMethod]
    public void HexToLvcColorArray_ConvertsCommaSeparatedList()
    {
        var converter = new HexToLvcColorArrayTypeConverter();

        Assert.IsTrue(converter.CanConvertFrom(null, typeof(string)));

        var colors = (LvcColor[])converter.ConvertFrom(null, null, "#FF0000, #00FF00, #0000FF")!;
        Assert.AreEqual(3, colors.Length);
        Assert.AreEqual((byte)255, colors[0].R);
        Assert.AreEqual((byte)255, colors[1].G);
        Assert.AreEqual((byte)255, colors[2].B);
    }

    [TestMethod]
    public void HexToLvcColorArray_FallsBackToTransparentPerInvalidEntry()
    {
        var converter = new HexToLvcColorArrayTypeConverter();

        // "zzz" is not parseable as hex so the converter falls back to (0,0,0,0).
        var colors = (LvcColor[])converter.ConvertFrom(null, null, "#FF0000, zzz")!;
        Assert.AreEqual(2, colors.Length);
        Assert.AreEqual((byte)255, colors[0].R);
        Assert.AreEqual((byte)0, colors[1].A);
        Assert.AreEqual((byte)0, colors[1].R);
    }

    [TestMethod]
    public void HexToPaint_ConvertsHexToSolidColorPaint()
    {
        var converter = new HexToPaintTypeConverter();

        Assert.IsTrue(converter.CanConvertFrom(null, typeof(string)));

        var paint = (SolidColorPaint)converter.ConvertFrom(null, null, "#FF0000")!;
        Assert.AreEqual((byte)255, paint.Color.Red);
        Assert.AreEqual((byte)0, paint.Color.Green);
        Assert.AreEqual((byte)0, paint.Color.Blue);
    }

    [TestMethod]
    public void HexToPaint_ReturnsTransparentPaintOnInvalidInput()
    {
        var converter = new HexToPaintTypeConverter();

        var paint = (SolidColorPaint)converter.ConvertFrom(null, null, "not-a-color")!;
        Assert.AreEqual((byte)0, paint.Color.Alpha);
    }

    [TestMethod]
    public void Margin_SingleValueAppliesToAllSides()
    {
        var margin = (Margin)MarginTypeConverter.ParseMargin("5");
        Assert.AreEqual(5f, margin.Left);
        Assert.AreEqual(5f, margin.Top);
        Assert.AreEqual(5f, margin.Right);
        Assert.AreEqual(5f, margin.Bottom);
    }

    [TestMethod]
    public void Margin_TwoValuesApplyAsXAndY()
    {
        var margin = (Margin)MarginTypeConverter.ParseMargin("4,8");
        Assert.AreEqual(4f, margin.Left);
        Assert.AreEqual(8f, margin.Top);
        Assert.AreEqual(4f, margin.Right);
        Assert.AreEqual(8f, margin.Bottom);
    }

    [TestMethod]
    public void Margin_FourValuesApplyInLeftTopRightBottomOrder()
    {
        var margin = (Margin)MarginTypeConverter.ParseMargin("1,2,3,4");
        Assert.AreEqual(1f, margin.Left);
        Assert.AreEqual(2f, margin.Top);
        Assert.AreEqual(3f, margin.Right);
        Assert.AreEqual(4f, margin.Bottom);
    }

    [TestMethod]
    public void Margin_AutoKeywordMapsToMarginAuto()
    {
        var margin = (Margin)MarginTypeConverter.ParseMargin("auto");
        Assert.IsTrue(Margin.IsAuto(margin.Left));
        Assert.IsTrue(Margin.IsAuto(margin.Top));
        Assert.IsTrue(Margin.IsAuto(margin.Right));
        Assert.IsTrue(Margin.IsAuto(margin.Bottom));
    }

    [TestMethod]
    public void Margin_InvalidPartCountReturnsDefault()
    {
        var margin = (Margin)MarginTypeConverter.ParseMargin("1,2,3");
        Assert.AreEqual(0f, margin.Left);
        Assert.AreEqual(0f, margin.Top);
        Assert.AreEqual(0f, margin.Right);
        Assert.AreEqual(0f, margin.Bottom);
    }

    [TestMethod]
    public void Margin_ConvertFromDelegatesToParse()
    {
        var converter = new MarginTypeConverter();

        Assert.IsTrue(converter.CanConvertFrom(null, typeof(string)));

        var margin = (Margin)converter.ConvertFrom(null, null, "7")!;
        Assert.AreEqual(7f, margin.Top);
    }

    [TestMethod]
    public void Padding_OneTwoAndFourValueForms()
    {
        var one = (Padding)PaddingTypeConverter.ParsePadding("6");
        Assert.AreEqual(6f, one.Left);
        Assert.AreEqual(6f, one.Top);
        Assert.AreEqual(6f, one.Right);
        Assert.AreEqual(6f, one.Bottom);

        var two = (Padding)PaddingTypeConverter.ParsePadding("3, 4");
        Assert.AreEqual(3f, two.Left);
        Assert.AreEqual(4f, two.Top);
        Assert.AreEqual(3f, two.Right);
        Assert.AreEqual(4f, two.Bottom);

        var four = (Padding)PaddingTypeConverter.ParsePadding("1,2,3,4");
        Assert.AreEqual(1f, four.Left);
        Assert.AreEqual(2f, four.Top);
        Assert.AreEqual(3f, four.Right);
        Assert.AreEqual(4f, four.Bottom);
    }

    [TestMethod]
    public void Padding_InvalidPartCountReturnsDefaultZero()
    {
        var padding = (Padding)PaddingTypeConverter.ParsePadding("1,2,3");
        Assert.AreEqual(0f, padding.Left);
        Assert.AreEqual(0f, padding.Top);
        Assert.AreEqual(0f, padding.Right);
        Assert.AreEqual(0f, padding.Bottom);
    }

    [TestMethod]
    public void Padding_ConvertFromDelegatesToParse()
    {
        var converter = new PaddingTypeConverter();
        Assert.IsTrue(converter.CanConvertFrom(null, typeof(string)));

        var padding = (Padding)converter.ConvertFrom(null, null, "9")!;
        Assert.AreEqual(9f, padding.Left);
    }

    [TestMethod]
    public void PointD_SingleValueDuplicatesToXAndY()
    {
        var converter = new PointDTypeConverter();
        Assert.IsTrue(converter.CanConvertFrom(null, typeof(string)));

        var point = (LvcPointD)converter.ConvertFrom(null, null, "12.5")!;
        Assert.AreEqual(12.5d, point.X);
        Assert.AreEqual(12.5d, point.Y);
    }

    [TestMethod]
    public void PointD_TwoValuesMapToXAndY()
    {
        var converter = new PointDTypeConverter();
        var point = (LvcPointD)converter.ConvertFrom(null, null, "3, 7")!;
        Assert.AreEqual(3d, point.X);
        Assert.AreEqual(7d, point.Y);
    }

    [TestMethod]
    public void PointD_InvalidPartCountReturnsOrigin()
    {
        var converter = new PointDTypeConverter();
        var point = (LvcPointD)converter.ConvertFrom(null, null, "1,2,3")!;
        Assert.AreEqual(0d, point.X);
        Assert.AreEqual(0d, point.Y);
    }

    [TestMethod]
    public void Point_SingleValueDuplicatesToXAndY()
    {
        var point = (LvcPoint)PointTypeConverter.ParsePoint("4.5");
        Assert.AreEqual(4.5f, point.X);
        Assert.AreEqual(4.5f, point.Y);
    }

    [TestMethod]
    public void Point_TwoValuesMapToXAndY()
    {
        var point = (LvcPoint)PointTypeConverter.ParsePoint("2, 9");
        Assert.AreEqual(2f, point.X);
        Assert.AreEqual(9f, point.Y);
    }

    [TestMethod]
    public void Point_ConvertFromNonStringFallsThroughToBase()
    {
        var converter = new PointTypeConverter();
        // The base TypeConverter.ConvertFrom throws for unsupported conversions.
        Assert.ThrowsExactly<NotSupportedException>(
            () => _ = converter.ConvertFrom(null, null, 123));
    }

    [TestMethod]
    public void StringArray_SplitsAndTrimsEntries()
    {
        var converter = new StringArrayTypeConverter();
        Assert.IsTrue(converter.CanConvertFrom(null, typeof(string)));

        var values = (string[])converter.ConvertFrom(null, null, " a, b ,c,, d ")!;
        CollectionAssert.AreEqual(new[] { "a", "b", "c", "d" }, values);
    }

    [TestMethod]
    public void StringToDoubleArray_ParsesNumericEntries()
    {
        var converter = new StringToDoubleArrayTypeConverter();
        Assert.IsTrue(converter.CanConvertFrom(null, typeof(string)));

        var values = (double[])converter.ConvertFrom(null, null, "1, 2.5, 3")!;
        CollectionAssert.AreEqual(new[] { 1d, 2.5d, 3d }, values);
    }

    [TestMethod]
    public void StringToDoubleArray_FallsBackToZeroOnInvalidEntry()
    {
        var converter = new StringToDoubleArrayTypeConverter();

        var values = (double[])converter.ConvertFrom(null, null, "1, nope, 3")!;
        CollectionAssert.AreEqual(new[] { 1d, 0d, 3d }, values);
    }

    [TestMethod]
    public void Values_ParsesNumericEntries()
    {
        var converter = new ValuesTypeConverter();
        Assert.IsTrue(converter.CanConvertFrom(null, typeof(string)));

        var values = (double[])converter.ConvertFrom(null, null, "10, 20, 30")!;
        CollectionAssert.AreEqual(new[] { 10d, 20d, 30d }, values);
    }

    [TestMethod]
    public void Values_FallsBackToZeroOnInvalidEntry()
    {
        var converter = new ValuesTypeConverter();

        var values = (double[])converter.ConvertFrom(null, null, "1, nan-ish, 3")!;
        Assert.AreEqual(3, values.Length);
        Assert.AreEqual(1d, values[0]);
        Assert.AreEqual(0d, values[1]);
        Assert.AreEqual(3d, values[2]);
    }

    [TestMethod]
    public void AllConverters_RejectNonStringSource()
    {
        // Every converter should opt-in only to string conversion and defer the rest to the base.
        var converters = new System.ComponentModel.TypeConverter[]
        {
            new HexToLvcColorTypeConverter(),
            new HexToLvcColorArrayTypeConverter(),
            new HexToPaintTypeConverter(),
            new MarginTypeConverter(),
            new PaddingTypeConverter(),
            new PointDTypeConverter(),
            new PointTypeConverter(),
            new StringArrayTypeConverter(),
            new StringToDoubleArrayTypeConverter(),
            new ValuesTypeConverter(),
        };

        Assert.IsTrue(converters.All(c => c.CanConvertFrom(null, typeof(string))));
        Assert.IsTrue(converters.All(c => !c.CanConvertFrom(null, typeof(int))));
    }
}
