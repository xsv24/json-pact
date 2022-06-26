using JsonPact.NewtonSoft;

[JsonPact]
public class JsonClass {
    public string RequiredValue { get; set; } = default!;
    public string? Nullable { get; set; }
    public string Defaulted { get; set; } = "default";
    public string? NullableDefault { get; set; } = null;
}

[JsonPact]
public record JsonRecordDTO {
    public string RequiredValue { get; init; } = default!;
    public string? Nullable { get; init; }
    public string Defaulted { get; init; } = "default";
    public string? NullableDefault { get; init; } = null;
}

[JsonPact]
public record JsonRecord(
    string RequiredValue,
    string? Nullable,
    string Defaulted = "default",
    string? NullableDefault = null
);
