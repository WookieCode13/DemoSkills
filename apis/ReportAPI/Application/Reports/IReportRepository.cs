using ReportAPI.Domain.Entities;

namespace ReportAPI.Application.Reports;

public interface IReportRepository
{
    Task<List<ReportDefinition>> GetReportsAsync(CancellationToken ct);
    Task<List<ReportDataRecord>> GetReportDataAsync(CancellationToken ct);
}
