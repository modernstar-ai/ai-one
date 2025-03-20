using System.Linq.Expressions;
using Agile.Chat.Domain.Shared.Interfaces;
using Agile.Framework.Authentication.Enums;
using Agile.Framework.Authentication.Interfaces;

namespace Agile.Chat.Application.ExtensionMethods;

public static class IQueryableExtensionMethods
{
    public static IQueryable<T> AccessControlQuery<T>(this IQueryable<T> queryable, IRoleService roleService) where T: IAccessControllable
    {
        var roleClaims = roleService.GetRoleClaims();
        var groupClaims = roleService.GetGroupClaims();
        var userId = roleService.UserId;
        
        return queryable.Where(x => 
            x.AccessControl.Users.AllowAccessToAll || 
            x.AccessControl.Users.UserIds.Contains(userId) || 
            x.AccessControl.ContentManagers.UserIds.Contains(userId) || 
            x.AccessControl.Users.Groups.Any((group) => groupClaims.Contains(group)) ||
            x.AccessControl.ContentManagers.Groups.Any((group) => groupClaims.Contains(group)) ||
            (roleClaims.Contains(UserRole.ContentManager.ToString()) && x.AccessControl.ContentManagers.AllowAccessToAll)
        );
    }
}