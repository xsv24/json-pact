
using System.Text.RegularExpressions;
using JsonPact;

public static class JsonExtensions {
    public static string Capitalize(this string value) => value.Length switch {
        > 0 => $"{value.Substring(0, 1).ToUpper()}{value.Substring(1)}",
        _ => value
    };

    public static string IntoKebabCase(this string value) {
        value = Regex.Replace(value.Replace("_", "-"), "[A-Z]", "-$0")
            .ToLower();

        return value.StartsWith("-") switch {
            true => value.Substring(1),
            false => value
        };
    }

    public static string IntoSnakeCase(this string value) {
        value = Regex.Replace(value.Replace("-", "_"), "[A-Z]", "_$0")
            .ToLower();

        return value.StartsWith("_") switch {
            true => value.Substring(1),
            false => value
        };
    }

    public static string IntoCamelCase(this string value) {
        var val = Regex.Replace(
            value.Replace("-", "_"),
            "_[a-z]",
            match => $"{match}".TrimStart('_').Capitalize()
        );

        return val.Substring(0, 1).ToLower() + val.Substring(1);
    }

    public static string IntoPascalCase(this string value) {
        var val = Regex.Replace(
            value.Replace("-", "_"),
            "_[a-z]",
            match => $"{match}".TrimStart('_').Capitalize()
        );

        return val.Substring(0, 1).ToUpper() + val.Substring(1);
    }

    public static string IntoCasedStr(this string value, JsonPactCase casing) => casing switch {
        JsonPactCase.Snake => IntoSnakeCase(value),
        JsonPactCase.Kebab => IntoKebabCase(value),
        JsonPactCase.Camel => IntoCamelCase(value),
        JsonPactCase.Pascal => IntoPascalCase(value),
        _ => throw new ArgumentException("Unsupported casing")
    };

    private static string JsonPropStr<T>(
        string key,
        T value,
        JsonPactCase casing,
        bool ignoreEmpty = true
    ) {
        var name = key.IntoCasedStr(casing);

        if (ignoreEmpty && value is null) {
            return "";
        }

        return @$"""{name}"":""{value}""";
    }

    public static string IntoJson<T>(this T obj, JsonPactCase casing, bool ignoreEmpty = true) where T : notnull {
        var props = obj.GetType().GetProperties()
            .Select(prop => JsonPropStr(prop.Name, prop.GetValue(obj), casing, ignoreEmpty))
            .Where(json => !string.IsNullOrWhiteSpace(json));

        return $"{{{string.Join(",", props)}}}";
    }
}
