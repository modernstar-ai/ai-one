using Agile.Framework.Authentication.Enums;
using Agile.Framework.Authentication.Interfaces;

namespace Agile.Framework.Authentication.Implementations.Default;

public class DefaultRoleService : IRoleService
{
    public async Task<(List<string>, List<string>)> GetRolesAndGroupsByUserIdAsync(string userId) => ([UserRole.SystemAdmin.ToString()], []);
    public bool IsSystemAdmin() => true;
    public bool IsContentManager() => true;
    public bool IsUserInRole(UserRole userRole, string group) => true;
    public bool IsUserInGroup(string group) => true;
    public List<string> GetRoleClaims() => [];
    public List<string> GetGroupClaims() => [];
}