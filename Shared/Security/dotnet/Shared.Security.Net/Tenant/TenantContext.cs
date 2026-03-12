namespace Shared.Security.Net.Tenant;

public sealed class TenantContext
{
    public string? CompanyShortName { get; init; }

    // what group they are in (from JWT later)
    //public string? Group { get; init; }

    // derived from Group (100/300/500)
    //public int? Precedence { get; init; }
}