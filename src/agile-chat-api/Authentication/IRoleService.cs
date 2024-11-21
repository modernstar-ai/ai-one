namespace agile_chat_api.Authentication;

public interface IRoleService
{
    Task<(List<string>, List<string>)> GetRolesAndGroupsByUserIdAsync(string userId);
    bool IsSystemAdmin();
    bool IsUserInRole(UserRole userRole, string group);
    bool IsUserInGroup(string group);
    List<string> GetRoleClaims();
    List<string> GetGroupClaims();
}