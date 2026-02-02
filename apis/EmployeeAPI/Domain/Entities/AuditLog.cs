namespace EmployeeAPI.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public DateTimeOffset OccurredUtc { get; set; }
    public string PerformedBy { get; set; } = "system";
    public string? ChangedFields { get; set; }
    public string? Note { get; set; }
    public string? CorrelationId { get; set; }
}
