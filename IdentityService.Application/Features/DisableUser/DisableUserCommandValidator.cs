namespace IdentityService.Application.Features.DisableUser;

public static class DisableUserCommandValidator
{
    public static void Validate(DisableUserCommand command)
    {
        if (command.Context.TenantId == Guid.Empty) throw new ArgumentException("TenantId is required.");
        if (command.UserId == Guid.Empty) throw new ArgumentException("UserId is required.");
    }
}
