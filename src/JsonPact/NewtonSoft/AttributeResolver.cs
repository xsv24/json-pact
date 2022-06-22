// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace JsonPact.NewtonSoft;

public class JsonPactAttributesResolver : DefaultContractResolver {

    protected override JsonContract CreateContract(Type objectType) => base.CreateContract(objectType) switch {
        JsonObjectContract contract => AddObjectContractProperties(contract),
        var other => other
    };

    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization) {
        var properties = base.CreateProperties(type, memberSerialization);
        var defaults = type.GetConstructor(Type.EmptyTypes) != null ? Activator.CreateInstance(type) : null;
        var merged = defaults switch { { } defaulted => MergeAllDefaults(properties, type, defaulted),
            null => MergeConstructorDefaults(properties, type)
        };

        return merged.ToList() ?? new List<JsonProperty>();
    }

    private IEnumerable<JsonProperty> MergeConstructorDefaults(IEnumerable<JsonProperty> props, Type type) {
        var parameters = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
            .FirstOrDefault() // TODO: Make this better could throw an error or have a constructor attribute.
            ?.GetParameters()
            ?.ToDictionary(
                val => val.Name!,
                val => val
            );

        return parameters switch { { } args => props.Select(prop => MergeConstructorDefaultParams(prop, args)),
            _ => props
        };
    }

    private static JsonProperty MergeConstructorDefaultParams(JsonProperty prop, Dictionary<string, ParameterInfo> args) {
        if (prop.UnderlyingName is null) return prop;

        var info = args[prop.UnderlyingName];

        prop.NullValueHandling = NullValueHandling.Ignore;
        prop.Required = info switch { { HasDefaultValue: false } when IsNullable(prop.PropertyType, info.CustomAttributes) => Required.Default, { HasDefaultValue: false } => Required.Always,
            _ => Required.Default
        };

        if (info.HasDefaultValue) return prop;

        prop.DefaultValue = info.DefaultValue;

        return prop;
    }

    private IEnumerable<JsonProperty> MergeAllDefaults(IEnumerable<JsonProperty> props, Type type, object defaulted) {
        var members = GetSerializableMembers(type).ToDictionary(
            val => val.Name,
            val => val
        );

        return members switch { { } fields => props.Select(prop => MergePropertyDefaults(prop, fields, defaulted)),
            null => props
        };
    }


    private static JsonProperty MergePropertyDefaults(JsonProperty prop, Dictionary<string, MemberInfo> fields, object defaulted) {
        if (prop.UnderlyingName is null) return prop;

        var member = fields[prop.UnderlyingName];
        var info = (PropertyInfo)member;

        var defaultedValue = info.GetValue(defaulted);

        prop.NullValueHandling = NullValueHandling.Ignore;
        prop.DefaultValue = defaultedValue;
        prop.Required = defaultedValue switch { { } => Required.Default,
            null when IsNullable(prop.PropertyType, info.CustomAttributes) => Required.Default,
            null => Required.Always
        };

        return prop;
    }

    private static JsonObjectContract AddObjectContractProperties(JsonObjectContract contract) {
        contract.ItemNullValueHandling = NullValueHandling.Ignore;
        return contract;
    }

    private static bool IsNullable(Type? type, IEnumerable<CustomAttributeData> attributes) =>
        (type != null && Nullable.GetUnderlyingType(type) != null) ||
        attributes.Any(attr => attr.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");
}
