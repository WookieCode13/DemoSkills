namespace Shared.Security.Net.Auth.Domain;

public class UserCompanyAccess
{
    public Guid UserCompanyAccessId { get; set; }
    public Guid AppUserId { get; set; }
    public Guid CompanyId { get; set; }
    public string CompanyRoleCode { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}
