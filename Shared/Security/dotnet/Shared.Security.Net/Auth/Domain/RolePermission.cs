namespace Shared.Security.Net.Auth.Domain;

public class RolePermission
{
    public string RoleCode { get; set; } = null!;
    public string PermissionCode { get; set; } = null!;
    public DateTime CreatedUtc { get; set; }
}
