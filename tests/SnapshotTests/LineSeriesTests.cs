using System.Collections.ObjectModel;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using SkiaSharp;

namespace SnapshotTests;

[TestClass]
public sealed class LineSeriesTests
{
    [TestMethod]
    public void Basic()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new LineSeries<int> {Values = [1, 5, 7, 3]},
                new LineSeries<int> {Values = [4, 2, 8, 6]},
                new LineSeries<int> {Values = [2, 6, 4, 8]}
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(LineSeriesTests)}_{nameof(Basic)}");
    }

    [TestMethod]
    public void Area()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new LineSeries<int>
                {
                    Values = [1, 5, 7, 3],
                    Fill = new SolidColorPaint(SKColor.Parse("#6495ED")),
                    Stroke = null,
                    GeometryFill = null,
                    GeometryStroke = null
                }
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(LineSeriesTests)}_{nameof(Area)}");
    }

    [TestMethod]
    public void Straight()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new LineSeries<int>
                {
                    Values = [0, 10, 0, 10, 0],
                    LineSmoothness = 0,
                }
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(LineSeriesTests)}_{nameof(Straight)}");
    }

    [TestMethod]
    public void Curved()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new LineSeries<int>
                {
                    Values = [0, 10, 0, 10, 0],
                    LineSmoothness = 1,
                }
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(LineSeriesTests)}_{nameof(Curved)}");
    }

    [TestMethod]
    public void VectorOperationsRemoveEnd()
    {
        var values = new ObservableCollection<int>([1, 2, 3, 4, 5]);

        var chart = new SKCartesianChart
        {
            Series = [
                new LineSeries<int>
                {
                    Values = values,
                    Fill = null
                }
            ],
            Width = 600,
            Height = 600
        };

        // draw the chart
        _ = chart.GetImage();

        // do the change, redraw and assert
        values.RemoveAt(values.Count - 1);
        chart.AssertSnapshotMatches($"{nameof(LineSeriesTests)}_{nameof(VectorOperationsRemoveEnd)}");
    }

    [TestMethod]
    public void VectorOperationsRemoveStart()
    {
        var values = new ObservableCollection<int>([1, 2, 3, 4, 5]);
        var chart = new SKCartesianChart
        {
            Series = [
                new LineSeries<int>
                {
                    Values = values,
                    Fill = null
                }
            ],
            Width = 600,
            Height = 600
        };
        // draw the chart
        _ = chart.GetImage();
        // do the change, redraw and assert
        values.RemoveAt(0);
        chart.AssertSnapshotMatches($"{nameof(LineSeriesTests)}_{nameof(VectorOperationsRemoveStart)}");
    }

    [TestMethod]
    public void VectorOperationsRemoveMiddle()
    {
        var values = new ObservableCollection<int>([1, 2, 3, 4, 5]);
        var chart = new SKCartesianChart
        {
            Series = [
                new LineSeries<int>
                {
                    Values = values,
                    Fill = null
                }
            ],
            Width = 600,
            Height = 600
        };
        // draw the chart
        _ = chart.GetImage();

        // do the change, redraw and assert
        values.RemoveAt(2);
        chart.AssertSnapshotMatches($"{nameof(LineSeriesTests)}_{nameof(VectorOperationsRemoveMiddle)}");
    }

    [TestMethod]
    public void VectorOperationsAdd()
    {
        var values = new ObservableCollection<int>([1, 2, 3, 4, 5]);
        var chart = new SKCartesianChart
        {
            Series = [
                new LineSeries<int>
                {
                    Values = values,
                    Fill = null
                }
            ],
            Width = 600,
            Height = 600
        };
        // draw the chart
        _ = chart.GetImage();

        // do the change, redraw and assert
        values.Add(6);
        chart.AssertSnapshotMatches($"{nameof(LineSeriesTests)}_{nameof(VectorOperationsAdd)}");
    }

    [TestMethod]
    public void VectorOperationsInsertMiddle()
    {
        var values = new ObservableCollection<int>([1, 2, 3, 4, 5]);

        var chart = new SKCartesianChart
        {
            Series = [
                new LineSeries<int>
                {
                    Values = values,
                    Fill = null
                }
            ],
            Width = 600,
            Height = 600
        };

        // draw the chart
        _ = chart.GetImage();

        // do the change, redraw and assert
        values.Insert(2, 6);
        chart.AssertSnapshotMatches($"{nameof(LineSeriesTests)}_{nameof(VectorOperationsInsertMiddle)}");
    }

    [TestMethod]
    public void VectorOperationsInsertStart()
    {
        var values = new ObservableCollection<int>([1, 2, 3, 4, 5]);
        var chart = new SKCartesianChart
        {
            Series = [
                new LineSeries<int>
                {
                    Values = values,
                    Fill = null
                }
            ],
            Width = 600,
            Height = 600
        };
        // draw the chart
        _ = chart.GetImage();
        // do the change, redraw and assert
        values.Insert(0, 6);
        chart.AssertSnapshotMatches($"{nameof(LineSeriesTests)}_{nameof(VectorOperationsInsertStart)}");
    }

    [TestMethod]
    public void VectorOperationsInsertEnd()
    {
        var values = new ObservableCollection<int>([1, 2, 3, 4, 5]);
        var chart = new SKCartesianChart
        {
            Series = [
                new LineSeries<int>
                {
                    Values = values,
                    Fill = null
                }
            ],
            Width = 600,
            Height = 600
        };
        // draw the chart
        _ = chart.GetImage();
        // do the change, redraw and assert
        values.Insert(values.Count, 6);
        chart.AssertSnapshotMatches($"{nameof(LineSeriesTests)}_{nameof(VectorOperationsInsertEnd)}");
    }

    [TestMethod]
    public void VectorOperationsReplace()
    {
        var values = new ObservableCollection<int>([1, 2, 3, 4, 5]);

        var chart = new SKCartesianChart
        {
            Series = [
                new LineSeries<int>
                {
                    Values = values,
                    Fill = null
                }
            ],
            Width = 600,
            Height = 600
        };

        // draw the chart
        _ = chart.GetImage();

        // do the change, redraw and assert
        values[2] = 6;
        chart.AssertSnapshotMatches($"{nameof(LineSeriesTests)}_{nameof(VectorOperationsReplace)}");
    }

    [TestMethod]
    public void VectorOperationsClear()
    {
        var values = new ObservableCollection<int>([1, 2, 3, 4, 5]);
        var chart = new SKCartesianChart
        {
            Series = [
                new LineSeries<int>
                {
                    Values = values,
                    Fill = null
                }
            ],
            Width = 600,
            Height = 600
        };
        // draw the chart
        _ = chart.GetImage();
        // do the change, redraw and assert
        values.Clear();
        chart.AssertSnapshotMatches($"{nameof(LineSeriesTests)}_{nameof(VectorOperationsClear)}");
    }


    [TestMethod]
    public void Gaps()
    {
        var points = new ObservableCollection<int?>([1, 1]);

        var chart = new SKCartesianChart
        {
            Series = [
                new LineSeries<int?>
                {
                    Values = points,
                    Fill = null
                }
            ],
            Width = 600,
            Height = 600
        };

        _ = chart.GetImage();

        var count = 0;
        int?[] toAdd = [null, 1];

        void Push()
        {
            points.Add(toAdd[count++ % toAdd.Length]);
            points.RemoveAt(0);

            _ = chart.GetImage();
        }

        Push();
        Push();
        Push();
        Push();

        chart.AssertSnapshotMatches($"{nameof(LineSeriesTests)}_{nameof(Gaps)}");
    }

    // Regression guard for https://github.com/Live-Charts/LiveCharts2/issues/2132.
    // Deterministic replay of the AutoUpdateGaps sample that triggered the bug:
    // starting from four ObservablePoints, repeatedly insert midpoints between
    // every consecutive pair, pulling Y values from a pre-baked sequence where
    // values > 6 become null. Each click is rendered between insertions. Through
    // the third pass we hit:
    //   - A sub-segment whose path slot held a single stale segment from the
    //     previous pass, which the now-removed "Count==1" branch would have
    //     discarded by RemoveAt(segmentI), shifting every subsequent sub-segment
    //     onto the wrong path in the container.
    //   - A preserved visual that crossed sub-segment boundaries between passes,
    //     previously subject to the cursor rewind + just-added eviction bug.
    //   - The first point of a brand-new sub-segment whose initial motion state
    //     would, without the unconditional pivot-init, default to (0, 0) and
    //     animate in from the top-left corner.
    // Both classes of regression show up as missing, duplicated, or misplaced
    // line segments in the final static render — pixel-comparing catches them.
    [TestMethod]
    public void Issue2132_InterpolatedInsertsWithGaps()
    {
        double?[] ySequence = [
            6, 1, 1, 5, 1, 2, null, 5, 1, null, 2, 2, 5, 3, 3, 2, 5, 0, null, 5, 3, 1
        ];
        var cursor = 0;

        var values = new ObservableCollection<ObservablePoint>
        {
            new(0, 1), new(1, 2), new(2, 3), new(3, 4)
        };

        var chart = new SKCartesianChart
        {
            Series = [
                new LineSeries<ObservablePoint>
                {
                    Values = values,
                    Fill = null
                }
            ],
            Width = 600,
            Height = 600
        };

        // Cold first draw.
        _ = chart.GetImage();

        // Three passes of interpolated inserts, each re-rendered. After pass 3 the
        // series is 25 points across four sub-segments — enough topology churn to
        // exercise the regressions listed above.
        for (var pass = 0; pass < 3; pass++)
        {
            for (var i = values.Count - 1; i > 0; i--)
            {
                values.Insert(i, new ObservablePoint
                {
                    X = (values[i - 1].X + values[i].X) / 2,
                    Y = ySequence[cursor++ % ySequence.Length]
                });
            }
            _ = chart.GetImage();
        }

        chart.AssertSnapshotMatches($"{nameof(LineSeriesTests)}_{nameof(Issue2132_InterpolatedInsertsWithGaps)}");
    }
}
