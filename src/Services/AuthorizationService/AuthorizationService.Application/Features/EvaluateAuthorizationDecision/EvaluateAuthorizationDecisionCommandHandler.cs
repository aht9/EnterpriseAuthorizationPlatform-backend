using AuthorizationService.Application.Common.Abstractions;
using AuthorizationService.Domain.Events;
using AuthorizationService.Domain.Models;
using AuthorizationService.Domain.Services;
using AuthorizationService.Domain.ValueObjects;
using MediatR;
using SharedKernel.Results;

namespace AuthorizationService.Application.Features.EvaluateAuthorizationDecision;

public sealed class EvaluateAuthorizationDecisionCommandHandler(IAuthorizationDecisionEngine decisionEngine, IEffectivePermissionResolver permissions, IAuthorizationAuditSink auditSink, IAuthorizationUnitOfWork unitOfWork, IClock clock) : IRequestHandler<EvaluateAuthorizationDecisionCommand, Result<EvaluateAuthorizationDecisionResponse>>
{
    public async Task<Result<EvaluateAuthorizationDecisionResponse>> Handle(EvaluateAuthorizationDecisionCommand command, CancellationToken cancellationToken)
    {
        var input = EmptyInput(command, clock.UtcNow);
        var result = AuthorizationDecisionResult.Deny(AuthorizationDecisionReason.EvaluationError, "Authorization evaluation failed.", command.Context.CorrelationId, evaluatedAtUtc: clock.UtcNow);
        try
        {
            if (command.Context.TenantId == Guid.Empty)
            {
                result = AuthorizationDecisionResult.Deny(AuthorizationDecisionReason.MissingTenant, "Tenant context is missing.", command.Context.CorrelationId, evaluatedAtUtc: clock.UtcNow);
            }
            else
            {
                var effectivePermissions = await permissions.ResolveAsync(command.Context.TenantId, command.SubjectId, cancellationToken);
                input = new AuthorizationDecisionInput(command.Context.TenantId, command.SubjectId, AuthorizationAction.Create(command.Action), ResourceDescriptor.Create(command.ResourceType, command.ResourceId, command.OwnerId, command.ResourceAttributes), AuthorizationContext.Create(command.Context.IpAddress, command.Context.UserAgent, command.EnvironmentAttributes, command.UsageAttributes, command.Context.CorrelationId, clock.UtcNow), Array.Empty<string>(), effectivePermissions);
                result = input.IsComplete ? await decisionEngine.EvaluateAsync(input, cancellationToken) : AuthorizationDecisionResult.Deny(AuthorizationDecisionReason.InvalidInput, "Authorization input is incomplete.", command.Context.CorrelationId, evaluatedAtUtc: clock.UtcNow);
                if (result.CorrelationId == Guid.Empty) result = AuthorizationDecisionResult.Deny(AuthorizationDecisionReason.EvaluationError, "Authorization engine returned invalid response.", command.Context.CorrelationId, evaluatedAtUtc: clock.UtcNow);
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            result = AuthorizationDecisionResult.Deny(AuthorizationDecisionReason.OpaTimeout, "OPA evaluation timed out.", command.Context.CorrelationId, evaluatedAtUtc: clock.UtcNow);
        }
        catch
        {
            result = AuthorizationDecisionResult.Deny(AuthorizationDecisionReason.EvaluationError, "Authorization evaluation failed.", command.Context.CorrelationId, evaluatedAtUtc: clock.UtcNow);
        }

        if (command.Context.TenantId != Guid.Empty)
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken);
            await unitOfWork.SaveChangesAsync([new AuthorizationEvaluatedDomainEvent(Guid.NewGuid(), command.SubjectId, command.Action, command.ResourceType, command.ResourceId, result.IsAllowed, result.ReasonCode, command.Context.TenantId, command.Context.CorrelationId)], cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        await auditSink.RecordDecisionAsync(input, result, cancellationToken);
        return Result<EvaluateAuthorizationDecisionResponse>.Success(new EvaluateAuthorizationDecisionResponse(result.IsAllowed, result.Decision.ToString(), result.ReasonCode, result.ReasonMessage, result.PolicyId, result.PolicyVersion, result.EvaluatedAtUtc, result.EvaluationDurationMs, result.CorrelationId));
    }

    private static AuthorizationDecisionInput EmptyInput(EvaluateAuthorizationDecisionCommand command, DateTimeOffset now) =>
        new(command.Context.TenantId, command.SubjectId == Guid.Empty ? Guid.NewGuid() : command.SubjectId, AuthorizationAction.Create(string.IsNullOrWhiteSpace(command.Action) ? "unknown" : command.Action), ResourceDescriptor.Create(string.IsNullOrWhiteSpace(command.ResourceType) ? "unknown" : command.ResourceType, string.IsNullOrWhiteSpace(command.ResourceId) ? "unknown" : command.ResourceId, command.OwnerId, command.ResourceAttributes), AuthorizationContext.Create(command.Context.IpAddress, command.Context.UserAgent, command.EnvironmentAttributes, command.UsageAttributes, command.Context.CorrelationId == Guid.Empty ? Guid.NewGuid() : command.Context.CorrelationId, now), Array.Empty<string>(), Array.Empty<string>());
}
