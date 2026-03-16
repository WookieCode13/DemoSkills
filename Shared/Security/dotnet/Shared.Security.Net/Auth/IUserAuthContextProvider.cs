namespace Shared.Security.Net.Auth;

/// <summary>
/// Provides the current user's authentication and authorization context, including permissions and roles.
/// </summary>
public interface IUserAuthContextProvider
{
    Task<UserAuthContext?> GetUserAuthContextAsync();
}