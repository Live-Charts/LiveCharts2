// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Console.Drawing.Geometries;

namespace LiveChartsCore.Console;

public class RowSeries<TModel>
    : CoreRowSeries<TModel, RoundedRectangleGeometry, LabelGeometry, LineGeometry>
{
    public RowSeries() : base(null) { }
    public RowSeries(IReadOnlyCollection<TModel>? values) : base(values) { }
    public RowSeries(params TModel[] values) : base(values) { }
}
