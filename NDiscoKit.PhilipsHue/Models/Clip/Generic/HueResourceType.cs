﻿using System.Text.Json;
using System.Text.Json.Serialization;

namespace NDiscoKit.PhilipsHue.Models.Clip.Generic;

[JsonConverter(typeof(HueResourceTypeJsonConverter))]
public readonly record struct HueResourceType(string Type)
{
    // List is not exhaustive
    // as there are just too many types possible

    // Basic types
    public bool IsLight => Is("light");
    public bool IsButton => Is("button");

    // Entertainment
    public bool IsEntertainmentService => Is("entertainment");
    public bool IsEntertainmentConfiguration => Is("entertainment_configuration");

    // Basic spaces
    public bool IsRoom => Is("room");
    public bool IsZone => Is("zone");

    private bool Is(string type) => Type == type;
}

internal class HueResourceTypeJsonConverter : JsonConverter<HueResourceType>
{
    public override HueResourceType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string typeValue = reader.GetString() ?? throw new JsonException();
        return new HueResourceType(typeValue);
    }

    public override void Write(Utf8JsonWriter writer, HueResourceType value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Type);
    }
}
