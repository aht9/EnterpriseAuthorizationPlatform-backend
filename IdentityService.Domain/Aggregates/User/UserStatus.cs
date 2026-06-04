namespace IdentityService.Domain.Aggregates.User;

public enum UserStatus
{
    Active = 1,
    Disabled = 2,
    MfaPending = 3
}
