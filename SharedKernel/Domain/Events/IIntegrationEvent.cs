namespace SharedKernel.Domain.Events;

public interface IIntegrationEvent : IDomainEvent
{
    string EventType { get; }
}
