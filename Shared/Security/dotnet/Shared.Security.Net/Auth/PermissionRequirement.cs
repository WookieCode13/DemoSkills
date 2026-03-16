using Microsoft.AspNetCore.Authorization;

namespace Shared.Security.Net.Auth;

/// <summary>
/// Represents a requirement for a specific permission code that can be used in authorization policies.
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Gets the permission code associated with this requirement.
    /// </summary>
    public string PermissionCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionRequirement"/> class with the specified permission code.
    /// </summary>
    /// <param name="permissionCode">The permission code associated with this requirement.</param>
    public PermissionRequirement(string permissionCode)
    {
        PermissionCode = permissionCode;
    }
}