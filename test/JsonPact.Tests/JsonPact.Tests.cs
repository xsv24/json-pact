// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using FluentAssertions;
using JsonPact;
using JsonPact.NewtonSoft;

namespace JsonPact.Tests;

[JsonPact]
public record JsonRecord(
    string RequiredValue,
    string? Nullable,
    string Defaulted = "default",
    string? NullableDefault = null
);

public class JsonPactTests {

    [Fact]
    public void Required_And_Defaulted_Values_Are_Populated() {
        var pact = JsonPacts.Default(JsonPactCase.Snake).IntoJsonPact();

        var data = new JsonRecord(
            RequiredValue: "required",
            Nullable: null
        );

        var json = pact.Serialize(data);

        json.Should().Be(@$"{{""required_value"":""{data.RequiredValue}"",""defaulted"":""{data.Defaulted}""}}");
    }

    [Theory]
    [InlineData(JsonPactCase.Snake, null)]
    [InlineData(JsonPactCase.Camel, null)]
    [InlineData(JsonPactCase.Kebab, null)]

    [InlineData(JsonPactCase.Snake, "{ }")]
    [InlineData(JsonPactCase.Camel, "{ }")]
    [InlineData(JsonPactCase.Kebab, "{ }")]

    [InlineData(JsonPactCase.Snake, @"{ ""nullable"": ""optional"", ""defaulted"": ""override"", ""nullable_default"": ""optional"" }")]
    [InlineData(JsonPactCase.Camel, @"{ ""nullable"": ""optional"", ""defaulted"": ""override"", ""nullable_default"": ""optional"" }")]
    [InlineData(JsonPactCase.Kebab, @"{ ""nullable"": ""optional"", ""defaulted"": ""override"", ""nullable_default"": ""optional"" }")]

    [InlineData(JsonPactCase.Snake, @"{ ""requiredValue"": ""required"" }")]
    [InlineData(JsonPactCase.Camel, @"{ ""required_value"": ""required"" }")]
    [InlineData(JsonPactCase.Kebab, @"{ ""required_value"": ""required"" }")]
    public void Missing_Required_Prop_On_Deserialize_Throws(JsonPactCase casing, string? json) {
        var pact = JsonPacts.Default(casing).IntoJsonPact();

        var act = () => pact.Deserialize<JsonRecord>(json!);

        act.Should().Throw<JsonPactDecodeException>();
    }

    [Theory]
    [InlineData(JsonPactCase.Snake, "required_value", "nullable_default")]
    [InlineData(JsonPactCase.Kebab, "required-value", "nullable-default")]
    [InlineData(JsonPactCase.Camel, "requiredValue", "nullableDefault")]
    public void Objects_Are_Serialized_With_Correct_Casing(JsonPactCase casing, string requiredKey, string nullableKey) {
        var pact = JsonPacts.Default(casing).IntoJsonPact();

        var data = new JsonRecord(
            RequiredValue: "required",
            Nullable: null,
            NullableDefault: "nullable default"
        );

        var json = pact.Serialize(data);

        json.Should().Be(@$"{{""{requiredKey}"":""{data.RequiredValue}"",""defaulted"":""{data.Defaulted}"",""{nullableKey}"":""{data.NullableDefault}""}}");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" ")]
    [InlineData("()")]
    public void On_Deserialize_With_Invalid_Json_Results_In_Decode_Error(string? json) {
        var pact = JsonPacts.Default(JsonPactCase.Snake).IntoJsonPact();

        var act = () => pact.Deserialize<JsonRecord>(json!);

        act.Should().Throw<JsonPactDecodeException>();
    }

    [Fact]
    public void On_Serialize_With_Invalid_Json_An_Json_Encode_Error_Is_Thrown() {
        var pact = JsonPacts.Default(JsonPactCase.Snake).IntoJsonPact();

        var act = () => pact.Serialize<JsonRecord>(null!);

        act.Should().Throw<JsonPactEncodeException>();
    }
}
