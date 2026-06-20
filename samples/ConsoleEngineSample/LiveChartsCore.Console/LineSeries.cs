// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Console.Drawing.Geometries;

namespace LiveChartsCore.Console;

public class LineSeries<TModel>
    : CoreLineSeries<TModel, RectangleGeometry, LabelGeometry, VectorAreaGeometry, LineGeometry>
{
    public LineSeries() : base(null) { }
    public LineSeries(IReadOnlyCollection<TModel>? values) : base(values) { }
    public LineSeries(params TModel[] values) : base(values) { }
}
