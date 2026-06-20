// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Console.Drawing.Geometries;

namespace LiveChartsCore.Console;

public class HeatSeries<TModel>
    : CoreHeatSeries<TModel, ColoredRectangleGeometry, LabelGeometry>
{
    public HeatSeries() : base(null) { }
    public HeatSeries(IReadOnlyCollection<TModel>? values) : base(values) { }
    public HeatSeries(params TModel[] values) : base(values) { }
}
