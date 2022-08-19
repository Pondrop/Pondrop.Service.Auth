using Pondrop.Service.Auth.Domain.Models;

public record UserViewRecord(
        Guid Id,
        string Email,
        string NormalizedEmail,
        DateTime? LastLogin,
        DateTime? LastLogout,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc)
    : UserRecord(Id, Email, NormalizedEmail, LastLogin, LastLogout, CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc)
{
    public UserViewRecord() : this(
        Guid.Empty,
        string.Empty,
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