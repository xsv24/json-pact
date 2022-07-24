// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace JsonPact.NewtonSoft {
    public class JsonPactAttributesResolver : DefaultContractResolver {
        private readonly ConcurrentDictionary<Type, IEnumerable<JsonProperty>> _map = new();

        /// <inheritdoc />
        protected override JsonContract CreateContract(Type objectType) => base.CreateContract(objectType) switch {
            JsonObjectContract contract => AddObjectContractProperties(contract),
            var other => other
        };

        /// <inheritdoc />
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization) {
            var hit = _map.GetValueOrDefault(type);

            var properties = hit switch {
                IEnumerable<JsonProperty> { } props => props.ToList(),
                null => CreateJsonProperties(type, memberSerialization).ToList()
            };

            _map.TryAdd(type, properties);

            return properties;
        }

        private IEnumerable<JsonProperty> CreateJsonProperties(Type type, MemberSerialization memberSerialization) {
            var properties = base.CreateProperties(type, memberSerialization);

            // Create an instance of an object if it has no or an empty constructor.
            var defaults = type.GetConstructor(Type.EmptyTypes) != null ? Activator.CreateInstance(type) : null;

            // Set up json properties based on whether the object is a plain DTO with an empty constructor. 
            return defaults switch {
                object { } defaulted => MergePropertyDefaults(properties, type, defaulted),
                null => MergeConstructorDefaults(properties, type)
            };
        }

        /// <summary>
        /// Attempt to find default values within the constructors and check
        /// if the '?' nullable operator is in use to mark if a property is required.
        /// </summary>
        /// <param name="props">Original json property values.</param>
        /// <param name="type">Object type used to serialize / deserialize json.</param>
        /// <returns>Updated list of <see cref="JsonProperty">json properties</see> with updated default value and required status.</returns>
        private static IEnumerable<JsonProperty> MergeConstructorDefaults(IEnumerable<JsonProperty> props, Type type) {
            var parameters = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault() // TODO: Make this better could throw an error or have a constructor attribute.
                ?.GetParameters()
                .ToDictionary(val => val.Name!, val => val);

            return parameters switch {
                Dictionary<string, ParameterInfo> { } args => props.Select(prop => MergeConstructorDefaultParams(prop, args)),
                null => props,
            };
        }

        private static JsonProperty MergeConstructorDefaultParams(JsonProperty prop, IReadOnlyDictionary<string, ParameterInfo> args) {
            var info = args.GetValueOrDefault(prop.UnderlyingName!);

            if (info is null) return prop;

            prop.Required = info switch {
                ParameterInfo { HasDefaultValue: false } when
                    prop.PropertyType.IsNullable() ||
                    info.CustomAttributes.IsNullable() ||
                    info.IsNullableContext()
                    => Required.Default,
                ParameterInfo { HasDefaultValue: false } => Required.Always,
                _ => Required.Default
            };

            if (!info.HasDefaultValue) return prop;

            prop.DefaultValue = info.DefaultValue;

            return prop;
        }

        /// <summary>
        /// Attempt to find defaulted property values and check if the '?' nullable operator 
        /// is in use to mark the property as required or not.
        /// </summary>
        /// <param name="props">Original json property values.</param>
        /// <param name="type">Object type used to serialize / deserialize json.</param>
        /// <param name="defaulted">Instance of type created from an empty or non-existent constructor i.e DTO.</param> 
        /// <returns>Updated list of <see cref="JsonProperty">json properties</see> with updated default value and required status.</returns>
        private IEnumerable<JsonProperty> MergePropertyDefaults(IEnumerable<JsonProperty> props, Type type, object defaulted) {
            var members = GetSerializableMembers(type)
                .ToDictionary(val => val.Name, val => val);

            return members switch {
                Dictionary<string, MemberInfo> { } fields => props.Select(prop => MergePropertyDefaults(prop, fields, defaulted)),
                null => props
            };
        }

        private static JsonProperty MergePropertyDefaults(JsonProperty prop, IReadOnlyDictionary<string, MemberInfo> fields, object defaulted) {
            var info = (PropertyInfo?)fields.GetValueOrDefault(prop.UnderlyingName!);

            if (info is null) return prop;

            var defaultedValue = info.GetValue(defaulted);

            prop.DefaultValue = defaultedValue;
            prop.Required = defaultedValue switch {
                object { } => Required.Default,
                null when
                    prop.PropertyType.IsNullable() ||
                    info.CustomAttributes.IsNullable() ||
                    (info.SetMethod?.CustomAttributes).IsNullable() ||
                    info.IsNullableContext()
                => Required.Default,
                null => Required.Always
            };

            return prop;
        }

        private static JsonObjectContract AddObjectContractProperties(JsonObjectContract contract) => contract;
    }
}
