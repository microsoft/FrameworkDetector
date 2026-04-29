// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using FrameworkDetector.Inputs;

namespace FrameworkDetector.Models;

/// <summary>
/// A <see cref="JsonConverter{T}"/> that deserializes the generalized Inputs dictionary of a <see cref="ToolRunResult"/>
/// into concrete <see cref="IInputType"/> .NET implementations based on the dictionary key (<see cref="IInputType.InputGroup"/> name),
/// rather than leaving them as <see cref="JsonElement"/> instances.
/// </summary>
public class InputTypeJsonConverter : JsonConverter<IReadOnlyDictionary<string, IReadOnlyList<object?>>>
{
    private static readonly Dictionary<string, Type> InputGroupTypeMap = new()
    {
        // TODO: Make InputGroup a const to avoid this "magic string" mapping and potential mismatches.
        ["processes"] = typeof(ProcessInput),
        ["executables"] = typeof(ExecutableInput),
        ["dotnetManifests"] = typeof(DotnetManifestInput),
        ["installedPackages"] = typeof(InstalledPackageInput),
    };

    public override IReadOnlyDictionary<string, IReadOnlyList<object?>> Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token.");
        }

        var result = new Dictionary<string, IReadOnlyList<object?>>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return result;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected PropertyName token.");
            }

            string key = reader.GetString()!;
            reader.Read();

            using var jsonDoc = JsonDocument.ParseValue(ref reader);

            if (jsonDoc.RootElement.ValueKind != JsonValueKind.Array)
            {
                throw new JsonException($"Expected array value for input group '{key}'.");
            }

            var list = new List<object?>();

            if (InputGroupTypeMap.TryGetValue(key, out var concreteType))
            {
                foreach (var element in jsonDoc.RootElement.EnumerateArray())
                {
                    list.Add(element.Deserialize(concreteType, options));
                }
            }
            else
            {
                foreach (var element in jsonDoc.RootElement.EnumerateArray())
                {
                    list.Add(element.Clone());
                }
            }

            result[key] = list;
        }

        throw new JsonException("Unexpected end of JSON.");
    }

    public override void Write(
        Utf8JsonWriter writer, IReadOnlyDictionary<string, IReadOnlyList<object?>> value, JsonSerializerOptions options)
    {
        // We don't care about special behavior for writing...
        writer.WriteStartObject();

        foreach (var kvp in value)
        {
            writer.WritePropertyName(kvp.Key);
            writer.WriteStartArray();

            foreach (var item in kvp.Value)
            {
                if (item is not null)
                {
                    JsonSerializer.Serialize(writer, item, item.GetType(), options);
                }
                else
                {
                    writer.WriteNullValue();
                }
            }

            writer.WriteEndArray();
        }

        writer.WriteEndObject();
    }
}
