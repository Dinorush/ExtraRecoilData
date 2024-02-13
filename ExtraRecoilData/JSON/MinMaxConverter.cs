using GameData;
using GTFO.API.Utilities;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ExtraRecoilData.JSON
{
    public sealed class MinMaxConverter : JsonConverter<MinMaxValue>
    {
        public override MinMaxValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException("Missing { for MinMaxValue object");

            MinMaxValue minMaxValue = new();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject) return minMaxValue;

                if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException("Expected PropertyName token");

                string? property = reader.GetString();
                reader.Read();
                switch (property.ToLowerInvariant())
                {
                    case "min": 
                        minMaxValue.Min = reader.GetSingle(); 
                        break;
                    case "max": 
                        minMaxValue.Max = reader.GetSingle(); 
                        break;
                    default:
                        throw new JsonException("Expected Min or Max property");
                }
            }

            throw new JsonException("Expected EndObject token");
        }

        public override void Write(Utf8JsonWriter writer, MinMaxValue value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("Min");
            writer.WriteNumberValue(value.Min);
            writer.WritePropertyName("Max");
            writer.WriteNumberValue(value.Max);
            writer.WriteEndObject();
        }
    }
}
