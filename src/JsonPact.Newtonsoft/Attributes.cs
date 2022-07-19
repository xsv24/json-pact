// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Newtonsoft.Json;

namespace JsonPact.NewtonSoft {
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public sealed class JsonPactAttribute : JsonContainerAttribute {
        public JsonPactAttribute() { }

        public JsonPactAttribute(JsonPactCase casing) {
            this.NamingStrategyType = casing.IntoNamingStrategy()?.GetType();
        }
    }
}
