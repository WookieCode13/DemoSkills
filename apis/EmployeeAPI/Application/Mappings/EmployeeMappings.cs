using EmployeeAPI.Domain.Entities;
using EmployeeAPI.Contracts.Employees;

namespace EmployeeAPI.Mappings;

public static class EmployeeMappings
{
        public static EmployeeResponse ToResponse(this Employee e) =>
        new(
            e.Id,
            e.FirstName,
            e.LastName,
            e.Email,
            e.Phone,
            e.DateOfBirth
        );
}