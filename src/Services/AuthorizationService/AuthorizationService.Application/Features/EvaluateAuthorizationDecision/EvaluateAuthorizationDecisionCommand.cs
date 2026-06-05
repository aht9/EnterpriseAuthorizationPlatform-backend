using AuthorizationService.Application.Common.Abstractions;
using MediatR;
using SharedKernel.Results;

namespace AuthorizationService.Application.Features.EvaluateAuthorizationDecision;

public sealed record EvaluateAuthorizationDecisionCommand(
    Guid SubjectId,
    string Action,
    string ResourceType,
    string ResourceId,
    Guid? OwnerId,
    IReadOnlyDictionary<string, string> ResourceAttributes,
    IReadOnlyDictionary<string, string> EnvironmentAttributes,
    IReadOnlyDictionary<string, string> UsageAttributes,
    RequestContext Context) : IRequest<Result<EvaluateAuthorizationDecisionResponse>>, IAuthorizationDecisionRequest;
