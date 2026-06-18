// The MIT License(MIT)
//
// Copyright(c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using Factos;
using LiveChartsCore;
using LiveChartsCore.Geo;
using LiveChartsCore.Kernel.Sketches;
using Xunit;

namespace SharedUITests.Helpers;

public static class UIHelpersExtensions
{
    extension(AppController controller)
    {
        public async Task<T> NavigateTo<T>()
            where T : class, new()
        {
#if BLAZOR_UI_TESTING
            var blazorController = (Factos.Blazor.BlazorAppController)controller;
            return await blazorController.NavigateToView<T>();
#else
            var view = new T();
            await controller.NavigateToView(view);
            return view;
#endif
        }

    }

    extension(IChartView chartView)
    {
        public async Task WaitUntilChartLoadsAndRenders(int timeoutMs = 10000, int pollMs = 50)
        {
            bool HasRenderedPoints()
            {
                if (!chartView.CoreChart.IsLoaded)
                    return false;

                var points = (chartView.Series ?? [])
                    .SelectMany(s => s.Fetch(chartView.CoreChart))
                    .ToArray();

                // mirrors Assert.ChartIsLoaded: non-empty, every point has a positioned visual
                return points.Length > 0 && points.All(p =>
                {
                    var v = p.Context.Visual;
                    return v is not null && v.X > 0 && v.Y > 0;
                });
            }

            var waited = 0;
            while (waited < timeoutMs && !HasRenderedPoints())
            {
                await Task.Delay(pollMs);
                waited += pollMs;
            }
        }

        public Task WaitUntilChartRenders()
        {
            var tcs = new TaskCompletionSource<object>();

            if (chartView.CoreCanvas.IsValid)
            {
                return Task.FromResult(new object());
            }

            // force an update, then wait for the update to start in the ui thread
            chartView.CoreChart.Update(new LiveChartsCore.Kernel.ChartUpdateParams
            {
                IsAutomaticUpdate = false,
                Throttling = false
            });

            void Handler(IChartView chart)
            {
                chartView.UpdateStarted -= Handler;
                tcs.SetResult(new());
            }

            chartView.UpdateStarted += Handler;

            return tcs.Task;
        }
    }

    extension(IGeoMapView chartView)
    {
        public async Task WaitUntilChartRenders()
        {
            // force an update, then wait for the update to start in the ui thread
            chartView.CoreChart.Update(new LiveChartsCore.Kernel.ChartUpdateParams
            {
                IsAutomaticUpdate = false,
                Throttling = false
            });

            await Task.Delay(1000);
        }
    }

    extension(Assert)
    {
        public static void ChartIsLoaded(IChartView chartView)
        {
            Assert.True(chartView.CoreChart.IsLoaded);

            var fetchedPoints = chartView.Series
                .SelectMany(s => s.Fetch(chartView.CoreChart))
                .ToArray();

            Assert.NotEmpty(fetchedPoints);

            Assert.All(fetchedPoints, point =>
            {
                // this only validates that points have a size and position
                // but the proper scale and position is tested in LiveChartsCore.UnitTesting

                var v = point.Context.Visual;
                Assert.NotNull(v);
                Assert.True(v.X > 0);
                Assert.True(v.Y > 0);
            });
        }

        public static void ChartIsLoaded(IGeoMapView chartView)
        {
            var strokeHasContent = chartView.Stroke != null && !chartView.Stroke.IsEmpty;
            var fillHasContent = chartView.Fill != null && !chartView.Fill.IsEmpty;

            Assert.True(strokeHasContent || fillHasContent);
        }

        public static void ChartIsLoaded(ITreemapChartView chartView)
        {
            // Treemap series manage tile visuals directly (no ChartPoint
            // round-trip via DataFactory), so the generic IChartView overload's
            // s.Fetch(...) path doesn't apply — it would throw on a HasMap
            // lookup for hierarchical user types. Instead validate that
            // the canvas accumulated geometries during the first measure.
            Assert.True(chartView.CoreChart.IsLoaded);
            Assert.True(chartView.CoreChart.Canvas.CountGeometries() > 0);
        }

        public static void ChartIsLoaded(ISankeyChartView chartView)
        {
            // Sankey series manage node + ribbon visuals directly (no
            // ChartPoint round-trip via DataFactory), so the generic
            // IChartView overload's s.Fetch(...) path would throw on a
            // HasMap lookup for the user's node type. Instead validate that
            // the canvas accumulated geometries during the first measure.
            Assert.True(chartView.CoreChart.IsLoaded);
            Assert.True(chartView.CoreChart.Canvas.CountGeometries() > 0);
        }
    }
}
