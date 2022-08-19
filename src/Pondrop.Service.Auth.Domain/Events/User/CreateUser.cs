namespace Pondrop.Service.Auth.Domain.Events.User;

public record CreateUser(Guid Id, string Email) : EventPayload;