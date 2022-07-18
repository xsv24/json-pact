using JsonPact;
using JsonPact.NewtonSoft;

namespace JsonPact.Tests; 
// Different types of schemas some with & without required, optional and nullable fields for testing.

#if NET5_0_OR_GREATER
[JsonPact]
public record JsonRecord(
    string RequiredValue,
    string? Nullable,
    string Defaulted = "default",
    string? NullableDefault = null
);
#else
[JsonPact]
public record JsonRecord(
    string RequiredValue,
    string? Nullable,
    string Defaulted = "default",
    string? NullableDefault = null
) {
    public string RequiredValue { get; } = RequiredValue;
    public string? Nullable { get; } = Nullable;
    public string Defaulted { get; } = Defaulted;
    public string? NullableDefault { get; } = NullableDefault;
}
#endif

#if NET5_0_OR_GREATER
[JsonPact]
public record JsonRecordDTO {
    public string RequiredValue { get; init; } = default!;
    public string? Nullable { get; init; }
    public string Defaulted { get; init; } = "default";
    public string? NullableDefault { get; init; } = null;
}
#else
[JsonPact]
public record JsonRecordDTO {
    public string RequiredValue { get; set; } = default!;
    public string? Nullable { get; set; }
    public string Defaulted { get; set; } = "default";
    public string? NullableDefault { get; set; } = null;
}
#endif

[JsonPact]
public class JsonClass {
    public string RequiredValue { get; set; } = default!;
    public string? Nullable { get; set; }
    public string Defaulted { get; set; } = "default";
    public string? NullableDefault { get; set; } = null;
}

[JsonPact]
public record OptionalAndDefaultedDTO {
    public string? Nullable { get; set; }
    public string Defaulted { get; set; } = "default";
    public string? NullableDefault { get; set; } = default;
}

#if NET5_0_OR_GREATER
[JsonPact]
public record OptionalAndDefaultedRecord(
    string? Nullable,
    string Defaulted = "default",
    string? NullableDefault = null
);
#else

[JsonPact]
public record OptionalAndDefaultedRecord(
    string? Nullable,
    string Defaulted = "default",
    string? NullableDefault = null
) {
    public string? Nullable { get; } = Nullable;
    public string Defaulted { get; } = Defaulted;
    public string? NullableDefault { get; } = NullableDefault;
}
#endif

#if NET5_0_OR_GREATER
[JsonPact]
public record NullableRecord(string? Nullable);
#else
[JsonPact]
public record NullableRecord(string? Nullable) {
    public string? Nullable { get; } = Nullable;
}
#endif


#if NET5_0_OR_GREATER
[JsonPact]
public record NullableDTO {
    public string? Nullable { get; init; }
}
#else
[JsonPact]
public record NullableDTO {
    public string? Nullable { get; set; }
}
#endif

#if NET5_0_OR_GREATER
[JsonPact]
public record DefaultedRecord(string Defaulted = "default");
#else
[JsonPact]
public record DefaultedRecord(string Defaulted = "default") {
    public string Defaulted { get; } = Defaulted;
}
#endif

[JsonPact]
public record DefaultedDTO {
    public string Defaulted { get; set; } = "default";
}

#if NET5_0_OR_GREATER
[JsonPact(JsonPactCase.Camel)]
public record CamelCase<T>(T RequiredValue) where T : class;
#else
[JsonPact(JsonPactCase.Camel)]
public record CamelCase<T>(T RequiredValue) where T : class {
    public T RequiredValue { get; } = RequiredValue;
}
#endif

#if NET5_0_OR_GREATER
[JsonPact(JsonPactCase.Snake)]
public record SnakeCase<T>(T RequiredValue) where T : class;
#else
[JsonPact(JsonPactCase.Snake)]
public record SnakeCase<T>(T RequiredValue) where T : class {
    public T RequiredValue { get; } = RequiredValue;
}
#endif

#if NET5_0_OR_GREATER
[JsonPact(JsonPactCase.Kebab)]
public record KebabCase<T>(T RequiredValue) where T : class;
#else
[JsonPact(JsonPactCase.Kebab)]
public record KebabCase<T>(T RequiredValue) where T : class {
    public T RequiredValue { get; } = RequiredValue;
}
#endif
