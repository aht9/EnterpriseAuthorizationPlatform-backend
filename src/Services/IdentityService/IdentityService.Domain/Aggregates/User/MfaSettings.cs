namespace IdentityService.Domain.Aggregates.User;

public sealed record MfaSettings(bool IsEnabled, string? Secret)
{
    public static readonly MfaSettings Disabled = new(false, null);

    public static MfaSettings Enable(string secret)
    {
        if (string.IsNullOrWhiteSpace(secret)) throw new ArgumentException("MFA secret is required.", nameof(secret));
        return new MfaSettings(true, secret.Trim());
    }
}
