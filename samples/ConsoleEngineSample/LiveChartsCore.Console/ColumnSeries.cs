// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Console.Drawing.Geometries;

namespace LiveChartsCore.Console;

public class ColumnSeries<TModel>
    : CoreColumnSeries<TModel, RoundedRectangleGeometry, LabelGeometry, LineGeometry>
{
    public ColumnSeries() : base(null) { }
    public ColumnSeries(IReadOnlyCollection<TModel>? values) : base(values) { }
    public ColumnSeries(params TModel[] values) : base(values) { }
}
