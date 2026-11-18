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

using System.ComponentModel;
using System.Runtime.CompilerServices;
using LiveChartsCore.Kernel;

namespace LiveChartsCore.Defaults;

/// <summary>
/// Defines a point with a weighted coordinate.
/// </summary>
/// <seealso cref="INotifyPropertyChanged" />
public class WeightedPoint : IChartEntity, INotifyPropertyChanged
{

    /// <summary>
    /// Initializes a new instance of the <see cref="WeightedPoint"/> class.
    /// </summary>
    public WeightedPoint()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="WeightedPoint"/> class.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <param name="weight">The weight.</param>
    public WeightedPoint(double? x, double? y, double? weight)
    {
        X = x;
        Y = y;
        Weight = weight;
    }

    /// <summary>
    /// Gets or sets the x.
    /// </summary>
    /// <value>
    /// The x.
    /// </value>
    public double? X { get; set { field = value; OnPropertyChanged(); } }

    /// <summary>
    /// Gets or sets the y.
    /// </summary>
    /// <value>
    /// The y.
    /// </value>
    public double? Y { get; set { field = value; OnPropertyChanged(); } }

    /// <summary>
    /// Gets or sets the weight.
    /// </summary>
    /// <value>
    /// The weight.
    /// </value>
    public double? Weight { get; set { field = value; OnPropertyChanged(); } }

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    /// <returns></returns>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc cref="IChartEntity.MetaData"/>
    [System.Text.Json.Serialization.JsonIgnore]
    public ChartEntityMetaData? MetaData { get; set; }

    /// <inheritdoc cref="IChartEntity.Coordinate"/>
    [System.Text.Json.Serialization.JsonIgnore]
    public Coordinate Coordinate { get; set; } = Coordinate.Empty;

    /// <summary>
    /// Called when a property changed.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        // a NaN X or Y makes the coordinate empty like null; a NaN weight counts as no weight, like
        // null (#2340).
        Coordinate = X is null || Y is null
            ? Coordinate.Empty
            : new(X ?? 0, Y ?? 0, Weight is { } weight && !double.IsNaN(weight) ? weight : 0);

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
