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
public class StackLayoutTest
{
    [TestMethod]
    public void StackLayoutHorizontalStart()
    {
        var stackPanel = new StackLayout
        {
            Padding = new Padding(10, 20),
            X = 100,
            Y = 200,
            VerticalAlignment = Align.Start,
            Orientation = ContainerOrientation.Horizontal
        };

        RectangleGeometry g1, g2;

        stackPanel.Children = [
            g1 = new RectangleGeometry
            {
                Width = 15,
                Height = 25,
                Fill = new SolidColorPaint(SKColors.Red)
            },
            g2 = new RectangleGeometry
            {
                Width = 15,
                Height = 15,
                Fill = new SolidColorPaint(SKColors.Blue)
            }
        ];

        var chart = new SKCartesianChart
        {
            Width = 1000,
            Height = 1000,
            Series = [
                new LineSeries<double>
                {
                    Values = [1, 2, 3]
                }
            ]
        };

        chart.CoreCanvas.AddGeometry(stackPanel);

        _ = chart.GetImage();

        var panelSize = stackPanel.Measure();

        Assert.IsTrue(
            // 100 left + 10 padding
            g1.X == 100 + 10 &&

            // 200 top + 20 padding
            g1.Y == 200 + 20 &&

            // 100 left + 10 padding + 15 previous width
            g2.X == 100 + 10 + 15 &&

            // preserve the previous y...
            g2.Y == 200 + 20 &&

            // 10 padding + 15 first shape + 15 second shape + 10 padding
            panelSize.Width == 10 + 15 + 15 + 10 &&
            panelSize.Height == 20 + 25 + 20);
    }

    [TestMethod]
    public void StackLayoutHorizontalMiddle()
    {
        var stackLayout = new StackLayout
        {
            Padding = new Padding(10, 20),
            X = 100,
            Y = 200,
            // also default should be middle...
            //VerticalAlignment = Drawing.Align.Middle,
            Orientation = ContainerOrientation.Horizontal
        };

        RectangleGeometry g1, g2;

        stackLayout.Children = [
            g1 = new RectangleGeometry
            {
                Width = 15,
                Height = 25,
                Fill = new SolidColorPaint(SKColors.Red)
            },
            g2 = new RectangleGeometry
            {
                Width = 15,
                Height = 15,
                Fill = new SolidColorPaint(SKColors.Blue)
            }
        ];

        var chart = new SKCartesianChart
        {
            Width = 1000,
            Height = 1000,
            Series = [
                new LineSeries<double>
                {
                    Values = [1, 2, 3]
                }
            ]
        };

        chart.CoreCanvas.AddGeometry(stackLayout);
        _ = chart.GetImage();

        Assert.IsTrue(
            // 100 left + 10 padding
            g1.X == 100 + 10 &&

            // 200 top + 20 padding
            g1.Y == 200 + 20 &&

            // 100 left + 10 padding + 15 previous width
            g2.X == 100 + 10 + 15 &&

            // preserve the previous y... + half of the difference between the heights
            g2.Y == 200 + 20 + 5);
    }

    [TestMethod]
    public void StackLayoutHorizontalEnd()
    {
        var stackLayout = new StackLayout
        {
            Padding = new Padding(10, 20),
            X = 100,
            Y = 200,
            VerticalAlignment = Align.End,
            Orientation = ContainerOrientation.Horizontal
        };

        RectangleGeometry g1, g2;

        stackLayout.Children = [
            g1 = new RectangleGeometry
            {
                Width = 15,
                Height = 25,
                Fill = new SolidColorPaint(SKColors.Red)
            },
            g2 = new RectangleGeometry
            {
                Width = 15,
                Height = 15,
                Fill = new SolidColorPaint(SKColors.Blue)
            }
        ];

        var chart = new SKCartesianChart
        {
            Width = 1000,
            Height = 1000,
            Series =
            [
                new LineSeries<double>
                {
                    Values = [1, 2, 3]
                }
            ]
        };

        chart.CoreCanvas.AddGeometry(stackLayout);
        _ = chart.GetImage();

        Assert.IsTrue(
            // 100 left + 10 padding
            g1.X == 100 + 10 &&

            // 200 top + 20 padding
            g1.Y == 200 + 20 &&

            // 100 left + 10 padding + 15 previous width
            g2.X == 100 + 10 + 15 &&

            // preserve the previous y... + difference between the heights
            g2.Y == 200 + 20 + 10);
    }

    [TestMethod]
    public void StackLayoutVerticalStart()
    {
        var stackPanel = new StackLayout
        {
            Padding = new Padding(10, 20),
            X = 100,
            Y = 200,
            HorizontalAlignment = Align.Start,
            Orientation = ContainerOrientation.Vertical
        };

        RectangleGeometry g1, g2;

        stackPanel.Children = [
            g1 = new RectangleGeometry
            {
                Width = 10,
                Height = 25,
                Fill = new SolidColorPaint(SKColors.Red)
            },
            g2 = new RectangleGeometry
            {
                Width = 25,
                Height = 25,
                Fill = new SolidColorPaint(SKColors.Blue)
            }
        ];

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

        chart.CoreCanvas.AddGeometry(stackPanel);
        _ = chart.GetImage();

        var panelSize = stackPanel.Measure();

        Assert.IsTrue(
            // 100 left + 10 padding 
            g1.X == 100 + 10 &&

            // 200 top + 20 padding
            g1.Y == 200 + 20 &&

            // preserve the previous x...
            g2.X == 110 &&

            // 200 top + 20 padding + 25 previous height
            g2.Y == 200 + 20 + 25 &&

            panelSize.Width == 10 + 25 + 10 &&
            panelSize.Height == 20 + 25 + 25 + 20);
    }

    [TestMethod]
    public void StackLayoutVerticalMiddle()
    {
        var stackPanel = new StackLayout
        {
            Padding = new Padding(10, 20),
            X = 100,
            Y = 200,
            // also default should be middle...
            //HorizontalAlignment = Drawing.Align.Middle,
            Orientation = ContainerOrientation.Vertical
        };

        RectangleGeometry g1, g2;

        stackPanel.Children = [
            g1 = new RectangleGeometry
            {
                Width = 10,
                Height = 25,
                Fill = new SolidColorPaint(SKColors.Red)
            },
            g2 = new RectangleGeometry
            {
                Width = 25,
                Height = 25,
                Fill = new SolidColorPaint(SKColors.Blue)
            }
        ];

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

        chart.CoreCanvas.AddGeometry(stackPanel);
        _ = chart.GetImage();

        Assert.IsTrue(
            // 100 left + 10 padding + 7.5 half of the widths difference
            g1.X == 100 + 10 + 7.5 &&

            // 200 top + 20 padding
            g1.Y == 200 + 20 &&

            // preserve the previous x...
            g2.X == 110 &&

            // 200 top + 20 padding + 25 previous height
            g2.Y == 200 + 20 + 25);
    }

    [TestMethod]
    public void StackLayoutVerticalEnd()
    {
        var stackPanel = new StackLayout
        {
            Padding = new Padding(10, 20),
            X = 100,
            Y = 200,
            HorizontalAlignment = Align.End,
            Orientation = ContainerOrientation.Vertical
        };

        RectangleGeometry g1, g2;

        stackPanel.Children = [
            g1 = new RectangleGeometry
            {
                Width = 10,
                Height = 25,
                Fill = new SolidColorPaint(SKColors.Red)
            },
            g2 = new RectangleGeometry
            {
                Width = 25,
                Height = 25,
                Fill = new SolidColorPaint(SKColors.Blue)
            }
        ];

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

        chart.CoreCanvas.AddGeometry(stackPanel);
        _ = chart.GetImage();

        Assert.IsTrue(
            // 100 left + 10 padding + the widths difference
            g1.X == 100 + 10 + 15 &&

            // 200 top + 20 padding
            g1.Y == 200 + 20 &&

            // preserve the previous x...
            g2.X == 110 &&

            // 200 top + 20 padding + 25 previous height
            g2.Y == 200 + 20 + 25);
    }

    [TestMethod]
    public void StackLayoutHorizontalStartWrap()
    {
        var stackPanel = new StackLayout
        {
            Padding = new Padding(0),
            X = 100,
            Y = 200,
            MaxWidth = 12 * 3,
            VerticalAlignment = Align.Start,
            Orientation = ContainerOrientation.Horizontal
        };

        RectangleGeometry g0, g1, g2;

        stackPanel.Children = [
            new RectangleGeometry
            {
                Width = 5,
                Height = 15,
                Fill = new SolidColorPaint(SKColors.Green)
            },
            new RectangleGeometry
            {
                Width = 5,
                Height = 15,
                Fill = new SolidColorPaint(SKColors.Blue)
            },
            new RectangleGeometry
            {
                Width = 5,
                Height = 15,
                Fill = new SolidColorPaint(SKColors.Green)
            },
            new RectangleGeometry
            {
                Width = 5,
                Height = 15,
                Fill = new SolidColorPaint(SKColors.Blue)
            },
            new RectangleGeometry
            {
                Width = 5,
                Height = 15,
                Fill = new SolidColorPaint(SKColors.Green)
            },
            g0 = new RectangleGeometry
            {
                Width = 5,
                Height = 5,
                Fill = new SolidColorPaint(SKColors.Yellow)
            },
            new RectangleGeometry
            {
                Width = 10,
                Height = 15,
                Fill = new SolidColorPaint(SKColors.Green)
            },
            new RectangleGeometry
            {
                Width = 10,
                Height = 15,
                Fill = new SolidColorPaint(SKColors.Blue)
            },
            g1 = new RectangleGeometry
            {
                Width = 10,
                Height = 5,
                Fill = new SolidColorPaint(SKColors.Yellow)
            },
            new RectangleGeometry
            {
                Width = 12,
                Height = 20,
                Fill = new SolidColorPaint(SKColors.Green)
            },
            new RectangleGeometry
            {
                Width = 12,
                Height = 20,
                Fill = new SolidColorPaint(SKColors.Blue)
            },
            g2 = new RectangleGeometry
            {
                Width = 12,
                Height = 5,
                Fill = new SolidColorPaint(SKColors.Yellow)
            }
        ];

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

        chart.CoreCanvas.AddGeometry(stackPanel);
        _ = chart.GetImage();

        var panelSize = stackPanel.Measure();

        Assert.IsTrue(
            g0.X == 100 + 25 &&
            g0.Y == 200 &&
            g1.X == 100 + 20 &&
            g1.Y == 200 + 15 &&
            g2.X == 100 + 24 &&
            g2.Y == 200 + 15 + 15 &&
            panelSize.Width == 36 &&
            panelSize.Height == 50);
    }

    [TestMethod]
    public void StackPanelLayoutMiddleWrap()
    {
        var stackPanel = new StackLayout
        {
            Padding = new Padding(0),
            X = 100,
            Y = 200,
            MaxWidth = 12 * 3,
            VerticalAlignment = Align.Middle,
            Orientation = ContainerOrientation.Horizontal
        };

        RectangleGeometry g0, g1, g2;

        stackPanel.Children = [
            new RectangleGeometry
            {
                Width = 5,
                Height = 15,
                Fill = new SolidColorPaint(SKColors.Green)
            },
            new RectangleGeometry
            {
                Width = 5,
                Height = 15,
                Fill = new SolidColorPaint(SKColors.Blue)
            },
            new RectangleGeometry
            {
                Width = 5,
                Height = 15,
                Fill = new SolidColorPaint(SKColors.Green)
            },
            new RectangleGeometry
            {
                Width = 5,
                Height = 15,
                Fill = new SolidColorPaint(SKColors.Blue)
            },
            new RectangleGeometry
            {
                Width = 5,
                Height = 15,
                Fill = new SolidColorPaint(SKColors.Green)
            },
            g0 = new RectangleGeometry
            {
                Width = 5,
                Height = 5,
                Fill = new SolidColorPaint(SKColors.Yellow)
            },
            new RectangleGeometry
            {
                Width = 10,
                Height = 15,
                Fill = new SolidColorPaint(SKColors.Green)
            },
            new RectangleGeometry
            {
                Width = 10,
                Height = 15,
                Fill = new SolidColorPaint(SKColors.Blue)
            },
            g1 = new RectangleGeometry
            {
                Width = 10,
                Height = 5,
                Fill = new SolidColorPaint(SKColors.Yellow)
            },
            new RectangleGeometry
            {
                Width = 12,
                Height = 20,
                Fill = new SolidColorPaint(SKColors.Green)
            },
            new RectangleGeometry
            {
                Width = 12,
                Height = 20,
                Fill = new SolidColorPaint(SKColors.Blue)
            },
            g2 = new RectangleGeometry
            {
                Width = 12,
                Height = 5,
                Fill = new SolidColorPaint(SKColors.Yellow)
            }
        ];

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

        chart.CoreCanvas.AddGeometry(stackPanel);
        _ = chart.GetImage();

        var panelSize = stackPanel.Measure();

        Assert.IsTrue(
            g0.X == 100 + 25 &&
            g0.Y == 205 &&
            g1.X == 100 + 20 &&
            g1.Y == 220 &&
            g2.X == 100 + 24 &&
            g2.Y == 237.5 &&
            panelSize.Width == 36 &&
            panelSize.Height == 50);
    }

    [TestMethod]
    public void StackLayoutHorizontalEndWrap()
    {
        var stackPanel = new StackLayout
        {
            Padding = new Padding(0),
            X = 100,
            Y = 200,
            MaxWidth = 12 * 3,
            VerticalAlignment = Align.End,
            Orientation = ContainerOrientation.Horizontal
        };

        RectangleGeometry g0, g1, g2;

        stackPanel.Children = [
            new RectangleGeometry
            {
                Width = 5,
                Height = 15,
                Fill = new SolidColorPaint(SKColors.Green)
            },
            new RectangleGeometry
            {
                Width = 5,
                Height = 15,
                Fill = new SolidColorPaint(SKColors.Blue)
            },
            new RectangleGeometry
            {
                Width = 5,
                Height = 15,
                Fill = new SolidColorPaint(SKColors.Green)
            },
            new RectangleGeometry
            {
                Width = 5,
                Height = 15,
                Fill = new SolidColorPaint(SKColors.Blue)
            },
            new RectangleGeometry
            {
                Width = 5,
                Height = 15,
                Fill = new SolidColorPaint(SKColors.Green)
            },
            g0 = new RectangleGeometry
            {
                Width = 5,
                Height = 5,
                Fill = new SolidColorPaint(SKColors.Yellow)
            },
            new RectangleGeometry
            {
                Width = 10,
                Height = 15,
                Fill = new SolidColorPaint(SKColors.Green)
            },
            new RectangleGeometry
            {
                Width = 10,
                Height = 15,
                Fill = new SolidColorPaint(SKColors.Blue)
            },
            g1 = new RectangleGeometry
            {
                Width = 10,
                Height = 5,
                Fill = new SolidColorPaint(SKColors.Yellow)
            },
            new RectangleGeometry
            {
                Width = 12,
                Height = 20,
                Fill = new SolidColorPaint(SKColors.Green)
            },
            new RectangleGeometry
            {
                Width = 12,
                Height = 20,
                Fill = new SolidColorPaint(SKColors.Blue)
            },
            g2 = new RectangleGeometry
            {
                Width = 12,
                Height = 5,
                Fill = new SolidColorPaint(SKColors.Yellow)
            }
        ];

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

        chart.CoreCanvas.AddGeometry(stackPanel);
        _ = chart.GetImage();

        var panelSize = stackPanel.Measure();

        Assert.IsTrue(
            g0.X == 100 + 25 &&
            g0.Y == 210 &&
            g1.X == 100 + 20 &&
            g1.Y == 225 &&
            g2.X == 100 + 24 &&
            g2.Y == 245 &&
            panelSize.Width == 36 &&
            panelSize.Height == 50);
    }

    [TestMethod]
    public void StackPanelLayoutStartWrap()
    {
        var stackPanel = new StackLayout
        {
            Padding = new Padding(0),
            X = 200,
            Y = 100,
            MaxHeight = 12 * 3,
            HorizontalAlignment = Align.Start,
            Orientation = ContainerOrientation.Vertical
        };

        RectangleGeometry g0, g1, g2;

        stackPanel.Children = [
            new RectangleGeometry
            {
                Width = 15,
                Height = 5,
                Fill = new SolidColorPaint(SKColors.Green)
            },
            new RectangleGeometry
            {
                Width = 15,
                Height = 5,
                Fill = new SolidColorPaint(SKColors.Blue)
            },
            new RectangleGeometry
            {
                Width = 15,
                Height = 5,
                Fill = new SolidColorPaint(SKColors.Green)
            },
            new RectangleGeometry
            {
                Width = 15,
                Height = 5,
                Fill = new SolidColorPaint(SKColors.Blue)
            },
            new RectangleGeometry
            {
                Width = 15,
                Height = 5,
                Fill = new SolidColorPaint(SKColors.Green)
            },
            g0 = new RectangleGeometry
            {
                Width = 5,
                Height = 5,
                Fill = new SolidColorPaint(SKColors.Yellow)
            },
            new RectangleGeometry
            {
                Width = 15,
                Height = 10,
                Fill = new SolidColorPaint(SKColors.Green)
            },
            new RectangleGeometry
            {
                Width = 15,
                Height = 10,
                Fill = new SolidColorPaint(SKColors.Blue)
            },
            g1 = new RectangleGeometry
            {
                Width = 5,
                Height = 10,
                Fill = new SolidColorPaint(SKColors.Yellow)
            },
            new RectangleGeometry
            {
                Width = 20,
                Height = 12,
                Fill = new SolidColorPaint(SKColors.Green)
            },
            new RectangleGeometry
            {
                Width = 20,
                Height = 12,
                Fill = new SolidColorPaint(SKColors.Blue)
            },
            g2 = new RectangleGeometry
            {
                Width = 5,
                Height = 12,
                Fill = new SolidColorPaint(SKColors.Yellow)
            }
        ];

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

        chart.CoreCanvas.AddGeometry(stackPanel);
        _ = chart.GetImage();

        var panelSize = stackPanel.Measure();

        Assert.IsTrue(
            g0.X == 200 &&
            g0.Y == 125 &&
            g1.X == 215 &&
            g1.Y == 120 &&
            g2.X == 230 &&
            g2.Y == 124 &&
            panelSize.Width == 50 &&
            panelSize.Height == 36);
    }

    [TestMethod]
    public void StackLayoutVerticalMiddleWrap()
    {
        var stackPanel = new StackLayout
        {
            Padding = new Padding(0),
            X = 200,
            Y = 100,
            MaxHeight = 12 * 3,
            HorizontalAlignment = Align.Middle,
            Orientation = ContainerOrientation.Vertical
        };

        RectangleGeometry g0, g1, g2;

        stackPanel.Children = [
            new RectangleGeometry
            {
                Width = 15,
                Height = 5,
                Fill = new SolidColorPaint(SKColors.Green)
            },
            new RectangleGeometry
            {
                Width = 15,
                Height = 5,
                Fill = new SolidColorPaint(SKColors.Blue)
            },
            new RectangleGeometry
            {
                Width = 15,
                Height = 5,
                Fill = new SolidColorPaint(SKColors.Green)
            },
            new RectangleGeometry
            {
                Width = 15,
                Height = 5,
                Fill = new SolidColorPaint(SKColors.Blue)
            },
            new RectangleGeometry
            {
                Width = 15,
                Height = 5,
                Fill = new SolidColorPaint(SKColors.Green)
            },
            g0 = new RectangleGeometry
            {
                Width = 5,
                Height = 5,
                Fill = new SolidColorPaint(SKColors.Yellow)
            },
            new RectangleGeometry
            {
                Width = 15,
                Height = 10,
                Fill = new SolidColorPaint(SKColors.Green)
            },
            new RectangleGeometry
            {
                Width = 15,
                Height = 10,
                Fill = new SolidColorPaint(SKColors.Blue)
            },
            g1 = new RectangleGeometry
            {
                Width = 5,
                Height = 10,
                Fill = new SolidColorPaint(SKColors.Yellow)
            },
            new RectangleGeometry
            {
                Width = 20,
                Height = 12,
                Fill = new SolidColorPaint(SKColors.Green)
            },
            new RectangleGeometry
            {
                Width = 20,
                Height = 12,
                Fill = new SolidColorPaint(SKColors.Blue)
            },
            g2 = new RectangleGeometry
            {
                Width = 5,
                Height = 12,
                Fill = new SolidColorPaint(SKColors.Yellow)
            }
        ];

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

        chart.CoreCanvas.AddGeometry(stackPanel);
        _ = chart.GetImage();

        var panelSize = stackPanel.Measure();

        Assert.IsTrue(
            g0.X == 205 &&
            g0.Y == 125 &&
            g1.X == 220 &&
            g1.Y == 120 &&
            g2.X == 237.5 &&
            g2.Y == 124 &&
            panelSize.Width == 50 &&
            panelSize.Height == 36);
    }

    [TestMethod]
    public void StackPanelLayoutEndWrap()
    {
        var stackPanel = new StackLayout
        {
            Padding = new Padding(0),
            X = 200,
            Y = 100,
            MaxHeight = 12 * 3,
            HorizontalAlignment = Align.End,
            Orientation = ContainerOrientation.Vertical
        };

        RectangleGeometry g0, g1, g2;

        stackPanel.Children = [
            new RectangleGeometry
            {
                Width = 15,
                Height = 5,
                Fill = new SolidColorPaint(SKColors.Green)
            },
            new RectangleGeometry
            {
                Width = 15,
                Height = 5,
                Fill = new SolidColorPaint(SKColors.Blue)
            },
            new RectangleGeometry
            {
                Width = 15,
                Height = 5,
                Fill = new SolidColorPaint(SKColors.Green)
            },
            new RectangleGeometry
            {
                Width = 15,
                Height = 5,
                Fill = new SolidColorPaint(SKColors.Blue)
            },
            new RectangleGeometry
            {
                Width = 15,
                Height = 5,
                Fill = new SolidColorPaint(SKColors.Green)
            },
            g0 = new RectangleGeometry
            {
                Width = 5,
                Height = 5,
                Fill = new SolidColorPaint(SKColors.Yellow)
            },
            new RectangleGeometry
            {
                Width = 15,
                Height = 10,
                Fill = new SolidColorPaint(SKColors.Green)
            },
            new RectangleGeometry
            {
                Width = 15,
                Height = 10,
                Fill = new SolidColorPaint(SKColors.Blue)
            },
            g1 = new RectangleGeometry
            {
                Width = 5,
                Height = 10,
                Fill = new SolidColorPaint(SKColors.Yellow)
            },
            new RectangleGeometry
            {
                Width = 20,
                Height = 12,
                Fill = new SolidColorPaint(SKColors.Green)
            },
            new RectangleGeometry
            {
                Width = 20,
                Height = 12,
                Fill = new SolidColorPaint(SKColors.Blue)
            },
            g2 = new RectangleGeometry
            {
                Width = 5,
                Height = 12,
                Fill = new SolidColorPaint(SKColors.Yellow)
            }
        ];

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

        chart.CoreCanvas.AddGeometry(stackPanel);
        _ = chart.GetImage();

        var panelSize = stackPanel.Measure();

        Assert.IsTrue(
            g0.X == 210 &&
            g0.Y == 125 &&
            g1.X == 225 &&
            g1.Y == 120 &&
            g2.X == 245 &&
            g2.Y == 124 &&
            panelSize.Width == 50 &&
            panelSize.Height == 36);
    }
}
