using System.Text.Json;
using System.Text.Json.Serialization;
using AIDIMS.Domain.Enums;

namespace AIDIMS.Application.Common;

/// <summary>
/// JSON converter that accepts both string names and numeric values for enums
/// </summary>
public class FlexibleEnumConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return false; // We'll use StringEnumConverter with special handling
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return null;
    }
}

/// <summary>
/// Custom JSON converter for string properties that can accept enum names or numeric values
/// Converts numeric enum values to their string representation
/// </summary>
public class EnumStringConverter : JsonConverter<string>
{
    private readonly Type _enumType;

    public EnumStringConverter(Type enumType)
    {
        _enumType = enumType;
    }

    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return reader.GetString();
            
            case JsonTokenType.Number:
                var numValue = reader.GetInt32();
                if (Enum.IsDefined(_enumType, numValue))
                {
                    return Enum.GetName(_enumType, numValue);
                }
                throw new JsonException($"Value {numValue} is not valid for {_enumType.Name}");
            
            default:
                throw new JsonException($"Unexpected token type: {reader.TokenType}");
        }
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}

/// <summary>
/// Converter for Modality enum values
/// </summary>
public class ModalityStringConverter : EnumStringConverter
{
    public ModalityStringConverter() : base(typeof(Modality)) { }
}

/// <summary>
/// Converter for BodyPart enum values
/// </summary>
public class BodyPartStringConverter : EnumStringConverter
{
    public BodyPartStringConverter() : base(typeof(BodyPart)) { }
}

/// <summary>
/// Converter for ImagingOrderStatus enum values
/// </summary>
public class ImagingOrderStatusStringConverter : EnumStringConverter
{
    public ImagingOrderStatusStringConverter() : base(typeof(ImagingOrderStatus)) { }
}

/// <summary>
/// Converter for PatientVisitStatus enum values
/// </summary>
public class PatientVisitStatusStringConverter : EnumStringConverter
{
    public PatientVisitStatusStringConverter() : base(typeof(PatientVisitStatus)) { }
}

/// <summary>
/// Converter for Gender enum values
/// </summary>
public class GenderStringConverter : EnumStringConverter
{
    public GenderStringConverter() : base(typeof(Gender)) { }
}

/// <summary>
/// Converter for DiagnosisReportStatus enum values
/// </summary>
public class DiagnosisReportStatusStringConverter : EnumStringConverter
{
    public DiagnosisReportStatusStringConverter() : base(typeof(DiagnosisReportStatus)) { }
}
