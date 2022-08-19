namespace Pondrop.Service.Auth.Domain.Events.User;

public record UserLogin(DateTime LastLoginDateTime) : EventPayload;