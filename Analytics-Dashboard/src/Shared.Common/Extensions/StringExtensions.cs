using System.Text;

namespace Shared.Common.Extensions;

/// <summary>
/// Extension methods for <see cref="string"/> that provide common
/// transformations used across all microservices.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Converts a <c>PascalCase</c> or <c>camelCase</c> string to
    /// <c>snake_case</c>.
    /// </summary>
    /// <param name="input">
    /// The string to convert. Accepts <c>PascalCase</c>, <c>camelCase</c>,
    /// or any mix of upper- and lower-case characters.
    /// Returns <paramref name="input"/> unchanged when it is
    /// <see langword="null"/> or empty.
    /// </param>
    /// <returns>
    /// A new lowercase string where every uppercase letter that follows
    /// a non-underscore character is prefixed with an underscore,
    /// or the original value when it is <see langword="null"/> or empty.
    /// </returns>
    /// <remarks>
    /// Primarily used to map EF Core entity and property names to
    /// PostgreSQL's snake_case column naming convention without relying
    /// on a third-party naming policy package.
    /// Consecutive uppercase letters (e.g. <c>"ID"</c>) each get their
    /// own underscore prefix, producing <c>"i_d"</c> — rename such
    /// properties to <c>"Id"</c> to avoid this.
    /// </remarks>
    /// <example>
    /// <code>
    /// "FirstName".ConvertToSnakeCase()   // → "first_name"
    /// "createdAt".ConvertToSnakeCase()   // → "created_at"
    /// "IsActive".ConvertToSnakeCase()    // → "is_active"
    /// "".ConvertToSnakeCase()            // → ""
    /// </code>
    /// </example>
    public static string ConvertToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (char.IsUpper(c))
            {
                if (i > 0 && builder.Length > 0 && builder[^1] != '_')
                {
                    builder.Append('_');
                }

                builder.Append(char.ToLowerInvariant(c));
            }
            else
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }
}