using System.Security.Claims;

namespace agile_chat_api.Authentication.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static bool IsInGroup(this ClaimsPrincipal principal, string groupName) => principal.Claims.Any(c => c.Type == "Groups" && c.Value == groupName);
}