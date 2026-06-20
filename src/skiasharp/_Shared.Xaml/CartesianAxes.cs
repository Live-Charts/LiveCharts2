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

#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable IDE0052 // Remove unread private member
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable CS8618  // Non-nullable field must contain a non-null value when exiting constructor.
#pragma warning disable CS0169  // The field is never used
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0040 // Add accessibility modifiers
#pragma warning disable format

using System;
using System.Collections.Generic;
using LiveChartsCore.Generators;
using LiveChartsCore.Kernel.Sketches;

#if AVALONIA_LVC
using Avalonia;
using BaseControl = Avalonia.Controls.Control;
namespace LiveChartsCore.SkiaSharpView.Avalonia;
#elif MAUI_LVC
using BaseControl = LiveChartsCore.SkiaSharpView.Maui.Handlers.EmptyContentView;
namespace LiveChartsCore.SkiaSharpView.Maui;
#elif WINUI_LVC
using BaseControl = Microsoft.UI.Xaml.FrameworkElement;
namespace LiveChartsCore.SkiaSharpView.WinUI;
#elif WPF_LVC
using BaseControl = System.Windows.FrameworkElement;
namespace LiveChartsCore.SkiaSharpView.WPF;
#endif

/// <summary>
/// The base class for XAML-based axes in LiveCharts.
/// </summary>
/// <typeparam name="T">The type of the axis.</typeparam>
[XamlClass(typeof(Axis), GenerateBaseTypeDeclaration = false, PropertyChangeMap = "MinLimit{=}MinLimitMap{,}MaxLimit{=}MaxLimitMap")]
public abstract partial class BaseXamlAxis<T> : BaseControl, ICartesianAxis
    where T : Axis
{
    private static readonly Dictionary<Type, Func<object>> _defaults = new()
    {
        [typeof(Axis)] = () => new Axis(),
        [typeof(DateTimeAxis)] = () => new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("d")),
        [typeof(TimeSpanAxis)] = () => new TimeSpanAxis(TimeSpan.FromMilliseconds(1), date => $"{date:fff}ms"),
        [typeof(LogarithmicAxis)] = () => new LogarithmicAxis(10d)
    };

    private readonly T _baseType = (T)_defaults[typeof(T)]();
    internal static T _defaultAxis = (T)_defaults[typeof(T)]();

    string? IPlane.Name { get => _baseType.Name; set => _baseType.Name = value; }
    double? IPlane.MinLimit { get => _baseType.MinLimit; set => _baseType.MinLimit = value; }
    double? IPlane.MaxLimit { get => _baseType.MaxLimit; set => _baseType.MaxLimit = value; }

    private void MinLimitMap(object? value) =>
        _baseType.MinLimit = (double?)value;

    private void MaxLimitMap(object? value) =>
        _baseType.MaxLimit = (double?)value;
}

/// <inheritdoc cref="Axis"/>
public partial class XamlAxis : BaseXamlAxis<Axis>
{ }

/// <inheritdoc cref="DateTimeAxis"/>
public partial class XamlDateTimeAxis : BaseXamlAxis<DateTimeAxis>
{
    static UIProperty<TimeSpan>                 interval =      new(TimeSpan.FromDays(1), XamlGeneration.OnAxisIntervalChanged);
    static UIProperty<Func<DateTime, string>>   dateFormatter = new(null, XamlGeneration.OnDateTimeAxisDateFormatterChanged);
    static UIProperty<bool>                     groupTimeUnits = new(false, OnGroupTimeUnitsChanged);

    private static void OnGroupTimeUnitsChanged(IXamlWrapper<ICartesianAxis> axis, bool oldValue, bool newValue) =>
        ((DateTimeAxis)axis.WrappedObject).GroupTimeUnits = newValue;

#if AVALONIA_LVC
    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        // hack #250809
        // we cant define property change handlers in avalonia properties,
        // required to run the property change of the generated UIProperty
        base.OnPropertyChanged(change);
        OnXamlPropertyChanged(change);
    }
#endif
}

/// <inheritdoc cref="TimeSpanAxis"/>
public partial class XamlTimeSpanAxis : BaseXamlAxis<TimeSpanAxis>
{
    static UIProperty<TimeSpan>                 interval =      new(TimeSpan.FromSeconds(1), XamlGeneration.OnAxisIntervalChanged);
    static UIProperty<Func<TimeSpan, string>>   timeFormatter = new(null, XamlGeneration.OnTimeSpanAxisFormatterChanged);
    static UIProperty<bool>                     groupTimeUnits = new(false, OnGroupTimeUnitsChanged);

    private static void OnGroupTimeUnitsChanged(IXamlWrapper<ICartesianAxis> axis, bool oldValue, bool newValue) =>
        ((TimeSpanAxis)axis.WrappedObject).GroupTimeUnits = newValue;

#if AVALONIA_LVC
    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        // hack #250809
        base.OnPropertyChanged(change);
        OnXamlPropertyChanged(change);
    }
#endif
}

/// <inheritdoc cref="LogarithmicAxis"/>
public partial class XamlLogarithmicAxis : BaseXamlAxis<LogarithmicAxis>
{
    static UIProperty<double>                   logBase =       new(10d, XamlGeneration.OnAxisLogBaseChanged);

#if AVALONIA_LVC
    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        // hack #250809
        base.OnPropertyChanged(change);
        OnXamlPropertyChanged(change);
    }
#endif
}
