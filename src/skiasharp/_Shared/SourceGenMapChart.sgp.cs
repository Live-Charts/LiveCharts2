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

#pragma warning disable IDE0005 // Using directive is unnecessary.
#pragma warning disable IDE0060 // Remove unused parameter

using LiveChartsCore;
using LiveChartsCore.Geo;
using LiveChartsCore.Generators;
using System.Windows.Input;
using System;
using LiveChartsCore.Painting;
using System.Collections.Generic;
using LiveChartsCore.Drawing;

#if SKIA_IMAGE_LVC
using SGChart = LiveChartsGeneratedCode.SourceGenSKMapChart;
#else
using SGChart = LiveChartsGeneratedCode.SourceGenMapChart;
#endif

// ==============================================================================================================
// the static fileds in this file generate bindable/dependency/avalonia or whatever properties...
// the disabled warning make it easier to maintain the code.
//
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable IDE0052 // Remove unread private member
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable CS8618  // Non-nullable field must contain a non-null value when exiting constructor.
#pragma warning disable CS0169  // The field is never used
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0040 // Add accessibility modifiers
#pragma warning disable format
// ==============================================================================================================

namespace LiveChartsGeneratedCode;

#if SKIA_IMAGE_LVC
public partial class SourceGenSKMapChart
#else
public partial class SourceGenMapChart
#endif
{
    /// <inheritdoc cref="IGeoMapView.SyncContext"/>
    static UIProperty<object>                   syncContext         = new(onChanged: OnSyncContextChanged);

    /// <inheritdoc cref="IGeoMapView.ActiveMap"/>
    static UIProperty<DrawnMap>                 activeMap;

    /// <inheritdoc cref="IGeoMapView.MapProjection"/>
    static UIProperty<MapProjection>            mapProjection       = new(defaultValue: MapProjection.Default);

    /// <inheritdoc cref="IGeoMapView.Stroke"/>
    static UIProperty<Paint>                    stroke              = new(defaultValue: GetPaint(new(255, 255, 255, 255), PaintStyle.Stroke), onChanged: OnPaintPropertyChanged(nameof(Stroke)));

    /// <inheritdoc cref="IGeoMapView.Fill"/>
    static UIProperty<Paint>                    fill                = new(defaultValue: GetPaint(new(240, 240, 240, 255), PaintStyle.Fill), onChanged: OnPaintPropertyChanged(nameof(Fill)));

    /// <inheritdoc cref="IGeoMapView.Series"/>
    static UIProperty<IEnumerable<IGeoSeries>>  series              = new(onChanged: OnSeriesPropertyChanged);

    static void OnSyncContextChanged(SGChart chart, object oldValue, object newValue)
    {
#if BLAZOR_LVC
        // hack for blazor, we need to wait for the OnAfterRender to have
        // a reference to the canvas in the UI, CoreChart is null until then.
        if (chart.CoreChart is null) return;
#endif
        chart.CoreCanvas.Sync = newValue;
        chart.CoreChart.Update();
    }

    static void OnSeriesPropertyChanged(SGChart chart, object oldValue, object newValue)
    {
#if BLAZOR_LVC
        // hack for blazor, we need to wait for the OnAfterRender to have
        // a reference to the canvas in the UI, CoreChart is null until then.
        if (chart.CoreChart is null) return;
#endif
        chart._seriesObserver?.Dispose();
        chart._seriesObserver?.Initialize((IEnumerable<IGeoSeries>)newValue);
        chart.CoreChart.Update();
    }

    static Action<SGChart, object, object> OnPaintPropertyChanged(
        string propertyName, object? oldValue = null, object? newValue = null) =>
        (chart, o, n) =>
        {
            if (n is not Paint newPaint) return;

            newPaint.PaintStyle = propertyName == nameof(Fill)
                ? PaintStyle.Fill
                : PaintStyle.Stroke;
        };

    static Paint GetPaint(LvcColor color, PaintStyle style)
    {
        var paint = LiveCharts.DefaultSettings.GetProvider().GetSolidColorPaint(color);
        paint.PaintStyle = style;
        return paint;
    }

#if AVALONIA_LVC
    // avalonia hack to mock the DependencyProperty.OnPropertyChanged delegate.

    /// <inheritdoc />
    protected override void OnPropertyChanged(Avalonia.AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property.Name == nameof(IsPointerOver)) return;

        OnXamlPropertyChanged(change);
    }
#endif
}
