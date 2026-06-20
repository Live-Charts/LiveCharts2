using LiveChartsCore.Defaults;

namespace ViewModelsSamples.Sankeys.Basic;

public class ViewModel
{
    // Pure-source nodes on the left, pure-sink nodes on the right; ribbons
    // route through the d3-sankey layout. Pass-through nodes work too —
    // change one of the sink/source rows to have BOTH incoming and outgoing
    // links and SankeyLayout will assign it a middle column.
    public SankeyNode[] Nodes { get; }
    public SankeyLink<SankeyNode>[] Links { get; }

    public ViewModel()
    {
        var bills = new SankeyNode("Bills");
        var groceries = new SankeyNode("Groceries");
        var transport = new SankeyNode("Transport");
        var entertainment = new SankeyNode("Entertainment");
        var rent = new SankeyNode("Rent");
        var fixedExpenses = new SankeyNode("Fixed Expenses");
        var savings = new SankeyNode("Savings");
        var leisure = new SankeyNode("Leisure");

        Nodes =
        [
            bills, groceries, transport, entertainment,
            rent, fixedExpenses, savings, leisure,
        ];

        Links =
        [
            new(bills, fixedExpenses, 320),
            new(bills, savings, 80),
            new(groceries, fixedExpenses, 420),
            new(groceries, savings, 60),
            new(transport, fixedExpenses, 180),
            new(transport, leisure, 40),
            new(rent, fixedExpenses, 950),
            new(entertainment, leisure, 220),
            new(entertainment, savings, 30),
        ];
    }
}
