![.net](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![nuget](https://img.shields.io/badge/NuGet-004880?style=for-the-badge&logo=nuget&logoColor=white)

[![tests status](https://github.com/xsv24/json-pact/actions/workflows/dotnet.yml/badge.svg?event=push)](https://github.com/xsv24/json-pact/actions?query=branch%3Amain+)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=xsv24_json-pact&metric=coverage)](https://sonarcloud.io/summary/new_code?id=xsv24_json-pact)

# 🤝 json-pact

json wrapper library that enforces casing & validates required properties by checking if the property uses the nullable `?` property marker as specified by [nullable value types](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/nullable-value-types)
and also makes use of optional values without the need for extra attributes.

```c#
[JsonPact(JsonPactCase.Snake)]
public record JsonDTO(
    string RequiredValue,
    string? NullableValue,
    string OptionalValue = "default"
);

// vs

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public record JsonDTO(
    [property: JsonProperty(Required = Required.Always)]
    string RequiredValue,
    string? NullableValue,
    [property: DefaultValue("fallback")]
    string OptionalValue
);
```

# 🏎️💨 Getting Started

```c#
[JsonPact]
public record JsonDTO(
    string RequiredValue,
    string? NullableValue,
    string OptionalValue = "default"
);
```

> Here we define the schema with a required value `RequiredValue` with rest of the values
> `NullableValue` & `OptionalValue` defined as default-able properties.

> So if the `RequiredValue` is found missing an error will be thrown and default-able values will be automatically defaulted.

```c#
using JsonPact.NewtonSoft;

var pact = JsonPacts.Default(JsonPactCase.Snake).IntoJsonPact();

var json = pact.Serialize(new JsonDTO("required", null));
// = { "required_value": "required", "optional_value": "default" }

var obj = pact.Deserialize<JsonDTO>(json);
// = JsonDTO("required", null, "default")

var err = pact.Deserialize<JsonDTO>("{ }");
// = JsonPactDecodeException
```

### 🎮 Overriding casing

> If you are dealing with mixed casing you can provide a casing type with the `JsonPact` attribute,
> this will override the default casing and will allow you to have nested objects with different casing.

```c#
[JsonPact(JsonPactCase.Snake)]
public record Snake(Camel RequiredValue);

[JsonPact(JsonPactCase.Camel)]
public record Camel(Kebab RequiredValue);

[JsonPact(JsonPactCase.Kebab)]
public record Kebab(string RequiredValue);
```

```c#
var pact = JsonPacts.Default(JsonPactCase.Snake).IntoJsonPact();

var snake = new Snake(new Camel(new Kebab("hello")));

var json = pact.Serialize(snake);
// = { "required_value": { "requiredValue": { "required-value": "hello" } } }
```
