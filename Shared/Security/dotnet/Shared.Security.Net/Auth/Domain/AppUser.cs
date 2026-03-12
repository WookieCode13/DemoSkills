namespace Shared.Security.Net.Auth.Domain;

public class AppUser
{
    public Guid AppUserId { get; set; }
    public string CognitoSub { get; set; } = null!;
    public string? Email { get; set; }
    public int BaseRoleLevel { get; set; }
    public string? GlobalRoleCode { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}