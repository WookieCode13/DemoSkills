namespace ReportAPI.Contracts;

public readonly record struct HealthResponse(
    string Status,
    string Service,
    string Timestamp);
