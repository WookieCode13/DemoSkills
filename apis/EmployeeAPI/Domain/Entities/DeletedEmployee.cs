using EmployeeAPI.Domain.Entities;

public class DeletedEmployee : Employee
{
    public DateTime DeletedUtc { get; set; }
}