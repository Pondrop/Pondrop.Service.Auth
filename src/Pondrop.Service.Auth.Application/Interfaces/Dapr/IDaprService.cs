using Pondrop.Service.Auth.Domain.Events;

namespace Pondrop.Service.Auth.Application.Interfaces;

public interface IDaprService
{
    Task<bool> InvokeServiceAsync(string appId, string methodName, object? data = null);

    Task<bool> SendEventsAsync(string eventGridTopic, IEnumerable<IEvent> events);
}