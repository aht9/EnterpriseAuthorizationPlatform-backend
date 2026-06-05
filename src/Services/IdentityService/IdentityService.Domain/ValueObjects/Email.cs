using System.Text.RegularExpressions;
using SharedKernel.Domain.Primitives;

namespace IdentityService.Domain.ValueObjects;

public sealed partial class Email : ValueObject
{
    private Email(string value) => Value = value;

    public string Value { get; }

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Email is required.", nameof(value));
        var normalized = value.Trim().ToLowerInvariant();
        if (!EmailRegex().IsMatch(normalized)) throw new ArgumentException("Email format is invalid.", nameof(value));
        return new Email(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    [GeneratedRegex("^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$", RegexOptions.Compiled)]
    private static partial Regex EmailRegex();
}
