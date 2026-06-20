using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;

namespace ViewModelsSamples.Maps.OrthographicWorld;

public class ViewModel
{
    public ViewModel()
    {
        var lands = new HeatLand[]
        {
            new() { Name = "bra", Value = 13 },
            new() { Name = "mex", Value = 10 },
            new() { Name = "usa", Value = 15 },
            new() { Name = "can", Value = 8 },
            new() { Name = "ind", Value = 12 },
            new() { Name = "deu", Value = 13 },
            new() { Name = "jpn", Value = 15 },
            new() { Name = "chn", Value = 14 },
            new() { Name = "rus", Value = 11 },
            new() { Name = "fra", Value = 8 },
            new() { Name = "esp", Value = 7 },
            new() { Name = "kor", Value = 10 },
            new() { Name = "zaf", Value = 12 },
            new() { Name = "are", Value = 13 },
            new() { Name = "aus", Value = 9 },
            new() { Name = "arg", Value = 6 },
            new() { Name = "egy", Value = 7 },
            new() { Name = "nga", Value = 11 },
            new() { Name = "gbr", Value = 9 }
        };

        Series = [new HeatLandSeries { Lands = lands }];
    }

    public HeatLandSeries[] Series { get; set; }
}
