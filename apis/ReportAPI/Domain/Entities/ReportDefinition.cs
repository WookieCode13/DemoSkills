namespace ReportAPI.Domain.Entities;

public class ReportDefinition
{
    public Guid Id { get; set; }
    public string ReportCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}
