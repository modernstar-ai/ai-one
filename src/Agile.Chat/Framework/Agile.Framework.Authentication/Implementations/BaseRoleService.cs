using System.Security.Claims;
using Agile.Framework.Authentication.Enums;
using Agile.Framework.Authentication.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Agile.Framework.Authentication.Implementations;

public abstract class BaseRoleService(IHttpContextAccessor httpContextAccessor) : IRoleService
{
    public abstract Task<(List<UserRole>, List<string>)> GetRolesAndGroupsByUserIdAsync(string userId);
    public bool IsSystemAdmin() => httpContextAccessor.HttpContext?.User.HasClaim(x => x.Type == ClaimTypes.Role && x.Value == UserRole.SystemAdmin.ToString()) ?? false;
    public bool IsContentManager() => httpContextAccessor.HttpContext?.User.HasClaim(x => x.Type == ClaimTypes.Role && x.Value == UserRole.ContentManager.ToString()) ?? false;
    public bool IsUserInRole(UserRole userRole) => httpContextAccessor.HttpContext?.User.HasClaim(x => x.Type == ClaimTypes.Role && x.Value == userRole.ToString()) ?? false;
    public bool IsUserInGroup(string group) => httpContextAccessor.HttpContext?.User.Claims.Any(c =>
        c.Type == "Groups" && string.Equals(c.Value, group, StringComparison.CurrentCultureIgnoreCase)) ?? false;
    public List<string> GetRoleClaims() => httpContextAccessor.HttpContext?.User.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToList() ?? [];
    public List<string> GetGroupClaims() => httpContextAccessor.HttpContext?.User.Claims.Where(x => x.Type == "Groups").Select(x => x.Value).ToList() ?? [];
    public string UserId => (httpContextAccessor.HttpContext!.User.Identity as ClaimsIdentity)!.Name!;
}