using System.Text.Json;
using System.Text.Json.Serialization;

namespace GmhWorkshop.VictronRemoteMonitoring.ApiDtos;

public class StatsRecordConverter : JsonConverter<Dictionary<string, decimal[][]?>>
{
    public override Dictionary<string, decimal[][]?> Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        var value = new Dictionary<string, decimal[][]?>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return value;
            }

            // Read the property name.
            var propertyName = reader.GetString();
            reader.Read();

            // Check the token type to determine if the value is a boolean or an array.
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                // Deserialize the value to a decimal[][].
                var array = JsonSerializer.Deserialize<decimal[][]>(ref reader, options);
                value.Add(propertyName, array);
            }
            else if (reader.TokenType == JsonTokenType.False)
            {
                value.Add(propertyName, null);
            }
            else
            {
                throw new JsonException(
                    "Unexpected Token - the StatsRecordConverter expects a string code identifier and an number[][] or 'False' (no records) - other values indicate that the API is not handling all valid values...");
            }
        }

        throw new JsonException("Expected an end object token.");
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<string, decimal[][]?> value,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (var kvp in value)
        {
            writer.WritePropertyName(kvp.Key);
            JsonSerializer.Serialize(writer, kvp.Value, options);
        }

        writer.WriteEndObject();
    }
}