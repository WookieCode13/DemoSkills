namespace Shared.Security.Net.Auth.Domain;

public class Permission
{
    public string PermissionCode { get; set; } = null!;
    public string PermissionName { get; set; } = null!;
    public string? Description { get; set; }
    public string SystemCode { get; set; } = null!;
    public string ResourceCode { get; set; } = null!;
    public bool CanCreate { get; set; }
    public bool CanRead { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}
