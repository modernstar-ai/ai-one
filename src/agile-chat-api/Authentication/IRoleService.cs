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

public class DefaultRoleService : IRoleService
{
    public async Task<(List<string>, List<string>)> GetRolesAndGroupsByUserIdAsync(string userId) => ([], []);
    public bool IsSystemAdmin() => true;
    public bool IsUserInRole(UserRole userRole, string group) => true;
    public bool IsUserInGroup(string group) => true;
    public List<string> GetRoleClaims() => [];
    public List<string> GetGroupClaims() => [];
}