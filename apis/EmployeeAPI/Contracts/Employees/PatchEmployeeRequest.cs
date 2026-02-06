using System.ComponentModel.DataAnnotations;

namespace EmployeeAPI.Contracts.Employees;

public record PatchEmployeeRequest(
    [StringLength(100)] string? FirstName,
    [StringLength(100)] string? LastName,
    [EmailAddress, StringLength(255)] string? Email,
    [StringLength(20)] string? Phone,
    DateTime? DateOfBirth
);
