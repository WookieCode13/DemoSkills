using Microsoft.AspNetCore.Authorization;

namespace Shared.Security.Net.Auth;

/// <summary>
/// Authorization handler that checks if the user has the required permission code specified in the <see cref="PermissionRequirement"/>.
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IAppAuthorizationService _appAuthorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionAuthorizationHandler"/> class with the specified permission service.
    /// </summary>
    /// <param name="appAuthorizationService">The app authorization service used to check user permissions.</param>
    /// <summary>
    public PermissionAuthorizationHandler(IAppAuthorizationService appAuthorizationService)
    {
        _appAuthorizationService = appAuthorizationService;
    }

    /// <summary>
    /// Handles the authorization requirement by checking if the user has the required permission.
    /// </summary>
    /// <param name="context">The authorization context.</param>
    /// <param name="requirement">The permission requirement.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var hasPermission = await _appAuthorizationService.HasPermissionAsync(requirement.PermissionCode);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}
