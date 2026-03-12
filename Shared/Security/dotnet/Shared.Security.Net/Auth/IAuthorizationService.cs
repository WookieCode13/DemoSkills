
namespace Shared.Security.Net.Auth;

public interface IAuthorizationService
{
    Task<bool> HasPermissionAsync(string permissionCode);
}