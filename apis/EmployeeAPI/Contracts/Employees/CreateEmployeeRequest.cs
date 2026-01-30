namespace EmployeeAPI.Contracts.Employees;

public record CreateEmployeeRequest(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    DateTime? DateOfBirth,
    string SSN
);