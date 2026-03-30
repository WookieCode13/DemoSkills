using System.ComponentModel.DataAnnotations;

namespace EmployeeAPI.Contracts.Companies;

public record CreateCompanyRequest(
    [Required, StringLength(10, MinimumLength = 10)] string ShortCode,
    [Required, StringLength(255)] string Name,
    [StringLength(255)] string? Industry,
    [EmailAddress, StringLength(255)] string? Email,
    [StringLength(20)] string? Phone
);
