using IdentityService.Application.Common.Abstractions;
using IdentityService.Domain.Aggregates.User;
using IdentityService.Domain.Repositories;
using IdentityService.Domain.Services;
using IdentityService.Domain.ValueObjects;

namespace IdentityService.Application.Features.Register;

public sealed class RegisterCommandHandler(IUserRepository users, IPasswordHasher passwordHasher, IIdentityUnitOfWork unitOfWork, IAuditSink auditSink)
{
    public async Task<RegisterResponse> HandleAsync(RegisterCommand command, CancellationToken cancellationToken)
    {
        RegisterCommandValidator.Validate(command);
        var email = Email.Create(command.Email);
        if (await users.ExistsByEmailAsync(command.Context.TenantId, email, cancellationToken))
            throw new InvalidOperationException("A user with this email already exists in the tenant.");

        var user = User.Register(command.Context.TenantId, email, PasswordHash.FromHash(passwordHasher.Hash(command.Password)), command.Context.CorrelationId);
        await users.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(user.DomainEvents, cancellationToken);
        await auditSink.RecordAsync("IdentityService.user_registered", user.TenantId, command.Context.CorrelationId, user.Id, true, null, cancellationToken);
        user.ClearDomainEvents();
        return new RegisterResponse(user.Id, user.Email.Value, user.Status.ToString());
    }
}
