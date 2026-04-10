using PayAPI.Domain.Entities;

namespace PayAPI.Application.Taxes;

public interface ITaxRepository
{
    Task<List<TaxDefinition>> GetTaxesAsync(CancellationToken ct);
}
