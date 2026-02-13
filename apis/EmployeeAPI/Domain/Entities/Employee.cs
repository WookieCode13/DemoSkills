
namespace EmployeeAPI.Domain.Entities;

public class Employee
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";

    public string Email { get; set; } = "";
    public string? Phone { get; set; }

    // Note: In a real application, sensitive fields like SSN should be encrypted in the database and not exposed in API responses.
    // Sensitive fields stay in domain/DB, not in response contracts
    // TODO: public string? SsnEncrypted { get; set; }
    // todo remeber tpo encrypt in audit log (and logger if we log it there at all - maybe just log that it was changed but not the values)
    public string SSN { get; set; } = ""; // for this demo project lets come back to encyryption later

    public DateTime? DateOfBirth { get; set; }
    public DateTime? DeletedUtc { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}