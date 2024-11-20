using System.Net.Http.Headers;
using agile_chat_api.Authentication.UTS.Models;

namespace agile_chat_api.Authentication.UTS;

public interface IUTSRoleService : IRoleService
{
    bool IsUserInRole(UserRole userRole, string group);
}
public class UTSRoleService(IHttpContextAccessor httpContextAccessor) : IUTSRoleService
{
    private const string endpoint = "https://subjapi-dev-api-phmd3jzubzalo.azurewebsites.net/api";
    private const string xApiKey = "611632d4-5a8e-4b00-81c1-ea5cc76ac0ac";
    
    public async Task<List<string>> GetRolesByUserIdAsync(string userId)
    {
        var roles = new List<string>();
        var client = new HttpClient();
        
        var message = new HttpRequestMessage(HttpMethod.Get, $"{endpoint}/rolelookup/getroles/{userId}");
        message.Headers.Add("XApiKey", xApiKey);
        
        var response = await client.SendAsync(message);
        if (response.IsSuccessStatusCode)
        {
            var userRoles = await response.Content.ReadFromJsonAsync<List<UTSUserRole>>();
            foreach (var userRole in userRoles)
                foreach (var group in userRole.Groups)
                    roles.Add($"{userRole.Role.ToString()}.{group}");
        }

        return roles;
    }

    public bool IsUserInRole(string role) => httpContextAccessor.HttpContext?.User.IsInRole(role) ?? false;

    public bool IsUserInRole(UserRole userRole, string group) => httpContextAccessor.HttpContext?.User.IsInRole($"{userRole.ToString()}.{group}") ?? false;
}