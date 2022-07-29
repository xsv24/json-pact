using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonPact.System {
    public class ObjectConvertor : JsonConverter<object?> {
        internal readonly ConcurrentDictionary<Type, IReadOnlyList<string>> RequiredParams = new();
        internal readonly ConcurrentDictionary<Type, IReadOnlyList<string>> RequiredProps = new();

        // ignore these types as they already have convertors and we only care about the base types.
        private readonly HashSet<Type> _ignoreClass = new() {
            typeof(string),
            typeof(byte[]),
            typeof(JsonDocument)
        };

        // ignore these types as they already have convertors and we only care about the base types.
        private readonly HashSet<Type> _ignoreStruct = new() {
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(Guid),
            typeof(JsonElement),
            typeof(decimal),
            typeof(double),
            typeof(long),
            typeof(ulong),
            typeof(ushort),
            typeof(int),
            typeof(uint)
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

        private object? Reader(
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

        private object ValidateParameters(object obj) {
            var type = obj.GetType();

            if (ValidateCache(RequiredParams, type, obj)) return obj;

            var required = new List<string>();

            var missing = type.GetConstructorParams()
                .Where(param => param.IsRequired(type))
                .ForEach(param => required.Add(param.Name!))
                .FirstOrDefault(param => PropertyIsNull(type, param.Name!, obj));

            RequiredParams.TryAdd(type, required);

            return missing is { }
                 ? throw new JsonPactDecodeException($"missing required value '{missing.Name}'")
                 : obj;
        }

        private object ValidateProperties(Type type, object obj, object defaults) {
            if (ValidateCache(RequiredProps, type, obj)) return obj;

            var required = new List<string>();

            var missing = type.GetProperties()
                .Where(prop => prop.IsRequired(type, defaults))
                .ForEach(prop => required.Add(prop.Name))
                .FirstOrDefault(prop => prop.GetValue(obj) == null);

            RequiredProps.TryAdd(type, required);

            return missing is { }
                 ? throw new JsonPactDecodeException($"missing required value '{missing.Name}'")
                 : obj;
        }

        internal static bool ValidateCache(ConcurrentDictionary<Type, IReadOnlyList<string>> cache, Type type, object obj) {
            if (!cache.ContainsKey(type)) return false;

            var missingName = cache[type]
                .FirstOrDefault(param => PropertyIsNull(type, param, obj));

            return missingName is { }
                ? throw new JsonPactDecodeException($"missing required value '{missingName}'")
                : true;
        }

        private static bool PropertyIsNull(Type type, string name, object obj) =>
            type.GetProperty(name)?.GetValue(obj) == null;

        private static JsonSerializerOptions AddCasing(JsonSerializerOptions options, CasingPolicy? policy) =>
            options.With(propertyNamingPolicy: policy ?? options.PropertyNamingPolicy);
    }
}
