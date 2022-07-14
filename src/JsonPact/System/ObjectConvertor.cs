using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonPact.System;

public class ObjectConvertor : JsonConverter<object?> {
    // ignore these types as they already have convertors and we only care about the base types.
    private readonly HashSet<Type> _ignoreClass = new() {
        typeof(string),
        typeof(byte[])
    };

    // ignore these types as they already have convertors and we only care about the base types.
    private readonly HashSet<Type> _ignoreStruct = new() {
        typeof(DateTime),
        typeof(DateTimeOffset),
        typeof(Guid),
        typeof(JsonElement),
        typeof(decimal)
    };

    public override bool CanConvert(Type objectType) => objectType switch {
        Type { IsClass: true } when _ignoreClass.All(type => type != objectType) => true,
        Type { IsValueType: true, IsEnum: false, IsPrimitive: false } when _ignoreStruct.All(type => type != objectType) => true,
        Type { IsInterface: true } => true,
        _ => false
    };

    public override object? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    ) => reader.TokenType switch {
        JsonTokenType.StartObject => Reader(ref reader, typeToConvert, options),
        _ => JsonSerializer.Deserialize(ref reader, typeToConvert, options.With())
    };

    public override void Write(
        Utf8JsonWriter writer,
        object? objectToWrite,
        JsonSerializerOptions options
    ) => JsonSerializer.Serialize(writer, objectToWrite, objectToWrite?.GetType()!, options.With());

    private static object? Reader(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    ) {
        var obj = JsonSerializer.Deserialize(ref reader, typeToConvert, options.With());

        if (obj is null) return null;

        // Create an instance of an object if it has no or an empty constructor.
        var defaults = typeToConvert.GetConstructor(Type.EmptyTypes) != null
            ? Activator.CreateInstance(typeToConvert)
            : null;

        return defaults switch {
            object { } defaulted => ValidateProperties(typeToConvert, obj, defaulted),
            null => ValidateParameters(obj)
        };
    }

    private static object ValidateParameters(object obj) {
        var type = obj.GetType();

        type.GetConstructorParams().ForEach(param => {
            var value = type.GetProperty(param.Name!)?.GetValue(obj);

            if (param.IsRequired(type) && value == null) {
                throw new JsonPactDecodeException($"missing required value '{param.Name}'");
            }
        });

        return obj;
    }

    private static object ValidateProperties(Type type, object obj, object defaults) {
        type.GetProperties().ForEach(prop => {
            var required = prop.IsRequired(type, defaults);

            if (required && prop.GetValue(obj) == null) {
                throw new JsonPactDecodeException($"missing required value '{prop.Name}'");
            }
        });

        return obj;
    }
}
