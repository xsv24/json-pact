using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonPact.System {
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

        public override bool CanConvert(Type typeToConvert) => typeToConvert switch {
            Type { IsClass: true } when _ignoreClass.All(type => type != typeToConvert) => true,
            Type { IsValueType: true, IsEnum: false, IsPrimitive: false } when _ignoreStruct.All(type => type != typeToConvert) => true,
            Type { IsInterface: true } => true,
            _ => false
        };

        public override object? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        ) {
            var attr = typeToConvert.GetJsonPactAttribute();

            return reader.TokenType switch {
                JsonTokenType.StartObject => Reader(ref reader, typeToConvert, AddCasing(options, attr?.Casing)),
                _ => JsonSerializer.Deserialize(ref reader, typeToConvert, AddCasing(options, attr?.Casing))
            };
        }

        public override void Write(
            Utf8JsonWriter writer,
            object? objectToWrite,
            JsonSerializerOptions options
        ) {
            var type = objectToWrite?.GetType();
            var attr = type?.GetJsonPactAttribute();

            JsonSerializer.Serialize(
                writer,
                objectToWrite,
                type!,
                AddCasing(options, attr?.Casing)
            );
        }

        private static object? Reader(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        ) {
            var obj = JsonSerializer.Deserialize(
                ref reader,
                typeToConvert,
                options
            );

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

        private static JsonSerializerOptions AddCasing(JsonSerializerOptions options, CasingPolicy? policy) =>
            options.With(propertyNamingPolicy: policy ?? options.PropertyNamingPolicy);

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
}
