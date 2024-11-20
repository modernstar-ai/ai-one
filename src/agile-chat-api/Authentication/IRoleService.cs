namespace agile_chat_api.Authentication;

public interface IRoleService
{
    Task<List<string>> GetRolesByUserIdAsync(string userId);
    bool IsUserInRole(string role);
}