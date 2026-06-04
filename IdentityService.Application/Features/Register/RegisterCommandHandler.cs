using IdentityService.Application.Common.Abstractions;
using IdentityService.Domain.Aggregates.User;
using IdentityService.Domain.Repositories;
using IdentityService.Domain.Services;
using IdentityService.Domain.ValueObjects;
using SharedKernel.Errors;
using SharedKernel.Results;

namespace IdentityService.Application.Features.Register;

public sealed class RegisterCommandHandler(IUserRepository users, IPasswordHasher passwordHasher, IIdentityUnitOfWork unitOfWork)
{
    public async Task<Result<RegisterResponse>> HandleAsync(RegisterCommand command, CancellationToken cancellationToken)
    {
        var email = Email.Create(command.Email);
        if (await users.ExistsByEmailAsync(command.Context.TenantId, email, cancellationToken))
        {
            return Result<RegisterResponse>.Failure(GeneralErrors.Conflict);
        }

        var user = User.Register(command.Context.TenantId, email, PasswordHash.FromHash(passwordHasher.Hash(command.Password)), command.Context.CorrelationId);
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        await users.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(user.DomainEvents, cancellationToken);
        await unitOfWork.CommitTransactionAsync(cancellationToken);
        user.ClearDomainEvents();
        return Result<RegisterResponse>.Success(new RegisterResponse(user.Id, user.Email.Value, user.Status.ToString()));
    }
}
