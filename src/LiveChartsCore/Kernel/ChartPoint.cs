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

using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Drawing;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;

namespace LiveChartsCore.Kernel;

/// <summary>
/// Defines a point in a chart.
/// </summary>
public class ChartPoint
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChartPoint"/> class.
    /// </summary>
    /// <param name="chart">The chart.</param>
    /// <param name="series">The series.</param>
    /// <param name="entity">The entity.</param>
    public ChartPoint(IChartView chart, ISeries series, IChartEntity entity)
    {
        Context = new ChartPointContext(chart, series, entity);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChartPoint"/> class.
    /// </summary>
    /// <param name="point">The point.</param>
    protected ChartPoint(ChartPoint point) : this(point.Context.Chart, point.Context.Series, point.Context.Entity)
    { }

    private ChartPoint()
    {
        Context = new ChartPointContext();
    }

    internal ChartPoint(IChartView chart, object dataSource, HoverArea? hoverArea)
    {
        Context = new ChartPointContext(chart, dataSource, hoverArea);
    }

    internal (double, double) HoverKey { get; set; }

    /// <summary>
    /// Gets a new instance of an empty chart point.
    /// </summary>
    public static ChartPoint Empty => new();

    /// <summary>
    /// Gets the position of the point the collection that was used when the point was drawn.
    /// </summary>
    public int Index => Context.Entity.MetaData?.EntityIndex ?? 0;

    /// <summary>
    /// Gets or sets the coordinate.
    /// </summary>
    public Coordinate Coordinate => Context.Entity.Coordinate;

    /// <summary>
    /// Gets or a value indicating whether this instance is empty, LivveCharts will ignore the point in the chart.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is empty; otherwise, <c>false</c>.
    /// </value>
    public bool IsEmpty => Context.Entity.Coordinate.IsEmpty;

    /// <summary>
    /// Gets or sets the stacked value, if the point do not belongs to a stacked series then this property is null.
    /// </summary>
    public StackedValue? StackedValue { get; set; }

    /// <summary>
    /// Gets the point as data label.
    /// </summary>
    /// <value>
    /// As tooltip string.
    /// </value>
    public string AsDataLabel => Context.Series.GetDataLabelText(this) ?? string.Empty;

    /// <summary>
    /// Gets the context.
    /// </summary>
    /// <value>
    /// The context.
    /// </value>
    public ChartPointContext Context { get; internal set; }

    internal bool IsPointerOver { get; set; }

    internal bool RemoveOnCompleted { get; set; }

    /// <summary>
    /// Gets the distance to a given point.
    /// </summary>
    /// <param name="point">The point to calculate the distance to.</param>
    /// <param name="strategy">The strategy to use to calculate the distance.</param>
    /// <returns>The distance in pixels.</returns>
    public double DistanceTo(LvcPoint point, FindingStrategy strategy) =>
        Context.HoverArea?.DistanceTo(point, strategy) ?? double.NaN;
}

/// <summary>
/// Defines a point in a chart with known visual and label types.
/// </summary>
/// <typeparam name="TModel">The type of the model.</typeparam>
/// <typeparam name="TVisual">The type of the visual.</typeparam>
/// <typeparam name="TLabel">The type of the label.</typeparam>
public class ChartPoint<TModel, TVisual, TLabel> : ChartPoint
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChartPoint{TModel, TVisual, TLabel}"/> class.
    /// </summary>
    /// <param name="point">The point.</param>
    public ChartPoint(ChartPoint point) : base(point)
    {
        StackedValue = point.StackedValue;
        Context.DataSource = point.Context.DataSource;
        Context.Visual = point.Context.Visual;
        Context.Label = point.Context.Label;
        Context.HoverArea = point.Context.HoverArea;
        Context.AdditionalVisuals = point.Context.AdditionalVisuals;
    }

    /// <summary>
    /// Gets the model, this is the actual object that represents the point in the chart.
    /// </summary>
    /// <value>
    /// The model.
    /// </value>
    public TModel? Model => (TModel?)Context.DataSource;

    /// <summary>
    /// Gets the visual.
    /// </summary>
    /// <value>
    /// The visual.
    /// </value>
    public TVisual? Visual => (TVisual?)Context.Visual;

    /// <summary>
    /// Gets the label.
    /// </summary>
    /// <value>
    /// The label.
    /// </value>
    public TLabel? Label => (TLabel?)Context.Label;
}
