using System;
using FluentAssertions;
using JsonPact.NewtonSoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace JsonPact.Newtonsoft.Test {
    public class JsonOptionsTests {
        private static string Message => $"JsonPact relies on '{nameof(JsonSerializerSettings.DefaultValueHandling)}' & '{nameof(JsonSerializerSettings.ContractResolver)}' properties and are immutable.";

        [Fact]
        public void Converting_Serializer_Settings_Into_JsonOptions() {
            var settings = new JsonSerializerSettings();

            Action act = () => settings.AddJsonPact(JsonPactCase.Snake);

            act.Should().NotThrow<ArgumentException>();
        }

        [Theory]
        [InlineData(default(DefaultValueHandling), false)]
        [InlineData(DefaultValueHandling.Populate, false)]
        [InlineData(DefaultValueHandling.Ignore, true)]
        [InlineData(DefaultValueHandling.IgnoreAndPopulate, true)]
        public void Attempting_To_Set_JsonOptions_DefaultValueHandling_Throws_An_Error(DefaultValueHandling value, bool error) {
            var settings = new JsonSerializerSettings {
                DefaultValueHandling = value
            };

            Action act = () => settings.AddJsonPact(JsonPactCase.Camel);

            if (error) {
                act.Should().Throw<ArgumentException>().WithMessage(Message);
            } else {
                act.Should().NotThrow<ArgumentException>();
            }
        }

        private class TestResolver : IContractResolver {
            public JsonContract ResolveContract(Type type) => throw new NotImplementedException();
        }

        [Fact]
        public void Attempting_To_Set_JsonOptions_ContractResolver_Throws_An_Error() {
            var settings = new JsonSerializerSettings {
                ContractResolver = new TestResolver()
            };

            Action act = () => settings.AddJsonPact(JsonPactCase.Camel);

            act.Should().Throw<ArgumentException>().WithMessage(Message);
        }
    }
}
