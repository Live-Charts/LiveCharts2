using System.Collections.Generic;
using System.Globalization;
using LiveChartsCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.CoreObjectsTests;

[TestClass]
public class LabelerTesting
{
    [TestMethod]
    public void NamedLabeler_NullEntry_ReturnsEmpty()
    {
        // regression for #1826: a null entry in Axis.Labels would flow as a null
        // string into the rendering pipeline. The named labeler must coerce
        // nulls to string.Empty so the chart renders the position blank instead
        // of failing silently downstream.
        var labeler = Labelers.BuildNamedLabeler(new List<string> { "A", null!, "C" });

        Assert.AreEqual("A", labeler(0));
        Assert.AreEqual(string.Empty, labeler(1));
        Assert.AreEqual("C", labeler(2));
        Assert.AreEqual(string.Empty, labeler(3));  // out of range
        Assert.AreEqual(string.Empty, labeler(-1)); // out of range
    }

    [TestMethod]
    public void Million()
    {
        string result;

        result = Labelers.SixRepresentativeDigits(100000000);
        Assert.IsTrue(result == "100 M");
        result = Labelers.SixRepresentativeDigits(10000000);
        Assert.IsTrue(result == "10 M");
        result = Labelers.SixRepresentativeDigits(1000000);
        Assert.IsTrue(result == "1 M");
        result = Labelers.SixRepresentativeDigits(100000);
        Assert.IsTrue(result == "100000");
        result = Labelers.SixRepresentativeDigits(-1000000);
        Assert.IsTrue(result == "-1 M");
        result = Labelers.SixRepresentativeDigits(-10000000);
        Assert.IsTrue(result == "-10 M");
        result = Labelers.SixRepresentativeDigits(-100000000);
        Assert.IsTrue(result == "-100 M");
    }

    [TestMethod]
    public void Micra()
    {
        string result;

        result = Labelers.SixRepresentativeDigits(0.00000001);
        Assert.IsTrue(result == "0.01 µ");
        result = Labelers.SixRepresentativeDigits(0.0000001);
        Assert.IsTrue(result == "0.1 µ");
        result = Labelers.SixRepresentativeDigits(0.000001);
        Assert.IsTrue(result == "1 µ");
        result = Labelers.SixRepresentativeDigits(0.00001);
        Assert.IsTrue(result == "0.00001");
        result = Labelers.SixRepresentativeDigits(-0.000001);
        Assert.IsTrue(result == "-1 µ");
        result = Labelers.SixRepresentativeDigits(-0.00000001);
        Assert.IsTrue(result == "-0.01 µ");
        result = Labelers.SixRepresentativeDigits(-0.0000001);
        Assert.IsTrue(result == "-0.1 µ");
    }

    [TestMethod]
    public void Currency()
    {
        string result;

        result = Labelers.FormatCurrency(-1000000000000000, ",", ".", NumberFormatInfo.CurrentInfo.CurrencySymbol);
        Assert.IsTrue(result == "-$1 Q");
        result = Labelers.FormatCurrency(-100000000000000, ",", ".", NumberFormatInfo.CurrentInfo.CurrencySymbol);
        Assert.IsTrue(result == "-$100 T");
        result = Labelers.FormatCurrency(-10000000000000, ",", ".", NumberFormatInfo.CurrentInfo.CurrencySymbol);
        Assert.IsTrue(result == "-$10 T");
        result = Labelers.FormatCurrency(-1000000000000, ",", ".", NumberFormatInfo.CurrentInfo.CurrencySymbol);
        Assert.IsTrue(result == "-$1 T");
        result = Labelers.FormatCurrency(-100000000000, ",", ".", NumberFormatInfo.CurrentInfo.CurrencySymbol);
        Assert.IsTrue(result == "-$100 B");
        result = Labelers.FormatCurrency(-10000000000, ",", ".", NumberFormatInfo.CurrentInfo.CurrencySymbol);
        Assert.IsTrue(result == "-$10 B");
        result = Labelers.FormatCurrency(-1000000000, ",", ".", NumberFormatInfo.CurrentInfo.CurrencySymbol);
        Assert.IsTrue(result == "-$1 B");
        result = Labelers.FormatCurrency(-100000000, ",", ".", NumberFormatInfo.CurrentInfo.CurrencySymbol);
        Assert.IsTrue(result == "-$100 M");
        result = Labelers.FormatCurrency(-10000000, ",", ".", NumberFormatInfo.CurrentInfo.CurrencySymbol);
        Assert.IsTrue(result == "-$10 M");
        result = Labelers.FormatCurrency(-1000000, ",", ".", NumberFormatInfo.CurrentInfo.CurrencySymbol);
        Assert.IsTrue(result == "-$1 M");
        result = Labelers.FormatCurrency(1000000, ",", ".", NumberFormatInfo.CurrentInfo.CurrencySymbol);
        Assert.IsTrue(result == "$1 M");
        result = Labelers.FormatCurrency(10000000, ",", ".", NumberFormatInfo.CurrentInfo.CurrencySymbol);
        Assert.IsTrue(result == "$10 M");
        result = Labelers.FormatCurrency(100000000, ",", ".", NumberFormatInfo.CurrentInfo.CurrencySymbol);
        Assert.IsTrue(result == "$100 M");
        result = Labelers.FormatCurrency(1000000000, ",", ".", NumberFormatInfo.CurrentInfo.CurrencySymbol);
        Assert.IsTrue(result == "$1 B");
        result = Labelers.FormatCurrency(10000000000, ",", ".", NumberFormatInfo.CurrentInfo.CurrencySymbol);
        Assert.IsTrue(result == "$10 B");
        result = Labelers.FormatCurrency(100000000000, ",", ".", NumberFormatInfo.CurrentInfo.CurrencySymbol);
        Assert.IsTrue(result == "$100 B");
        result = Labelers.FormatCurrency(1000000000000, ",", ".", NumberFormatInfo.CurrentInfo.CurrencySymbol);
        Assert.IsTrue(result == "$1 T");
        result = Labelers.FormatCurrency(10000000000000, ",", ".", NumberFormatInfo.CurrentInfo.CurrencySymbol);
        Assert.IsTrue(result == "$10 T");
        result = Labelers.FormatCurrency(100000000000000, ",", ".", NumberFormatInfo.CurrentInfo.CurrencySymbol);
        Assert.IsTrue(result == "$100 T");
        result = Labelers.FormatCurrency(1000000000000000, ",", ".", NumberFormatInfo.CurrentInfo.CurrencySymbol);
        Assert.IsTrue(result == "$1 Q");
    }
}
