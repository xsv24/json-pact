using System;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonPact.System {
    public class Serializer : IJsonPact {
        private readonly JsonSerializerOptions _options;

        public Serializer(JsonSerializerOptions options) {
            _options = options;
        }

        public T? Deserialize<T>(string json) {
            try {
                return JsonSerializer.Deserialize<T>(json, _options);
            } catch (Exception inner) {
                throw new JsonPactDecodeException($"Failed to deserialize json: {inner.Message}", inner);
            }
        }

        public string Serialize<T>(T value) {
            if (value is null) {
                throw new JsonPactEncodeException($"Invalid {nameof(value)} of {typeof(T).Name} must be an instance of an object to serialize.");
            }

            try {
                return JsonSerializer.Serialize(value, _options)
                    ?? throw new JsonPactEncodeException($"Failed to serialize given {typeof(T).Name} into valid json.");
            } catch (Exception inner) when (inner is not JsonPactException) {
                throw new JsonPactEncodeException($"Failed to serialize object: {inner.Message}", inner);
            }
        }
    }

    public static class JsonPacts {
        public static JsonSerializerOptions Default(JsonPactCase casing) => new() {
            PropertyNameCaseInsensitive = false,
            PropertyNamingPolicy = new CasingPolicy(casing),
            Converters = { new ObjectConvertor() },
#if NET5_0_OR_GREATER
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
#endif
        };

        public static JsonSerializerOptions With(
            this JsonSerializerOptions options,
            bool? includeFields = null,
            bool? ignoreReadOnlyFields = null,
            bool? ignoreReadOnlyProperties = null,
            bool? allowTrailingCommas = null,
            int? defaultBufferSize = null,
            JsonNamingPolicy? dictionaryKeyPolicy = null,
            JavaScriptEncoder? encoder = null,
            bool? propertyNameCaseInsensitive = null,
            int? maxDepth = null,
            bool? writeIndented = null,
#if NET5_0_OR_GREATER
            JsonIgnoreCondition? defaultIgnoreCondition = null,
            JsonNumberHandling? numberHandling = null,
#endif
#if NET6_0_OR_GREATER
            JsonUnknownTypeHandling? unknownTypeHandling = null,
#endif
            JsonCommentHandling? readCommentHandling = null,
            JsonNamingPolicy? propertyNamingPolicy = null
        ) {
            var opts = new JsonSerializerOptions {
#if NET5_0_OR_GREATER
                IncludeFields = includeFields ?? options.IncludeFields,
                IgnoreReadOnlyFields = ignoreReadOnlyFields ?? options.IgnoreReadOnlyFields,
                DefaultIgnoreCondition = defaultIgnoreCondition ?? options.DefaultIgnoreCondition,
                NumberHandling = numberHandling ?? options.NumberHandling,
#endif
#if NET6_0_OR_GREATER
                UnknownTypeHandling = unknownTypeHandling ?? options.UnknownTypeHandling,
#endif
                IgnoreReadOnlyProperties = ignoreReadOnlyProperties ?? options.IgnoreReadOnlyProperties,
                AllowTrailingCommas = allowTrailingCommas ?? options.AllowTrailingCommas,
                DefaultBufferSize = defaultBufferSize ?? options.DefaultBufferSize,
                DictionaryKeyPolicy = dictionaryKeyPolicy ?? options.DictionaryKeyPolicy,
                Encoder = encoder ?? options.Encoder,
                PropertyNameCaseInsensitive = propertyNameCaseInsensitive ?? options.PropertyNameCaseInsensitive,
                MaxDepth = maxDepth ?? options.MaxDepth,
                WriteIndented = writeIndented ?? options.WriteIndented,
                ReadCommentHandling = readCommentHandling ?? options.ReadCommentHandling,
                PropertyNamingPolicy = propertyNamingPolicy ?? options.PropertyNamingPolicy
            };

            // Filter out Object convertor to prevent unwanted recursions.
            var converters = options.Converters.Where(opt => opt is not ObjectConvertor).ToList();
            converters.ForEach(converter => opts.Converters.Add(converter));

            return opts;
        }

        public static IJsonPact IntoJsonPact(this JsonSerializerOptions options) => new Serializer(options);
    }
}
