// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace JsonPact;

public abstract class JsonPactException : Exception {
    protected JsonPactException(string message, Exception? inner = null) : base(message, inner) { }
}

public class JsonPactDecodeException : JsonPactException {
    public JsonPactDecodeException(string message, Exception? inner = null) : base(message, inner) { }
}

public class JsonPactEncodeException : JsonPactException {
    public JsonPactEncodeException(string message, Exception? inner = null) : base(message, inner) { }
}
