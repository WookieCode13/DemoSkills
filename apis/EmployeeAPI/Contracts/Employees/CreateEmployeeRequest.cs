using System.ComponentModel.DataAnnotations;

namespace EmployeeAPI.Contracts.Employees;

public record CreateEmployeeRequest(
    [Required, StringLength(100)] string FirstName,
    [Required, StringLength(100)] string LastName,
    [Required, EmailAddress, StringLength(255)] string Email,
    [StringLength(20)] string? Phone,
    DateTime? DateOfBirth,
    [Required, StringLength(11)] string SSN
);
