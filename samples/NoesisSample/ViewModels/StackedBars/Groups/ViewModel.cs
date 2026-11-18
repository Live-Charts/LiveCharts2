namespace ViewModelsSamples.StackedBars.Groups;

public class ViewModel
{
    public int[] Values1 { get; set; } = [3, 5, 3];
    public int[] Values2 { get; set; } = [4, 2, 3];
    public int[] Values3 { get; set; } = [4, 6, 6];
    public int[] Values4 { get; set; } = [2, 5, 4];
    public string[] Labels { get; set; } =
        ["Category 1", "Category 2", "Category 3"];
}
