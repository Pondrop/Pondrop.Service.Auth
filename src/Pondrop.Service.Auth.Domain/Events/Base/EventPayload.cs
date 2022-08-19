using Newtonsoft.Json;

namespace Pondrop.Service.Auth.Domain.Events;

public record EventPayload : IEventPayload
{
    public DateTime CreatedUtc { get; } = DateTime.UtcNow;
}