// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Console.Drawing.Geometries;

namespace LiveChartsCore.Console;

public class PolarLineSeries<TModel>
    : CorePolarLineSeries<TModel, CircleGeometry, LabelGeometry, VectorAreaGeometry, LineGeometry>
{
    public PolarLineSeries() : base(null) { }
    public PolarLineSeries(IReadOnlyCollection<TModel>? values) : base(values) { }
    public PolarLineSeries(params TModel[] values) : base(values) { }
}
