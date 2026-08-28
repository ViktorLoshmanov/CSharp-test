using apiTest.Arena;
using drawer.Models;
//using RyuCsharp;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace apiTest;


/** Запись double в строку с алгоритмом Ryu */
//public class MemoryDoubleRyuConverter : JsonConverter<Memory<double>>
//{
//   public override void Write(Utf8JsonWriter writer, Memory<double> value, JsonSerializerOptions options)
//   {
//       Span<char> charSpan = stackalloc char[128];
//       var sp = value.Span;
//       for (var i = 0; i < sp.Length; i++)
//           writer.WriteRawValue(RyuDotNet.Ryu.WriteTo(sp[i], charSpan), true);
//       //  writer.WriteNumberValue(sp[i]);
//   }

//   public override Memory<double> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//   {
//       throw new NotImplementedException();
//   }
//}

public class ObrazResultBlazingConverter : JsonConverter<ObrazResultBlazing>
{
    private static readonly JsonEncodedText PropName_name = JsonEncodedText.Encode("name");
    private static readonly JsonEncodedText PropName_coords = JsonEncodedText.Encode("coords");

    public override void Write(Utf8JsonWriter writer, ObrazResultBlazing value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString(PropName_name, value.Name);
        writer.WritePropertyName(PropName_coords);

        Span<char> charSpan = stackalloc char[128];
        var sp = value.Coords.Span;
        for (var i = 0; i < sp.Length; i++)
            writer.WriteRawValue(RyuDotNet.Ryu.WriteTo(sp[i], charSpan), true);

        writer.WriteEndObject();
    }

    public override ObrazResultBlazing Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}

/** Упрощённая запись double в строку */
// public class MemoryDoubleConverter : JsonConverter<Memory<double>>
// {
//    public override void Write(Utf8JsonWriter writer, Memory<double> value, JsonSerializerOptions options)
//    {
//        Span<char> charSpan = stackalloc char[128];
//        var sp = value.Span;
//        for (var i = 0; i < sp.Length; i++)
//        {
//            var c = sp[i];
//            var celoe = (int)c;
//            var drobnoe = (int)((c - celoe) * 10000);

//            if (!celoe.TryFormat(charSpan, out var charsCWritten))
//                continue;

//            charSpan[charsCWritten++] = '.';

//            if (!drobnoe.TryFormat(charSpan[charsCWritten..], out var charsDWritten))
//                continue;

//            writer.WriteRawValue(charSpan[..(charsCWritten + charsDWritten)], true);
//        }
//    }

//    public override Memory<double> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//    {
//        throw new NotImplementedException();
//    }
// }
