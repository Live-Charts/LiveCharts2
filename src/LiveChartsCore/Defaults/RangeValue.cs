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
/// Defines a value with low / high endpoints, suitable for range column and range row series.
/// </summary>
public class RangeValue : IChartEntity, INotifyPropertyChanged
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RangeValue"/> class.
    /// </summary>
    public RangeValue()
    {
        MetaData = new ChartEntityMetaData(OnCoordinateChanged);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RangeValue"/> class.
    /// </summary>
    /// <param name="low">The low endpoint (bottom of a range column, left of a range row).</param>
    /// <param name="high">The high endpoint (top of a range column, right of a range row).</param>
    public RangeValue(double low, double high)
        : this()
    {
        Low = low;
        High = high;
    }

    /// <summary>
    /// Gets or sets the low endpoint of the range. Null marks the point as empty.
    /// </summary>
    public double? Low { get; set { field = value; OnPropertyChanged(); } }

    /// <summary>
    /// Gets or sets the high endpoint of the range. Null marks the point as empty.
    /// </summary>
    public double? High { get; set { field = value; OnPropertyChanged(); } }

    /// <inheritdoc cref="IChartEntity.MetaData"/>
    [System.Text.Json.Serialization.JsonIgnore]
    public ChartEntityMetaData? MetaData { get; set; }

    /// <inheritdoc cref="IChartEntity.Coordinate"/>
    [System.Text.Json.Serialization.JsonIgnore]
    public Coordinate Coordinate { get; set; } = Coordinate.Empty;

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Called when a property changed.
    /// </summary>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        if (MetaData is not null) OnCoordinateChanged(MetaData.EntityIndex);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Called when the entity index changes (after a measure pass) or any data property mutates.
    /// </summary>
    protected virtual void OnCoordinateChanged(int index)
    {
        // PrimaryValue = high, TertiaryValue = low. Range series consumes the
        // coordinate from these slots; X is the EntityIndex carried in SecondaryValue.
        Coordinate = High is null || Low is null
            ? Coordinate.Empty
            : new(High.Value, index, Low.Value, 0, 0, 0, Error.Empty);
    }
}
