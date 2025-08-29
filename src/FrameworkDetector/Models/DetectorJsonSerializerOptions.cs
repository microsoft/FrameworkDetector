using System.Text.Json;

namespace FrameworkDetector.Models;

internal class DetectorJsonSerializerOptions
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new System.Text.Json.Serialization.JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
        },
    };
}
