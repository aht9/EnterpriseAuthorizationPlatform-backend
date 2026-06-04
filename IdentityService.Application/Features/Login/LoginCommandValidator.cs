namespace IdentityService.Application.Features.Login;

public static class LoginCommandValidator
{
    public static void Validate(LoginCommand command)
    {
        if (command.Context.TenantId == Guid.Empty) throw new ArgumentException("TenantId is required.");
        if (string.IsNullOrWhiteSpace(command.Email)) throw new ArgumentException("Email is required.");
        if (string.IsNullOrWhiteSpace(command.Password)) throw new ArgumentException("Password is required.");
    }
}
