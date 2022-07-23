[![.net](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=csharp&logoColor=white)](https://docs.microsoft.com/en-us/dotnet/core/introduction)

[![NuGet](https://img.shields.io/nuget/v/JsonPact.Newtonsoft?style=flat-square&logo=Nuget)](https://www.nuget.org/packages/JsonPact.Newtonsoft)
[![license](https://img.shields.io/github/license/xsv24/json-pact?color=blue&style=flat-square&logo=)](./LICENSE)
<!---
Once we get some downloads.
[![NuGet](https://img.shields.io/nuget/dt/JsonPact.Newtonsoft?style=flat-square&logo=Nuget)](https://www.nuget.org/packages/JsonPact.Newtonsoft)
-->

[![tests status](https://img.shields.io/github/workflow/status/xsv24/json-pact/tests?label=tests&logo=Github&style=flat-square)](https://github.com/xsv24/json-pact/actions?query=branch%3Amain+)
[![release status](https://img.shields.io/github/workflow/status/xsv24/json-pact/release?label=release&logo=Github&style=flat-square)](https://github.com/xsv24/json-pact/actions?query=branch%3Amain+)
[![coverage](https://img.shields.io/sonar/coverage/xsv24_json-pact/main?logo=sonarcloud&server=https%3A%2F%2Fsonarcloud.io&style=flat-square)](https://sonarcloud.io/summary/new_code?id=xsv24_json-pact)
[![coverage](https://img.shields.io/sonar/quality_gate/xsv24_json-pact/main?logo=sonarcloud&server=https%3A%2F%2Fsonarcloud.io&style=flat-square)](https://sonarcloud.io/summary/new_code?id=xsv24_json-pact)

# 🤝 json-pact

json wrapper library that enforces casing & validates required properties by checking if the property uses the nullable `?` property marker as specified by [nullable value types](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/nullable-value-types)
and also makes use of optional values without the need for extra attributes.

💪 Extending
- [x] [Newtonsoft](https://www.newtonsoft.com/json)
- [ ] [System.Text.Json](https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-how-to?pivots=dotnet-6-0)

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

## 🥽 Prerequisites

You will need to enable the nullable project setting within your projects
`.csproj` file.

```xml
<PropertyGroup>
    <!-- ... -->
    <Nullable>enable</Nullable>
</PropertyGroup>
```

## Install

```bash
dotnet add package JsonPact.Newtonsoft
```

## 🏎️💨 Getting Started

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
var pact = JsonOptions.Default(JsonPactCase.Snake).IntoJsonPact();
var pact = JsonPacts.Default(JsonPactCase.Snake).IntoJsonPact();

var json = pact.Serialize(new JsonDTO("required", null));
// = { "required_value": "required", "optional_value": "default" }

var obj = pact.Deserialize<JsonDTO>(json);
// = JsonDTO("required", null, "default")

var err = pact.Deserialize<JsonDTO>("{ }");
// = JsonPactDecodeException
```

## ⚙️ Settings 

> Setup`JsonPact` settings along with your own `Newtonsoft` settings.

```c#
var settings = new NewtonSoft.JsonSerializerSettings { 
 // ...
}.AddJsonPact(JsonPactCase.Camel);

var pact = settings.IntoJsonPact(); 
```

## 🎮 Overriding casing

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
