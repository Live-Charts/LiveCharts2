using LiveChartsCore.Defaults;

namespace ViewModelsSamples.Treemaps.Basic;

public class ViewModel
{
    // Two-level hierarchy: regions roll up their child countries automatically
    // (internal node Value=0 -> the series sums leaves under it).
    public TreemapNode[] Regions { get; } =
    [
        new TreemapNode("Asia", new[]
        {
            new TreemapNode(1400, "China"),
            new TreemapNode(1300, "India"),
            new TreemapNode(125,  "Japan"),
            new TreemapNode(270,  "Indonesia"),
        }),
        new TreemapNode("Europe", new[]
        {
            new TreemapNode(83, "Germany"),
            new TreemapNode(67, "France"),
            new TreemapNode(60, "Italy"),
            new TreemapNode(46, "Spain"),
        }),
        new TreemapNode("Americas", new[]
        {
            new TreemapNode(330, "USA"),
            new TreemapNode(220, "Brazil"),
            new TreemapNode(130, "Mexico"),
        }),
    ];
}
