// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Console.Drawing.Geometries;

namespace LiveChartsCore.Console;

public class ScatterSeries<TModel>
    : CoreScatterSeries<TModel, CircleGeometry, LabelGeometry, LineGeometry>
{
    public ScatterSeries() : base(null) { }
    public ScatterSeries(IReadOnlyCollection<TModel>? values) : base(values) { }
    public ScatterSeries(params TModel[] values) : base(values) { }
}
