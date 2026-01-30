namespace EmployeeAPI.Contracts.Employees;

public record PatchEmployeeRequest(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    DateTime? DateOfBirth
);
