namespace EmployeeAPI.Domain.Entities;

public class Company
{
    public Guid Id { get; set; }
    public string ShortCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Industry { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
    public DateTime? DeletedUtc { get; set; }
}
