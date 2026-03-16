
namespace Shared.Security.Net.Auth;

public interface IAppAuthorizationService
{
    Task<bool> HasPermissionAsync(string permissionCode);
}