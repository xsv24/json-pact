using System;
using System.Text.Json.Serialization;

namespace JsonPact.System {

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class JsonPactAttribute : JsonAttribute {
        public CasingPolicy? Casing { get; }

        public JsonPactAttribute() { }

        public JsonPactAttribute(JsonPactCase casing) {
            Casing = new CasingPolicy(casing);
        }
    }
}
