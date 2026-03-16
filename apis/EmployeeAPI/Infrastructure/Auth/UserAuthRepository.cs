using EmployeeAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Security.Net.Auth;
using Shared.Security.Net.Auth.Domain;

namespace EmployeeAPI.Infrastructure.Auth;

public class UserAuthRepository : IUserAuthRepository
{
    private readonly AppDbContext _db;

    public UserAuthRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<UserAuthContext?> GetByCognitoSubAsync(string cognitoSub)
    {
        var user = await _db.AuthAppUsers
            .AsNoTracking()
            .Where(u => u.CognitoSub == cognitoSub && u.IsActive)
            .Select(u => new
            {
                u.AppUserId,
                u.CognitoSub,
                u.Email,
                u.BaseRoleLevel,
                u.GlobalRoleCode
            })
            .FirstOrDefaultAsync();

        if (user is null)
        {
            return null;
        }

        var permissions = new List<UserPermission>();

        if (!string.IsNullOrWhiteSpace(user.GlobalRoleCode))
        {
            permissions = await (
                from rp in _db.AuthRolePermissions.AsNoTracking()
                join p in _db.AuthPermissions.AsNoTracking()
                    on rp.PermissionCode equals p.PermissionCode
                where rp.RoleCode == user.GlobalRoleCode && p.IsActive
                select new UserPermission(p.PermissionCode)
            ).ToListAsync();
        }

        return new UserAuthContext
        {
            AppUserId = user.AppUserId,
            CognitoSub = user.CognitoSub,
            Email = user.Email,
            BaseRoleLevel = user.BaseRoleLevel,
            GlobalRoleCode = user.GlobalRoleCode,
            Permissions = permissions
        };
    }
}
