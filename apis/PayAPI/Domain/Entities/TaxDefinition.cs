namespace PayAPI.Domain.Entities;

public class TaxDefinition
{
    public Guid Id { get; set; }
    public int Year { get; set; }
    public string TaxCode { get; set; } = string.Empty;
    public decimal? Percent { get; set; }
    public decimal? Amount { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}
