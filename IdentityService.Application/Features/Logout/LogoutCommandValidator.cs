namespace IdentityService.Application.Features.Logout;

public static class LogoutCommandValidator
{
    public static void Validate(LogoutCommand command)
    {
        if (command.Context.TenantId == Guid.Empty) throw new ArgumentException("TenantId is required.");
        if (command.SessionId == Guid.Empty) throw new ArgumentException("SessionId is required.");
    }
}
