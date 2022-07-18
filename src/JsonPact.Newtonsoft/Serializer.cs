// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace JsonPact.NewtonSoft;

public class Serializer : IJsonPact {
    private readonly JsonSerializerSettings _settings;

    public Serializer(JsonSerializerSettings settings) {
        _settings = settings;
    }

    public T? Deserialize<T>(string json) {
        if (string.IsNullOrWhiteSpace(json)) {
            throw new JsonPactDecodeException($"Empty json strings are invalid, failed to deserialize json.");
        }

        try {
            return JsonConvert.DeserializeObject<T>(json, _settings);
        } catch (Exception inner) {
            throw new JsonPactDecodeException($"Failed to deserialize json: {inner.Message}", inner);
        }
    }

    public string Serialize<T>(T value) {
        if (value is null) {
            throw new JsonPactEncodeException($"Invalid {nameof(value)} of {typeof(T).Name} must be an instance of an object to serialize.");
        }

        try {
            return JsonConvert.SerializeObject(value, _settings)
                ?? throw new JsonPactEncodeException($"Failed to serialize given {typeof(T).Name} into valid json.");
        } catch (Exception inner) when (inner is not JsonPactException) {
            throw new JsonPactEncodeException($"Failed to serialize object: {inner.Message}", inner);
        }
    }
}

public static class JsonPacts {
    public static JsonSerializerSettings Default(JsonPactCase casing) => new() {
        NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Populate,
        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
        ContractResolver = new JsonPactAttributesResolver {
            NamingStrategy = casing.IntoNamingStrategy()
        }
    };

    public static NamingStrategy? IntoNamingStrategy(this JsonPactCase casing) => casing switch {
        JsonPactCase.Snake => new SnakeCaseNamingStrategy(),
        JsonPactCase.Camel => new CamelCaseNamingStrategy(),
        JsonPactCase.Kebab => new KebabCaseNamingStrategy(),
        JsonPactCase.Pascal => null, // Is used by default.
        _ => throw new ArgumentOutOfRangeException(nameof(casing), $"Unsupported casing type {casing} for 'Newtonsoft' settings.")
    };

    /// <summary>
    /// Converts Newtonsoft settings into an <see cref="IJsonPact"/>.
    /// </summary>
    /// <param name="options">Newtonsoft settings to used to serialize and deserialize json.</param>
    /// <returns><see cref="IJsonPact"/></returns>
    public static IJsonPact IntoJsonPact(this JsonSerializerSettings options) => new Serializer(options);
}
