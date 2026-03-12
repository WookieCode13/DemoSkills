namespace Shared.Security.Net.Auth;

public record UserAuthContext
{
    public Guid AppUserId { get; init; }
    public string CognitoSub { get; init; } = string.Empty;
    public string? Email { get; init; }
    public int BaseRoleLevel { get; init; }
    public string? GlobalRoleCode { get; init; }

    public IReadOnlyCollection<UserPermission> Permissions { get; init; } = new List<UserPermission>();
}
