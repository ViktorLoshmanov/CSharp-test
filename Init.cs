using drawer.Models;
//using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace drawer;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(Legend[]))]
//[JsonSerializable(typeof(IEnumerable<LayerResultBlazing>))]
// [JsonSerializable(typeof(int))]
public partial class LegendTypeJsonContext : JsonSerializerContext { }


[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(Legend[]))]
[JsonSerializable(typeof(IEnumerable<LayerResultBlazing>))]
[JsonSerializable(typeof(IEnumerable<LayerResult>))]
[JsonSerializable(typeof(Memory<LayerResultBlazing>))]
[JsonSerializable(typeof(Memory<ObrazResultBlazing>))]
public partial class ResultsTypeJsonContext : JsonSerializerContext { }


public static class Init
{
    public static Legend[] ls;

    public static Rect r;
    public static double cx;
    public static double cy;

    static Init()
    {
        var json = File.ReadAllText("primitives.json");

        ls = JsonSerializer.Deserialize(json, LegendTypeJsonContext.Default.LegendArray)!;

        r = new Rect { Left = 1200, Bottom = 50, Right = 4000, Top = 2850 };
        cx = (r.Right + r.Left) / 2;
        cy = (r.Bottom + r.Top) / 2;
    }

    public static void tt() { }

}
