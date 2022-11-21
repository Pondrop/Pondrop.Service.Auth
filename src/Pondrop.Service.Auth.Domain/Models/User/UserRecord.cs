using Pondrop.Service.Models;

namespace Pondrop.Service.Auth.Domain.Models;

public record UserRecord(
        Guid Id,
        string Email,
        string NormalizedEmail,
        DateTime? LastLoginDateTime,
        DateTime? LastLogoutDateTime,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc)
    : AuditRecord(CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc)
{
    public UserRecord() : this(
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