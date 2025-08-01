﻿using Agile.Framework.Authentication.Enums;

namespace Agile.Framework.Authentication.Interfaces;

public interface IRoleService
{
    Task<(List<UserRole>, List<string>)> GetRolesAndGroupsByUserIdAsync(string userId);
    bool IsSystemAdmin();
    bool IsContentManager();
    bool IsUserInRole(UserRole userRole);
    bool IsUserInGroup(string group);
    List<string> GetRoleClaims();
    List<string> GetGroupClaims();
    string UserId { get; }
}