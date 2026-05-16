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

using LiveChartsCore.Generators;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Drawing;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Providers;

#if AVALONIA_LVC
using Avalonia;
namespace LiveChartsCore.SkiaSharpView.Avalonia;
#elif MAUI_LVC
namespace LiveChartsCore.SkiaSharpView.Maui;
#elif WINUI_LVC
namespace LiveChartsCore.SkiaSharpView.WinUI;
#elif WPF_LVC
namespace LiveChartsCore.SkiaSharpView.WPF;
#endif

[XamlClass(typeof(ColumnSeries<,,>), TVisual = typeof(RoundedRectangleGeometry))]
public partial class XamlColumnSeries<TModel, TVisual, TLabel> : XamlSeries, IBarSeries, IInternalSeries
    where TVisual : BoundedDrawnGeometry, new()
    where TLabel : BaseLabelGeometry, new()
{ }

[XamlClass(typeof(RowSeries<,,>), TVisual = typeof(RoundedRectangleGeometry))]
public partial class XamlRowSeries<TModel, TVisual, TLabel> : XamlSeries, IBarSeries, IInternalSeries
    where TVisual : BoundedDrawnGeometry, new()
    where TLabel : BaseLabelGeometry, new()
{ }

[XamlClass(typeof(LineSeries<,,>), TVisual = typeof(CircleGeometry))]
public partial class XamlLineSeries<TModel, TVisual, TLabel> : XamlSeries, ILineSeries, IInternalSeries
    where TVisual : BoundedDrawnGeometry, new()
    where TLabel : BaseLabelGeometry, new()
{ }

[XamlClass(typeof(StepLineSeries<,,>), TVisual = typeof(CircleGeometry))]
public partial class XamlStepLineSeries<TModel, TVisual, TLabel> : XamlSeries, IStepLineSeries, IInternalSeries
    where TVisual : BoundedDrawnGeometry, new()
    where TLabel : BaseLabelGeometry, new()
{ }

[XamlClass(typeof(ScatterSeries<,,>), TVisual = typeof(CircleGeometry))]
public partial class XamlScatterSeries<TModel, TVisual, TLabel> : XamlSeries, IScatterSeries, IInternalSeries
    where TVisual : BoundedDrawnGeometry, new()
    where TLabel : BaseLabelGeometry, new()
{ }

[XamlClass(typeof(CandlesticksSeries<,,>), TVisual = typeof(CandlestickGeometry))]
public partial class XamlCandlesticksSeries<TModel, TVisual, TLabel> : XamlSeries, IFinancialSeries, IInternalSeries
    where TVisual : BaseCandlestickGeometry, new()
    where TLabel : BaseLabelGeometry, new()
{ }

[XamlClass(typeof(BoxSeries<,,>), TVisual = typeof(BoxGeometry))]
public partial class XamlBoxSeries<TModel, TVisual, TLabel> : XamlSeries, IBoxSeries, IInternalSeries
    where TVisual : BaseBoxGeometry, new()
    where TLabel : BaseLabelGeometry, new()
{ }

[XamlClass(typeof(HeatSeries<,,>), TVisual = typeof(ColoredRectangleGeometry))]
public partial class XamlHeatSeries<TModel, TVisual, TLabel> : XamlSeries, IHeatSeries, IInternalSeries
    where TVisual : BoundedDrawnGeometry, IColoredGeometry, new()
    where TLabel : BaseLabelGeometry, new()
{ }

[XamlClass(typeof(PieSeries<,,>), TVisual = typeof(DoughnutGeometry))]
public partial class XamlPieSeries<TModel, TVisual, TLabel> : XamlSeries, IPieSeries, IInternalSeries
    where TVisual : BaseDoughnutGeometry, new()
    where TLabel : BaseLabelGeometry, new()
{ }

[XamlClass(typeof(PolarLineSeries<,,>), TVisual = typeof(CircleGeometry))]
public partial class XamlPolarLineSeries<TModel, TVisual, TLabel> : XamlSeries, IPolarLineSeries, IInternalSeries
    where TVisual : BoundedDrawnGeometry, new()
    where TLabel : BaseLabelGeometry, new()
{ }

[XamlClass(typeof(StackedAreaSeries<,,>), TVisual = typeof(CircleGeometry))]
public partial class XamlStackedAreaSeries<TModel, TVisual, TLabel> : XamlSeries, ILineSeries, IInternalSeries
    where TVisual : BoundedDrawnGeometry, new()
    where TLabel : BaseLabelGeometry, new()
{ }

[XamlClass(typeof(StackedStepAreaSeries<,,>), TVisual = typeof(CircleGeometry))]
public partial class XamlStackedStepAreaSeries<TModel, TVisual, TLabel> : XamlSeries, IStepLineSeries, IInternalSeries
    where TVisual : BoundedDrawnGeometry, new()
    where TLabel : BaseLabelGeometry, new()
{ }

[XamlClass(typeof(StackedColumnSeries<,,>), TVisual = typeof(RoundedRectangleGeometry))]
public partial class XamlStackedColumnSeries<TModel, TVisual, TLabel> : XamlSeries, IBarSeries, IInternalSeries
    where TVisual : BoundedDrawnGeometry, new()
    where TLabel : BaseLabelGeometry, new()
{ }

[XamlClass(typeof(StackedRowSeries<,,>), TVisual = typeof(RoundedRectangleGeometry))]
public partial class XamlStackedRowSeries<TModel, TVisual, TLabel> : XamlSeries, IBarSeries, IInternalSeries
    where TVisual : BoundedDrawnGeometry, new()
    where TLabel : BaseLabelGeometry, new()
{ }

#if AVALONIA_LVC
[XamlClass(typeof(PieSeries<,,>),
    FileHeader = "using TModel = LiveChartsCore.Defaults.ObservableValue;",
    TVisual = typeof(DoughnutGeometry),
    GenerateOnChange = false)]
#else
[XamlClass(typeof(PieSeries<,,>),
    FileHeader = "using TModel = LiveChartsCore.Defaults.ObservableValue;",
    TVisual = typeof(DoughnutGeometry))]
#endif
public partial class XamlGaugeSeries<TVisual, TLabel> : XamlSeries, IPieSeries, IInternalSeries
    where TVisual : BaseDoughnutGeometry, new()
    where TLabel : BaseLabelGeometry, new()
{
    private readonly ObservableValue _value = new(0d);
#pragma warning disable IDE0052 // Remove unread private members
#pragma warning disable IDE1006 // Naming Styles
    private static readonly UIProperty<double> gaugeValue = new(0d, OnGaugeValueChanged);
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0052 // Remove unread private members

    /// <inheritdoc cref="PieSeries{TModel, TVisual, TLabel}.PieSeries()"/>
    public XamlGaugeSeries()
    {
        Setup(this, _value, _baseType.DataFactory);
    }

    internal virtual void Setup(
        IXamlWrapper<IPieSeries> series, ObservableValue value, DataFactory<ObservableValue> dataFactory)
            => XamlGeneration.SetupGaugeSeries(this, _value, _baseType.DataFactory);

    private static void OnGaugeValueChanged(
        XamlGaugeSeries<TVisual, TLabel> series, double oldValue, double newValue) =>
            series._value.Value = newValue;

#if AVALONIA_LVC
    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        // we cant define property change handlers in avalonia properties,
        // we map the change manually.
        MapChangeToBaseType(change.Property.Name);
        OnXamlPropertyChanged(change);
    }

    /// <inheritdoc />
    public override void EndInit()
    {
        base.EndInit();

        // Avalonia skips OnPropertyChanged when SetValue assigns a value equal to the
        // property's default (e.g. CornerRadius="0" where the DP default is also 0).
        // MapChangeToBaseType then never runs for that property, _userSets stays empty,
        // and a theme rule (HasRuleForGaugeSeries setting CornerRadius=8) silently
        // overrides the user's explicit value (issue #2008). Series controls don't
        // get rooted in the visual tree (they're collected by Series, not Children),
        // so OnInitialized never fires for them — but EndInit does, after the XAML
        // parser has applied every value.
        //
        // Walk the registered DPs and sync any property whose value source isn't the
        // default. MapChangeToBaseType pushes the value into the wrapped CorePieSeries
        // and SetProperty there records the property in _userSets, blocking subsequent
        // theme overrides. Properties at default source are skipped so theme defaults
        // still apply for unspecified properties.
        foreach (var prop in global::Avalonia.AvaloniaPropertyRegistry.Instance.GetRegistered(GetType()))
        {
            if (IsSet(prop))
                MapChangeToBaseType(prop.Name);
        }
    }
#endif
}

/// <inheritdoc cref="PieSeries{TModel, TVisual, TLabel}"/>
public partial class XamlGaugeBackgroundSeries : XamlGaugeSeries<DoughnutGeometry, LabelGeometry>
{
    internal override void Setup(
        IXamlWrapper<IPieSeries> series, ObservableValue value, DataFactory<ObservableValue> dataFactory)
            => XamlGeneration.SetupBackgroundGaugeSeries(this, value, dataFactory);
}

/// <inheritdoc cref="PieSeries{TModel, TVisual, TLabel}"/>
public partial class XamlAngularGaugeSeries : XamlGaugeSeries<DoughnutGeometry, LabelGeometry>
{
    internal override void Setup(
        IXamlWrapper<IPieSeries> series, ObservableValue value, DataFactory<ObservableValue> dataFactory)
            => XamlGeneration.SetupAngularGaugeSeries(this, value, dataFactory);
}
