using System.Security.Claims;
using Agile.Framework.Authentication.Enums;
using Agile.Framework.Authentication.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Agile.Framework.Authentication.Implementations.Default;

public class DefaultRoleService(IHttpContextAccessor httpContextAccessor) : IRoleService
{
    public async Task<(List<UserRole>, List<string>)> GetRolesAndGroupsByUserIdAsync(string userId) => ([UserRole.SystemAdmin], []);
    public bool IsSystemAdmin() => true;
    public bool IsContentManager() => true;
    public bool IsUserInRole(UserRole userRole) => true;
    public bool IsUserInGroup(string group) => true;
    public List<string> GetRoleClaims() => [];
    public List<string> GetGroupClaims() => [];
    public string UserId => (httpContextAccessor.HttpContext!.User.Identity as ClaimsIdentity)!.Name!;
}