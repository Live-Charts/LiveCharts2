using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.VisualElements;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.OtherTests;

// Regression for code-only chart titles being clipped at the top in WinForms /
// Blazor / Eto / Console samples.
// Root cause: BaseLabelGeometry defaults HorizontalAlign and VerticalAlign to
// Align.Middle. Chart.AddTitleToChart positions the title at (X, 0) and assumes
// the bbox top-left lives at that point (i.e. Align.Start). With the user-
// provided LabelGeometry going through DrawnLabelVisual(LabelGeometry) unchanged,
// the bbox was centered on Y=0 and the top half rendered at negative Y.
[TestClass]
public class DrawnLabelTitleAlignmentTests
{
    [TestMethod]
    public void DrawnLabelVisual_Ctor_With_LabelGeometry_Forces_Align_Start()
    {
        var label = new LabelGeometry { Text = "title" };

        // sanity: BaseLabelGeometry's defaults are still Middle, so this test
        // exercises the constructor's override (not pre-set values).
        Assert.AreEqual(Align.Middle, label.HorizontalAlign);
        Assert.AreEqual(Align.Middle, label.VerticalAlign);

        _ = new DrawnLabelVisual(label);

        Assert.AreEqual(Align.Start, label.HorizontalAlign);
        Assert.AreEqual(Align.Start, label.VerticalAlign);
    }
}
