using Newtonsoft.Json;
using Pondrop.Service.Auth.Domain.Events.User;
using Pondrop.Service.Events;
using Pondrop.Service.Models;

namespace Pondrop.Service.Auth.Domain.Models;

public record UserEntity : EventEntity
{
    public UserEntity()
    {
        Id = Guid.Empty;
        Email = string.Empty;
    }

    public UserEntity(IEnumerable<IEvent> events) : this()
    {
        foreach (var e in events)
        {
            Apply(e);
        }
    }

    public UserEntity(string email) : this()
    {
        var create = new CreateUser(Guid.NewGuid(), email);
        Apply(create, string.Empty);
    }

    [JsonProperty(PropertyName = "email")]
    public string Email { get; private set; }

    [JsonProperty(PropertyName = "normalizedEmail")]
    public string NormalizedEmail { get; private set; }

    [JsonProperty(PropertyName = "lastLogin")]
    public DateTime? LastLogin { get; private set; }

    [JsonProperty(PropertyName = "lastLogout")]
    public DateTime? LastLogout { get; private set; }

    protected sealed override void Apply(IEvent eventToApply)
    {
        switch (eventToApply.GetEventPayload())
        {
            case CreateUser create:
                When(create, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            case UserLogin userLogin:
                When(userLogin, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            case UserLogout userLogout:
                When(userLogout, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
         
            default:
                throw new InvalidOperationException($"Unrecognised event type for '{StreamType}', got '{eventToApply.GetType().Name}'");
        }

        Events.Add(eventToApply);

        AtSequence = eventToApply.SequenceNumber;
    }

    public sealed override void Apply(IEventPayload eventPayloadToApply, string createdBy)
    {
        if (eventPayloadToApply is CreateUser create)
        {
            Apply(new Event(GetStreamId<UserEntity>(create.Id), StreamType, 0, create, createdBy));
        }
        else
        {
            Apply(new Event(StreamId, StreamType, AtSequence + 1, eventPayloadToApply, createdBy));
        }
    }

    private void When(CreateUser create, string createdBy, DateTime createdUtc)
    {
        Id = create.Id;
        Email = create.Email;
        NormalizedEmail = create.Email.ToUpper();
        LastLogin = DateTime.UtcNow;
        CreatedBy = UpdatedBy = createdBy;
        CreatedUtc = UpdatedUtc = createdUtc;
    }

    private void When(UserLogin userLogin, string createdBy, DateTime createdUtc)
    {
        LastLogin = userLogin.LastLoginDateTime;
        UpdatedBy = createdBy;
        UpdatedUtc = createdUtc;
    }

    private void When(UserLogout userLogout, string createdBy, DateTime createdUtc)
    {
        LastLogout = userLogout.LastLogoutDateTime;
        UpdatedBy = createdBy;
        UpdatedUtc = createdUtc;
    }


}