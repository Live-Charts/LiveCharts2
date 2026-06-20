using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsGeneratedCode;
using SkiaSharp;

namespace SnapshotTests;

public static class Extensions
{
    extension(SourceGenSKChart chart)
    {
        public void PointerAt(double x, double y)
        {
            chart.CoreChart._isPointerIn = true;
            chart.CoreChart._isToolTipOpen = true;
            chart.CoreChart._pointerPosition = new(x, y);
        }
    }

    extension(InMemorySkiaSharpChart chart)
    {
        public void AssertSnapshotMatches(string name)
        {
            if (!Directory.Exists("SnapshotsNew")) _ = Directory.CreateDirectory("SnapshotsNew");
            if (!Directory.Exists("SnapshotsDiff")) _ = Directory.CreateDirectory("SnapshotsDiff");

            var newPath = Path.Combine("SnapshotsNew", $"{name}.png");
            var referencePath = Path.Combine("Snapshots", $"{name}.png");

            chart.SaveImage(newPath);

            if (!File.Exists(referencePath))
            {
                File.Copy(newPath, Path.Combine("SnapshotsDiff", $"{name}[NEW].png"), overwrite: true);
                Assert.Fail(
                    $"Reference snapshot not found for '{name}'. " +
                    $"A new snapshot was generated at '{newPath}'. " +
                    $"Review it and commit it to the Snapshots folder as '{referencePath}'.");
                return;
            }

            ImageComparisonResult result;

            using (var data = SKData.Create(referencePath))
            using (var expectedEncoded = SKImage.FromEncodedData(data))
            using (var resultData = SKData.Create(newPath))
            using (var resultImage = SKImage.FromEncodedData(resultData))
            using (var expectedBitmap = SKBitmap.FromImage(expectedEncoded))
            using (var expectedCpu = SKImage.FromBitmap(expectedBitmap))
            using (var actualBitmap = SKBitmap.FromImage(resultImage))
            using (var actualCpu = SKImage.FromBitmap(actualBitmap))
            {
                result = Compare(
                    expectedCpu, actualCpu, perChannelTolerance: 2, maxDifferentPixelsRatio: 0.001);
            }

            if (!result.IsSuccessful)
            {
                File.Copy(
                    referencePath,
                    Path.Combine("SnapshotsDiff", $"{name}[EXPECTED].png"));

                File.Copy(
                    newPath,
                    Path.Combine("SnapshotsDiff", $"{name}[RESULT].png"));
            }

            Assert.IsTrue(result.IsSuccessful, result.Message);
        }
    }

    public static ImageComparisonResult Compare(
        SKImage expected,
        SKImage actual,
        int perChannelTolerance = 0,          // 0 = exact match
        double maxDifferentPixelsRatio = 0.0, // 0 = no differences allowed
        string? diffOutputPath = null)
    {
        // Convert to pixmaps (zero-copy access to raw pixels)
        using var expectedPixmap = expected.PeekPixels();
        using var actualPixmap = actual.PeekPixels();

        if (expectedPixmap == null || actualPixmap == null)
            return ImageComparisonResult.Fail("Unable to access pixel data.");

        if (expectedPixmap.Width != actualPixmap.Width ||
            expectedPixmap.Height != actualPixmap.Height)
        {
            return ImageComparisonResult.Fail("Image sizes differ.");
        }

        var width = expectedPixmap.Width;
        var height = expectedPixmap.Height;
        var totalPixels = width * height;

        var differentPixels = 0;

        SKBitmap? diffBitmap = null;
        if (diffOutputPath != null)
            diffBitmap = new SKBitmap(width, height);

        unsafe
        {
            var ePtr = (byte*)expectedPixmap.GetPixels();
            var aPtr = (byte*)actualPixmap.GetPixels();

            var byteCount = totalPixels * 4; // RGBA

            for (var i = 0; i < byteCount; i += 4)
            {
                var eR = ePtr[i + 0];
                var eG = ePtr[i + 1];
                var eB = ePtr[i + 2];
                var eA = ePtr[i + 3];

                var aR = aPtr[i + 0];
                var aG = aPtr[i + 1];
                var aB = aPtr[i + 2];
                var aA = aPtr[i + 3];

                var isDifferent =
                    Math.Abs(eR - aR) > perChannelTolerance ||
                    Math.Abs(eG - aG) > perChannelTolerance ||
                    Math.Abs(eB - aB) > perChannelTolerance ||
                    Math.Abs(eA - aA) > perChannelTolerance;

                if (isDifferent)
                {
                    differentPixels++;

                    if (diffBitmap != null)
                    {
                        var pixelIndex = i / 4;
                        var x = pixelIndex % width;
                        var y = pixelIndex / width;

                        diffBitmap.SetPixel(x, y, new SKColor(255, 0, 0)); // red highlight
                    }

                    // Early exit if too many differences
                    if ((double)differentPixels / totalPixels > maxDifferentPixelsRatio)
                        break;
                }
                else if (diffBitmap != null)
                {
                    var pixelIndex = i / 4;
                    var x = pixelIndex % width;
                    var y = pixelIndex / width;

                    var gray = (byte)((eR + eG + eB) / 3);
                    diffBitmap.SetPixel(x, y, new SKColor(gray, gray, gray));
                }
            }
        }

        // Save diff image if requested
        if (diffBitmap != null)
        {
            using var img = SKImage.FromBitmap(diffBitmap);
            using var data = img.Encode(SKEncodedImageFormat.Png, 100);
            File.WriteAllBytes(diffOutputPath!, data.ToArray());
        }

        var ratio = (double)differentPixels / totalPixels;

        return ratio <= maxDifferentPixelsRatio
            ? ImageComparisonResult.Success()
            : ImageComparisonResult.Fail($"Different pixels ratio {ratio:P2} exceeds allowed {maxDifferentPixelsRatio:P2}");
    }

    public record ImageComparisonResult(bool IsSuccessful, string Message)
    {
        public static ImageComparisonResult Success() => new(true, "Images match.");
        public static ImageComparisonResult Fail(string msg) => new(false, msg);
    }

}
