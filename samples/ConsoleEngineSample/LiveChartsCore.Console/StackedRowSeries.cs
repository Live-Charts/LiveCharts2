// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Console.Drawing.Geometries;

namespace LiveChartsCore.Console;

public class StackedRowSeries<TModel>
    : CoreStackedRowSeries<TModel, RoundedRectangleGeometry, LabelGeometry, LineGeometry>
{
    public StackedRowSeries() : base(null) { }
    public StackedRowSeries(IReadOnlyCollection<TModel>? values) : base(values) { }
    public StackedRowSeries(params TModel[] values) : base(values) { }
}
