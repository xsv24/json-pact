using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using JsonPact;
using JsonPact.Tests;

public class JsonExtensionTests {

    [Fact]
    public void Test_Json_Extensions_Work() {
        var data = new JsonRecord(
            RequiredValue: "required",
            Nullable: null
        );

        var snake = data.IntoJson(JsonPactCase.Snake);
        var kebab = data.IntoJson(JsonPactCase.Kebab);
        var camel = data.IntoJson(JsonPactCase.Camel);

        snake.Should().Be(@$"{{""required_value"":""{data.RequiredValue}"",""defaulted"":""{data.Defaulted}""}}");
        camel.Should().Be(@$"{{""requiredValue"":""{data.RequiredValue}"",""defaulted"":""{data.Defaulted}""}}");
        kebab.Should().Be(@$"{{""required-value"":""{data.RequiredValue}"",""defaulted"":""{data.Defaulted}""}}");

        kebab = snake.IntoKebabCase();
        camel = kebab.IntoCamelCase();
        snake = camel.IntoSnakeCase();

        snake.Should().Be(@$"{{""required_value"":""{data.RequiredValue}"",""defaulted"":""{data.Defaulted}""}}");
        camel.Should().Be(@$"{{""requiredValue"":""{data.RequiredValue}"",""defaulted"":""{data.Defaulted}""}}");
        kebab.Should().Be(@$"{{""required-value"":""{data.RequiredValue}"",""defaulted"":""{data.Defaulted}""}}");
    }
}

