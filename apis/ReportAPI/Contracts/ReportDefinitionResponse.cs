namespace ReportAPI.Contracts;

public sealed record ReportDefinitionResponse(
    Guid Id,
    string ReportCode,
    string? Description,
    DateTime CreatedUtc,
    DateTime UpdatedUtc
);
