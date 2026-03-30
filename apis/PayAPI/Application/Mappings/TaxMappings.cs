using PayAPI.Contracts.Taxes;
using PayAPI.Domain.Entities;

namespace PayAPI.Mappings;

public static class TaxMappings
{
    public static TaxResponse ToResponse(this TaxDefinition tax) =>
        new(
            tax.Id,
            tax.Year,
            tax.TaxCode,
            tax.Percent,
            tax.Amount,
            tax.CreatedUtc,
            tax.UpdatedUtc
        );
}
