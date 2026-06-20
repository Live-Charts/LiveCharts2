// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Console.Drawing.Geometries;

namespace LiveChartsCore.Console;

public class StackedColumnSeries<TModel>
    : CoreStackedColumnSeries<TModel, RoundedRectangleGeometry, LabelGeometry, LineGeometry>
{
    public StackedColumnSeries() : base(null) { }
    public StackedColumnSeries(IReadOnlyCollection<TModel>? values) : base(values) { }
    public StackedColumnSeries(params TModel[] values) : base(values) { }
}
