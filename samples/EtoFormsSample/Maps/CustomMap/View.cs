using System.IO;
using Eto.Forms;
using LiveChartsCore.Geo;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Eto;

namespace EtoFormsSample.Maps.CustomMap;

public class View : Panel
{
    public GeoMap Chart;

    public View()
    {
        // the mexico-states.geojson file is an embedded resource of the ViewModelsSamples
        // assembly, Maps also exposes GetMapFromDirectory(path) for files on disk
        using var stream = typeof(ViewModelsSamples.Index).Assembly
            .GetManifestResourceStream("ViewModelsSamples.Maps.CustomMap.mexico-states.geojson")!;
        using var reader = new StreamReader(stream);

        var chart = new GeoMap
        {
            ActiveMap = LiveChartsCore.Geo.Maps.GetMapFromStreamReader(reader),
            Series =
            [
                new HeatLandSeries
                {
                    Name = "Population (synthetic)",
                    // the names are the ISO 3166-2 region codes without the 'mx-' prefix
                    Lands =
                    [
                        new HeatLand { Name = "agu", Value = 13 }, // Aguascalientes
                        new HeatLand { Name = "bcn", Value = 36 }, // Baja California
                        new HeatLand { Name = "bcs", Value =  8 }, // Baja California Sur
                        new HeatLand { Name = "cam", Value = 10 }, // Campeche
                        new HeatLand { Name = "chp", Value = 56 }, // Chiapas
                        new HeatLand { Name = "chh", Value = 38 }, // Chihuahua
                        new HeatLand { Name = "cmx", Value = 92 }, // Ciudad de México
                        new HeatLand { Name = "coa", Value = 32 }, // Coahuila
                        new HeatLand { Name = "col", Value =  7 }, // Colima
                        new HeatLand { Name = "dur", Value = 18 }, // Durango
                        new HeatLand { Name = "gua", Value = 62 }, // Guanajuato
                        new HeatLand { Name = "gro", Value = 35 }, // Guerrero
                        new HeatLand { Name = "hid", Value = 31 }, // Hidalgo
                        new HeatLand { Name = "jal", Value = 84 }, // Jalisco
                        new HeatLand { Name = "mex", Value = 170 }, // México (state)
                        new HeatLand { Name = "mic", Value = 47 }, // Michoacán
                        new HeatLand { Name = "mor", Value = 19 }, // Morelos
                        new HeatLand { Name = "nay", Value = 12 }, // Nayarit
                        new HeatLand { Name = "nle", Value = 58 }, // Nuevo León
                        new HeatLand { Name = "oax", Value = 41 }, // Oaxaca
                        new HeatLand { Name = "pue", Value = 65 }, // Puebla
                        new HeatLand { Name = "que", Value = 23 }, // Querétaro
                        new HeatLand { Name = "roo", Value = 18 }, // Quintana Roo
                        new HeatLand { Name = "slp", Value = 28 }, // San Luis Potosí
                        new HeatLand { Name = "sin", Value = 30 }, // Sinaloa
                        new HeatLand { Name = "son", Value = 29 }, // Sonora
                        new HeatLand { Name = "tab", Value = 24 }, // Tabasco
                        new HeatLand { Name = "tam", Value = 35 }, // Tamaulipas
                        new HeatLand { Name = "tla", Value = 13 }, // Tlaxcala
                        new HeatLand { Name = "ver", Value = 80 }, // Veracruz
                        new HeatLand { Name = "yuc", Value = 23 }, // Yucatán
                        new HeatLand { Name = "zac", Value = 15 }, // Zacatecas
                    ],
                },
            ],
            MapProjection = MapProjection.Mercator,
            MinLatitude = 13,
            MaxLatitude = 33,
            MinLongitude = -120,
            MaxLongitude = -85,
        };

        Content = chart;
        Chart = chart;
    }
}
