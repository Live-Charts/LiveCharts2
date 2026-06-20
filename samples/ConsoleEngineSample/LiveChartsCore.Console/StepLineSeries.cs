// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Console.Drawing.Geometries;

namespace LiveChartsCore.Console;

public class StepLineSeries<TModel>
    : CoreStepLineSeries<TModel, CircleGeometry, LabelGeometry, StepLineAreaGeometry, LineGeometry>
{
    public StepLineSeries() : base(null) { }
    public StepLineSeries(IReadOnlyCollection<TModel>? values) : base(values) { }
    public StepLineSeries(params TModel[] values) : base(values) { }
}
