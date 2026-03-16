using EmployeeAPI.Infrastructure.Auth;
using EmployeeAPI.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shared.Security.Net.Auth.Domain;
using Xunit;

namespace EmployeeAPI.UnitTests.Infrastructure.Auth;

public sealed class UserAuthRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;
    private readonly UserAuthRepository _sut;

    public UserAuthRepositoryTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();
        _sut = new UserAuthRepository(_db);
    }

    [Fact]
    public async Task GetByCognitoSubAsync_ReturnsNull_WhenUserDoesNotExist()
    {
        var result = await _sut.GetByCognitoSubAsync("missing-sub");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByCognitoSubAsync_ReturnsNull_WhenUserIsInactive()
    {
        SeedUser(cognitoSub: "inactive-sub", isActive: false);
        await _db.SaveChangesAsync();

        var result = await _sut.GetByCognitoSubAsync("inactive-sub");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByCognitoSubAsync_ReturnsUserWithEmptyPermissions_WhenGlobalRoleCodeIsMissing()
    {
        var userId = SeedUser(cognitoSub: "no-role-sub", globalRoleCode: null);
        await _db.SaveChangesAsync();

        var result = await _sut.GetByCognitoSubAsync("no-role-sub");

        Assert.NotNull(result);
        Assert.Equal(userId, result!.AppUserId);
        Assert.Empty(result.Permissions);
    }

    [Fact]
    public async Task GetByCognitoSubAsync_ReturnsOnlyActivePermissions_ForUsersGlobalRole()
    {
        var userId = SeedUser(cognitoSub: "sub-123", globalRoleCode: "ADMIN");

        _db.AuthPermissions.AddRange(
            CreatePermission("employee.read", isActive: true),
            CreatePermission("employee.write", isActive: false),
            CreatePermission("reports.read", isActive: true));

        _db.AuthRolePermissions.AddRange(
            new RolePermission
            {
                RoleCode = "ADMIN",
                PermissionCode = "employee.read",
                CreatedUtc = DateTime.UtcNow
            },
            new RolePermission
            {
                RoleCode = "ADMIN",
                PermissionCode = "employee.write",
                CreatedUtc = DateTime.UtcNow
            },
            new RolePermission
            {
                RoleCode = "HR",
                PermissionCode = "reports.read",
                CreatedUtc = DateTime.UtcNow
            });

        await _db.SaveChangesAsync();

        var result = await _sut.GetByCognitoSubAsync("sub-123");

        Assert.NotNull(result);
        Assert.Equal(userId, result!.AppUserId);
        Assert.Equal("user@test.com", result.Email);
        Assert.Equal("ADMIN", result.GlobalRoleCode);
        Assert.Single(result.Permissions);
        Assert.Contains(result.Permissions, permission => permission.PermissionCode == "employee.read");
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private Guid SeedUser(string cognitoSub, bool isActive = true, string? globalRoleCode = "ADMIN")
    {
        var userId = Guid.NewGuid();

        _db.AuthAppUsers.Add(new AppUser
        {
            AppUserId = userId,
            CognitoSub = cognitoSub,
            Email = "user@test.com",
            BaseRoleLevel = 1,
            GlobalRoleCode = globalRoleCode,
            IsActive = isActive,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow
        });

        return userId;
    }

    private static Permission CreatePermission(string permissionCode, bool isActive)
    {
        return new Permission
        {
            PermissionCode = permissionCode,
            PermissionName = permissionCode,
            Description = $"Permission for {permissionCode}",
            SystemCode = "employee",
            ResourceCode = "employee",
            CanCreate = false,
            CanRead = true,
            CanUpdate = false,
            CanDelete = false,
            IsActive = isActive,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow
        };
    }
}
