using System.ComponentModel.DataAnnotations;

namespace EmployeeAPI.Contracts.Employees;

public record CreateEmployeeRequest(
    [property: Required, StringLength(100)] string FirstName,
    [property: Required, StringLength(100)] string LastName,
    [property: Required, EmailAddress, StringLength(255)] string Email,
    [property: StringLength(20)] string? Phone,
    DateTime? DateOfBirth,
    [property: Required, StringLength(11)] string SSN
);
