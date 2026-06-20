// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Console.Drawing.Geometries;

namespace LiveChartsCore.Console;

public class StackedStepAreaSeries<TModel>
    : CoreStackedStepAreaSeries<TModel, CircleGeometry, LabelGeometry, StepLineAreaGeometry, LineGeometry>
{
    public StackedStepAreaSeries() : base(null) { }
    public StackedStepAreaSeries(IReadOnlyCollection<TModel>? values) : base(values) { }
    public StackedStepAreaSeries(params TModel[] values) : base(values) { }
}
