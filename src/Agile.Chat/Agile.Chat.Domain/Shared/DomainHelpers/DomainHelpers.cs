using Agile.Chat.Domain.Shared.ValueObjects;

namespace Agile.Chat.Domain.Shared.DomainHelpers;

public static class DomainHelpers
{
    public static void NormalizeAccessControl(PermissionsAccessControl? accessControl)
    {
        if (accessControl == null)
            return;
        // Normalize all user IDs and group names to lowercase so that cosmos DB queries are case-insensitive
        accessControl.Users.UserIds = accessControl.Users.UserIds
            .Select(id => id.ToLowerInvariant())
            .ToList();
        accessControl.Users.Groups = accessControl.Users.Groups
            .Select(g => g.ToLowerInvariant())
            .ToList();
        accessControl.ContentManagers.UserIds = accessControl.ContentManagers.UserIds
            .Select(id => id.ToLowerInvariant())
            .ToList();
        accessControl.ContentManagers.Groups = accessControl.ContentManagers.Groups
            .Select(g => g.ToLowerInvariant())
            .ToList();
    }
}
