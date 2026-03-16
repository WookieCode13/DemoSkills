using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Shared.Security.Net.Auth;

/// <summary>
/// Implements IUserAuthContextProvider to retrieve the current user's 
/// authentication and authorization context from the HTTP context and a user repository.
/// </summary>
public class UserAuthContextProvider : IUserAuthContextProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserAuthRepository _userAuthRepository;

    public UserAuthContextProvider(IHttpContextAccessor httpContextAccessor, IUserAuthRepository userAuthRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _userAuthRepository = userAuthRepository;
    }

    public async Task<UserAuthContext?> GetUserAuthContextAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var cognitoSub =
            user.FindFirstValue("sub") ??
            user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(cognitoSub))
            return null;

        return await _userAuthRepository.GetByCognitoSubAsync(cognitoSub);
    }
}
