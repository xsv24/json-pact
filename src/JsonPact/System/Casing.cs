using System.Text.Json;
using JsonPact;

namespace JsonPact.System;

public class NamingStrategy : JsonNamingPolicy {
    private readonly JsonPactCase _casing;

    public NamingStrategy(JsonPactCase casing) {
        _casing = casing;
    }

    public override string ConvertName(string name) => name.IntoCasedStr(_casing);
}
