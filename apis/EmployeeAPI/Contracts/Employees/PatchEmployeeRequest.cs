using System.ComponentModel.DataAnnotations;

namespace EmployeeAPI.Contracts.Employees;

public record PatchEmployeeRequest(
    [property: StringLength(100)] string? FirstName,
    [property: StringLength(100)] string? LastName,
    [property: EmailAddress, StringLength(255)] string? Email,
    [property: StringLength(20)] string? Phone,
    DateTime? DateOfBirth
);
