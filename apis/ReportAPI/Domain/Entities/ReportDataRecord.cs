using System.Text.Json;

namespace ReportAPI.Domain.Entities;

public class ReportDataRecord
{
    public Guid Id { get; set; }
    public string ReportCode { get; set; } = string.Empty;
    public JsonDocument ReportData { get; set; } = JsonDocument.Parse("{}");
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}
