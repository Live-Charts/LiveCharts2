using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Drawing.Layouts;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.LayoutTests;

[TestClass]
public class TableLayoutTest
{
    [TestMethod]
    public void TableStart()
    {
        var px = 10;
        var py = 20;

        var table = new TableLayout
        {
            Padding = new Padding(px, py),
            X = 100,
            Y = 200,
            VerticalAlignment = Align.Start,
            HorizontalAlignment = Align.Start
        };

        RectangleGeometry g1, g2;

        table.AddChild(new RectangleGeometry
        {
            Width = 10,
            Height = 20,
            Fill = new SolidColorPaint(SKColors.Red)
        }, 0, 0);

        table.AddChild(g1 = new RectangleGeometry
        {
            Width = 15,
            Height = 10,
            Fill = new SolidColorPaint(SKColors.Green)
        }, 0, 1);

        table.AddChild(new RectangleGeometry
        {
            Width = 20,
            Height = 30,
            Fill = new SolidColorPaint(SKColors.Yellow)
        }, 1, 0);

        table.AddChild(
            g2 = new RectangleGeometry
            {
                Width = 25,
                Height = 20,
                Fill = new SolidColorPaint(SKColors.Blue)
            }, 1, 1);

        var chart = new SKCartesianChart
        {
            Width = 1000,
            Height = 1000,
            Series = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = new double[] { 1, 2, 3 }
                }
            }
        };

        chart.CoreCanvas.AddGeometry(table);
        _ = chart.GetImage();

        var tableSize = table.Measure();

        Assert.IsTrue(
            // 100 left + 10 padding + 20 column width
            g1.X == 100 + px + 20 &&

            // 200 top + 20 padding
            g1.Y == 200 + py &&

            // 100 left + 10 padding + 20 column width
            g2.X == 100 + px + 20 &&

            // 200 top + 20 padding + 20 previous row height
            g2.Y == 200 + py + 20 &&

            // 10 padding + columns width + 10 padding
            tableSize.Width == px + 20 + 25 + px &&
            // 10 padding + rows width + 10 padding
            tableSize.Height == py + 20 + 30 + py);
    }

    [TestMethod]
    public void TableStartFirstEmpty()
    {
        var px = 10;
        var py = 20;

        var table = new TableLayout
        {
            Padding = new Padding(px, py),
            X = 100,
            Y = 200,
            VerticalAlignment = Align.Start,
            HorizontalAlignment = Align.Start
        };

        RectangleGeometry g1, g2;

        table.AddChild(new RectangleGeometry
        {
            Width = 10,
            Height = 20,
            Fill = new SolidColorPaint(SKColors.Red)
        }, 1, 1);

        table.AddChild(g1 = new RectangleGeometry
        {
            Width = 15,
            Height = 10,
            Fill = new SolidColorPaint(SKColors.Green)
        }, 1, 2);

        table.AddChild(new RectangleGeometry
        {
            Width = 20,
            Height = 30,
            Fill = new SolidColorPaint(SKColors.Yellow)
        }, 2, 1);

        table.AddChild(
            g2 = new RectangleGeometry
            {
                Width = 25,
                Height = 20,
                Fill = new SolidColorPaint(SKColors.Blue)
            }, 2, 2);

        var chart = new SKCartesianChart
        {
            Width = 1000,
            Height = 1000,
            Series = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = new double[] { 1, 2, 3 }
                }
            }
        };

        chart.CoreCanvas.AddGeometry(table);
        _ = chart.GetImage();

        var tableSize = table.Measure();

        Assert.IsTrue(
            // 100 left + 10 padding + 20 column width
            g1.X == 100 + px + 20 &&

            // 200 top + 20 padding
            g1.Y == 200 + py &&

            // 100 left + 10 padding + 20 column width
            g2.X == 100 + px + 20 &&

            // 200 top + 20 padding + 20 previous row height
            g2.Y == 200 + py + 20 &&

            // 10 padding + columns width + 10 padding
            tableSize.Width == px + 20 + 25 + px &&
            // 10 padding + rows width + 10 padding
            tableSize.Height == py + 20 + 30 + py);
    }

    [TestMethod]
    public void TableStartSomeEmpty()
    {
        var px = 10;
        var py = 20;

        var table = new TableLayout
        {
            Padding = new Padding(px, py),
            X = 100,
            Y = 200,
            VerticalAlignment = Align.Start,
            HorizontalAlignment = Align.Start
        };

        RectangleGeometry g1, g2;

        table.AddChild(new RectangleGeometry
        {
            Width = 5,
            Height = 20,
            Fill = new SolidColorPaint(SKColors.Green.WithAlpha(100))
        }, 0, 0);

        // ignore the 0,1 cell

        table.AddChild(g1 = new RectangleGeometry
        {
            Width = 15,
            Height = 10,
            Fill = new SolidColorPaint(SKColors.Green)
        }, 0, 2);

        // also ignore the row 1

        //table.AddChild(new RectangleGeometry
        //{
        //    Width = 10,
        //    Height = 30,
        //    Fill = new SolidColorPaint(SKColors.Blue.WithAlpha(100))
        //}, 2, 0);

        table.AddChild(new RectangleGeometry
        {
            Width = 10,
            Height = 30,
            Fill = new SolidColorPaint(SKColors.Blue.WithAlpha(200))
        }, 2, 1);

        table.AddChild(
            g2 = new RectangleGeometry
            {
                Width = 25,
                Height = 20,
                Fill = new SolidColorPaint(SKColors.Blue)
            }, 2, 2);

        var chart = new SKCartesianChart
        {
            Width = 1000,
            Height = 1000,
            Series = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = new double[] { 1, 2, 3 }
                }
            }
        };

        chart.CoreCanvas.AddGeometry(table);
        _ = chart.GetImage();

        var tableSize = table.Measure();

        Assert.IsTrue(
            // 100 left + 10 padding + 15 column width
            g1.X == 100 + px + 15 &&

            // 200 top + 20 padding
            g1.Y == 200 + py &&

            // 100 left + 10 padding + 15 column width
            g2.X == 100 + px + 15 &&

            // 200 top + 20 padding + 20 previous row height
            g2.Y == 200 + py + 20 &&

            // 10 padding + columns width + 10 padding
            tableSize.Width == px + 5 + 10 + 25 + px &&
            // 10 padding + rows width + 10 padding
            tableSize.Height == py + 20 + 30 + py);
    }

    [TestMethod]
    public void TableMiddle()
    {
        var px = 10;
        var py = 20;

        var table = new TableLayout
        {
            Padding = new Padding(px, py),
            X = 100,
            Y = 200,
            VerticalAlignment = Align.Middle,
            HorizontalAlignment = Align.Middle
        };

        RectangleGeometry g1, g2;

        table.AddChild(new RectangleGeometry
        {
            Width = 10,
            Height = 20,
            Fill = new SolidColorPaint(SKColors.Red)
        }, 0, 0);

        table.AddChild(g1 = new RectangleGeometry
        {
            Width = 15,
            Height = 10,
            Fill = new SolidColorPaint(SKColors.Green)
        }, 0, 1);

        table.AddChild(new RectangleGeometry
        {
            Width = 20,
            Height = 30,
            Fill = new SolidColorPaint(SKColors.Yellow)
        }, 1, 0);

        table.AddChild(
            g2 = new RectangleGeometry
            {
                Width = 25,
                Height = 20,
                Fill = new SolidColorPaint(SKColors.Blue)
            }, 1, 1);

        var chart = new SKCartesianChart
        {
            Width = 1000,
            Height = 1000,
            Series = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = new double[] { 1, 2, 3 }
                }
            }
        };

        chart.CoreCanvas.AddGeometry(table);
        _ = chart.GetImage();

        var tableSize = table.Measure();

        Assert.IsTrue(
            g1.X == 135 &&

            // 200 top + 20 padding
            g1.Y == 225 &&

            // 100 left + 10 padding + 20 column width
            g2.X == 130 &&

            // 200 top + 20 padding + 20 previous row height
            g2.Y == 245 &&

            // 10 padding + columns width + 10 padding
            tableSize.Width == px + 20 + 25 + px &&
            // 10 padding + rows width + 10 padding
            tableSize.Height == py + 20 + 30 + py);
    }

    [TestMethod]
    public void TableEnd()
    {
        var px = 10;
        var py = 20;

        var table = new TableLayout
        {
            Padding = new Padding(px, py),
            X = 100,
            Y = 200,
            VerticalAlignment = Align.End,
            HorizontalAlignment = Align.End
        };

        RectangleGeometry g1, g2;

        table.AddChild(new RectangleGeometry
        {
            Width = 10,
            Height = 20,
            Fill = new SolidColorPaint(SKColors.Red)
        }, 0, 0);

        table.AddChild(g1 = new RectangleGeometry
        {
            Width = 15,
            Height = 10,
            Fill = new SolidColorPaint(SKColors.Green)
        }, 0, 1);

        table.AddChild(new RectangleGeometry
        {
            Width = 20,
            Height = 30,
            Fill = new SolidColorPaint(SKColors.Yellow)
        }, 1, 0);

        table.AddChild(
            g2 = new RectangleGeometry
            {
                Width = 25,
                Height = 20,
                Fill = new SolidColorPaint(SKColors.Blue)
            }, 1, 1);

        var chart = new SKCartesianChart
        {
            Width = 1000,
            Height = 1000,
            Series = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = new double[] { 1, 2, 3 }
                }
            }
        };

        chart.CoreCanvas.AddGeometry(table);
        _ = chart.GetImage();

        var tableSize = table.Measure();

        Assert.IsTrue(
            g1.X == 140 &&

            // 200 top + 20 padding
            g1.Y == 230 &&

            // 100 left + 10 padding + 20 column width
            g2.X == 130 &&

            // 200 top + 20 padding + 20 previous row height
            g2.Y == 250 &&

            // 10 padding + columns width + 10 padding
            tableSize.Width == px + 20 + 25 + px &&
            // 10 padding + rows width + 10 padding
            tableSize.Height == py + 20 + 30 + py);
    }

    [TestMethod]
    public void UnevenColumns()
    {
        var px = 0;
        var py = 0;

        var table = new TableLayout
        {
            Padding = new Padding(px, py),
            X = 100,
            Y = 200,
            VerticalAlignment = Align.Start,
            HorizontalAlignment = Align.Start
        };

        LabelGeometry g00, g01, g10, g11;

        table.AddChild(g00 = new LabelGeometry
        {
            Text = "xxxx",
            Paint = new SolidColorPaint(SKColors.Red)
        }, 0, 0);

        table.AddChild(g01 = new LabelGeometry
        {
            Text = "xxxxxxxxxxxxxxxxxxxxxxxx",
            Paint = new SolidColorPaint(SKColors.Green)
        }, 0, 1);

        table.AddChild(g10 = new LabelGeometry
        {
            Text = "xxxxxxxxxxxxxxxxxxxxxxxx",
            Paint = new SolidColorPaint(SKColors.Yellow)
        }, 1, 0);

        table.AddChild(g11 = new LabelGeometry
        {
            Text = "xxxx",
            Paint = new SolidColorPaint(SKColors.Blue)
        }, 1, 1);

        var chart = new SKCartesianChart
        {
            Width = 1000,
            Height = 1000,
            Series = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = new double[] { 1, 2, 3 }
                }
            },
        };

        chart.CoreCanvas.AddGeometry(table);
        _ = chart.GetImage();

        g00.Paint = g00.Paint;
        g01.Paint = g01.Paint;
        g10.Paint = g10.Paint;
        g11.Paint = g11.Paint;

        var g00m = g00.Measure();
        var g01m = g01.Measure();
        var g10m = g10.Measure();
        var g11m = g11.Measure();

        var wc0 = g00m.Width > g01m.Width ? g00m.Width : g10m.Width;
        var wc1 = g10m.Width > g11m.Width ? g01m.Width : g11m.Width;

        var tableSize = table.Measure();

        Assert.IsTrue(
            // 10 padding + columns width + 10 padding
            tableSize.Width == wc0 + wc1);
    }
}
