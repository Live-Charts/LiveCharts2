// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Console.Drawing.Geometries;

namespace LiveChartsCore.Console;

public class StackedAreaSeries<TModel>
    : CoreStackedAreaSeries<TModel, CircleGeometry, LabelGeometry, VectorAreaGeometry, LineGeometry>
{
    public StackedAreaSeries() : base(null) { }
    public StackedAreaSeries(IReadOnlyCollection<TModel>? values) : base(values) { }
    public StackedAreaSeries(params TModel[] values) : base(values) { }
}
