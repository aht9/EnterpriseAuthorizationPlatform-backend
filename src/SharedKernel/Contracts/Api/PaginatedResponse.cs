namespace SharedKernel.Contracts.Api;
public sealed record PaginatedResponse<T>(IReadOnlyCollection<T> Items, int Page, int PageSize, long TotalCount);
