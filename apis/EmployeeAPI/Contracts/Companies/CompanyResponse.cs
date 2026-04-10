namespace EmployeeAPI.Contracts.Companies;

public record CompanyResponse(
    Guid Id,
    string ShortCode,
    string Name,
    string? Industry,
    string? Email,
    string? Phone
);
