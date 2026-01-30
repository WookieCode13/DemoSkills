namespace EmployeeAPI.Contracts.Employees;

public record EmployeeResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    DateTime? DateOfBirth
    // TODO: SSN masked or omitted for security
);