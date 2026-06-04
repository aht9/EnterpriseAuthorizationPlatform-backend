namespace IdentityService.Application.Features.MfaVerify;

public static class MfaVerifyCommandValidator
{
    public static void Validate(MfaVerifyCommand command)
    {
        if (command.Context.TenantId == Guid.Empty) throw new ArgumentException("TenantId is required.");
        if (command.UserId == Guid.Empty) throw new ArgumentException("UserId is required.");
        if (string.IsNullOrWhiteSpace(command.Code)) throw new ArgumentException("MFA code is required.");
    }
}
