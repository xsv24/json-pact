using JsonPact.NewtonSoft;

// Different types of schemas some with & without required, optional and nullable fields for testing.

[JsonPact]
public record JsonRecord(
    string RequiredValue,
    string? Nullable,
    string Defaulted = "default",
    string? NullableDefault = null
);

[JsonPact]
public record JsonRecordDTO {
    public string RequiredValue { get; init; } = default!;
    public string? Nullable { get; init; }
    public string Defaulted { get; init; } = "default";
    public string? NullableDefault { get; init; } = null;
}

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
    public string? NullableDefault { get; set; } = null;
}

[JsonPact]
public record OptionalAndDefaultedRecord(
    string? Nullable,
    string Defaulted = "default",
    string? NullableDefault = null
);

[JsonPact]
public record NullableRecord(string? Nullable);

[JsonPact]
public record NullableDTO {
    public string? Nullable { get; init; }
}

[JsonPact]
public record DefaultedRecord(string Defaulted = "default");

[JsonPact]
public record DefaultedDTO {
    public string Defaulted { get; set; } = "default";
}

// TODO: Nested structure with different casing.
