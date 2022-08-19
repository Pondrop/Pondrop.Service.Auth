using Pondrop.Service.Auth.Domain.Models;

public record UserViewRecord(
        Guid Id,
        string Email,
        DateTime? LastLogin,
        DateTime? LastLogout,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc)
    : UserRecord(Id, Email, LastLogin, LastLogout, CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc)
{
    public UserViewRecord() : this(
        Guid.Empty,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue,
        string.Empty,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue)
    {
    }
}