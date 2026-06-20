// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Console.Drawing.Geometries;

namespace LiveChartsCore.Console;

public class PieSeries<TModel>
    : CorePieSeries<TModel, DoughnutGeometry, LabelGeometry, CircleGeometry>
{
    public PieSeries() : base(null, false, false) { }
    public PieSeries(IReadOnlyCollection<TModel>? values) : base(values, false, false) { }
    public PieSeries(params TModel[] values) : base(values, false, false) { }
}
