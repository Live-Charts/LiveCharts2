// MIT License - Copyright (c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors

using LiveChartsGeneratedCode;

namespace LiveChartsCore.Console;

// Polar chart wrapper — same SourceGen-based pattern as CartesianChart and PieChart.
// The Polar engine emits the radial axis lines / angular separators as IChartElement instances
// that paint via the same drawing-context dispatch we already have, so the series side just
// reuses VectorAreaGeometry (cubic bezier path) for the polar line itself.
public class PolarChart : SourceGenPolarChart
{
}
