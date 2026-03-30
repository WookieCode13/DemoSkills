namespace PayAPI.Contracts.Taxes;

public record TaxResponse(
    Guid Id,
    int Year,
    string TaxCode,
    decimal? Percent,
    decimal? Amount,
    DateTime CreatedUtc,
    DateTime UpdatedUtc
);
