// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace JsonPact.NewtonSoft {
    public class Serializer : IJsonPact {
        private readonly JsonSerializerSettings _settings;

        public Serializer(JsonSerializerSettings settings) {
            _settings = settings;
        }

        public T? Deserialize<T>(string json) {
            if (string.IsNullOrWhiteSpace(json)) {
                throw new JsonPactDecodeException("Empty json strings are invalid, failed to deserialize json.");
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
        /// <summary>
        /// Default <see cref="JsonSerializerSettings"/> for JsonPact.
        /// </summary>
        /// <param name="casing"><see cref="JsonPactCase"/></param>
        /// <returns><see cref="JsonOptions"/></returns>
        public static JsonOptions Default(JsonPactCase casing) => JsonOptions.Default(casing);

        /// <summary>
        /// Add JsonPact settings to <see cref="JsonSerializerSettings" />.
        /// </summary>
        /// <param name="settings"><see cref="JsonSerializerSettings" /></param>
        /// <param name="casing"><see cref="JsonSerializerSettings" /></param>
        /// <returns><see cref="JsonOptions"/></returns>
        public static JsonOptions AddJsonPact(
            this JsonSerializerSettings settings,
            JsonPactCase casing
        ) => new(casing, settings);

        /// <summary>
        /// Convert <see cref="JsonPactCase"/> into Newtonsoft's <see cref="NamingStrategy"/>.
        /// </summary>
        /// <param name="casing"><see cref="JsonPactCase"/></param>
        /// <returns><see cref="NamingStrategy"/></returns>
        /// <exception cref="ArgumentOutOfRangeException">Occurs on an invalid <see cref="JsonPactCase"/>.</exception>
        public static NamingStrategy? IntoNamingStrategy(this JsonPactCase casing) => casing switch {
            JsonPactCase.Snake => new SnakeCaseNamingStrategy(),
            JsonPactCase.Camel => new CamelCaseNamingStrategy(),
            JsonPactCase.Kebab => new KebabCaseNamingStrategy(),
            JsonPactCase.Pascal => null, // Is used by default.
            _ => throw new ArgumentOutOfRangeException(
                nameof(casing),
                $"Unsupported casing type {casing} for 'Newtonsoft' settings."
            )
        };
    }
}
