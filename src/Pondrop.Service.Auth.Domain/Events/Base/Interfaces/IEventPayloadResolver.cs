namespace Pondrop.Service.Auth.Domain.Events;

public interface IEventTypePayloadResolver
{
    Type? GetEventPayloadType(string streamType, string typeName);
}