namespace Shared.Security.Net.Auth;

public interface IUserAuthRepository
{
    Task<UserAuthContext?> GetByCognitoSubAsync(string cognitoSub);
}