using System.Collections.ObjectModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using SkiaSharp;

namespace SnapshotTests;

// Pixel-level regression coverage for https://github.com/Live-Charts/LiveCharts2/issues/1970.
//
// The reporter saw old markers/geometries piling up after a real-time data update.
// Each test renders three frames:
//   * Before: chart with the full dataset (rendered for diagnostic value, also used as
//     the committed reference snapshot so reviewers can see the starting state).
//   * After: same chart instance, after Values are shrunk.
//   * Reference: a brand-new chart built directly with the smaller dataset.
// The "After" image must match the "Reference" image bit-for-bit (within the same
// tolerance the rest of the snapshot suite uses) — any zombie marker, line segment,
// or fill remnant would shift pixels and fail the comparison.
//
// Both the reassignment path (Series.Values = newCollection) and the in-place mutation
// path (ObservableCollection.Clear() + Add()) are covered for both Line and Scatter,
// matching the two flavors in the issue body.
[TestClass]
public sealed class Issue1970Tests
{
    private const int InitialCount = 20;
    private const int UpdatedCount = 5;
    private const int Width = 600;
    private const int Height = 400;

    [TestMethod]
    public void LineSeries_ValuesReassignedToSmallerArray_NoZombieShapes()
    {
        var sutSeries = new LineSeries<ObservableValue> { Values = MakeCollection(InitialCount) };
        var sutChart = NewChart(sutSeries);

        using var beforeImage = sutChart.GetImage();

        sutSeries.Values = MakeCollection(UpdatedCount);
        using var afterImage = sutChart.GetImage();

        AssertMatchesFreshChart(
            afterImage, beforeImage,
            new LineSeries<ObservableValue> { Values = MakeCollection(UpdatedCount) },
            $"{nameof(Issue1970Tests)}_{nameof(LineSeries_ValuesReassignedToSmallerArray_NoZombieShapes)}");
    }

    [TestMethod]
    public void LineSeries_ObservableCollectionClearedAndRepopulatedSmaller_NoZombieShapes()
    {
        var values = MakeCollection(InitialCount);
        var sutSeries = new LineSeries<ObservableValue> { Values = values };
        var sutChart = NewChart(sutSeries);

        using var beforeImage = sutChart.GetImage();

        // Exact pattern from the issue body: mutate the same bound collection in place.
        values.Clear();
        for (var i = 0; i < UpdatedCount; i++) values.Add(new ObservableValue(i + 1));
        using var afterImage = sutChart.GetImage();

        AssertMatchesFreshChart(
            afterImage, beforeImage,
            new LineSeries<ObservableValue> { Values = MakeCollection(UpdatedCount) },
            $"{nameof(Issue1970Tests)}_{nameof(LineSeries_ObservableCollectionClearedAndRepopulatedSmaller_NoZombieShapes)}");
    }

    [TestMethod]
    public void ScatterSeries_ValuesReassignedToSmallerArray_NoZombieShapes()
    {
        var sutSeries = new ScatterSeries<ObservableValue> { Values = MakeCollection(InitialCount) };
        var sutChart = NewChart(sutSeries);

        using var beforeImage = sutChart.GetImage();

        sutSeries.Values = MakeCollection(UpdatedCount);
        using var afterImage = sutChart.GetImage();

        AssertMatchesFreshChart(
            afterImage, beforeImage,
            new ScatterSeries<ObservableValue> { Values = MakeCollection(UpdatedCount) },
            $"{nameof(Issue1970Tests)}_{nameof(ScatterSeries_ValuesReassignedToSmallerArray_NoZombieShapes)}");
    }

    [TestMethod]
    public void ScatterSeries_ObservableCollectionClearedAndRepopulatedSmaller_NoZombieShapes()
    {
        var values = MakeCollection(InitialCount);
        var sutSeries = new ScatterSeries<ObservableValue> { Values = values };
        var sutChart = NewChart(sutSeries);

        using var beforeImage = sutChart.GetImage();

        values.Clear();
        for (var i = 0; i < UpdatedCount; i++) values.Add(new ObservableValue(i + 1));
        using var afterImage = sutChart.GetImage();

        AssertMatchesFreshChart(
            afterImage, beforeImage,
            new ScatterSeries<ObservableValue> { Values = MakeCollection(UpdatedCount) },
            $"{nameof(Issue1970Tests)}_{nameof(ScatterSeries_ObservableCollectionClearedAndRepopulatedSmaller_NoZombieShapes)}");
    }

    private static void AssertMatchesFreshChart(
        SKImage afterImage, SKImage beforeImage, ISeries freshSeries, string testName)
    {
        var freshChart = NewChart(freshSeries);
        using var freshImage = freshChart.GetImage();

        var result = Extensions.Compare(
            freshImage, afterImage,
            perChannelTolerance: 2,
            maxDifferentPixelsRatio: 0.001);

        if (!result.IsSuccessful)
        {
            // Save all three frames for diagnosis: before-shrink, after-shrink, and the
            // fresh small reference. Any zombie shape shows up as a difference between
            // the latter two.
            if (!Directory.Exists("SnapshotsDiff")) _ = Directory.CreateDirectory("SnapshotsDiff");
            SaveImage(beforeImage, Path.Combine("SnapshotsDiff", $"{testName}[BEFORE].png"));
            SaveImage(afterImage, Path.Combine("SnapshotsDiff", $"{testName}[AFTER].png"));
            SaveImage(freshImage, Path.Combine("SnapshotsDiff", $"{testName}[EXPECTED].png"));
        }

        Assert.IsTrue(
            result.IsSuccessful,
            $"after-shrink image differs from a fresh small-dataset render — zombie shapes likely remain. {result.Message}");
    }

    private static ObservableCollection<ObservableValue> MakeCollection(int count)
    {
        var values = new ObservableCollection<ObservableValue>();
        for (var i = 0; i < count; i++) values.Add(new ObservableValue(i + 1));
        return values;
    }

    private static SKCartesianChart NewChart(ISeries series) => new()
    {
        Series = [series],
        Width = Width,
        Height = Height
    };

    private static void SaveImage(SKImage image, string path)
    {
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(path);
        data.SaveTo(stream);
    }
}
