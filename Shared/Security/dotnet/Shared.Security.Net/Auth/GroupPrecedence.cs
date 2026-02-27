namespace Shared.Security.Net.Auth;

public static class GroupPrecedence
{
    public const int InternalAdmin = 100;
    public const int InternalSupport = 300;
    public const int ExternalUser = 500;

    public static int? FromGroup(string? group) => group switch
    {
        GroupNames.InternalAdmin => InternalAdmin,
        GroupNames.InternalSupport => InternalSupport,
        GroupNames.ExternalUser => ExternalUser,
        _ => null
    };
}