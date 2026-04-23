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
    // Four ObservablePoints on an XY chart, no nulls, rendered cold. PhBrb reported
    // that an earlier fix attempt left the last point's geometry visible but the
    // line didn't reach it — the TrimTail cleanup evicted the bezier segment for the
    // final point because AddConsecutiveSegment's AddLast branch didn't advance
    // _currentNode past the just-appended node. This snapshot is the "all points
    // connected" shape; a regression breaks its pixels.
    [TestMethod]
    public void Issue2132_LastPointStaysConnected()
    {
        var chart = new SKCartesianChart
        {
            Series = [
                new LineSeries<ObservablePoint>
                {
                    Values = [
                        new(0, 1), new(1, 2), new(2, 3), new(3, 4)
                    ],
                    Fill = null
                }
            ],
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(LineSeriesTests)}_{nameof(Issue2132_LastPointStaysConnected)}");
    }
}
