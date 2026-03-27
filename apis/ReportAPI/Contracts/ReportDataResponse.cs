using System.Text.Json;

namespace ReportAPI.Contracts;

public sealed record ReportDataResponse(
    Guid Id,
    string ReportCode,
    JsonElement ReportData,
    DateTime CreatedUtc,
    DateTime UpdatedUtc
);
