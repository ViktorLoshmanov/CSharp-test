//using drawer.Models;
//using RyuCsharp;
//using System.Text;
//using System.Text.Json;
//using System.Text.Json.Serialization;

//namespace apiTest;

//public class LayerConverter : JsonConverter<ILayer>
//{
//    public override ILayer Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//    {
//        throw new NotImplementedException();
//    }

//    public override void Write(Utf8JsonWriter writer, ILayer layer, JsonSerializerOptions options)
//    {   
//        Span<char> charSpan = stackalloc char[128];
//        writer.WriteStartObject();

//        writer.WriteNumber("legendId", layer.LegendId);

//        writer.WriteStartArray("obrazes");
//        //int writtenLength;

//        foreach (var obraz in layer.Obrazes)
//        {
//            writer.WriteStartObject();

//            writer.WriteString("name", obraz.Name);

//            writer.WriteStartArray("coords");

//            foreach (var c in obraz.Coords)
//            {
//                //var celoe = (int)c;
//                //var drobnoe = (int)((c - celoe) * 10000);
//                //writer.WriteRawValue($"{celoe}.{drobnoe}");

//                writer.WriteRawValue(RyuDotNet.Ryu.WriteTo(c, charSpan), true);

//                //writtenLength = Ryu.d2s_buffered_n(c, ref charSpan[0]);
//                //writer.WriteRawValue(charSpan[..writtenLength], true);
//            }

//            writer.WriteEndArray();

//            writer.WriteEndObject();
//        }
//        writer.WriteEndArray();

//        writer.WriteEndObject();
//    }
//}
