using LiveChartsCore.Drawing;
using LiveChartsCore.Painting;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.VisualElements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.OtherTests;

// Regression for #2124: DrawnLabelVisual hid its underlying LabelGeometry
// behind a protected internal DrawnElement, so callers that received a
// DrawnLabelVisual (e.g. chart.Title) had no way to read Text / TextSize /
// Paint / Padding back without reflection.
[TestClass]
public class DrawnLabelVisualLabelAccessTests
{
    [TestMethod]
    public void Label_Exposes_Default_Geometry_For_Parameterless_Ctor()
    {
        var visual = new DrawnLabelVisual();

        Assert.IsNotNull(visual.Label);
        Assert.AreEqual(string.Empty, visual.Label.Text);
    }

    [TestMethod]
    public void Label_Returns_The_LabelGeometry_Passed_To_The_Ctor()
    {
        var geometry = new LabelGeometry { Text = "Hello" };
        var visual = new DrawnLabelVisual(geometry);

        Assert.AreSame(geometry, visual.Label);
    }

    [TestMethod]
    public void Label_Mutations_Are_Visible_Through_The_Property()
    {
        var visual = new DrawnLabelVisual();
        var padding = new Padding(4f);
        Paint paint = new SolidColorPaint(SKColors.Red);

        visual.Label.Text = "Revenue";
        visual.Label.TextSize = 16f;
        visual.Label.Padding = padding;
        visual.Label.Paint = paint;

        Assert.AreEqual("Revenue", visual.Label.Text);
        Assert.AreEqual(16f, visual.Label.TextSize);
        Assert.AreSame(padding, visual.Label.Padding);
        Assert.AreSame(paint, visual.Label.Paint);
    }

    [TestMethod]
    public void Label_Roundtrips_Through_Untyped_Reference()
    {
        // Models the user's reported use case: setting chart.Title (typed as
        // Visual?), then reading it back later through a pattern match.
        var padding = new Padding(2f);
        var paint = new SolidColorPaint(SKColors.Black);
        Visual title = new DrawnLabelVisual(new LabelGeometry
        {
            Text = "Revenue",
            TextSize = 16f,
            Paint = paint,
            Padding = padding
        });

        Assert.IsInstanceOfType(title, typeof(DrawnLabelVisual));
        var label = ((DrawnLabelVisual)title).Label;

        Assert.AreEqual("Revenue", label.Text);
        Assert.AreEqual(16f, label.TextSize);
        Assert.AreSame(paint, label.Paint);
        Assert.AreSame(padding, label.Padding);
    }
}
