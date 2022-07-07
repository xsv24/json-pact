using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using JsonPact;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace JsonPact.System.Json;

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
        PropertyNamingPolicy = new NamingStrategy(casing),
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new ObjectConvertor() }
    };

    public static JsonSerializerOptions With(
        this JsonSerializerOptions options,
        bool? includeFields = null,
        bool? ignoreReadOnlyFields = null,
        bool? ignoreReadOnlyProperties = null,
        JsonIgnoreCondition? defaultIgnoreCondition = null,
        bool? allowTrailingCommas = null,
        int? defaultBufferSize = null,
        JsonNamingPolicy? dictionaryKeyPolicy = null,
        JavaScriptEncoder? encoder = null,
        JsonUnknownTypeHandling? unknownTypeHandling = null,
        bool? propertyNameCaseInsensitive = null,
        JsonNumberHandling? numberHandling = null,
        int? maxDepth = null,
        bool? writeIndented = null,
        JsonCommentHandling? readCommentHandling = null,
        JsonNamingPolicy? propertyNamingPolicy = null
    ) {
        var opts = new JsonSerializerOptions {
            IncludeFields = includeFields ?? options.IncludeFields,
            IgnoreReadOnlyFields = ignoreReadOnlyFields ?? options.IgnoreReadOnlyFields,
            IgnoreReadOnlyProperties = ignoreReadOnlyProperties ?? options.IgnoreReadOnlyProperties,
            DefaultIgnoreCondition = defaultIgnoreCondition ?? options.DefaultIgnoreCondition,
            AllowTrailingCommas = allowTrailingCommas ?? options.AllowTrailingCommas,
            DefaultBufferSize = defaultBufferSize ?? options.DefaultBufferSize,
            DictionaryKeyPolicy = dictionaryKeyPolicy ?? options.DictionaryKeyPolicy,
            Encoder = encoder ?? options.Encoder,
            UnknownTypeHandling = unknownTypeHandling ?? options.UnknownTypeHandling,
            PropertyNameCaseInsensitive = propertyNameCaseInsensitive ?? options.PropertyNameCaseInsensitive,
            NumberHandling = numberHandling ?? options.NumberHandling,
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

    public static JsonConverter<T> GetConverter<T>(this JsonSerializerOptions options)
        => (JsonConverter<T>)options.GetConverter(typeof(T));
}
