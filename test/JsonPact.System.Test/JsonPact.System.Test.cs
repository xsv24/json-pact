using System.Linq;
using FluentAssertions;
using JsonPact.Tests;
using Xunit;

namespace JsonPact.System.Test;

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
        // json.Should().Be(@$"{{""required_value"":{{""requiredValue"":""hello""}}}}");
        // obj.Should().Be(origin);
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

    private void AssertSerializedRequiredAndDefaults<T>(
        JsonPactCase casing,
        string[] populate,
        string[] ignore,
        T data
    ) where T : notnull {
        var pact = JsonPacts.Default(casing).IntoJsonPact();

        var json = pact.Serialize(data);

        json.Should().Be(data.IntoJson(casing));

        var populated = populate
            .Select(key => json.Contains(key.IntoCasedStr(casing)))
            .All(item => item == true);

        var ignored = ignore
            .Select(key => json.Contains(key.IntoCasedStr(casing)))
            .All(item => item == false);

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

    private void AssertDecodeError<T>(string? json, JsonPactCase casing = JsonPactCase.Snake) {
        var pact = JsonPacts.Default(casing).IntoJsonPact();

        var act = () => pact.Deserialize<T>(json!);

        act.Should().Throw<JsonPactDecodeException>();
    }

    private void AssertEncodeError<T>(T? value, JsonPactCase casing = JsonPactCase.Snake) {
        var pact = JsonPacts.Default(casing).IntoJsonPact();

        var act = () => pact.Serialize<T>(value!);

        act.Should().Throw<JsonPactEncodeException>();
    }
}
