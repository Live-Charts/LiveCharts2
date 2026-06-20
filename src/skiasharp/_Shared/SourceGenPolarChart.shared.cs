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

using System.Collections.ObjectModel;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Observers;
using LiveChartsCore.Kernel.Sketches;

#if SKIA_IMAGE_LVC
using SGChart = LiveChartsGeneratedCode.SourceGenSKChart;
#else
using SGChart = LiveChartsGeneratedCode.SourceGenChart;
#endif

namespace LiveChartsGeneratedCode;

// ==============================================================
// this file contains the shared code between all UI frameworks
// ==============================================================

/// <inheritdoc cref="IPolarChartView" />
#if SKIA_IMAGE_LVC
public partial class SourceGenSKPolarChart : SGChart, IPolarChartView
#else
public partial class SourceGenPolarChart : SGChart, IPolarChartView
#endif
{
    PolarChartEngine IPolarChartView.Core => (PolarChartEngine)CoreChart;

    /// <inheritdoc cref="IPolarChartView.ScalePixelsToData(LvcPointD, int, int)"/>
    public LvcPointD ScalePixelsToData(LvcPointD point, int angleAxisIndex = 0, int radiusAxisIndex = 0)
        => ((PolarChartEngine)CoreChart).ScalePixelsToData(point, angleAxisIndex, radiusAxisIndex);

    /// <inheritdoc cref="IPolarChartView.ScaleDataToPixels(LvcPointD, int, int)"/>
    public LvcPointD ScaleDataToPixels(LvcPointD point, int angleAxisIndex = 0, int radiusAxisIndex = 0)
        => ((PolarChartEngine)CoreChart).ScaleDataToPixels(point, angleAxisIndex, radiusAxisIndex);

    /// <inheritdoc cref="SGChart.CreateCoreChart"/>
    protected override Chart CreateCoreChart() =>
        new PolarChartEngine(this, CoreCanvas);

    /// <inheritdoc cref="SGChart.ConfigureObserver(ChartObserver)"/>
    protected override ChartObserver ConfigureObserver(ChartObserver observe)
    {
        return base.ConfigureObserver(observe)
            .Collection(nameof(AngleAxes), () => AngleAxes)
            .Collection(nameof(RadiusAxes), () => RadiusAxes);
    }

    /// <inheritdoc cref="SGChart.InitializeObservedProperties"/>
    protected override void InitializeObservedProperties()
    {
        SyncContext = new object();
#if !WPF_LVC
        // WPF lazy-inits these in the property getter via SetCurrentValue
        // (issue #1981) so this constructor-time SetValue would block the
        // FrameworkElementFactory binding. Other platforms pre-init here so
        // `chart.Series.Add(...)` works pre-Loaded.
        AngleAxes = new ObservableCollection<IPolarAxis>();
        RadiusAxes = new ObservableCollection<IPolarAxis>();
        Series = new ObservableCollection<ISeries>();
        VisualElements = new ObservableCollection<IChartElement>();
#endif
    }
}
