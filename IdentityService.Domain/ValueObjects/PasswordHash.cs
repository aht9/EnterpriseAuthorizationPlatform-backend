using SharedKernel.Domain.Primitives;

namespace IdentityService.Domain.ValueObjects;

public sealed class PasswordHash : ValueObject
{
    private PasswordHash(string value) => Value = value;

    public string Value { get; }

    public static PasswordHash FromHash(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Password hash is required.", nameof(value));
        return new PasswordHash(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
