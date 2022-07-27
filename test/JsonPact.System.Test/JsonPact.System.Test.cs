using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using JsonPact.Tests;
using Xunit;

namespace JsonPact.System.Test {
    public class JsonPactSystemTest {

        [Theory]
        [InlineData(JsonPactCase.Snake)]
        [InlineData(JsonPactCase.Camel)]
        [InlineData(JsonPactCase.Kebab)]
        [InlineData(JsonPactCase.Pascal)]
        public void Required_And_Defaulted_Values_Are_Populated(JsonPactCase casing) {
            var populated = new[] { "required_value", "defaulted" };
            var ignored = new[] { "nullable_default", "nullable" };

            AssertSerializedRequiredAndDefaults(
                casing,
                populated,
                ignored,
                new JsonRecord(RequiredValue: "required", Nullable: null)
            );

            AssertSerializedRequiredAndDefaults(
                casing,
                populated,
                ignored,
                new JsonRecordDTO { RequiredValue = "required", Nullable = null }
            );

            AssertSerializedRequiredAndDefaults(
                casing,
                populated,
                ignored,
                new JsonClass { RequiredValue = "required", Nullable = null }
            );
        }

        [Fact]
        public void JsonPact_Casing_Attribute_Overrides_Default_Casing() {
            var pact = JsonPacts.Default(JsonPactCase.Camel).IntoJsonPact();

            var origin = new SnakeCase<CamelCase<string>>(
                RequiredValue: new CamelCase<string>("hello")
            );

            var json = pact.Serialize(origin);
            var obj = pact.Deserialize<SnakeCase<CamelCase<string>>>(json);

            // TODO: Update the casing in the settings based on the attribute.
            // i.e { "required_value": { "requiredValue":"hello" } }

            json.Should().Be(@$"{{""required_value"":{{""required_value"":""hello""}}}}");
            obj.Should().Be(origin);
        }

        [Fact]
        public void All_Optional_Property_DTOs_Are_Not_Affected_By_Required_Check() {
            // Arrange
            var pact = JsonPacts.Default(JsonPactCase.Snake).IntoJsonPact();
            var json = @"{""extra_prop"":""value""}";

            // Act
            var nullableRecord = pact.Deserialize<NullableRecord>(json);
            var nullableDto = pact.Deserialize<NullableDTO>(json);

            var optionalRecord = pact.Deserialize<DefaultedRecord>(json);
            var optionalDto = pact.Deserialize<DefaultedDTO>(json);

            var optionalAndDefaultedRecord = pact.Deserialize<OptionalAndDefaultedRecord>(json);
            var optionalAndDefaultedDto = pact.Deserialize<OptionalAndDefaultedDTO>(json);

            // Assert
            nullableRecord.Should().Be(new NullableRecord(Nullable: null));
            nullableDto.Should().Be(new NullableDTO { Nullable = null });

            optionalRecord.Should().Be(new DefaultedRecord(Defaulted: "default"));
            optionalDto.Should().Be(new DefaultedDTO { Defaulted = "default" });

            optionalAndDefaultedRecord.Should().Be(new OptionalAndDefaultedRecord(
                Defaulted: "default",
                Nullable: null,
                NullableDefault: null
            ));
            optionalAndDefaultedDto.Should().Be(new OptionalAndDefaultedDTO {
                Defaulted = "default",
                Nullable = null,
                NullableDefault = null
            });
        }

        [Theory]
        [InlineData(JsonPactCase.Snake)]
        [InlineData(JsonPactCase.Camel)]
        [InlineData(JsonPactCase.Kebab)]
        [InlineData(JsonPactCase.Pascal)]
        public void Objects_Are_Serialized_With_Correct_Casing(JsonPactCase casing) {
            var populated = new[] { "required_value", "nullable_default", "defaulted" };
            var ignored = new[] { @"""nullable""" };

            AssertSerializedRequiredAndDefaults(
                casing,
                populated,
                ignored,
                new JsonRecord(
                    RequiredValue: "required",
                    Nullable: null,
                    NullableDefault: "nullable default"
                )
            );

            AssertSerializedRequiredAndDefaults(
                casing,
                populated,
                ignored,
                new JsonRecordDTO {
                    RequiredValue = "required",
                    Nullable = null,
                    NullableDefault = "nullable default"
                }
            );

            AssertSerializedRequiredAndDefaults(
                casing,
                populated,
                ignored,
                new JsonClass {
                    RequiredValue = "required",
                    Nullable = null,
                    NullableDefault = "nullable default"
                }
            );
        }

        private static void AssertSerializedRequiredAndDefaults<T>(
            JsonPactCase casing,
            IEnumerable<string> populate,
            IEnumerable<string> ignore,
            T data
        ) where T : notnull {
            var pact = JsonPacts.Default(casing).IntoJsonPact();

            var json = pact.Serialize(data);

            json.Should().Be(data.IntoJson(casing));

            var populated = populate
                .Select(key => json.Contains(key.IntoCasedStr(casing)))
                .All(item => item);

            var ignored = ignore
                .Select(key => json.Contains(key.IntoCasedStr(casing)))
                .All(item => !item);

            populated.Should().Be(true);
            ignored.Should().Be(true);
        }

        [Theory]
        [InlineData(JsonPactCase.Snake, null)]
        [InlineData(JsonPactCase.Camel, null)]
        [InlineData(JsonPactCase.Kebab, null)]
        [InlineData(JsonPactCase.Pascal, null)]

        [InlineData(JsonPactCase.Snake, "{ }")]
        [InlineData(JsonPactCase.Camel, "{ }")]
        [InlineData(JsonPactCase.Kebab, "{ }")]
        [InlineData(JsonPactCase.Pascal, "{ }")]

        [InlineData(JsonPactCase.Snake, @"{ ""nullable"": ""optional"", ""defaulted"": ""override"", ""nullable_default"": ""optional"" }")]
        [InlineData(JsonPactCase.Camel, @"{ ""nullable"": ""optional"", ""defaulted"": ""override"", ""nullable_default"": ""optional"" }")]
        [InlineData(JsonPactCase.Kebab, @"{ ""nullable"": ""optional"", ""defaulted"": ""override"", ""nullable_default"": ""optional"" }")]
        [InlineData(JsonPactCase.Pascal, @"{ ""nullable"": ""optional"", ""defaulted"": ""override"", ""nullable_default"": ""optional"" }")]

        [InlineData(JsonPactCase.Snake, @"{ ""requiredValue"": ""required"" }")]
        [InlineData(JsonPactCase.Camel, @"{ ""required_value"": ""required"" }")]
        [InlineData(JsonPactCase.Kebab, @"{ ""required_value"": ""required"" }")]
        [InlineData(JsonPactCase.Pascal, @"{ ""required_value"": ""required"" }")]
        public void Missing_Required_Prop_On_Deserialize_Throws(JsonPactCase casing, string? json) {
            AssertDecodeError<JsonRecord>(json, casing);
            AssertDecodeError<JsonRecordDTO>(json, casing);
            AssertDecodeError<JsonClass>(json, casing);
        }

        [Fact]
        public void ObjectConvertor_Constructor_Cache_Is_Used() {
            var convertor = new ObjectConvertor();

            ReadJson<JsonRecord>(convertor, @"{ ""required_value"": ""required"" }");

            convertor.RequiredProps.Should().HaveCount(0);
            convertor.RequiredParams.Should().HaveCount(1);

            var required = convertor.RequiredParams.First();
            required.Key.Should().Be(typeof(JsonRecord));
            required.Value.Should().BeEquivalentTo(new List<string> { "RequiredValue" }.ToImmutableArray());
        }

        [Fact]
        public void Check_Cache_Hit_With_No_Matching_Key() {
            var convertor = new ObjectConvertor();

            var valid = ObjectConvertor.ValidateCache(convertor.RequiredParams, typeof(JsonRecord), new JsonRecord("hi", null));

            valid.Should().Be(false);
        }

        [Fact]
        public void Check_Cache_Hit_With_Required_Satisfied() {
            var convertor = new ObjectConvertor();

            var obj = ReadJson<JsonRecord>(convertor, @"{ ""required_value"": ""required"" }")!;

            var valid = ObjectConvertor.ValidateCache(convertor.RequiredParams, typeof(JsonRecord), obj);

            valid.Should().Be(true);
        }

        [Fact]
        public void Check_Cache_Hit_With_Required_Not_Satisfied() {
            var convertor = new ObjectConvertor();

            _ = ReadJson<JsonRecord>(convertor, @"{ ""required_value"": ""required"" }")!;

            Action act = () => ObjectConvertor.ValidateCache(
                convertor.RequiredParams,
                typeof(JsonRecord),
                new JsonRecord(null!, null)
            );

            act.Should().Throw<JsonPactDecodeException>();
        }

        [Fact]
        public void ObjectConvertor_Properties_Cache_Is_Used() {
            var convertor = new ObjectConvertor();

            ReadJson<JsonRecordDTO>(convertor, @"{ ""required_value"": ""required"" }");

            convertor.RequiredParams.Should().HaveCount(0);
            convertor.RequiredProps.Should().HaveCount(1);

            var required = convertor.RequiredProps.First();
            required.Key.Should().Be(typeof(JsonRecordDTO));
            required.Value.Should().BeEquivalentTo(new List<string> { "RequiredValue" }.ToImmutableArray());
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData(" ")]
        [InlineData("()")]
        public void On_Deserialize_With_Invalid_Json_Results_In_Decode_Error(string? json) {
            AssertDecodeError<JsonRecord>(json);
            AssertDecodeError<JsonRecordDTO>(json);
            AssertDecodeError<JsonClass>(json);
        }

        [Fact]
        public void On_Serialize_With_Invalid_Json_An_Json_Encode_Error_Is_Thrown() {
            AssertEncodeError<JsonRecord>(null!);
            AssertEncodeError<JsonRecordDTO>(null!);
            AssertEncodeError<JsonClass>(null!);
        }

        private static T? ReadJson<T>(ObjectConvertor convertor, string json) {
            ReadOnlySpan<byte> span = Encoding.UTF8.GetBytes(json);
            var reader = new Utf8JsonReader(span);

            object? obj = null;

            while (reader.Read()) {
                obj = convertor.Read(ref reader, typeof(T), JsonPacts.Default(JsonPactCase.Snake));
            }

            return (T?)obj;
        }

        private static void AssertDecodeError<T>(string? json, JsonPactCase casing = JsonPactCase.Snake) {
            var pact = JsonPacts.Default(casing).IntoJsonPact();

            Func<T?> act = () => pact.Deserialize<T>(json!);

            act.Should().Throw<JsonPactDecodeException>();
        }

        private static void AssertEncodeError<T>(T? value, JsonPactCase casing = JsonPactCase.Snake) {
            var pact = JsonPacts.Default(casing).IntoJsonPact();

            Func<string> act = () => pact.Serialize<T>(value!);

            act.Should().Throw<JsonPactEncodeException>();
        }
    }
}
