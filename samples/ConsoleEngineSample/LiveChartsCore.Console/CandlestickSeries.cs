// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsCore.Console.Drawing.Geometries;

namespace LiveChartsCore.Console;

public class CandlestickSeries<TModel>
    : CoreFinancialSeries<TModel, CandlestickGeometry, LabelGeometry, RoundedRectangleGeometry>
{
    public CandlestickSeries() : base(null) { }
    public CandlestickSeries(IReadOnlyCollection<TModel>? values) : base(values) { }
    public CandlestickSeries(params TModel[] values) : base(values) { }
}
