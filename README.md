![tests status](https://github.com/xsv24/json-pact/actions/workflows/dotnet.yml/badge.svg?event=push)

# 🤝 json-pact

json wrapper library that enforces casing & validates required properties by checking if the property
uses the nullable `?` property marker as specified in [nullable value types](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/nullable-value-types).

## Newtonsoft

```c#
[JsonPact]
public record JsonDTO(
    string RequiredValue,
    string? NullableValue,
    string OptionalValue = "default"
);
```

> Here we define the schema with a required value with rest of the values
> defined as default-able without need for additional property attributes.

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

### Overriding casing

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
