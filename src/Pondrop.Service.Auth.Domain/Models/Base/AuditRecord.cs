namespace Pondrop.Service.Auth.Domain.Models;

public record AuditRecord(string CreatedBy, string UpdatedBy, DateTime CreatedUtc, DateTime UpdatedUtc);