using LiveChartsGenerators.Frameworks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NoesisTests;

[TestClass]
public sealed class NoesisTemplateTests
{
    [TestMethod]
    public void XamlWrapper_MapsDependencyPropertyChangesThroughMetadata()
    {
        var template = new NoesisTemplate(FrameworkTemplate.Context.XamlObject);
        var generated = template.CreateBindableProperty("Values", "object", false, "XamlLineSeries", "null");

        StringAssert.Contains(generated, "global::Noesis.DependencyProperty.Register");
        StringAssert.Contains(generated, "MapChangeToBaseType(\"Values\")");
        Assert.AreEqual(string.Empty, template.GetPropertyChangedMetod());
    }

    [TestMethod]
    public void ChartProperty_InvokesItsChangeHandler()
    {
        var template = new NoesisTemplate(FrameworkTemplate.Context.XamlProperty);
        var change = new FrameworkTemplate.OnChangeInfo("OnSeriesChanged", false, false);
        var generated = template.CreateBindableProperty("Series", "object?", false, "Chart", "null", change);

        StringAssert.Contains(generated, "typeof(object)");
        StringAssert.Contains(generated, "OnSeriesChanged((Chart)o)");
        Assert.IsFalse(generated.Contains("MapChangeToBaseType"));
    }
}
