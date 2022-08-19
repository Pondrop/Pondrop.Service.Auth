using Newtonsoft.Json.Linq;

namespace Pondrop.Service.Auth.Domain.Events;

public interface IEventPayload
{
    DateTime CreatedUtc { get; }
}