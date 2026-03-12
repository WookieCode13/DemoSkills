namespace Shared.Security.Net.Auth;

public class AuthorizationService : IAuthorizationService
{
    private readonly IUserAuthContextProvider _userAuthContextProvider;

    public AuthorizationService(IUserAuthContextProvider userAuthContextProvider)
    {
        _userAuthContextProvider = userAuthContextProvider;
    }

    public async Task<bool> HasPermissionAsync(string permissionCode)
    {
        var userAuthContext = await _userAuthContextProvider.GetUserAuthContextAsync();
        if (userAuthContext == null)
        {
            return false;
        }
        return userAuthContext.Permissions
            .Any(p => p.PermissionCode.Equals(permissionCode, StringComparison.OrdinalIgnoreCase));
    }
}