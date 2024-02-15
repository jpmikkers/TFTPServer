using System;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AvaTFTPServer;

public class IPEndPointJsonConverter : JsonConverter<IPEndPoint>
{
    public override IPEndPoint? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var v = reader.GetString();
        return v != null && IPEndPoint.TryParse(v, out var result) ? result : null;
    }

    public override void Write(Utf8JsonWriter writer, IPEndPoint value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
