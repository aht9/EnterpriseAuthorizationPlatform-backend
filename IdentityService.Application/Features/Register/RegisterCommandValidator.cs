namespace IdentityService.Application.Features.Register;

public static class RegisterCommandValidator
{
    public static void Validate(RegisterCommand command)
    {
        if (command.Context.TenantId == Guid.Empty) throw new ArgumentException("TenantId is required.");
        if (string.IsNullOrWhiteSpace(command.Email)) throw new ArgumentException("Email is required.");
        if (string.IsNullOrWhiteSpace(command.Password) || command.Password.Length < 12)
            throw new ArgumentException("Password must be at least 12 characters.");
    }
}
