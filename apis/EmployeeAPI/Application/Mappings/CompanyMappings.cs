using EmployeeAPI.Contracts.Companies;
using EmployeeAPI.Domain.Entities;

namespace EmployeeAPI.Mappings;

public static class CompanyMappings
{
    public static CompanyResponse ToResponse(this Company company) =>
        new(
            company.Id,
            company.ShortCode,
            company.Name,
            company.Industry,
            company.Email,
            company.Phone
        );
}
