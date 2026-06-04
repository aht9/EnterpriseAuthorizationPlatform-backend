namespace IdentityService.Application.Features.EnableMfa;

public static class EnableMfaCommandValidator
{
    public static void Validate(EnableMfaCommand command)
    {
        if (command.Context.TenantId == Guid.Empty) throw new ArgumentException("TenantId is required.");
        if (command.UserId == Guid.Empty) throw new ArgumentException("UserId is required.");
        if (string.IsNullOrWhiteSpace(command.Secret)) throw new ArgumentException("MFA secret is required.");
    }
}
