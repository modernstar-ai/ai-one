using Agile.Framework.Authentication.Enums;
using Agile.Framework.Common.EnvironmentVariables;
using Microsoft.AspNetCore.Http;
using Microsoft.Graph;

namespace Agile.Framework.Authentication.Implementations.AzureAD;

public class AzureAdRoleService(GraphServiceClient graphServiceClient, IHttpContextAccessor httpContextAccessor) : BaseRoleService(httpContextAccessor)
{
    public override async Task<(List<UserRole>, List<string>)> GetRolesAndGroupsByUserIdAsync(string userId)
    {
        var settings = Configs.AzureAdPermissionsSettings;
        if(settings is null) throw new Exception("Settings is not configured for Azure AD Permissions Handler.");
        
        var roles = new List<UserRole>();
        var groups = new List<string>();
        // Fetch the user's memberOf groups (all groups the user is a member of)
        var groupsReq = await graphServiceClient.Me.MemberOf.Request().GetAsync();
        // Loop through the results and display group information
        while (true)
        {
            if(groupsReq == null || groupsReq.Count == 0) break;
            foreach (var directoryObject in groupsReq)
            {
                if (directoryObject is Group group)
                {
                    groups.Add(group.DisplayName);
                }
            }

            // If there are more results, get the next page
            if (groupsReq.NextPageRequest != null)
            {
                groupsReq = await groupsReq.NextPageRequest.GetAsync();
            }
            else
            {
                break;
            }
        }
        
        if (settings.SystemAdminGroups.Any(sysAdminGroup =>
                groups.Contains(sysAdminGroup, StringComparer.InvariantCultureIgnoreCase)))
            roles.Add(UserRole.SystemAdmin);
        else if (settings.ContentManagerGroups.Any(contentManagerGroup =>
                groups.Contains(contentManagerGroup, StringComparer.InvariantCultureIgnoreCase)))
            roles.Add(UserRole.ContentManager);
        else
            roles.Add(UserRole.EndUser);

        return (roles, groups);
    }
}