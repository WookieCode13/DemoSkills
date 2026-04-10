using Microsoft.EntityFrameworkCore;
using PayAPI.Application.Taxes;
using PayAPI.Domain.Entities;
using PayAPI.Infrastructure.Data;

namespace PayAPI.Infrastructure.Taxes;

public class TaxRepository : ITaxRepository
{
    private readonly AppDbContext _db;

    public TaxRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<List<TaxDefinition>> GetTaxesAsync(CancellationToken ct)
    {
        return _db.Taxes
            .AsNoTracking()
            .OrderBy(t => t.Year)
            .ThenBy(t => t.TaxCode)
            .ToListAsync(ct);
    }
}
