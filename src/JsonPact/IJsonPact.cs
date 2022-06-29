// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Newtonsoft.Json;

namespace JsonPact;

public interface IJsonPact {

    /// <summary>
    /// Attempts to serialize an object into a json formatted string.
    /// </summary>
    /// <param name="value">Value to be converted into json.</param>
    /// <typeparam name="T">Type of <paramref name="value"/>.</typeparam>
    /// <returns>Json string.</returns>
    /// <exception cref="JsonPactEncodeException">Occurs if serialization fails.</exception> 
    string Serialize<T>(T value);

    /// <summary>
    /// Attempts to deserialize a json string into an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="json">Json string to be converted.</param>
    /// <typeparam name="T">Desired type that json will be parsed into.</typeparam>
    /// <returns>Parse object of type <typeparamref name="T"/>.</returns>
    /// <exception cref="JsonPactDecodeException">Occurs if deserialization fails.</exception> 
    T? Deserialize<T>(string json);
}

public static class JsonPactConvertor {
    /// <summary>
    /// Converts Newtonsoft settings into an <see cref="IJsonPact"/>.
    /// </summary>
    /// <param name="options">Newtonsoft settings to used to serialize and deserialize json.</param>
    /// <returns><see cref="IJsonPact"/></returns>
    public static IJsonPact IntoJsonPact(this JsonSerializerSettings options) => new NewtonSoft.Serializer(options);
}
