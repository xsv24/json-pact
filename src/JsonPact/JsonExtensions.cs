using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace JsonPact;

public static class JsonExtensions {
    internal static string Capitalize(this string value) => value.Length switch {
        > 0 => $"{value[..1].ToUpper()}{value[1..]}",
        _ => value
    };

    internal static string IntoKebabCase(this string value) {
        value = Regex.Replace(
           input: value.Replace("_", "-"),
           pattern: "[A-Z]",
            replacement: "-$0"
        ).ToLower();

        return value.StartsWith("-") switch {
            true => value[1..],
            false => value
        };
    }

    internal static string IntoSnakeCase(this string value) {
        value = Regex.Replace(
            input: value.Replace("-", "_"),
            pattern: "[A-Z]",
            replacement: "_$0"
        ).ToLower();

        return value.StartsWith("_") switch {
            true => value[1..],
            false => value
        };
    }

    internal static string IntoCamelCase(this string value) {
        var val = Regex.Replace(
            input: value.Replace("-", "_"),
            pattern: "_[a-z]",
            evaluator: match => $"{match}".TrimStart('_').Capitalize()
        );

        return string.Concat(val[..1].ToLower(), val.AsSpan(1));
    }

    internal static string IntoPascalCase(this string value) {
        var val = Regex.Replace(
            value.Replace("-", "_"),
            "_[a-z]",
            match => $"{match}".TrimStart('_').Capitalize()
        );

        return string.Concat(val[..1].ToUpper(), val.AsSpan(1));
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
