namespace JsonPact.Tests {
    // Different types of schemas some with & without required, optional and nullable fields for testing.

    [NewtonSoft.JsonPact]
    [System.JsonPact]
    public record JsonRecord(
        string RequiredValue,
        string? Nullable,
        string Defaulted = "default",
        string? NullableDefault = null
    );

    [NewtonSoft.JsonPact]
    [System.JsonPact]
    public record JsonRecordDTO {
        public string RequiredValue { get; init; } = default!;
        public string? Nullable { get; init; }
        public string Defaulted { get; init; } = "default";
        public string? NullableDefault { get; init; } = null;
    }

    [NewtonSoft.JsonPact]
    [System.JsonPact]
    public class JsonClass {
        public string RequiredValue { get; set; } = default!;
        public string? Nullable { get; set; }
        public string Defaulted { get; set; } = "default";
        public string? NullableDefault { get; set; } = null;
    }

    [System.JsonPact]
    [NewtonSoft.JsonPact]
    public record OptionalAndDefaultedDTO {
        public string? Nullable { get; set; }
        public string Defaulted { get; set; } = "default";
        public string? NullableDefault { get; set; } = default;
    }

    [System.JsonPact]
    [NewtonSoft.JsonPact]
    public record OptionalAndDefaultedRecord(
        string? Nullable,
        string Defaulted = "default",
        string? NullableDefault = null
    );

    [System.JsonPact]
    [NewtonSoft.JsonPact]
    public record NullableRecord(string? Nullable);

    [System.JsonPact]
    [NewtonSoft.JsonPact]
    public record NullableDTO {
        public string? Nullable { get; init; }
    }

    [System.JsonPact]
    [NewtonSoft.JsonPact]
    public record DefaultedRecord(string Defaulted = "default");

    [System.JsonPact]
    [NewtonSoft.JsonPact]
    public record DefaultedDTO {
        public string Defaulted { get; set; } = "default";
    }

    [System.JsonPact(JsonPactCase.Camel)]
    [NewtonSoft.JsonPact(JsonPactCase.Camel)]
    public record CamelCase<T>(T RequiredValue) where T : class;

    [System.JsonPact(JsonPactCase.Snake)]
    [NewtonSoft.JsonPact(JsonPactCase.Snake)]
    public record SnakeCase<T>(T RequiredValue) where T : class;

    [System.JsonPact(JsonPactCase.Kebab)]
    [NewtonSoft.JsonPact(JsonPactCase.Kebab)]
    public record KebabCase<T>(T RequiredValue) where T : class;
}
