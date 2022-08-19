namespace Pondrop.Service.Auth.Domain.Events.User;

public record UserLogout(DateTime LastLogoutDateTime) : EventPayload;