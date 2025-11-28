using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace AIDIMS.Application.DTOs;

// Custom JsonConverter to handle mixed types in DICOM tags
public class FlexibleDicomTagsConverter : JsonConverter<Dictionary<string, string>>
{
    public override Dictionary<string, string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var result = new Dictionary<string, string>();

        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject");

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Expected PropertyName");

            string propertyName = reader.GetString()!;
            reader.Read();

            string value;
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    value = reader.GetString()!;
                    break;
                case JsonTokenType.Number:
                    value = reader.GetDecimal().ToString();
                    break;
                case JsonTokenType.StartArray:
                    // Skip array completely
                    SkipArray(ref reader);
                    continue; // Skip this property
                case JsonTokenType.StartObject:
                    // Skip object completely
                    SkipObject(ref reader);
                    continue; // Skip this property
                case JsonTokenType.True:
                    value = "true";
                    break;
                case JsonTokenType.False:
                    value = "false";
                    break;
                case JsonTokenType.Null:
                    value = "";
                    break;
                default:
                    throw new JsonException($"Unexpected token type: {reader.TokenType}");
            }

            result[propertyName] = value;
        }

        return result;
    }

    private void SkipArray(ref Utf8JsonReader reader)
    {
        var depth = 1;
        while (depth > 0 && reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.StartArray:
                    depth++;
                    break;
                case JsonTokenType.EndArray:
                    depth--;
                    break;
            }
        }
    }

    private void SkipObject(ref Utf8JsonReader reader)
    {
        var depth = 1;
        while (depth > 0 && reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.StartObject:
                    depth++;
                    break;
                case JsonTokenType.EndObject:
                    depth--;
                    break;
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<string, string> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (var kvp in value)
        {
            writer.WriteString(kvp.Key, kvp.Value);
        }
        writer.WriteEndObject();
    }
}

public class DicomUploadDto
{
    public required IFormFile File { get; set; }
    public Guid? OrderId { get; set; } // Optional: link to existing order
    public Guid? PatientId { get; set; } // Optional: link to existing patient
}

public class DicomUploadResultDto
{
    [JsonPropertyName("ID")]
    public string ID { get; set; } = string.Empty;

    [JsonPropertyName("ParentPatient")]
    public string ParentPatient { get; set; } = string.Empty;

    [JsonPropertyName("ParentSeries")]
    public string ParentSeries { get; set; } = string.Empty;

    [JsonPropertyName("ParentStudy")]
    public string ParentStudy { get; set; } = string.Empty;

    [JsonPropertyName("Path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("Status")]
    public string Status { get; set; } = string.Empty;
}

// DTOs for Orthanc metadata
public class OrthancPatientDto
{
    [JsonPropertyName("ID")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("MainDicomTags")]
    [JsonConverter(typeof(FlexibleDicomTagsConverter))]
    public Dictionary<string, string> MainDicomTags { get; set; } = new();
}

public class OrthancStudyDto
{
    [JsonPropertyName("ID")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("MainDicomTags")]
    [JsonConverter(typeof(FlexibleDicomTagsConverter))]
    public Dictionary<string, string> MainDicomTags { get; set; } = new();

    [JsonPropertyName("PatientMainDicomTags")]
    [JsonConverter(typeof(FlexibleDicomTagsConverter))]
    public Dictionary<string, string> PatientMainDicomTags { get; set; } = new();
}

public class OrthancSeriesDto
{
    [JsonPropertyName("ID")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("MainDicomTags")]
    [JsonConverter(typeof(FlexibleDicomTagsConverter))]
    public Dictionary<string, string> MainDicomTags { get; set; } = new();
}

public class OrthancInstanceDto
{
    [JsonPropertyName("ID")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("MainDicomTags")]
    [JsonConverter(typeof(FlexibleDicomTagsConverter))]
    public Dictionary<string, string> MainDicomTags { get; set; } = new();
}
