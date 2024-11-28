using System.Net.Http.Json;
using System.Security.Claims;
using Agile.Framework.Authentication.Enums;
using Agile.Framework.Authentication.Extensions;
using Agile.Framework.Authentication.Implementations.UTS.Models;
using Agile.Framework.Authentication.Interfaces;
using Agile.Framework.Common.EnvironmentVariables;
using Microsoft.AspNetCore.Http;

namespace Agile.Framework.Authentication.Implementations.UTS;

public class UtsRoleSerivce(IHttpContextAccessor httpContextAccessor) : IRoleService
{
    private string endpoint = Configs.UtsRoleApiEndpoint;
    private string xApiKey = Configs.UtsXApiKey;
    
    public async Task<(List<string>, List<string>)> GetRolesAndGroupsByUserIdAsync(string userId)
    {
        var roles = new List<string>();
        var groups = new List<string>();
        var client = new HttpClient();

        var message = new HttpRequestMessage(HttpMethod.Get, $"{endpoint}/rolelookup/getroles?userEmail={userId}");
        message.Headers.Add("XApiKey", xApiKey);
        
        var response = await client.SendAsync(message);
        if (!response.IsSuccessStatusCode) return (roles, groups);
        
        var userRolesstr = await response.Content.ReadAsStringAsync();
        var userRoles = await response.Content.ReadFromJsonAsync<List<UtsUserRole>>();
        foreach (var userRole in userRoles)
        {
            //System Admin role
            if (userRole.Role == UserRole.SystemAdmin)
            {
                roles.Add(userRole.Role.ToString());
                continue;
            }

            roles.AddRange(userRole.Groups.Select(group => $"{userRole.Role.ToString()}.{group.ToLower()}"));
            groups.AddRange(userRole.Groups.Select(group => group.ToLower()));
        }

        return (roles, groups);
    }

    public bool IsSystemAdmin() =>
        (httpContextAccessor.HttpContext?.User.IsInRole(UserRole.SystemAdmin.ToString()) ?? false);
    
    public bool IsContentManager() => GetRoleClaims().Any(role => role.Contains(UserRole.ContentManager.ToString()));
    
    public bool IsUserInRole(UserRole userRole, string group) => 
        (httpContextAccessor.HttpContext?.User.IsInRole(UserRole.SystemAdmin.ToString()) ?? false) ||
        (httpContextAccessor.HttpContext?.User.IsInRole($"{userRole.ToString()}.{group.ToLower()}") ?? false);
    
    public bool IsUserInGroup(string group) => 
        (httpContextAccessor.HttpContext?.User.IsInRole(UserRole.SystemAdmin.ToString()) ?? false) || 
        (httpContextAccessor.HttpContext?.User.IsInGroup(group.ToLower()) ?? false);
    
    public List<string> GetRoleClaims() => httpContextAccessor.HttpContext?.User.Claims
        .Where(x => x.Type == ClaimTypes.Role)
        .Select(x => x.Value)
        .ToList() ?? [];
    
    public List<string> GetGroupClaims() => httpContextAccessor.HttpContext?.User.Claims
        .Where(x => x.Type == "Groups")
        .Select(x => x.Value)
        .ToList() ?? [];
}