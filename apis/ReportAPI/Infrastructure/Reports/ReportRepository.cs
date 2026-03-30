using Microsoft.EntityFrameworkCore;
using ReportAPI.Application.Reports;
using ReportAPI.Domain.Entities;
using ReportAPI.Infrastructure.Data;

namespace ReportAPI.Infrastructure.Reports;

public class ReportRepository : IReportRepository
{
    private readonly AppDbContext _db;

    public ReportRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<List<ReportDefinition>> GetReportsAsync(CancellationToken ct)
    {
        return _db.Reports
            .AsNoTracking()
            .OrderBy(r => r.ReportCode)
            .ToListAsync(ct);
    }

    public Task<List<ReportDataRecord>> GetReportDataAsync(CancellationToken ct)
    {
        return _db.ReportDataRecords
            .AsNoTracking()
            .OrderBy(r => r.ReportCode)
            .ThenByDescending(r => r.CreatedUtc)
            .ToListAsync(ct);
    }
}
