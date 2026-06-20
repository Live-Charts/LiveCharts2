// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Console.Drawing.Geometries;

namespace LiveChartsCore.Console;

public class BoxSeries<TModel>
    : CoreBoxSeries<TModel, BoxGeometry, LabelGeometry, RoundedRectangleGeometry>
{
    public BoxSeries() : base(null) { }
    public BoxSeries(IReadOnlyCollection<TModel>? values) : base(values) { }
    public BoxSeries(params TModel[] values) : base(values) { }
}
