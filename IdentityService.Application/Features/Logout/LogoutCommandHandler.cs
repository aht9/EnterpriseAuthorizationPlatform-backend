using IdentityService.Application.Common.Abstractions;
using IdentityService.Domain.Repositories;

namespace IdentityService.Application.Features.Logout;

public sealed class LogoutCommandHandler(ISessionRepository sessions, IIdentityUnitOfWork unitOfWork, IAuditSink auditSink)
{
    public async Task HandleAsync(LogoutCommand command, CancellationToken cancellationToken)
    {
        LogoutCommandValidator.Validate(command);
        var session = await sessions.GetByIdAsync(command.Context.TenantId, command.SessionId, cancellationToken);
        if (session is null) return;
        session.Revoke(command.Context.CorrelationId);
        await sessions.UpdateAsync(session, cancellationToken);
        await unitOfWork.SaveChangesAsync(session.DomainEvents, cancellationToken);
        await auditSink.RecordAsync("IdentityService.logout", session.TenantId, command.Context.CorrelationId, session.UserId, true, null, cancellationToken);
        session.ClearDomainEvents();
    }
}
