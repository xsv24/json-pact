
using System.Reflection;

namespace JsonPact;

internal static class Extensions {
    public static void ForEach<T>(this IEnumerable<T> collection, Action<T> fn) {
        foreach (var item in collection) {
            fn(item);
        }
    }

    /// <summary>
    /// Checks to see if the '?' nullable operator has been used on a property within an object schema.
    /// </summary>
    /// <param name="type">Type of a property obtained through reflection.</param>
    /// <returns>true if the underlying type is nullable otherwise false.</returns>
    public static bool IsNullable(this Type? type) =>
        type != null && Nullable.GetUnderlyingType(type) != null;

    /// <summary>
    /// Checks to see if the '?' nullable operator has been used on a property within an object schema.
    /// </summary>
    /// <param name="attributes">These are the extensions added on a property which can be obtained through reflection.</param>
    /// <returns>true if attributes contains a 'NullableAttribute' otherwise false.</returns>
    public static bool IsNullable(this IEnumerable<CustomAttributeData>? attributes, bool recurse = false) {
        if (attributes is null) return false;

        return attributes.Any(attr => attr.AttributeType.FullName switch {
            "System.Runtime.CompilerServices.NullableAttribute" => true,
            "System.Runtime.CompilerServices.CompilerGeneratedAttribute" when recurse => IsNullable(attr.AttributeType.BaseType?.CustomAttributes),
            _ => false
        });
    }

    public static IEnumerable<ParameterInfo> GetConstructorParams(this Type type) =>
        type.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
            .FirstOrDefault() // TODO: Make this better could throw an error or have a constructor attribute.
            ?.GetParameters()
        ?? Array.Empty<ParameterInfo>();

    public static bool IsNullableContext(this PropertyInfo info) {
        var context = info.DeclaringType?.CustomAttributes.FirstOrDefault(attr => attr.AttributeType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute");
        // https://github.com/dotnet/roslyn/blob/6ab12a2b4d94b60822c8bcecad82307251ffdd78/docs/features/nullable-metadata.md?plain=1#L69
        return (byte?)context?.ConstructorArguments[0].Value == 2;
    }

    public static bool IsNullableContext(this ParameterInfo info) {
        var context = info.Member.DeclaringType?.CustomAttributes.FirstOrDefault(attr => attr.AttributeType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute");
        // https://github.com/dotnet/roslyn/blob/6ab12a2b4d94b60822c8bcecad82307251ffdd78/docs/features/nullable-metadata.md?plain=1#L69
        return (byte?)context?.ConstructorArguments[0].Value == 2;
    }

    public static bool IsRequired(this PropertyInfo info, Type prop, object? defaulted) {
        var defaultedValue = info.GetValue(defaulted);

        return defaultedValue switch {
            object { } => false,
            null when
                prop.IsNullable() ||
                info.CustomAttributes.IsNullable() ||
                (info.SetMethod?.CustomAttributes).IsNullable() ||
                info.IsNullableContext()
            => false,
            null => true
        };
    }

    public static bool IsRequired(this ParameterInfo info, Type prop) => info switch {
        ParameterInfo { HasDefaultValue: false } when
            prop.IsNullable() ||
            info.CustomAttributes.IsNullable() ||
            info.IsNullableContext() => false,
        ParameterInfo { HasDefaultValue: false } => true,
        _ => false
    };
}
